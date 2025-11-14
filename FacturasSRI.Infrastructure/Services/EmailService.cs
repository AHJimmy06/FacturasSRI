using FacturasSRI.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text; // <--- AÑADE ESTE USING
using System.Threading.Tasks;

namespace FacturasSRI.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName, string temporaryPassword)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"]; // Ej: "Aether Tech" o "FacturasSRI"

            // --- DATOS PARA LA PLANTILLA ---
            // ¡IMPORTANTE! Reemplaza esto con la URL real de tu página de login
            string loginUrl = "http://localhost:5000/login"; 
            
            string companyName = "Aether Tecnologías";
            string ruc = "1799999999001";
            string address = "Av. de los Shyris N37-271 y Holanda, Edificio Shyris Center, Quito, Ecuador";
            string currentYear = DateTime.Now.Year.ToString();

            // --- ASUNTO DEL CORREO ---
            var subject = $"¡Bienvenido a {fromName}!";

            // --- CONTENIDO EN TEXTO PLANO (Para clientes de email antiguos) ---
            var plainTextContent = new StringBuilder();
            plainTextContent.AppendLine($"Hola {userName},");
            plainTextContent.AppendLine();
            plainTextContent.AppendLine($"¡Estamos encantados de tenerte con nosotros! Tu cuenta para {fromName} ha sido creada exitosamente.");
            plainTextContent.AppendLine();
            plainTextContent.AppendLine("Estos son tus datos de acceso para ingresar al sistema:");
            plainTextContent.AppendLine($"Usuario: {toEmail}");
            plainTextContent.AppendLine($"Contraseña Temporal: {temporaryPassword}");
            plainTextContent.AppendLine();
            plainTextContent.AppendLine("Por tu seguridad, te recomendamos cambiar esta contraseña la primera vez que inicies sesión.");
            plainTextContent.AppendLine();
            plainTextContent.AppendLine($"Inicia sesión aquí: {loginUrl}");
            plainTextContent.AppendLine();
            plainTextContent.AppendLine($"Saludos,");
            plainTextContent.AppendLine($"El equipo de {fromName}");
            plainTextContent.AppendLine();
            plainTextContent.AppendLine("---");
            plainTextContent.AppendLine($"© {currentYear} {companyName}. Todos los derechos reservados.");
            plainTextContent.AppendLine($"RUC: {ruc}");
            plainTextContent.AppendLine(address);

            // --- CONTENIDO EN HTML (Para la mayoría de clientes de email) ---
            var htmlContent = new StringBuilder();
            htmlContent.Append("<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'></head>");
            htmlContent.Append($"<body style='font-family: Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f7f6;'>");
            htmlContent.Append($"<div style='width: 90%; max-width: 600px; margin: 20px auto; border: 1px solid #dcdcdc; border-radius: 8px; overflow: hidden; background-color: #ffffff;'>");
            
            htmlContent.Append($"<div style='background-color: #004a99; color: #ffffff; padding: 20px 30px; text-align: center;'>"); // Color corporativo (puedes cambiarlo)
            htmlContent.Append($"<h1 style='margin: 0; font-size: 24px;'>¡Bienvenido a {fromName}!</h1>");
            htmlContent.Append("</div>");

            htmlContent.Append($"<div style='padding: 30px; color: #333333; line-height: 1.6;'>");
            htmlContent.Append($"<p style='font-size: 18px;'>Hola {userName},</p>");
            htmlContent.Append($"<p>¡Estamos encantados de tenerte con nosotros! Tu cuenta ha sido creada exitosamente.</p>");
            htmlContent.Append($"<p>Estos son tus datos de acceso para ingresar al sistema:</p>");
            htmlContent.Append($"<div style='background-color: #f9f9f9; padding: 15px 20px; border-radius: 5px; border: 1px solid #eeeeee;'>");
            htmlContent.Append($"<strong>Usuario:</strong> {toEmail}<br>");
            htmlContent.Append($"<strong>Contraseña Temporal:</strong> <span style='font-size: 18px; font-weight: bold; color: #004a99;'>{temporaryPassword}</span>");
            htmlContent.Append("</div>");
            htmlContent.Append($"<p style='margin-top: 20px;'>Por tu seguridad, te recomendamos cambiar esta contraseña la primera vez que inicies sesión.</p>");

            htmlContent.Append($"<p style='text-align: center; margin: 20px 0;'>"); // Contenedor para centrar
            htmlContent.Append($"<a href='{loginUrl}' target='_blank' style='display: inline-block; background-color: #007bff; color: #ffffff; text-decoration: none; padding: 12px 25px; border-radius: 5px; font-weight: bold;'>Iniciar Sesión Ahora</a>");
            htmlContent.Append($"</p>");

            htmlContent.Append($"<p style='margin-top: 30px;'>Saludos,<br>El equipo de {fromName}</p>");
            htmlContent.Append("</div>");

            htmlContent.Append($"<div style='background-color: #f4f7f6; color: #888888; padding: 20px 30px; text-align: center; font-size: 12px; border-top: 1px solid #dcdcdc;'>");
            htmlContent.Append($"<p style='margin: 5px 0;'>© {currentYear} {companyName}. Todos los derechos reservados.</p>");
            htmlContent.Append($"<p style='margin: 5px 0;'><strong>RUC:</strong> {ruc}</p>");
            htmlContent.Append($"<p style='margin: 5px 0;'>{address}</p>");
            htmlContent.Append($"<p style='margin-top: 10px;'>Este es un correo electrónico transaccional. Por favor, no respondas a este mensaje.</p>");
            htmlContent.Append("</div>");

            htmlContent.Append("</div></body></html>");

            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = subject,
                PlainTextContent = plainTextContent.ToString(),
                HtmlContent = htmlContent.ToString()
            };
            msg.AddTo(new EmailAddress(toEmail));

            await client.SendEmailAsync(msg);
        }
    }
}