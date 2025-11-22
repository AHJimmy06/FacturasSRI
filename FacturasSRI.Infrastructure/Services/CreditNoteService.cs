using FacturasSRI.Application.Dtos;
using FacturasSRI.Core.Models;
using FacturasSRI.Core.Services;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Domain.Enums;
using FacturasSRI.Infrastructure.Persistence;
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
    public class CreditNoteService
    {
        private readonly FacturasSRIDbContext _context;
        private readonly ILogger<CreditNoteService> _logger;
        private readonly IConfiguration _configuration;
        private readonly XmlGeneratorService _xmlGeneratorService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        // Semáforo para evitar colisiones de secuenciales
        private static readonly SemaphoreSlim _ncSemaphore = new SemaphoreSlim(1, 1);

        public CreditNoteService(
            FacturasSRIDbContext context,
            ILogger<CreditNoteService> logger,
            IConfiguration configuration,
            XmlGeneratorService xmlGeneratorService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _xmlGeneratorService = xmlGeneratorService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<NotaDeCredito> CreateCreditNoteAsync(CreateCreditNoteDto dto)
        {
            await _ncSemaphore.WaitAsync();
            try
            {
                // 1. Validaciones Básicas
                var factura = await _context.Facturas
                    .Include(f => f.Cliente)
                    .Include(f => f.Detalles).ThenInclude(d => d.Producto).ThenInclude(p => p.ProductoImpuestos).ThenInclude(pi => pi.Impuesto)
                    .FirstOrDefaultAsync(f => f.Id == dto.FacturaId);

                if (factura == null) throw new Exception("Factura no encontrada.");
                if (factura.Estado != EstadoFactura.Autorizada) throw new Exception("Solo se pueden emitir Notas de Crédito a Facturas AUTORIZADAS.");

                // 2. Configuración SRI
                var establishmentCode = _configuration["CompanyInfo:EstablishmentCode"];
                var emissionPointCode = _configuration["CompanyInfo:EmissionPointCode"];
                var rucEmisor = _configuration["CompanyInfo:Ruc"];
                var environmentType = _configuration["CompanyInfo:EnvironmentType"];

                // 3. Obtener Secuencial
                var secuencialEntity = await _context.Secuenciales
                    .FirstOrDefaultAsync(s => s.Establecimiento == establishmentCode && s.PuntoEmision == emissionPointCode);
                
                // NOTA: Deberías tener una propiedad 'UltimoSecuencialNotaCredito' en tu entidad Secuencial.
                // Si no la tienes, usa una lógica temporal o agrega la columna a la BD. 
                // Asumiremos que usas el mismo objeto Secuencial pero necesitas una columna nueva. 
                // *SI NO TIENES LA COLUMNA*: Puedes usar una tabla separada o un contador en codigo (no recomendado).
                // Por ahora simulo que usas una propiedad genérica o que agregaste la columna.
                // Asumiré que agregas la columna 'UltimoSecuencialNotaCredito' a tu Entidad Secuencial.
                
                // *** IMPORTANTE: SI ESTO DA ERROR, AGREGA LA COLUMNA EN TU ENTIDAD Y MIGRACIÓN ***
                // secuencialEntity.UltimoSecuencialNotaCredito++; 
                // int nextNum = secuencialEntity.UltimoSecuencialNotaCredito;
                
                // COMO SOLUCIÓN RÁPIDA SI NO QUIERES MIGRAR YA: Consultar la última NC creada
                var lastNc = await _context.NotasDeCredito
                    .OrderByDescending(n => n.NumeroNotaCredito)
                    .FirstOrDefaultAsync();
                
                int nextSequence = 1;
                if(lastNc != null && int.TryParse(lastNc.NumeroNotaCredito, out int lastNum))
                    nextSequence = lastNum + 1;

                string numeroSecuencialStr = nextSequence.ToString("D9");
                // Fin lógica secuencial temporal

                // 4. Crear Entidad Nota Crédito
                var nc = new NotaDeCredito
                {
                    Id = Guid.NewGuid(),
                    FacturaId = factura.Id,
                    ClienteId = factura.ClienteId.Value,
                    FechaEmision = DateTime.UtcNow,
                    NumeroNotaCredito = numeroSecuencialStr,
                    Estado = EstadoNotaDeCredito.Pendiente, // Se crea directo como pendiente para enviar
                    RazonModificacion = dto.RazonModificacion,
                    UsuarioIdCreador = dto.UsuarioIdCreador,
                    FechaCreacion = DateTime.UtcNow
                };

                decimal subtotalAccum = 0;
                decimal ivaAccum = 0;

                // 5. Procesar Detalles y DEVOLVER STOCK
                foreach (var itemDto in dto.Items)
                {
                    if (itemDto.CantidadDevolucion <= 0) continue;

                    var detalleFactura = factura.Detalles.FirstOrDefault(d => d.ProductoId == itemDto.ProductoId);
                    if (detalleFactura == null) throw new Exception($"Producto {itemDto.ProductoId} no existe en la factura original.");

                    if (itemDto.CantidadDevolucion > detalleFactura.Cantidad)
                        throw new Exception($"No puedes devolver más de lo comprado para el producto {detalleFactura.Producto.Nombre}.");

                    // Calcular valores proporcionales
                    decimal precioUnit = detalleFactura.PrecioVentaUnitario;
                    decimal subtotalItem = itemDto.CantidadDevolucion * precioUnit;
                    
                    // Calcular IVA
                    var impuestoIva = detalleFactura.Producto.ProductoImpuestos.FirstOrDefault(pi => pi.Impuesto.Porcentaje > 0);
                    decimal valorIvaItem = 0;
                    if (impuestoIva != null)
                    {
                        valorIvaItem = subtotalItem * (impuestoIva.Impuesto.Porcentaje / 100);
                    }

                    // Crear Detalle NC
                    var detalleNc = new NotaDeCreditoDetalle
                    {
                        Id = Guid.NewGuid(),
                        NotaDeCreditoId = nc.Id,
                        ProductoId = itemDto.ProductoId,
                        Producto = detalleFactura.Producto, // EF Navigation
                        Cantidad = itemDto.CantidadDevolucion,
                        PrecioVentaUnitario = precioUnit,
                        DescuentoAplicado = 0, // Simplificado por ahora
                        Subtotal = subtotalItem,
                        ValorIVA = valorIvaItem
                    };
                    nc.Detalles.Add(detalleNc);

                    subtotalAccum += subtotalItem;
                    ivaAccum += valorIvaItem;

                    // LOGICA CRÍTICA: DEVOLUCIÓN DE STOCK
                    if (detalleFactura.Producto.ManejaInventario)
                    {
                        if (detalleFactura.Producto.ManejaLotes)
                        {
                            await DevolverStockALotes(detalleFactura.Id, itemDto.CantidadDevolucion);
                        }
                        else
                        {
                            detalleFactura.Producto.StockTotal += itemDto.CantidadDevolucion;
                        }
                    }
                }

                nc.SubtotalSinImpuestos = subtotalAccum;
                nc.TotalIVA = ivaAccum;
                nc.Total = subtotalAccum + ivaAccum;

                _context.NotasDeCredito.Add(nc);

                // 6. Generar Clave Acceso
                var fechaEcuador = GetEcuadorTime(nc.FechaEmision);
                // Tipo Comprobante "04" para Nota Crédito
                string claveAcceso = GenerarClaveAcceso(fechaEcuador, "04", rucEmisor, establishmentCode, emissionPointCode, numeroSecuencialStr, environmentType);

                var ncSri = new NotaDeCreditoSRI
                {
                    NotaDeCreditoId = nc.Id,
                    ClaveAcceso = claveAcceso
                };
                _context.NotasDeCreditoSRI.Add(ncSri);

                // 7. Generar XML y Firmar
                var certPath = _configuration["CompanyInfo:CertificatePath"];
                var certPass = _configuration["CompanyInfo:CertificatePassword"];

                var (xmlGenerado, xmlFirmadoBytes) = _xmlGeneratorService.GenerarYFirmarNotaCredito(
                    claveAcceso, nc, factura.Cliente, factura, certPath, certPass);

                ncSri.XmlGenerado = xmlGenerado;
                ncSri.XmlFirmado = Encoding.UTF8.GetString(xmlFirmadoBytes);

                // Actualizar estado a Enviada (optimista) o lista para enviar
                nc.Estado = EstadoNotaDeCredito.EnviadaSRI; 

                await _context.SaveChangesAsync();

                // 8. Background Task para Enviar al SRI (Fire and Forget)
                _ = Task.Run(() => EnviarNcAlSriEnFondoAsync(nc.Id, xmlFirmadoBytes));

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

        private async Task DevolverStockALotes(Guid detalleFacturaId, int cantidadADevolver)
        {
            // Buscamos qué lotes se consumieron para esa línea de factura específica
            var consumos = await _context.FacturaDetalleConsumoLotes
                .Where(c => c.FacturaDetalleId == detalleFacturaId)
                .Include(c => c.Lote)
                .OrderByDescending(c => c.Lote.FechaCaducidad) // Preferencia: Devolver al que vence más tarde (opcional)
                .ToListAsync();

            int remanente = cantidadADevolver;

            // Primero intentamos devolver a los mismos lotes de donde salieron
            foreach (var consumo in consumos)
            {
                if (remanente <= 0) break;

                // No hay límite estricto de cuánto devolver a un lote, 
                // pero lo lógico es no devolver más de lo que salió de ese lote específico si quisiéramos ser estrictos.
                // Para simplificar en 4to semestre: Simplemente sumamos al lote.
                
                consumo.Lote.CantidadDisponible += remanente; // Aquí sumamos todo al primer lote encontrado o distribuimos
                // Si quisiéramos distribuir exactamente:
                // int devolverAqui = Math.Min(remanente, consumo.CantidadConsumida); 
                // consumo.Lote.CantidadDisponible += devolverAqui;
                // remanente -= devolverAqui;

                // Estrategia Simple: Devolvemos todo al lote más reciente usado
                remanente = 0; 
            }

            // Si por alguna razón no hubo consumo registrado (error de datos), sumamos al stock general o lanzamos alerta
            if (remanente > 0)
            {
                 _logger.LogWarning($"No se encontró lote para devolver {remanente} items del detalle {detalleFacturaId}. Stock podría descuadrar a nivel de lote.");
            }
        }

        private async Task EnviarNcAlSriEnFondoAsync(Guid ncId, byte[] xmlFirmadoBytes)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var scopedContext = scope.ServiceProvider.GetRequiredService<FacturasSRIDbContext>();
                var scopedSriClient = scope.ServiceProvider.GetRequiredService<SriApiClientService>();
                var scopedParser = scope.ServiceProvider.GetRequiredService<SriResponseParserService>();
                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<CreditNoteService>>();

                try
                {
                    scopedLogger.LogInformation($"[BG-NC] Enviando Nota Credito {ncId} al SRI...");
                    
                    string respuestaRecepcionXml = await scopedSriClient.EnviarRecepcionAsync(xmlFirmadoBytes);
                    var respuestaRecepcion = scopedParser.ParsearRespuestaRecepcion(respuestaRecepcionXml);

                    var nc = await scopedContext.NotasDeCredito.FindAsync(ncId);
                    var ncSri = await scopedContext.NotasDeCreditoSRI.FirstOrDefaultAsync(x => x.NotaDeCreditoId == ncId);

                    if (respuestaRecepcion.Estado == "DEVUELTA")
                    {
                        nc.Estado = EstadoNotaDeCredito.RechazadaSRI;
                        ncSri.RespuestaSRI = JsonSerializer.Serialize(respuestaRecepcion.Errores);
                        scopedLogger.LogWarning("[BG-NC] NC Rechazada.");
                    }
                    else
                    {
                        // Si fue recibida, intentamos autorizar inmediatamente
                         try
                        {
                            await Task.Delay(1500); // Espera prudencial
                            string respAut = await scopedSriClient.ConsultarAutorizacionAsync(ncSri.ClaveAcceso);
                            var autObj = scopedParser.ParsearRespuestaAutorizacion(respAut);

                            if(autObj.Estado == "AUTORIZADO")
                            {
                                nc.Estado = EstadoNotaDeCredito.Autorizada;
                                ncSri.NumeroAutorizacion = autObj.NumeroAutorizacion;
                                ncSri.FechaAutorizacion = autObj.FechaAutorizacion;
                                ncSri.RespuestaSRI = "AUTORIZADO";
                            }
                            else
                            {
                                // Queda en EnviadaSRI, un job posterior o consulta manual revisará
                                ncSri.RespuestaSRI = JsonSerializer.Serialize(autObj.Errores);
                            }
                        }
                        catch
                        {
                            // Si falla autorización, queda en RECIBIDA
                            nc.Estado = EstadoNotaDeCredito.EnviadaSRI;
                        }
                    }
                    
                    await scopedContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    scopedLogger.LogError(ex, "[BG-NC] Error enviando NC al SRI");
                }
            }
        }

        // --- LÓGICA DUPLICADA DE KEY GENERATION (Mover a SharedHelper en el futuro) ---
        private string GenerarClaveAcceso(DateTime fechaEmision, string tipoComprobante, string ruc, string establecimiento, string puntoEmision, string secuencial, string tipoAmbiente)
        {
            var fecha = fechaEmision.ToString("ddMMyyyy");
            var tipoEmision = "1"; // Normal
            var codigoNumerico = "12345678"; // Estático para pruebas o random
            var clave = new StringBuilder();
            clave.Append(fecha);
            clave.Append(tipoComprobante);
            clave.Append(ruc);
            clave.Append(tipoAmbiente);
            clave.Append(establecimiento);
            clave.Append(puntoEmision);
            clave.Append(secuencial);
            clave.Append(codigoNumerico);
            clave.Append(tipoEmision);

            if (clave.Length != 48) throw new InvalidOperationException($"Error long clave: {clave.Length}");

            clave.Append(CalcularDigitoVerificador(clave.ToString()));
            return clave.ToString();
        }

        private int CalcularDigitoVerificador(string clave)
        {
            var reverso = clave.Reverse().ToArray();
            var suma = 0;
            var factor = 2;
            for (int i = 0; i < reverso.Length; i++)
            {
                suma += (int)char.GetNumericValue(reverso[i]) * factor;
                factor++;
                if (factor > 7) factor = 2;
            }
            int modulo = suma % 11;
            int digito = 11 - modulo;
            if (digito == 11) return 0;
            if (digito == 10) return 1;
            return digito;
        }

        private DateTime GetEcuadorTime(DateTime utcTime)
        {
             try { var tz = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz); }
             catch { return utcTime.AddHours(-5); }
        }
    }
}