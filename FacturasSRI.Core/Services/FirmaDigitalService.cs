using System.Security.Cryptography.X509Certificates;
using System.Xml;
using FirmaXadesNet;
using FirmaXadesNet.Signature.Parameters;

namespace FacturasSRI.Core.Services
{
    public class FirmaDigitalService
    {
        public string FirmarXml(string xmlSinFirmar, string rutaCertificado, string passwordCertificado)
        {
            // 1. Cargar el certificado digital (.p12)
            // Este certificado te lo debe proveer el SRI para el ambiente de pruebas.
            // O puedes comprar uno si ya estuvieras en producción.
            var certificado = new X509Certificate2(rutaCertificado, passwordCertificado, X509KeyStorageFlags.Exportable);

            // 2. Configurar la firma XAdES-BES
            var xadesService = new XadesService();
            var parametros = new SignatureParameters
            {
                SignaturePolicyInfo = new SignaturePolicyInfo
                {
                    PolicyIdentifier = "http://www.w3.org/2000/09/xmldsig#", // Política estándar
                },
                SignaturePackaging = SignaturePackaging.ENVELOPED, // El XML se firma "dentro" de sí mismo
                InputMimeType = "text/xml"
            };

            // 3. Cargar el XML en un documento
            var documentoXml = new XmlDocument();
            documentoXml.LoadXml(xmlSinFirmar);

            // 4. Ejecutar la firma
            // El servicio de FirmaXadesNet busca la etiqueta principal (ej. <factura>)
            // y añade el bloque <ds:Signature> dentro de ella.
            var resultadoFirma = xadesService.Sign(documentoXml, certificado, parametros);

            // 5. Devolver el XML firmado como string
            if (resultadoFirma.Status == FirmaXadesNet.Signature.SignatureStatus.Signed)
            {
                return resultadoFirma.Signature.Document.OuterXml;
            }
            else
            {
                // Manejar el error. En un caso real, lanza una excepción más específica.
                throw new Exception($"Error al firmar el XML: {resultadoFirma.Status}");
            }
        }
    }
}