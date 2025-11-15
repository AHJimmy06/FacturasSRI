namespace FacturasSRI.Application.Interfaces
{
    public interface IValidationService
    {
        bool IsValid(string identification, string type);
    }
}
