using FacturasSRI.Application.Dtos;
using FacturasSRI.Core.Models;
using FacturasSRI.Core.Services;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Domain.Enums;
using FacturasSRI.Infrastructure.Persistence;
using FacturasSRI.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class CreditNoteService : ICreditNoteService
    {
        private readonly FacturasSRIDbContext _context;
        private readonly ILogger<CreditNoteService> _logger;
        private readonly IConfiguration _configuration;
        private readonly XmlGeneratorService _xmlGeneratorService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly SriApiClientService _sriApiClientService;
        private readonly SriResponseParserService _sriResponseParserService;
        private readonly IEmailService _emailService;
        private readonly PdfGeneratorService _pdfGenerator;

        // Semáforo para evitar concurrencia en la generación de números secuenciales
        private static readonly SemaphoreSlim _ncSemaphore = new SemaphoreSlim(1, 1);

        public CreditNoteService(
            FacturasSRIDbContext context,
            ILogger<CreditNoteService> logger,
            IConfiguration configuration,
            XmlGeneratorService xmlGeneratorService,
            IServiceScopeFactory serviceScopeFactory,
            SriApiClientService sriApiClientService,
            SriResponseParserService sriResponseParserService,
            IEmailService emailService,
            PdfGeneratorService pdfGenerator)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _xmlGeneratorService = xmlGeneratorService;
            _serviceScopeFactory = serviceScopeFactory;
            _sriApiClientService = sriApiClientService;
            _sriResponseParserService = sriResponseParserService;
            _emailService = emailService;
            _pdfGenerator = pdfGenerator;
        }

        public async Task ResendCreditNoteEmailAsync(Guid creditNoteId)
        {
            await Task.Run(async () =>
            {
                var creditNote = await GetCreditNoteDetailByIdAsync(creditNoteId);
                var creditNoteEntity = await _context.NotasDeCredito.Include(i => i.InformacionSRI).FirstOrDefaultAsync(i => i.Id == creditNoteId);

                if (creditNote == null || creditNoteEntity == null) return;
                if (string.IsNullOrEmpty(creditNote.ClienteEmail)) return;

                try
                {
                    var pdfBytes = _pdfGenerator.GenerarNotaCreditoPdf(creditNote);
                    var xmlFirmado = creditNoteEntity.InformacionSRI?.XmlFirmado ?? "";

                    await _emailService.SendCreditNoteEmailAsync(
                        creditNote.ClienteEmail,
                        creditNote.ClienteNombre,
                        creditNote.NumeroNotaCredito,
                        creditNote.Id,
                        pdfBytes,
                        xmlFirmado
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando email de nota de crédito en background");
                }
            });
        }


        // 1. PARA EL LISTADO
        public async Task<List<CreditNoteDto>> GetCreditNotesAsync()
        {
            // Primero, obtenemos los IDs de las NC que no están en un estado definitivo
            var nonFinalizedIds = await _context.NotasDeCredito
                .Where(nc => nc.Estado == EstadoNotaDeCredito.EnviadaSRI || nc.Estado == EstadoNotaDeCredito.Pendiente)
                .Select(nc => nc.Id)
                .ToListAsync();

            // Ahora, iteramos sobre esos IDs y actualizamos su estado
            // Esto no es ideal para un rendimiento a gran escala, pero asegura la consistencia para el usuario.
            foreach (var id in nonFinalizedIds)
            {
                await CheckSriStatusAsync(id);
            }

            // Finalmente, obtenemos la lista actualizada para mostrarla
            return await _context.NotasDeCredito
                .AsNoTracking()
                .Include(nc => nc.Cliente)
                .Include(nc => nc.Factura)
                .OrderByDescending(nc => nc.FechaCreacion) // Ordenar por fecha de creación para ver las más recientes primero
                .Select(nc => new CreditNoteDto
                {
                    Id = nc.Id,
                    NumeroNotaCredito = nc.NumeroNotaCredito,
                    FechaEmision = nc.FechaEmision,
                    ClienteNombre = nc.Cliente.RazonSocial,
                    NumeroFacturaModificada = nc.Factura.NumeroFactura,
                    Total = nc.Total,
                    Estado = nc.Estado,
                    RazonModificacion = nc.RazonModificacion
                })
                .ToListAsync();
        }

        // 2. PARA VER DETALLES
        public async Task<CreditNoteDetailViewDto?> GetCreditNoteDetailByIdAsync(Guid id)
        {
            // Intentamos actualizar estado si no es definitivo
            await CheckSriStatusAsync(id);

            var nc = await _context.NotasDeCredito
                .Include(n => n.Cliente)
                .Include(n => n.Factura)
                .Include(n => n.InformacionSRI)
                .Include(n => n.Detalles)
                    .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p.ProductoImpuestos)
                    .ThenInclude(pi => pi.Impuesto)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nc == null) return null;

            var itemsDto = nc.Detalles.Select(d => new CreditNoteItemDetailDto
            {
                ProductoId = d.ProductoId,
                ProductCode = d.Producto.CodigoPrincipal,
                ProductName = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioVentaUnitario = d.PrecioVentaUnitario,
                Subtotal = d.Subtotal
            }).ToList();

            // Cálculo de impuestos para visualización
            var taxSummaries = nc.Detalles
                .SelectMany(d => d.Producto.ProductoImpuestos.Select(pi => new
                {
                    d.Subtotal,
                    TaxName = pi.Impuesto.Nombre,
                    TaxRate = pi.Impuesto.Porcentaje
                }))
                .GroupBy(x => new { x.TaxName, x.TaxRate })
                .Select(g => new TaxSummary
                {
                    TaxName = g.Key.TaxName,
                    TaxRate = g.Key.TaxRate,
                    Amount = g.Sum(x => x.Subtotal * (x.TaxRate / 100m))
                })
                .Where(x => x.Amount > 0 || x.TaxRate == 0)
                .ToList();

            return new CreditNoteDetailViewDto
            {
                Id = nc.Id,
                NumeroNotaCredito = nc.NumeroNotaCredito,
                FechaEmision = nc.FechaEmision,
                ClienteNombre = nc.Cliente.RazonSocial,
                ClienteIdentificacion = nc.Cliente.NumeroIdentificacion,
                ClienteDireccion = nc.Cliente.Direccion,
                ClienteEmail = nc.Cliente.Email,
                FacturaId = nc.FacturaId,
                NumeroFacturaModificada = nc.Factura.NumeroFactura,
                FechaEmisionFacturaModificada = nc.Factura.FechaEmision,
                RazonModificacion = nc.RazonModificacion,
                SubtotalSinImpuestos = nc.SubtotalSinImpuestos,
                TotalIVA = nc.TotalIVA,
                Total = nc.Total,
                Estado = nc.Estado,
                ClaveAcceso = nc.InformacionSRI?.ClaveAcceso,
                NumeroAutorizacion = nc.InformacionSRI?.NumeroAutorizacion,
                RespuestaSRI = nc.InformacionSRI?.RespuestaSRI,
                Items = itemsDto,
                TaxSummaries = taxSummaries
            };
        }

        // 3. CREACIÓN (STOCK ELIMINADO DE AQUÍ)
        public async Task<NotaDeCredito> CreateCreditNoteAsync(CreateCreditNoteDto dto)
        {
            await _ncSemaphore.WaitAsync();
            try
            {
                var factura = await _context.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Detalles).ThenInclude(d => d.Producto).ThenInclude(p => p.ProductoImpuestos).ThenInclude(pi => pi.Impuesto)
                    .FirstOrDefaultAsync(f => f.Id == dto.FacturaId);

                if (factura == null) throw new InvalidOperationException("La factura original no existe.");
                if (factura.Estado != EstadoFactura.Autorizada) throw new InvalidOperationException("Solo se pueden emitir Notas de Crédito a facturas AUTORIZADAS.");

                // Configuración
                var establishmentCode = _configuration["CompanyInfo:EstablishmentCode"];
                var emissionPointCode = _configuration["CompanyInfo:EmissionPointCode"];
                var rucEmisor = _configuration["CompanyInfo:Ruc"];
                var environmentType = _configuration["CompanyInfo:EnvironmentType"];
                var certPath = _configuration["CompanyInfo:CertificatePath"];
                var certPass = _configuration["CompanyInfo:CertificatePassword"];

                // Secuencial
                var secuencialEntity = await _context.Secuenciales.FirstOrDefaultAsync(s => s.Establecimiento == establishmentCode && s.PuntoEmision == emissionPointCode);
                if (secuencialEntity == null)
                {
                    secuencialEntity = new Secuencial { Id = Guid.NewGuid(), Establecimiento = establishmentCode, PuntoEmision = emissionPointCode, UltimoSecuencialFactura = 0, UltimoSecuencialNotaCredito = 0 };
                    _context.Secuenciales.Add(secuencialEntity);
                }
                secuencialEntity.UltimoSecuencialNotaCredito++;
                string numeroSecuencialStr = secuencialEntity.UltimoSecuencialNotaCredito.ToString("D9");

                // Crear Entidad
                var nc = new NotaDeCredito
                {
                    Id = Guid.NewGuid(),
                    FacturaId = factura.Id,
                    ClienteId = factura.ClienteId.Value,
                    FechaEmision = DateTime.UtcNow,
                    NumeroNotaCredito = numeroSecuencialStr,
                    Estado = dto.EsBorrador ? EstadoNotaDeCredito.Borrador : EstadoNotaDeCredito.Pendiente,
                    RazonModificacion = dto.RazonModificacion,
                    UsuarioIdCreador = dto.UsuarioIdCreador,
                    FechaCreacion = DateTime.UtcNow
                };

                decimal subtotalAccum = 0;
                decimal ivaAccum = 0;

                foreach (var itemDto in dto.Items)
                {
                    if (itemDto.CantidadDevolucion <= 0) continue;

                    var detalleFactura = factura.Detalles.FirstOrDefault(d => d.ProductoId == itemDto.ProductoId);
                    if (detalleFactura == null) throw new Exception($"Producto ID {itemDto.ProductoId} inválido.");
                    if (itemDto.CantidadDevolucion > (detalleFactura.Cantidad - detalleFactura.CantidadDevuelta)) throw new Exception($"La cantidad a devolver para '{detalleFactura.Producto.Nombre}' excede la cantidad disponible ({detalleFactura.Cantidad - detalleFactura.CantidadDevuelta}).");

                    decimal precioUnit = detalleFactura.PrecioVentaUnitario;
                    decimal subtotalItem = itemDto.CantidadDevolucion * precioUnit;
                    decimal valorIvaItem = 0;
                    var impuestoIva = detalleFactura.Producto.ProductoImpuestos.FirstOrDefault(pi => pi.Impuesto.Porcentaje > 0);
                    if (impuestoIva != null) valorIvaItem = subtotalItem * (impuestoIva.Impuesto.Porcentaje / 100);

                    var detalleNc = new NotaDeCreditoDetalle
                    {
                        Id = Guid.NewGuid(),
                        NotaDeCreditoId = nc.Id,
                        ProductoId = itemDto.ProductoId,
                        Producto = detalleFactura.Producto,
                        Cantidad = itemDto.CantidadDevolucion,
                        PrecioVentaUnitario = precioUnit,
                        DescuentoAplicado = 0,
                        Subtotal = subtotalItem,
                        ValorIVA = valorIvaItem
                    };
                    nc.Detalles.Add(detalleNc);

                    subtotalAccum += subtotalItem;
                    ivaAccum += valorIvaItem;

                    // ¡¡IMPORTANTE!!: AQUÍ YA NO DEVOLVEMOS STOCK. 
                    // El stock solo se devuelve si el SRI Autoriza.
                }

                nc.SubtotalSinImpuestos = subtotalAccum;
                nc.TotalIVA = ivaAccum;
                nc.Total = subtotalAccum + ivaAccum;

                _context.NotasDeCredito.Add(nc);

                // SRI Data must be created for both drafts and regular notes to reserve the sequential number and clave de acceso
                var fechaEcuador = GetEcuadorTime(nc.FechaEmision);
                string claveAcceso = GenerarClaveAcceso(fechaEcuador, "04", rucEmisor, establishmentCode, emissionPointCode, numeroSecuencialStr, environmentType);

                var ncSri = new NotaDeCreditoSRI
                {
                    NotaDeCreditoId = nc.Id,
                    ClaveAcceso = claveAcceso
                };
                _context.NotasDeCreditoSRI.Add(ncSri);

                // If it's not a draft, proceed with XML generation and SRI submission
                if (!dto.EsBorrador)
                {
                    var (xmlGenerado, xmlFirmadoBytes) = _xmlGeneratorService.GenerarYFirmarNotaCredito(claveAcceso, nc, factura.Cliente, factura, certPath, certPass);

                    ncSri.XmlGenerado = xmlGenerado;
                    ncSri.XmlFirmado = Encoding.UTF8.GetString(xmlFirmadoBytes);
                    
                    nc.Estado = EstadoNotaDeCredito.EnviadaSRI;

                    // Save changes before starting background task
                    await _context.SaveChangesAsync();

                    // Launch background process
                    _ = Task.Run(() => EnviarNcAlSriEnFondoAsync(nc.Id, xmlFirmadoBytes));
                }
                else
                {
                    // Just save the draft and its associated SRI entity
                    await _context.SaveChangesAsync();
                }

                return nc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando Nota de Crédito");
                throw;
            }
            finally
            {
                _ncSemaphore.Release();
            }
        }

        // 4. VERIFICACIÓN MANUAL DE ESTADO (Corrección de Stock aquí también)
        public async Task CheckSriStatusAsync(Guid ncId)
        {
            var nc = await _context.NotasDeCredito
                .Include(n => n.InformacionSRI)
                .Include(n => n.Detalles).ThenInclude(d => d.Producto) // Necesario para devolver stock
                .FirstOrDefaultAsync(n => n.Id == ncId);

            if (nc == null || nc.InformacionSRI == null) return;
            
            // Si ya está autorizada o cancelada, no hacemos nada
            if (nc.Estado == EstadoNotaDeCredito.Autorizada || nc.Estado == EstadoNotaDeCredito.Cancelada) return;

            try
            {
                if (string.IsNullOrEmpty(nc.InformacionSRI.ClaveAcceso)) return;

                string respAut = await _sriApiClientService.ConsultarAutorizacionAsync(nc.InformacionSRI.ClaveAcceso);
                var autObj = _sriResponseParserService.ParsearRespuestaAutorizacion(respAut);

                if (autObj.Estado == "AUTORIZADO")
                {
                    bool wasAlreadyAuthorized = nc.Estado == EstadoNotaDeCredito.Autorizada;

                    // Actualizar estado y datos del SRI
                    nc.Estado = EstadoNotaDeCredito.Autorizada;
                    nc.InformacionSRI.NumeroAutorizacion = autObj.NumeroAutorizacion;
                    nc.InformacionSRI.FechaAutorizacion = autObj.FechaAutorizacion;
                    nc.InformacionSRI.RespuestaSRI = "AUTORIZADO";

                    // Solo si antes NO estaba autorizada, realizamos acciones críticas
                    if (!wasAlreadyAuthorized)
                    {
                        _logger.LogInformation("NC {Id} cambiando a AUTORIZADO por primera vez. Restaurando stock y enviando correo.", ncId);
                        
                        await RestaurarStockAsync(_context, nc);
                        await ActualizarCantidadesDevueltasAsync(_context, nc);
                        
                        // Guardamos los cambios de estado y stock antes de encolar el correo
                        await _context.SaveChangesAsync();

                        // Encolar el envío de correo en un fire-and-forget para no bloquear la respuesta
                        _ = Task.Run(async () => {
                            try
                            {
                                // Necesitamos cargar de nuevo las navegaciones para el DTO en este nuevo hilo
                                var ncWithDetailsForEmail = await GetCreditNoteDetailByIdAsync(ncId);
                                if (ncWithDetailsForEmail != null && !string.IsNullOrEmpty(ncWithDetailsForEmail.ClienteEmail))
                                {
                                    byte[] pdfBytes = _pdfGenerator.GenerarNotaCreditoPdf(ncWithDetailsForEmail);
                                    string xmlSigned = nc.InformacionSRI.XmlFirmado;
                                    await _emailService.SendCreditNoteEmailAsync(
                                        ncWithDetailsForEmail.ClienteEmail, 
                                        ncWithDetailsForEmail.ClienteNombre, 
                                        ncWithDetailsForEmail.NumeroNotaCredito, 
                                        ncId, 
                                        pdfBytes, 
                                        xmlSigned);
                                    _logger.LogInformation("Correo para NC {Id} encolado desde CheckSriStatusAsync.", ncId);
                                }
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, "Error al enviar correo para NC {Id} desde CheckSriStatusAsync.", ncId);
                            }
                        });
                    }
                    else
                    {
                        // Si ya estaba autorizada, solo guardamos por si hay algún cambio menor en la info del SRI
                        await _context.SaveChangesAsync();
                    }
                }
                else if (autObj.Estado == "NO AUTORIZADO")
                {
                    nc.Estado = EstadoNotaDeCredito.RechazadaSRI;
                    nc.InformacionSRI.RespuestaSRI = JsonSerializer.Serialize(autObj.Errores);
                    await _context.SaveChangesAsync();
                }
                // Si sigue en PROCESAMIENTO, no cambiamos estado
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error consultando estado SRI para NC {ncId}: {ex.Message}");
            }
        }

        // 5. MÉTODO PRIVADO PARA RESTAURAR STOCK (LÓGICA CENTRALIZADA)
        private async Task RestaurarStockAsync(FacturasSRIDbContext context, NotaDeCredito nc)
        {
            foreach (var detalle in nc.Detalles)
            {
                var producto = await context.Productos.FindAsync(detalle.ProductoId);
                if (producto != null && producto.ManejaInventario)
                {
                    if (producto.ManejaLotes)
                    {
                        // Buscar los consumos de la FACTURA original para saber a qué lotes devolver
                        var consumos = await context.FacturaDetalleConsumoLotes
                            .Include(c => c.FacturaDetalle)
                            .Where(c => c.FacturaDetalle.FacturaId == nc.FacturaId && c.FacturaDetalle.ProductoId == detalle.ProductoId)
                            .Include(c => c.Lote)
                            .OrderByDescending(c => c.Lote.FechaCaducidad)
                            .ToListAsync();

                        int remanente = detalle.Cantidad;
                        foreach (var consumo in consumos)
                        {
                            if (remanente <= 0) break;
                            consumo.Lote.CantidadDisponible += remanente; // Simplificación: devolvemos al más reciente disponible
                            remanente = 0;
                        }
                    }
                    else
                    {
                        producto.StockTotal += detalle.Cantidad;
                    }
                }
            }
            _logger.LogInformation($"Stock restaurado para Nota de Crédito {nc.NumeroNotaCredito}");
        }

        private async Task ActualizarCantidadesDevueltasAsync(FacturasSRIDbContext context, NotaDeCredito nc)
        {
            foreach (var detalleNc in nc.Detalles)
            {
                var facturaDetalle = await context.FacturaDetalles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fd => fd.FacturaId == nc.FacturaId && fd.ProductoId == detalleNc.ProductoId);

                if (facturaDetalle != null)
                {
                    var newReturnedQuantity = facturaDetalle.CantidadDevuelta + detalleNc.Cantidad;
                    var detailToUpdate = new FacturaDetalle { Id = facturaDetalle.Id, CantidadDevuelta = newReturnedQuantity };
                    context.FacturaDetalles.Attach(detailToUpdate);
                    context.Entry(detailToUpdate).Property(x => x.CantidadDevuelta).IsModified = true;
                }
            }
            await context.SaveChangesAsync();
            _logger.LogInformation($"Cantidades devueltas actualizadas en la factura original para Nota de Crédito {nc.NumeroNotaCredito}");
        }

        // 6. PROCESO DE FONDO
       private async Task EnviarNcAlSriEnFondoAsync(Guid ncId, byte[] xmlFirmadoBytes)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<FacturasSRIDbContext>();
                var scopedSriClient = scope.ServiceProvider.GetRequiredService<SriApiClientService>();
                var scopedParser = scope.ServiceProvider.GetRequiredService<SriResponseParserService>();
                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<CreditNoteService>>();
                var scopedPdf = scope.ServiceProvider.GetRequiredService<PdfGeneratorService>();
                var scopedEmail = scope.ServiceProvider.GetRequiredService<IEmailService>();

                try
                {
                    // 1. RECEPCIÓN
                    string respuestaRecepcionXml = await scopedSriClient.EnviarRecepcionAsync(xmlFirmadoBytes);
                    var respuestaRecepcion = scopedParser.ParsearRespuestaRecepcion(respuestaRecepcionXml);

                    var nc = await scopedContext.NotasDeCredito
                        .Include(n => n.Cliente)
                        .Include(n => n.Factura)
                        .Include(n => n.InformacionSRI)
                        .Include(n => n.Detalles)
                            .ThenInclude(d => d.Producto)
                            .ThenInclude(p => p.ProductoImpuestos)
                            .ThenInclude(pi => pi.Impuesto)
                        .FirstOrDefaultAsync(n => n.Id == ncId);
                    
                    if (nc == null)
                    {
                        scopedLogger.LogError("[BG-NC] No se encontró la nota de crédito con ID {Id} en el proceso de fondo.", ncId);
                        return;
                    }
                    
                    var ncSri = nc.InformacionSRI; 
                    if (ncSri == null)
                    {
                        scopedLogger.LogError("[BG-NC] No se encontró la información SRI para la nota de crédito {Id}.", ncId);
                        return;
                    }

                    bool esClaveRepetida = respuestaRecepcion.Estado == "DEVUELTA" &&
                                           respuestaRecepcion.Errores.Any(e => e.Identificador == "43");

                    if (respuestaRecepcion.Estado == "DEVUELTA" && !esClaveRepetida)
                    {
                        nc.Estado = EstadoNotaDeCredito.RechazadaSRI;
                        ncSri.RespuestaSRI = JsonSerializer.Serialize(respuestaRecepcion.Errores);
                    }
                    else
                    {
                        if (esClaveRepetida)
                        {
                            scopedLogger.LogInformation("[BG-NC] Clave de acceso ya registrada para NC {Id}. Procediendo a consultar autorización.", ncId);
                        }
                        
                        // 2. AUTORIZACIÓN CON REINTENTOS
                        RespuestaAutorizacion? autObj = null;
                        int intentosMaximos = 4;
                        var delays = new[] { 2500, 5000, 10000, 15000 };

                        for (int i = 0; i < intentosMaximos; i++)
                        {
                            scopedLogger.LogInformation("[BG-NC] Consultando autorización... Intento {Intento} de {Maximos}", i + 1, intentosMaximos);
                            await Task.Delay(delays[i]);

                            try
                            {
                                string respAut = await scopedSriClient.ConsultarAutorizacionAsync(ncSri.ClaveAcceso);
                                autObj = scopedParser.ParsearRespuestaAutorizacion(respAut);

                                if (autObj.Estado != "PROCESANDO")
                                {
                                    break;
                                }
                            }
                            catch (Exception exAuth)
                            {
                                scopedLogger.LogWarning(exAuth, "[BG-NC] Error en el intento {Intento} de consultar autorización.", i + 1);
                                if (i == intentosMaximos - 1)
                                {
                                    nc.Estado = EstadoNotaDeCredito.EnviadaSRI;
                                    ncSri.RespuestaSRI = "Error final al consultar autorización: " + exAuth.Message;
                                    break;
                                }
                            }
                        }

                        if (autObj != null)
                        {
                            if (autObj.Estado == "AUTORIZADO")
                            {
                                if (nc.Estado != EstadoNotaDeCredito.Autorizada)
                                {
                                    await RestaurarStockAsync(scopedContext, nc);
                                    await ActualizarCantidadesDevueltasAsync(scopedContext, nc);
                                }

                                nc.Estado = EstadoNotaDeCredito.Autorizada;
                                ncSri.NumeroAutorizacion = autObj.NumeroAutorizacion;
                                ncSri.FechaAutorizacion = autObj.FechaAutorizacion;
                                ncSri.RespuestaSRI = "AUTORIZADO";
                                
                                await scopedContext.SaveChangesAsync();

                                try
                                {
                                    var ncDto = await BuildCreditNoteDetailViewDto(nc, ncSri);
                                    byte[] pdfBytes = scopedPdf.GenerarNotaCreditoPdf(ncDto);
                                    string xmlSigned = ncSri.XmlFirmado;
                                    
                                    await scopedEmail.SendCreditNoteEmailAsync(nc.Cliente.Email, nc.Cliente.RazonSocial, nc.NumeroNotaCredito, nc.Id, pdfBytes, xmlSigned);
                                    scopedLogger.LogInformation("[BG-NC] Correo de nota de crédito {Numero} enviado a {Email}.", nc.NumeroNotaCredito, nc.Cliente.Email);
                                }
                                catch(Exception exEmail) 
                                { 
                                    scopedLogger.LogError(exEmail, "[BG-NC] Error enviando correo de nota de crédito autorizada."); 
                                }
                            }
                            else if (autObj.Estado == "NO AUTORIZADO")
                            {
                                nc.Estado = EstadoNotaDeCredito.RechazadaSRI;
                                ncSri.RespuestaSRI = JsonSerializer.Serialize(autObj.Errores);
                                scopedLogger.LogWarning("[BG-NC] Nota de Crédito NO AUTORIZADA por SRI: {Errores}", ncSri.RespuestaSRI);
                            }
                            else // Sigue en PROCESANDO o estado desconocido
                            {
                                nc.Estado = EstadoNotaDeCredito.EnviadaSRI;
                                ncSri.RespuestaSRI = $"El SRI sigue procesando la NC tras {intentosMaximos} intentos. Consulta manual requerida.";
                                scopedLogger.LogInformation("[BG-NC] La NC {Id} sigue en procesamiento.", ncId);
                            }
                        }
                        else
                        {
                            nc.Estado = EstadoNotaDeCredito.EnviadaSRI;
                            ncSri.RespuestaSRI = "No se pudo obtener una respuesta definitiva del SRI sobre la autorización de la NC.";
                            scopedLogger.LogError("[BG-NC] No se pudo obtener respuesta de autorización para NC {Id} tras varios intentos.", ncId);
                        }
                    }
                    await scopedContext.SaveChangesAsync();
                }
                catch (Exception ex) { scopedLogger.LogError(ex, "[BG-NC] Error crítico en el proceso de fondo."); }
            }
        }
        
        private Task<CreditNoteDetailViewDto> BuildCreditNoteDetailViewDto(NotaDeCredito nc, NotaDeCreditoSRI ncSri)
        {
            var itemsDto = nc.Detalles.Select(d => new CreditNoteItemDetailDto
            {
                ProductName = d.Producto.Nombre,
                Cantidad = d.Cantidad,
                PrecioVentaUnitario = d.PrecioVentaUnitario,
                Subtotal = d.Subtotal
            }).ToList();

            var taxSummaries = nc.Detalles
                .SelectMany(d => d.Producto.ProductoImpuestos.Select(pi => new
                {
                    d.Subtotal,
                    TaxName = pi.Impuesto.Nombre,
                    TaxRate = pi.Impuesto.Porcentaje
                }))
                .GroupBy(x => new { x.TaxName, x.TaxRate })
                .Select(g => new TaxSummary { TaxName = g.Key.TaxName, TaxRate = g.Key.TaxRate, Amount = g.Sum(x => x.Subtotal * (x.TaxRate / 100m)) })
                .Where(x => x.Amount > 0 || x.TaxRate == 0).ToList();

            return Task.FromResult(new CreditNoteDetailViewDto
            {
                Id = nc.Id,
                NumeroNotaCredito = nc.NumeroNotaCredito,
                FechaEmision = nc.FechaEmision,
                ClienteNombre = nc.Cliente.RazonSocial,
                ClienteIdentificacion = nc.Cliente.NumeroIdentificacion,
                ClienteDireccion = nc.Cliente.Direccion,
                ClienteEmail = nc.Cliente.Email,
                NumeroFacturaModificada = nc.Factura.NumeroFactura,
                FechaEmisionFacturaModificada = nc.Factura.FechaEmision,
                RazonModificacion = nc.RazonModificacion,
                SubtotalSinImpuestos = nc.SubtotalSinImpuestos,
                TotalIVA = nc.TotalIVA,
                Total = nc.Total,
                ClaveAcceso = ncSri.ClaveAcceso,
                NumeroAutorizacion = ncSri.NumeroAutorizacion,
                Items = itemsDto,
                TaxSummaries = taxSummaries
            });
        }

        private string GenerarClaveAcceso(DateTime fechaEmision, string tipoComprobante, string ruc, string establecimiento, string puntoEmision, string secuencial, string tipoAmbiente)
        {
            var fecha = fechaEmision.ToString("ddMMyyyy");
            var tipoEmision = "1";
            var codigoNumerico = "12345678";
            var clave = new StringBuilder();
            clave.Append(fecha).Append(tipoComprobante).Append(ruc).Append(tipoAmbiente).Append(establecimiento).Append(puntoEmision).Append(secuencial).Append(codigoNumerico).Append(tipoEmision);
            if (clave.Length != 48) throw new InvalidOperationException($"Error long clave: {clave.Length}");
            clave.Append(CalcularDigitoVerificador(clave.ToString()));
            return clave.ToString();
        }

        private int CalcularDigitoVerificador(string clave)
        {
            var reverso = clave.Reverse().ToArray();
            var suma = 0;
            var factor = 2;
            for (int i = 0; i < reverso.Length; i++) { suma += (int)char.GetNumericValue(reverso[i]) * factor; factor++; if (factor > 7) factor = 2; }
            int modulo = suma % 11; int digito = 11 - modulo; if (digito == 11) return 0; if (digito == 10) return 1; return digito;
        }

        private DateTime GetEcuadorTime(DateTime utcTime)
        {
            try { var tz = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz); } catch { return utcTime.AddHours(-5); }
        }

        public async Task CancelCreditNoteAsync(Guid creditNoteId)
        {
            var nc = await _context.NotasDeCredito.FindAsync(creditNoteId);
            if (nc == null)
            {
                throw new InvalidOperationException("La nota de crédito no existe.");
            }
            if (nc.Estado != EstadoNotaDeCredito.Borrador)
            {
                throw new InvalidOperationException("Solo se pueden cancelar notas de crédito en estado Borrador.");
            }
            nc.Estado = EstadoNotaDeCredito.Cancelada;
            await _context.SaveChangesAsync();
        }

        public async Task ReactivateCancelledCreditNoteAsync(Guid creditNoteId)
        {
            var nc = await _context.NotasDeCredito.FindAsync(creditNoteId);
            if (nc == null)
            {
                throw new InvalidOperationException("La nota de crédito no existe.");
            }
            if (nc.Estado != EstadoNotaDeCredito.Cancelada)
            {
                throw new InvalidOperationException("Solo se pueden reactivar notas de crédito en estado Cancelada.");
            }
            nc.Estado = EstadoNotaDeCredito.Borrador;
            await _context.SaveChangesAsync();
        }

        public async Task<CreditNoteDetailViewDto?> IssueDraftCreditNoteAsync(Guid creditNoteId)
        {
            var nc = await _context.NotasDeCredito
                .Include(n => n.Cliente)
                .Include(n => n.Factura)
                .Include(n => n.InformacionSRI)
                .Include(n => n.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(n => n.Id == creditNoteId);

            if (nc == null) throw new InvalidOperationException("La nota de crédito no existe.");
            if (nc.Estado != EstadoNotaDeCredito.Borrador) throw new InvalidOperationException("Solo se pueden emitir notas de crédito en estado Borrador.");
            
            _logger.LogInformation("Iniciando emisión de Nota de Crédito borrador ID: {Id}", creditNoteId);

            var certPath = _configuration["CompanyInfo:CertificatePath"];
            var certPass = _configuration["CompanyInfo:CertificatePassword"];

            var (xmlGenerado, xmlFirmadoBytes) = _xmlGeneratorService.GenerarYFirmarNotaCredito(nc.InformacionSRI.ClaveAcceso, nc, nc.Cliente, nc.Factura, certPath, certPass);

            nc.InformacionSRI.XmlGenerado = xmlGenerado;
            nc.InformacionSRI.XmlFirmado = Encoding.UTF8.GetString(xmlFirmadoBytes);
            nc.Estado = EstadoNotaDeCredito.EnviadaSRI;

            await _context.SaveChangesAsync();

            _ = Task.Run(() => EnviarNcAlSriEnFondoAsync(nc.Id, xmlFirmadoBytes));
            
            return await GetCreditNoteDetailByIdAsync(nc.Id);
        }

        public async Task<CreditNoteDto?> UpdateCreditNoteAsync(UpdateCreditNoteDto dto)
        {
            var nc = await _context.NotasDeCredito
                .Include(n => n.Detalles)
                .Include(n => n.Factura).ThenInclude(f => f.Detalles)
                .FirstOrDefaultAsync(n => n.Id == dto.Id);

            if (nc == null) throw new InvalidOperationException("La nota de crédito no existe.");
            if (nc.Estado != EstadoNotaDeCredito.Borrador) throw new InvalidOperationException("Solo se pueden modificar borradores.");

            nc.RazonModificacion = dto.RazonModificacion;

            var oldDetails = nc.Detalles.ToList();
            var newDetails = new List<NotaDeCreditoDetalle>();

            decimal subtotalAccum = 0;
            decimal ivaAccum = 0;

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.CantidadDevolucion <= 0) continue;

                var detalleFactura = nc.Factura.Detalles.FirstOrDefault(d => d.ProductoId == itemDto.ProductoId);
                if (detalleFactura == null) throw new Exception($"Producto ID {itemDto.ProductoId} inválido.");
                
                if (itemDto.CantidadDevolucion > (detalleFactura.Cantidad - detalleFactura.CantidadDevuelta)) throw new Exception($"La cantidad a devolver para el producto excede la cantidad disponible en la factura original.");

                decimal precioUnit = detalleFactura.PrecioVentaUnitario;
                decimal subtotalItem = itemDto.CantidadDevolucion * precioUnit;
                decimal valorIvaItem = 0;
                
                var producto = await _context.Productos.Include(p => p.ProductoImpuestos).ThenInclude(pi => pi.Impuesto).FirstAsync(p => p.Id == itemDto.ProductoId);
                var impuestoIva = producto.ProductoImpuestos.FirstOrDefault(pi => pi.Impuesto.Porcentaje > 0);
                if (impuestoIva != null) valorIvaItem = subtotalItem * (impuestoIva.Impuesto.Porcentaje / 100);

                var detalleNc = new NotaDeCreditoDetalle
                {
                    NotaDeCreditoId = nc.Id,
                    ProductoId = itemDto.ProductoId,
                    Cantidad = itemDto.CantidadDevolucion,
                    PrecioVentaUnitario = precioUnit,
                    Subtotal = subtotalItem,
                    ValorIVA = valorIvaItem
                };
                newDetails.Add(detalleNc);

                subtotalAccum += subtotalItem;
                ivaAccum += valorIvaItem;
            }

            _context.NotaDeCreditoDetalles.RemoveRange(oldDetails);
            nc.Detalles = newDetails;

            nc.SubtotalSinImpuestos = subtotalAccum;
            nc.TotalIVA = ivaAccum;
            nc.Total = subtotalAccum + ivaAccum;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Borrador de Nota de Crédito {Id} actualizado.", nc.Id);

            if (dto.EmitirTrasGuardar)
            {
                _logger.LogInformation("Flag 'EmitirTrasGuardar' detectado para NC. Emisión inmediata...");
                await IssueDraftCreditNoteAsync(nc.Id);
            }

            var resultDto = await GetCreditNoteDetailByIdAsync(nc.Id);
            return new CreditNoteDto { Id = resultDto.Id, NumeroNotaCredito = resultDto.NumeroNotaCredito };
        }
    }
}