using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml;
using System;

namespace FacturasSRI.Core.Services
{
    public class SriApiClientService
    {
        private const string URL_RECEPCION_PRUEBAS = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/RecepcionComprobantesOffline";
        private const string URL_AUTORIZACION_PRUEBAS = "https://celcer.sri.gob.ec/comprobantes-electronicos-ws/AutorizacionComprobantesOffline";

        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<string> EnviarRecepcionAsync(byte[] xmlFirmadoBytes)
        {
            string base64Xml = Convert.ToBase64String(xmlFirmadoBytes);
            string sobreSoap = $@"
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.recepcion"">
                   <soapenv:Header/>
                   <soapenv:Body>
                      <ec:validarComprobante>
                            <xml>{base64Xml}</xml>
                      </ec:validarComprobante>
                   </soapenv:Body>
                </soapenv:Envelope>";

            var httpContent = new StringContent(sobreSoap, Encoding.UTF8, "text/xml");
            
            try 
            {
                HttpResponseMessage respuesta = await _httpClient.PostAsync(URL_RECEPCION_PRUEBAS, httpContent);
                string respuestaSoap = await respuesta.Content.ReadAsStringAsync();

                // --- LOGS DE DEPURACIÓN ---
                Console.WriteLine("==========================================");
                Console.WriteLine("RESPUESTA SRI (RECEPCIÓN):");
                Console.WriteLine(respuestaSoap);
                Console.WriteLine("==========================================");
                // ---------------------------

                return respuestaSoap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR HTTP RECEPCIÓN: {ex.Message}");
                throw;
            }
        }

        public async Task<string> ConsultarAutorizacionAsync(string claveAcceso)
        {
            string sobreSoap = $@"
                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ec=""http://ec.gob.sri.ws.autorizacion"">
                   <soapenv:Header/>
                   <soapenv:Body>
                      <ec:autorizacionComprobante>
                         <claveAccesoComprobante>{claveAcceso}</claveAccesoComprobante>
                      </ec:autorizacionComprobante>
                   </soapenv:Body>
                </soapenv:Envelope>";
            
            var httpContent = new StringContent(sobreSoap, Encoding.UTF8, "text/xml");

            try
            {
                HttpResponseMessage respuesta = await _httpClient.PostAsync(URL_AUTORIZACION_PRUEBAS, httpContent);
                string respuestaSoap = await respuesta.Content.ReadAsStringAsync();

                // --- LOGS DE DEPURACIÓN ---
                Console.WriteLine("==========================================");
                Console.WriteLine("RESPUESTA SRI (AUTORIZACIÓN):");
                Console.WriteLine(respuestaSoap);
                Console.WriteLine("==========================================");
                // ---------------------------

                return respuestaSoap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR HTTP AUTORIZACIÓN: {ex.Message}");
                throw;
            }
        }
    }
}