using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacturasSRI.Web.Services
{
    public class CookieAuthenticationStateProvider : ServerAuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CookieAuthenticationStateProvider> _logger;

        public CookieAuthenticationStateProvider(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CookieAuthenticationStateProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identities.Any(i => i.IsAuthenticated) ?? false)
            {
                var user = httpContext.User;
                var primaryIdentity = user.Identities.FirstOrDefault(i => i.IsAuthenticated);
                _logger.LogInformation("CookieAuthenticationStateProvider: Usuario AUTENTICADO encontrado. Esquema: {AuthenticationType}, Usuario: {UserName}", 
                    primaryIdentity?.AuthenticationType, primaryIdentity?.Name);
                return Task.FromResult(new AuthenticationState(user));
            }
            
            _logger.LogInformation("CookieAuthenticationStateProvider: No se encontró usuario autenticado. Devolviendo estado anónimo.");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
}