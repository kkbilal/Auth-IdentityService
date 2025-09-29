using Auth_IdentityService.Application.Dtos.TokenDtos;
using Auth_IdentityService.Application.Dtos.UserDtos;

namespace Auth_IdentityService.Application.Services
{
	public interface IAuthService
	{
		Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
		Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequest);
		string GenerateAccessToken(Guid userId, string email, List<string> roles);
		string GenerateRefreshToken();
		bool ValidateToken(string token);
	}
}