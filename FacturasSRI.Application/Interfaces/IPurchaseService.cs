using FacturasSRI.Application.Dtos;
using System.Threading.Tasks;

namespace FacturasSRI.Application.Interfaces
{
    public interface IPurchaseService
    {
        Task CreatePurchaseAsync(PurchaseDto purchase);
    }
}
