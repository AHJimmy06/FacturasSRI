using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using FacturasSRI.Domain.Enums; 
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

// === ALIAS PARA DOMINIO ===
using FacturaDominio = FacturasSRI.Domain.Entities.Factura;
using NotaCreditoDominio = FacturasSRI.Domain.Entities.NotaDeCredito;
using ClienteDominio = FacturasSRI.Domain.Entities.Cliente;

// === ALIAS PARA XML FACTURA ===
using FacturaXml = FacturasSRI.Core.XmlModels.Factura.Factura;
using InfoTributariaXml = FacturasSRI.Core.XmlModels.Factura.InfoTributaria;
using InfoFacturaXml = FacturasSRI.Core.XmlModels.Factura.FacturaInfoFactura;
using DetalleXml = FacturasSRI.Core.XmlModels.Factura.FacturaDetallesDetalle;
using TotalImpuestoXml = FacturasSRI.Core.XmlModels.Factura.FacturaInfoFacturaTotalConImpuestosTotalImpuesto;
using ImpuestoDetalleXml = FacturasSRI.Core.XmlModels.Factura.Impuesto;
using PagosPago = FacturasSRI.Core.XmlModels.Factura.PagosPago;

// === ALIAS PARA XML NOTA DE CRÉDITO ===
using NotaCreditoXml = FacturasSRI.Core.XmlModels.NotaCredito.NotaCredito;
using ObligadoContabilidadFactura = FacturasSRI.Core.XmlModels.Factura.ObligadoContabilidad;
using ObligadoContabilidadNC = FacturasSRI.Core.XmlModels.NotaCredito.ObligadoContabilidad;
using InfoTributariaNCXml = FacturasSRI.Core.XmlModels.NotaCredito.InfoTributaria; 
using InfoNotaCreditoXml = FacturasSRI.Core.XmlModels.NotaCredito.NotaCreditoInfoNotaCredito;
using DetalleNCXml = FacturasSRI.Core.XmlModels.NotaCredito.NotaCreditoDetallesDetalle;
using TotalImpuestoNCXml = FacturasSRI.Core.XmlModels.NotaCredito.TotalConImpuestosTotalImpuesto;
using ImpuestoDetalleNCXml = FacturasSRI.Core.XmlModels.NotaCredito.Impuesto;


namespace FacturasSRI.Core.Services
{
    public class XmlGeneratorService
    {
        private readonly FirmaDigitalService _firmaService;
        private readonly ILogger<XmlGeneratorService> _logger;

        private const string RUC_EMISOR = "1850641927001";
        private const string RAZON_SOCIAL_EMISOR = "AÑILEMA HOFFMANN JIMMY ALEXANDER";
        private const string NOMBRE_COMERCIAL_EMISOR = "AETHER TECH";
        private const string DIRECCION_MATRIZ_EMISOR = "AV. BENJAMIN FRANKLIN SNN Y EDWARD JENNER";
        private const string COD_ESTABLECIMIENTO = "001";
        private const string COD_PUNTO_EMISION = "001";
        private const ObligadoContabilidadFactura OBLIGADO_CONTABILIDAD = ObligadoContabilidadFactura.No;
        private const string TIPO_AMBIENTE = "1"; 
        
        private readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        public XmlGeneratorService(FirmaDigitalService firmaService, ILogger<XmlGeneratorService> logger)
        {
            _firmaService = firmaService;
            _logger = logger;
        }

        // --- LÓGICA FACTURA (SE QUEDA CON STRING PORQUE ASÍ LO PIDE TU CLASE GENERADA DE FACTURA) ---
        public (string XmlGenerado, byte[] XmlFirmadoBytes) GenerarYFirmarFactura(
            string claveAcceso,
            FacturaDominio facturaDominio, 
            ClienteDominio clienteDominio,
            string rutaCertificado,
            string passwordCertificado
            )
        {
            FacturaXml facturaXml = GenerarXmlFactura(claveAcceso, facturaDominio, clienteDominio);
            string xmlSinFirmar = SerializarObjeto(facturaXml);
            _logger.LogWarning("--- INICIO XML FACTURA SIN FIRMAR ---\n{Xml}\n--- FIN XML ---", xmlSinFirmar);
            byte[] xmlFirmadoBytes = _firmaService.FirmarXml(xmlSinFirmar, rutaCertificado, passwordCertificado);
            return (xmlSinFirmar, xmlFirmadoBytes);
        }

        private FacturaXml GenerarXmlFactura( 
            string claveAcceso,
            FacturaDominio facturaDominio, 
            ClienteDominio clienteDominio
            )
        {
            string secuencialFormateado = facturaDominio.NumeroFactura.PadLeft(9, '0');

            var facturaXml = new FacturaXml 
            {
                Id = FacturasSRI.Core.XmlModels.Factura.FacturaId.Comprobante,
                IdSpecified = true,
                Version = "1.1.0", 
            };
            
            facturaXml.InfoTributaria = new InfoTributariaXml 
            {
                Ambiente = TIPO_AMBIENTE,
                TipoEmision = "1", 
                RazonSocial = NormalizeString(RAZON_SOCIAL_EMISOR),
                NombreComercial = NormalizeString(NOMBRE_COMERCIAL_EMISOR),
                Ruc = RUC_EMISOR,
                ClaveAcceso = claveAcceso,
                CodDoc = "01", 
                Estab = COD_ESTABLECIMIENTO,
                PtoEmi = COD_PUNTO_EMISION,
                Secuencial = secuencialFormateado,
                DirMatriz = NormalizeString(DIRECCION_MATRIZ_EMISOR)
            };

            var fechaEmisionEcuador = GetEcuadorTime(facturaDominio.FechaEmision);

            facturaXml.InfoFactura = new InfoFacturaXml 
            {
                FechaEmision = fechaEmisionEcuador.ToString("dd/MM/yyyy"),
                DirEstablecimiento = NormalizeString(DIRECCION_MATRIZ_EMISOR), 
                ObligadoContabilidad = OBLIGADO_CONTABILIDAD,
                ObligadoContabilidadSpecified = true,
                
                TipoIdentificacionComprador = MapearTipoIdentificacion(clienteDominio.TipoIdentificacion, clienteDominio.NumeroIdentificacion),
                RazonSocialComprador = NormalizeString(clienteDominio.RazonSocial),
                IdentificacionComprador = clienteDominio.NumeroIdentificacion,
                
                TotalSinImpuestos = facturaDominio.SubtotalSinImpuestos.ToString("F2", _cultureInfo),
                TotalDescuento = facturaDominio.TotalDescuento.ToString("F2", _cultureInfo),
                Propina = "0.00",
                PropinaSpecified = true,
                ImporteTotal = facturaDominio.Total.ToString("F2", _cultureInfo)
            };
            
            facturaXml.InfoFactura.Pagos.Add(new PagosPago {
                FormaPago = "01",
                Total = facturaDominio.Total.ToString("F2", _cultureInfo)
            });
            
            var gruposImpuestos = facturaDominio.Detalles
                .SelectMany(d => d.Producto.ProductoImpuestos.Select(pi => new { Detalle = d, Impuesto = pi.Impuesto }))
                .GroupBy(x => new { Codigo = "2", CodigoPorcentaje = x.Impuesto.CodigoSRI })
                .Select(g => new TotalImpuestoXml 
                {
                    Codigo = g.Key.Codigo, 
                    CodigoPorcentaje = g.Key.CodigoPorcentaje, 
                    BaseImponible = g.Sum(x => x.Detalle.Subtotal).ToString("F2", _cultureInfo),
                    Valor = g.Sum(x => x.Detalle.ValorIVA).ToString("F2", _cultureInfo)
                });

            foreach (var grupo in gruposImpuestos)
            {
                facturaXml.InfoFactura.TotalConImpuestos.Add(grupo);
            }

            foreach (var detalle in facturaDominio.Detalles)
            {
                var detalleXml = new DetalleXml
                {
                    CodigoPrincipal = detalle.Producto.CodigoPrincipal,
                    Descripcion = NormalizeString(detalle.Producto.Nombre),
                    Cantidad = detalle.Cantidad.ToString("F2", _cultureInfo),
                    PrecioUnitario = detalle.PrecioVentaUnitario.ToString("F2", _cultureInfo),
                    Descuento = detalle.Descuento.ToString("F2", _cultureInfo),
                    PrecioTotalSinImpuesto = detalle.Subtotal.ToString("F2", _cultureInfo)
                };

                var impuestosDetalle = detalle.Producto.ProductoImpuestos
                    .Select(pi => new ImpuestoDetalleXml 
                    {
                        Codigo = "2", 
                        CodigoPorcentaje = pi.Impuesto.CodigoSRI, 
                        Tarifa = pi.Impuesto.Porcentaje.ToString("F2", _cultureInfo),
                        BaseImponible = detalle.Subtotal.ToString("F2", _cultureInfo),
                        Valor = (detalle.Subtotal * (pi.Impuesto.Porcentaje / 100)).ToString("F2", _cultureInfo)
                    });

                foreach (var impuesto in impuestosDetalle)
                {
                    detalleXml.Impuestos.Add(impuesto);
                }
                
                facturaXml.Detalles.Add(detalleXml);
            }

            return facturaXml;
        }

        // ===========================================================
        //  NOTA DE CRÉDITO (CORREGIDA PARA USAR DECIMALES)
        // ===========================================================

        public (string XmlGenerado, byte[] XmlFirmadoBytes) GenerarYFirmarNotaCredito(
            string claveAcceso,
            NotaCreditoDominio ncDominio,
            ClienteDominio clienteDominio,
            FacturaDominio facturaOriginal,
            string rutaCertificado,
            string passwordCertificado
            )
        {
            NotaCreditoXml ncXml = GenerarXmlNotaCredito(claveAcceso, ncDominio, clienteDominio, facturaOriginal);

            string xmlSinFirmar = SerializarObjeto(ncXml);
            _logger.LogWarning("--- INICIO XML NOTA CREDITO SIN FIRMAR ---\n{Xml}\n--- FIN XML ---", xmlSinFirmar);

            byte[] xmlFirmadoBytes = _firmaService.FirmarXml(xmlSinFirmar, rutaCertificado, passwordCertificado);

            return (xmlSinFirmar, xmlFirmadoBytes);
        }

        private NotaCreditoXml GenerarXmlNotaCredito(
            string claveAcceso,
            NotaCreditoDominio ncDominio,
            ClienteDominio clienteDominio,
            FacturaDominio facturaOriginal
            )
        {
            string secuencialFormateado = ncDominio.NumeroNotaCredito.PadLeft(9, '0');
            var fechaEmisionEcuador = GetEcuadorTime(ncDominio.FechaEmision);
            var fechaSustentoEcuador = GetEcuadorTime(facturaOriginal.FechaEmision);

            var ncXml = new NotaCreditoXml
            {
                Id = FacturasSRI.Core.XmlModels.NotaCredito.NotaCreditoId.Comprobante,
                Version = "1.1.0",
                InfoTributaria = new InfoTributariaNCXml
                {
                    Ambiente = TIPO_AMBIENTE,
                    TipoEmision = "1",
                    RazonSocial = NormalizeString(RAZON_SOCIAL_EMISOR),
                    NombreComercial = NormalizeString(NOMBRE_COMERCIAL_EMISOR),
                    Ruc = RUC_EMISOR,
                    ClaveAcceso = claveAcceso,
                    CodDoc = "04", 
                    Estab = COD_ESTABLECIMIENTO,
                    PtoEmi = COD_PUNTO_EMISION,
                    Secuencial = secuencialFormateado,
                    DirMatriz = NormalizeString(DIRECCION_MATRIZ_EMISOR)
                }
            };

            // FORMATO CORRECTO PARA EL SRI (ESTO ES STRING, ESTÁ BIEN)
            string numDocModificadoFormateado = $"{COD_ESTABLECIMIENTO}-{COD_PUNTO_EMISION}-{facturaOriginal.NumeroFactura.PadLeft(9, '0')}";

            // CORRECCIÓN: ASIGNACIÓN DIRECTA DE DECIMALES (Sin .ToString())
            ncXml.InfoNotaCredito = new InfoNotaCreditoXml
            {
                FechaEmision = fechaEmisionEcuador.ToString("dd/MM/yyyy"),
                DirEstablecimiento = NormalizeString(DIRECCION_MATRIZ_EMISOR),
                TipoIdentificacionComprador = MapearTipoIdentificacion(clienteDominio.TipoIdentificacion, clienteDominio.NumeroIdentificacion),
                RazonSocialComprador = NormalizeString(clienteDominio.RazonSocial),
                IdentificacionComprador = clienteDominio.NumeroIdentificacion,
                ObligadoContabilidad = ObligadoContabilidadNC.No,
                ObligadoContabilidadSpecified = true,

                CodDocModificado = "01", 
                NumDocModificado = numDocModificadoFormateado, 
                
                FechaEmisionDocSustento = fechaSustentoEcuador.ToString("dd/MM/yyyy"),
                
                // AQUI ESTABAN LOS ERRORES CS0029:
                TotalSinImpuestos = ncDominio.SubtotalSinImpuestos, // Decimal directo
                ValorModificacion = ncDominio.Total,               // Decimal directo
                Moneda = "DOLAR",
                Motivo = NormalizeString(ncDominio.RazonModificacion)
            };

            var gruposImpuestos = ncDominio.Detalles
               .SelectMany(d => d.Producto.ProductoImpuestos.Select(pi => new { Detalle = d, Impuesto = pi.Impuesto }))
               .GroupBy(x => new { Codigo = "2", CodigoPorcentaje = x.Impuesto.CodigoSRI })
               .Select(g => new TotalImpuestoNCXml
               {
                   Codigo = g.Key.Codigo,
                   CodigoPorcentaje = g.Key.CodigoPorcentaje,
                   // CORRECCIÓN DE TIPOS:
                   BaseImponible = g.Sum(x => x.Detalle.Subtotal),
                   Valor = g.Sum(x => x.Detalle.ValorIVA)
               });

            foreach (var grupo in gruposImpuestos)
            {
                ncXml.InfoNotaCredito.TotalConImpuestos.Add(grupo);
            }

            foreach (var detalle in ncDominio.Detalles)
            {
                var detalleXml = new DetalleNCXml
                {
                    CodigoInterno = detalle.Producto.CodigoPrincipal,
                    Descripcion = NormalizeString(detalle.Producto.Nombre),
                    // CORRECCIÓN DE TIPOS:
                    Cantidad = detalle.Cantidad, 
                    PrecioUnitario = detalle.PrecioVentaUnitario,
                    Descuento = detalle.DescuentoAplicado,
                    PrecioTotalSinImpuesto = detalle.Subtotal
                };

                var impuestosDetalle = detalle.Producto.ProductoImpuestos
                    .Select(pi => new ImpuestoDetalleNCXml
                    {
                        Codigo = "2",
                        CodigoPorcentaje = pi.Impuesto.CodigoSRI,
                        // CORRECCIÓN DE TIPOS:
                        Tarifa = pi.Impuesto.Porcentaje,
                        BaseImponible = detalle.Subtotal,
                        Valor = (detalle.Subtotal * (pi.Impuesto.Porcentaje / 100))
                    });

                foreach (var impuesto in impuestosDetalle)
                {
                    detalleXml.Impuestos.Add(impuesto);
                }

                ncXml.Detalles.Add(detalleXml);
            }

            return ncXml;
        }
        
        // ... (Resto de métodos privados iguales: NormalizeString, SerializarObjeto, etc.) ...
        private string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        private string SerializarObjeto(object objeto)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                using (var stringWriter = new Utf8StringWriter())
                {
                    var settings = new XmlWriterSettings { Indent = false, Encoding = new UTF8Encoding(false), OmitXmlDeclaration = true };
                    using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                    {
                        var serializer = new XmlSerializer(objeto.GetType());
                        var namespaces = new XmlSerializerNamespaces();
                        namespaces.Add(string.Empty, string.Empty);
                        serializer.Serialize(xmlWriter, objeto, namespaces);
                    }
                    return stringWriter.ToString();
                }
            }
            finally { System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture; }
        }

        private string MapearTipoIdentificacion(TipoIdentificacion tipo, string numeroIdentificacion)
        {
            if (numeroIdentificacion == "9999999999999") return "07"; 
            switch (tipo) {
                case TipoIdentificacion.Cedula: return "05"; 
                case TipoIdentificacion.RUC: return "04"; 
                case TipoIdentificacion.Pasaporte: return "06"; 
                default: return "07";
            }
        }

        private DateTime GetEcuadorTime(DateTime utcTime)
        {
            try { var tz = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz); }
            catch { try { var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil"); return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz); } catch { return utcTime.AddHours(-5); } }
        }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}