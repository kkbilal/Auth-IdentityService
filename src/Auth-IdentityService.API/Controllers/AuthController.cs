using Auth_IdentityService.Application.Dtos.TokenDtos;
using Auth_IdentityService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_IdentityService.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("refresh-token")]
		public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshTokenRequestDto refreshTokenRequest)
		{
			try
			{
				var result = await _authService.RefreshTokenAsync(refreshTokenRequest);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}