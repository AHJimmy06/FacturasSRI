using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string userName, string temporaryPassword);
    }
}
