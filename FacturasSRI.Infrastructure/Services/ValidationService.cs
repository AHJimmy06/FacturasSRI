using FacturasSRI.Application.Interfaces;
using FacturasSRI.Domain.Enums;

namespace FacturasSRI.Infrastructure.Services
{
    public class ValidationService : IValidationService
    {
        public bool IsValid(string identification, string type)
        {
            return type switch
            {
                "Cedula" => IsValidCedula(identification),
                "RUC" => IsValidRuc(identification),
                "Pasaporte" => IsValidPassport(identification),
                _ => false,
            };
        }

        private bool IsValidPassport(string passport)
        {
            if (string.IsNullOrEmpty(passport))
            {
                return false;
            }

            if (passport.Length < 6 || passport.Length > 9)
            {
                return false;
            }

            return passport.All(char.IsLetterOrDigit);
        }

        private bool IsValidCedula(string cedula)
        {
            if (cedula.Length != 10)
            {
                return false;
            }

            if (!long.TryParse(cedula, out _))
            {
                return false;
            }

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || provincia > 24)
            {
                return false;
            }

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int producto = int.Parse(cedula[i].ToString()) * coeficientes[i];
                if (producto >= 10)
                {
                    producto -= 9;
                }
                suma += producto;
            }

            int digitoVerificador = (suma % 10 == 0) ? 0 : 10 - (suma % 10);

            return digitoVerificador == int.Parse(cedula[9].ToString());
        }

        private bool IsValidRuc(string ruc)
        {
            if (ruc.Length != 13)
            {
                return false;
            }

            if (!long.TryParse(ruc, out _))
            {
                return false;
            }

            if (!ruc.EndsWith("001"))
            {
                return false;
            }

            return IsValidCedula(ruc.Substring(0, 10));
        }
    }
}
