using Auth_IdentityService.Application.Dtos.UserDtos;
using Auth_IdentityService.Application.Services;
using Auth_IdentityService.Domain.Entities;
using Auth_IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Auth_IdentityService.Infrastructure.Services
{
	public class UserService : IUserService
	{
		private readonly AppDbContext _context;
		private readonly IAuthService _authService;
		private readonly IPasswordValidator _passwordValidator;
		private readonly IRateLimitService _rateLimitService;
		private readonly ILogger<UserService> _logger;

		public UserService(
			AppDbContext context, 
			IAuthService authService, 
			IPasswordValidator passwordValidator,
			IRateLimitService rateLimitService,
			ILogger<UserService> logger)
		{
			_context = context;
			_authService = authService;
			_passwordValidator = passwordValidator;
			_rateLimitService = rateLimitService;
			_logger = logger;
		}

		public async Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto registerRequest)
		{
			// Null kontrolleri
			if (registerRequest == null)
			{
				throw new ArgumentNullException(nameof(registerRequest));
			}

			if (string.IsNullOrEmpty(registerRequest.Email))
			{
				throw new Exception("Email is required.");
			}

			if (string.IsNullOrEmpty(registerRequest.Password))
			{
				throw new Exception("Password is required.");
			}

			// Rate limiting kontrolü
			var ipAddress = GetClientIpAddress();
			if (_rateLimitService.IsRateLimited($"register_{ipAddress}", 5, TimeSpan.FromMinutes(10)))
			{
				throw new Exception("Too many registration attempts. Please try again later.");
			}

			// Şifre validasyonu
			if (!_passwordValidator.IsValid(registerRequest.Password, out var passwordErrors))
			{
				throw new Exception($"Password validation failed: {string.Join(", ", passwordErrors)}");
			}

			// Kullanıcı zaten var mı kontrol et
			var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
			if (existingUser != null)
			{
				_rateLimitService.RecordRequest($"register_{ipAddress}");
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

			// Rate limit kaydını yap
			_rateLimitService.RecordRequest($"register_{ipAddress}");

			// Audit log
			await LogAuditAsync(user.Id, "UserRegistered", "User", user.Id.ToString(), "", $"User {user.Email} registered");

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
			// Null kontrolleri
			if (loginRequest == null)
			{
				throw new ArgumentNullException(nameof(loginRequest));
			}

			if (string.IsNullOrEmpty(loginRequest.Email))
			{
				throw new Exception("Email is required.");
			}

			if (string.IsNullOrEmpty(loginRequest.Password))
			{
				throw new Exception("Password is required.");
			}

			// Rate limiting kontrolü
			var ipAddress = GetClientIpAddress();
			if (_rateLimitService.IsRateLimited($"login_{loginRequest.Email}_{ipAddress}", 5, TimeSpan.FromMinutes(15)))
			{
				throw new Exception("Too many login attempts. Account is temporarily locked.");
			}

			// Kullanıcıyı email ile bul
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
			if (user == null)
			{
				_rateLimitService.RecordRequest($"login_{loginRequest.Email}_{ipAddress}");
				throw new Exception("Invalid email or password.");
			}

			// Hesap kilitli mi kontrolü
			if (user.IsLockedOut && user.LockoutEndDate > DateTime.UtcNow)
			{
				throw new Exception("Account is locked. Please try again later.");
			}

			// Şifre doğrulaması yap
			bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
			if (!isPasswordValid)
			{
				// Başarısız giriş denemesini kaydet
				user.FailedLoginAttempts++;
				user.LastFailedLoginAttempt = DateTime.UtcNow;

				// 5 başarısız denemeden sonra hesabı kilitle
				if (user.FailedLoginAttempts >= 5)
				{
					user.IsLockedOut = true;
					user.LockoutEndDate = DateTime.UtcNow.AddHours(1);
				}

				await _context.SaveChangesAsync();
				_rateLimitService.RecordRequest($"login_{loginRequest.Email}_{ipAddress}");
				throw new Exception("Invalid email or password.");
			}

			// Kullanıcı aktif mi kontrolü
			if (!user.IsActive)
			{
				throw new Exception("Account is deactivated.");
			}

			// Başarılı giriş işlemleri
			user.FailedLoginAttempts = 0;
			user.LastLoginAt = DateTime.UtcNow;
			user.IsLockedOut = false;
			user.LockoutEndDate = null;

			// Access token oluştur
			var userRoles = await _context.UserRoles
				.Where(ur => ur.UserId == user.Id)
				.Include(ur => ur.Role)
				.Select(ur => ur.Role.Name)
				.ToListAsync();

			string accessToken = _authService.GenerateAccessToken(
				user.Id, 
				user.Email ?? "", 
				userRoles ?? new List<string>());

			// Refresh token oluştur
			string refreshToken = _authService.GenerateRefreshToken();

			// Refresh token'ı veritabanına kaydet
			user.RefreshToken = refreshToken;
			user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // 7 gün geçerli

			await _context.SaveChangesAsync();

			// Rate limit kaydını temizle
			// (Başarılı girişler rate limiti sıfırlamaz)

			// Audit log
			await LogAuditAsync(user.Id, "UserLoggedIn", "User", user.Id.ToString(), "", $"User {user.Email} logged in from IP {ipAddress}");

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
			// Null kontrolleri
			if (changePasswordRequest == null)
			{
				throw new ArgumentNullException(nameof(changePasswordRequest));
			}

			if (string.IsNullOrEmpty(changePasswordRequest.CurrentPassword))
			{
				throw new Exception("Current password is required.");
			}

			if (string.IsNullOrEmpty(changePasswordRequest.NewPassword))
			{
				throw new Exception("New password is required.");
			}

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

			// Yeni şifre validasyonu
			if (!_passwordValidator.IsValid(changePasswordRequest.NewPassword, out var passwordErrors))
			{
				throw new Exception($"Password validation failed: {string.Join(", ", passwordErrors)}");
			}

			// Yeni şifreyi hashle ve güncelle
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);
			await _context.SaveChangesAsync();

			// Audit log
			await LogAuditAsync(user.Id, "PasswordChanged", "User", user.Id.ToString(), "", "User changed password");

			return true;
		}

		private string GetClientIpAddress()
		{
			// Bu metod gerçek uygulamada daha gelişmiş IP adresi tespiti yapılabilir
			return "127.0.0.1";
		}

		private async Task LogAuditAsync(Guid userId, string action, string entityType, string entityId, string oldValues, string newValues)
		{
			var auditLog = new AuditLog
			{
				UserId = userId,
				Action = action,
				EntityType = entityType,
				EntityId = entityId,
				OldValues = oldValues,
				NewValues = newValues,
				IpAddress = GetClientIpAddress(),
				UserAgent = "API Client" // Gerçek uygulamada HttpContext.Request.Headers["User-Agent"] kullanılabilir
			};

			_context.AuditLogs.Add(auditLog);
			await _context.SaveChangesAsync();
		}
	}
}