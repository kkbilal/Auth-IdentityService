using System.Text;
using System.Text.Json;

namespace Auth_IdentityService.API.Middleware
{
	public class SecurityMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<SecurityMiddleware> _logger;

		public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// IP adresi kontrolü
			var ipAddress = context.Connection.RemoteIpAddress?.ToString();
			_logger.LogInformation($"Request from IP: {ipAddress}");

			// XSS koruması
			context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
			context.Response.Headers.Add("X-Frame-Options", "DENY");
			context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

			await _next(context);
		}
	}
}