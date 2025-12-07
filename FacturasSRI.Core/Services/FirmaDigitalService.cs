using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Signature.Parameters;

namespace FacturasSRI.Core.Services
{
    public class FirmaDigitalService
    {
        public byte[] FirmarXml(string xmlSinFirmar, string rutaCertificado, string passwordCertificado)
        {
            X509Certificate2 certificado;

            // MODIFICACIÓN: Lógica híbrida para soportar Archivo Local y Base64 en la Nube
            if (File.Exists(rutaCertificado))
            {
                // Si existe el archivo físico (Entorno Local), lo cargamos normal
                certificado = new X509Certificate2(rutaCertificado, passwordCertificado, X509KeyStorageFlags.Exportable);
            }
            else
            {
                try
                {
                    // Si no existe archivo, intentamos convertir el string Base64 a bytes (Entorno Nube)
                    // En Render, 'rutaCertificado' contendrá la cadena larga Base64
                    byte[] certificadoBytes = Convert.FromBase64String(rutaCertificado);
                    certificado = new X509Certificate2(certificadoBytes, passwordCertificado, X509KeyStorageFlags.Exportable);
                }
                catch (Exception)
                {
                    // Si falla, es que no era ni archivo válido ni base64 válido
                    throw new FileNotFoundException($"No se encontró el certificado en la ruta física '{rutaCertificado}' y tampoco se pudo procesar como una cadena Base64 válida.");
                }
            }

            var xadesService = new XadesService();

            var parametros = new SignatureParameters
            {
                Signer = new FirmaXadesNetCore.Crypto.Signer(certificado),
                
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                
                DataFormat = new DataFormat 
                { 
                    MimeType = "text/xml" 
                }
            };

            var documentoXml = new XmlDocument();
            documentoXml.PreserveWhitespace = true;
            documentoXml.LoadXml(xmlSinFirmar);

            using (var stream = new MemoryStream())
            {
                documentoXml.Save(stream);
                stream.Position = 0;

                try
                {
                    var signedXml = xadesService.Sign(stream, parametros);
                    
                    using (var ms = new MemoryStream())
                    {
                        signedXml.Document.Save(ms);
                        return ms.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al firmar el XML: {ex.Message}", ex);
                }
            }
        }
    }
}