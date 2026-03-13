using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace BFF.Infrastructure.HttpHandlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthHeaderHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _contextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(token))
            {
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer", token.Substring("Bearer ".Length));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
