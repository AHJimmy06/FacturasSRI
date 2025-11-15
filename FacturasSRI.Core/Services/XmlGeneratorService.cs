// En: FacturasSRI.Core/Services/XmlGeneratorService.cs

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using FacturasSRI.Core.XmlModels.Factura;
using FacturasSRI.Domain.Entities;
using FacturasSRI.Domain.Enums; // <-- AÑADIDO
using System.Globalization;
using System.Collections.Generic;

namespace FacturasSRI.Core.Services
{
    public class XmlGeneratorService
    {
        private readonly FirmaDigitalService _firmaService;

        // === DATOS FICTICIOS DE TU EMPRESA (EMISOR) ===
        private const string RUC_EMISOR = "1799999999001";
        private const string RAZON_SOCIAL_EMISOR = "Aether Tecnologías";
        private const string NOMBRE_COMERCIAL_EMISOR = "Aether Tech";
        private const string DIRECCION_MATRIZ_EMISOR = "Av. de los Shyris N37-271 y Holanda, Edificio Shyris Center, Quito, Ecuador";
        private const string COD_ESTABLECIMIENTO = "001";
        private const string COD_PUNTO_EMISION = "001";
        private const string OBLIGADO_CONTABILIDAD = "SI";
        private const string TIPO_AMBIENTE = "1"; // 1 = Pruebas, 2 = Producción
        
        // Formato de cultura "invariante" para que "100.00" no se escriba como "100,00"
        private readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;

        // Asumimos que este servicio será inyectado
        public XmlGeneratorService(FirmaDigitalService firmaService)
        {
            _firmaService = firmaService;
        }

        /// <summary>
        /// Orquestador principal. Genera, firma y devuelve el XML listo.
        /// </summary>
        public string GenerarYFirmarFactura(
            string claveAcceso,
            Factura facturaDominio,
            Cliente clienteDominio
            )
        {
            // 1. Generar el objeto 'factura' (el modelo XML)
            factura facturaXml = GenerarXmlFactura(claveAcceso, facturaDominio, clienteDominio);

            // 2. Convertir ese objeto a un string XML (sin firmar)
            string xmlSinFirmar = SerializarObjeto(facturaXml);

            // 3. Firmar el XML
            string rutaCertificado = @"C:\Users\THINKPAD\Desktop\certificado_prueba_sri.p12";
            string passwordCertificado = "TuPasswordSegura123"; // <-- ¡LA CONTRASEÑA QUE CREASTE!

            string xmlFirmado = _firmaService.FirmarXml(xmlSinFirmar, rutaCertificado, passwordCertificado);

            return xmlFirmado;
        }


        /// <summary>
        /// El "Mapper". Convierte tus datos de dominio al objeto XML del SRI.
        /// </summary>
        private factura GenerarXmlFactura(
            string claveAcceso,
            Factura facturaDominio,
            Cliente clienteDominio
            )
        {
            // El secuencial del SRI son 9 dígitos.
            string secuencialFormateado = facturaDominio.NumeroFactura.PadLeft(9, '0');

            // --- LÓGICA DE IMPUESTOS (AGRUPACIÓN) ---
            // El SRI pide un resumen de impuestos. Agrupamos todos los impuestos de 
            // todas las líneas de detalle por su tipo (IVA 0, IVA 12, etc.)
            var gruposImpuestos = facturaDominio.Detalles
                // 1. Obtenemos pares de (Detalle, Impuesto) por cada producto
                .SelectMany(d => d.Producto.ProductoImpuestos.Select(pi => new { Detalle = d, Impuesto = pi.Impuesto }))
                // 2. Agrupamos por el objeto Impuesto (por su ID)
                .GroupBy(x => x.Impuesto)
                // 3. Creamos el objeto 'totalConImpuesto' que pide el SRI
                .Select(g => new totalConImpuesto
                {
                    codigo = "2", // Asumimos que todo es IVA (Código SRI para IVA)
                    codigoPorcentaje = g.Key.CodigoSRI, // "0", "2", "3" (según tus datos)
                    baseImponible = g.Sum(x => x.Detalle.Subtotal).ToString("F2", _cultureInfo),
                    valor = g.Sum(x => x.Detalle.ValorIVA).ToString("F2", _cultureInfo)
                })
                .ToArray();

            // --- CONSTRUCCIÓN DEL OBJETO XML ---
            var facturaXml = new factura
            {
                id = "comprobante",
                version = "1.0.0", // Revisa el XSD si es necesario
                
                // === 1. INFORMACIÓN TRIBUTARIA (Tu empresa ficticia) ===
                infoTributaria = new infoTributaria
                {
                    ambiente = TIPO_AMBIENTE,
                    tipoEmision = "1", // 1 = Emisión Normal
                    razonSocial = RAZON_SOCIAL_EMISOR,
                    nombreComercial = NOMBRE_COMERCIAL_EMISOR,
                    ruc = RUC_EMISOR,
                    claveAcceso = claveAcceso,
                    codDoc = "01", // 01 = Factura
                    estab = COD_ESTABLECIMIENTO,
                    ptoEmi = COD_PUNTO_EMISION,
                    secuencial = secuencialFormateado,
                    dirMatriz = DIRECCION_MATRIZ_EMISOR
                },

                // === 2. INFORMACIÓN DE LA FACTURA (Cliente, Totales) ===
                infoFactura = new facturaInfoFactura
                {
                    fechaEmision = facturaDominio.FechaEmision.ToString("dd/MM/yyyy"),
                    dirEstablecimiento = DIRECCION_MATRIZ_EMISOR, 
                    obligadoContabilidad = OBLIGADO_CONTABILIDAD,
                    
                    // --- DATOS DEL CLIENTE (MAPEADOS) ---
                    tipoIdentificacionComprador = MapearTipoIdentificacion(clienteDominio.TipoIdentificacion),
                    razonSocialComprador = clienteDominio.RazonSocial,
                    identificacionComprador = clienteDominio.NumeroIdentificacion,
                    
                    // --- TOTALES (MAPEADOS) ---
                    totalSinImpuesto = facturaDominio.SubtotalSinImpuestos.ToString("F2", _cultureInfo),
                    totalDescuento = facturaDominio.TotalDescuento.ToString("F2", _cultureInfo),
                    propina = "0.00",
                    importeTotal = facturaDominio.Total.ToString("F2", _cultureInfo),
                    
                    // --- DESGLOSE DE IMPUESTOS (GENERADO) ---
                    totalConImpuestos = gruposImpuestos
                },

                // === 3. DETALLES DE LA FACTURA (Productos) ===
                detalles = facturaDominio.Detalles.Select(detalle => new facturaDetalle
                {
                    // --- DATOS DEL PRODUCTO (MAPEADOS) ---
                    codigoPrincipal = detalle.Producto.CodigoPrincipal,
                    descripcion = detalle.Producto.Nombre,
                    cantidad = detalle.Cantidad.ToString("F2", _cultureInfo),
                    precioUnitario = detalle.PrecioVentaUnitario.ToString("F2", _cultureInfo),
                    descuento = detalle.Descuento.ToString("F2", _cultureInfo),
                    precioTotalSinImpuesto = detalle.Subtotal.ToString("F2", _cultureInfo),

                    // --- IMPUESTOS POR DETALLE (MAPEADOS) ---
                    // Por cada detalle, creamos su lista de impuestos
                    impuestos = detalle.Producto.ProductoImpuestos.Select(pi => new impuesto
                    {
                        codigo = "2", // Asumimos IVA
                        codigoPorcentaje = pi.Impuesto.CodigoSRI, // "0", "2", etc.
                        tarifa = pi.Impuesto.Porcentaje.ToString("F2", _cultureInfo),
                        baseImponible = detalle.Subtotal.ToString("F2", _cultureInfo),
                        valor = detalle.ValorIVA.ToString("F2", _cultureInfo)
                    }).ToArray()

                }).ToArray()
            };

            return facturaXml;
        }

        /// <summary>
        /// Helper para convertir un objeto C# (como 'factura') en un string XML.
        /// </summary>
        private string SerializarObjeto(object objeto)
        {
            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true, 
                    Encoding = new System.Text.UTF8Encoding(false) // Sin BOM
                };

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

        /// <summary>
        /// Helper para traducir tu Enum de Dominio a los códigos del SRI.
        /// </summary>
        private string MapearTipoIdentificacion(TipoIdentificacion tipo)
        {
            switch (tipo)
            {
                case TipoIdentificacion.Cedula:
                    return "05"; // Cédula
                case TipoIdentificacion.RUC:
                    return "04"; // RUC
                case TipoIdentificacion.Pasaporte:
                    return "06"; // Pasaporte
                case TipoIdentificacion.ConsumidorFinal:
                    return "07"; // Consumidor Final
                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), $"Tipo de identificación no soportado por el SRI: {tipo}.");
            }
        }
    }
}