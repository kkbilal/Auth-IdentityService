using Auth_IdentityService.Application.Dtos.UserDtos;
using Auth_IdentityService.Application.Services;
using Auth_IdentityService.Domain.Entities;
using Auth_IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auth_IdentityService.Infrastructure.Services
{
	public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly IAuthService _authService;

		public UserService(AppDbContext context, IAuthService authService)
		{
			_context = context;
			_authService = authService;
		}

		public async Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto registerRequest)
		{
			// Kullanıcı zaten var mı kontrol et
			var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
			if (existingUser != null)
			{
				throw new Exception("User with this email already exists.");
			}

			// Şifreyi hashle
			string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

			// Yeni kullanıcı oluştur
			var user = new User
			{
				Email = registerRequest.Email,
				PasswordHash = passwordHash,
				Username = registerRequest.Email.Split('@')[0] // Basit bir username oluştur
			};

			_context.Users.Add(user);
			await _context.SaveChangesAsync();

			// Kullanıcıya varsayılan rol ata (örneğin "User" rolü)
			var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
			if (defaultRole == null)
			{
				defaultRole = new Role { Name = "User" };
				_context.Roles.Add(defaultRole);
				await _context.SaveChangesAsync();
			}

			var userRole = new UserRole
			{
				UserId = user.Id,
				RoleId = defaultRole.Id
			};

			_context.UserRoles.Add(userRole);
			await _context.SaveChangesAsync();

			// Kullanıcı bilgilerini döndür
			return new UserResponseDto
			{
				Id = user.Id,
				Email = user.Email,
				IsActive = user.IsActive,
				Roles = new List<string> { defaultRole.Name }
			};
		}

		public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
		{
			// Kullanıcıyı email ile bul
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
			if (user == null)
			{
				throw new Exception("Invalid email or password.");
			}

			// Şifre doğrulaması yap
			bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
			if (!isPasswordValid)
			{
				throw new Exception("Invalid email or password.");
			}

			// Kullanıcının rollerini al
			var userRoles = await _context.UserRoles
				.Where(ur => ur.UserId == user.Id)
				.Include(ur => ur.Role)
				.Select(ur => ur.Role.Name)
				.ToListAsync();

			// Access token oluştur
			string accessToken = _authService.GenerateAccessToken(user.Id, user.Email, userRoles);

			// Refresh token oluştur
			string refreshToken = _authService.GenerateRefreshToken();

			// Refresh token'ı veritabanına kaydet
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // 7 gün geçerli

			await _context.SaveChangesAsync();

			// Token bilgilerini döndür
			return new LoginResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				Expiration = DateTime.Now.AddMinutes(30) // Varsayılan 30 dakika
			};
		}

		public async Task<UserResponseDto> GetUserByIdAsync(Guid id)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
			if (user == null)
			{
				throw new Exception("User not found.");
			}

			// Kullanıcının rollerini al
			var userRoles = await _context.UserRoles
				.Where(ur => ur.UserId == user.Id)
				.Include(ur => ur.Role)
				.Select(ur => ur.Role.Name)
				.ToListAsync();

			return new UserResponseDto
			{
				Id = user.Id,
				Email = user.Email,
				IsActive = user.IsActive,
				Roles = userRoles
			};
		}

		public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto changePasswordRequest)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == changePasswordRequest.UserId);
			if (user == null)
			{
				throw new Exception("User not found.");
			}

			// Mevcut şifre doğrulaması yap
			bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordRequest.CurrentPassword, user.PasswordHash);
			if (!isCurrentPasswordValid)
			{
				throw new Exception("Current password is incorrect.");
			}

			// Yeni şifreyi hashle ve güncelle
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);
			await _context.SaveChangesAsync();

			return true;
		}
	}
}