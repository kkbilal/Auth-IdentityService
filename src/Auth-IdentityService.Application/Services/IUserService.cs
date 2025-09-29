using Auth_IdentityService.Application.Dtos.UserDtos;

namespace Auth_IdentityService.Application.Services
{
	public interface IUserService
	{
		Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto registerRequest);
		Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
		Task<UserResponseDto> GetUserByIdAsync(Guid id);
		Task<bool> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest);
	}
}