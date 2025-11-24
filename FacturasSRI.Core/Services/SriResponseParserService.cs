using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FacturasSRI.Core.Models;
using Microsoft.Extensions.Logging;

namespace FacturasSRI.Core.Services
{
    public class SriResponseParserService
    {
        private static XNamespace ns2 = "http://ec.gob.sri.ws.recepcion";
        private static XNamespace ns2_auth = "http://ec.gob.sri.ws.autorizacion";
        private readonly ILogger<SriResponseParserService> _logger;

        public SriResponseParserService(ILogger<SriResponseParserService> logger)
        {
            _logger = logger;
        }

        public RespuestaRecepcion ParsearRespuestaRecepcion(string soapResponse)
        {
            _logger.LogWarning("--- INICIO RESPUESTA SRI (RECEPCIÓN) ---\n{Xml}\n--- FIN RESPUESTA SRI (RECEPCIÓN) ---", soapResponse);

            var respuesta = new RespuestaRecepcion();
            var xmlDoc = XDocument.Parse(soapResponse);

            var respuestaNode = xmlDoc.Descendants(ns2 + "validarComprobanteResponse").FirstOrDefault();
            if (respuestaNode == null)
            {
                var faultString = xmlDoc.Descendants("faultstring").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(faultString))
                {
                    throw new Exception($"El servicio del SRI devolvió un error (SOAP Fault): {faultString}");
                }
                throw new Exception("No se encontró 'validarComprobanteResponse' en la respuesta SOAP.");
            }

            respuesta.Estado = respuestaNode.Descendants("estado").FirstOrDefault()?.Value ?? "ERROR";

            if (respuesta.Estado == "DEVUELTA")
            {
                respuesta.Errores = xmlDoc.Descendants("mensaje")
                    .Select(m => new SriError
                    {
                        Identificador = m.Descendants("identificador").FirstOrDefault()?.Value ?? "",
                        Mensaje = m.Descendants("mensaje").FirstOrDefault()?.Value ?? "",
                        InformacionAdicional = m.Descendants("informacionAdicional").FirstOrDefault()?.Value ?? "",
                        Tipo = m.Descendants("tipo").FirstOrDefault()?.Value ?? ""
                    })
                    .ToList();
            }

            return respuesta;
        }

        public RespuestaAutorizacion ParsearRespuestaAutorizacion(string soapResponse)
        {
            var respuesta = new RespuestaAutorizacion();
            var xmlDoc = XDocument.Parse(soapResponse);

            // El nodo puede estar en un namespace, o no. Buscamos de forma más flexible.
            var autorizacionNode = xmlDoc.Descendants().FirstOrDefault(d => d.Name.LocalName == "autorizacion");
            
            if (autorizacionNode == null)
            {
                var faultString = xmlDoc.Descendants("faultstring").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(faultString))
                {
                    respuesta.Estado = "ERROR";
                    respuesta.Errores.Add(new SriError { Identificador = "SOAP", Mensaje = faultString });
                    _logger.LogWarning("Respuesta SRI (Autorización) es un SOAP Fault: {Fault}", faultString);
                    return respuesta;
                }
                
                // Si no hay nodo de autorización ni fault, es porque sigue en procesamiento.
                respuesta.Estado = "PROCESANDO";
                _logger.LogInformation("Respuesta SRI (Autorización) no contiene nodo 'autorizacion', se asume PROCESANDO.");
                return respuesta;
            }

            respuesta.Estado = autorizacionNode.Descendants().FirstOrDefault(d => d.Name.LocalName == "estado")?.Value ?? "ERROR";

            if (respuesta.Estado == "AUTORIZADO")
            {
                respuesta.NumeroAutorizacion = autorizacionNode.Descendants().FirstOrDefault(d => d.Name.LocalName == "numeroAutorizacion")?.Value ?? "";
                
                var fechaStr = autorizacionNode.Descendants().FirstOrDefault(d => d.Name.LocalName == "fechaAutorizacion")?.Value;
                if (DateTime.TryParse(fechaStr, out DateTime fecha))
                {
                    respuesta.FechaAutorizacion = fecha.ToUniversalTime(); 
                }
                _logger.LogInformation("Factura AUTORIZADA. Número: {Numero}", respuesta.NumeroAutorizacion);
            }
            else if (respuesta.Estado == "NO AUTORIZADO")
            {
                respuesta.Errores = autorizacionNode.Descendants().Where(d => d.Name.LocalName == "mensaje")
                    .Select(m => new SriError
                    {
                        Identificador = m.Descendants().FirstOrDefault(d => d.Name.LocalName == "identificador")?.Value ?? "",
                        Mensaje = m.Descendants().FirstOrDefault(d => d.Name.LocalName == "mensaje")?.Value ?? "",
                        InformacionAdicional = m.Descendants().FirstOrDefault(d => d.Name.LocalName == "informacionAdicional")?.Value ?? "",
                        Tipo = m.Descendants().FirstOrDefault(d => d.Name.LocalName == "tipo")?.Value ?? ""
                    })
                    .ToList();
                _logger.LogWarning("Factura NO AUTORIZADA. Errores: {ErrorCount}", respuesta.Errores.Count);
            }

            return respuesta;
        }
    }
}