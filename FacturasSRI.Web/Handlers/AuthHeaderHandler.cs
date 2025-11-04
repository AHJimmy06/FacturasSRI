using FacturasSRI.Web.States;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FacturasSRI.Web.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"--- AuthHeaderHandler: SendAsync TRASLADADO para {request.RequestUri} ---");

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                Console.WriteLine("--- AuthHeaderHandler: HttpContext es nulo. Petición irá sin autorización. ---");
                return await base.SendAsync(request, cancellationToken);
            }

            var currentUserState = httpContext.RequestServices.GetRequiredService<CurrentUserState>();

            if (currentUserState.IsLoggedIn && !string.IsNullOrEmpty(currentUserState.Token))
            {
                Console.WriteLine($"--- AuthHeaderHandler: Token encontrado. Añadiendo cabecera. ---");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentUserState.Token);
            }
            else
            {
                Console.WriteLine("--- AuthHeaderHandler: No se encontró token en CurrentUserState. Petición irá sin autorización. ---");
            }
            
            return await base.SendAsync(request, cancellationToken);
        }
    }
}