using Auth_IdentityService.Application.Dtos.UserDtos;
using Auth_IdentityService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth_IdentityService.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost("register")]
		public async Task<ActionResult<UserResponseDto>> Register(RegisterUserRequestDto registerRequest)
		{
			try
			{
				var result = await _userService.RegisterAsync(registerRequest);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("login")]
		public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto loginRequest)
		{
			try
			{
				var result = await _userService.LoginAsync(loginRequest);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
		{
			try
			{
				var result = await _userService.GetUserByIdAsync(id);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		[HttpPost("change-password")]
		public async Task<ActionResult<bool>> ChangePassword(ChangePasswordRequestDto changePasswordRequest)
		{
			try
			{
				var result = await _userService.ChangePasswordAsync(changePasswordRequest);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}