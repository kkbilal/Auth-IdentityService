using Auth_IdentityService.Application.Dtos.TokenDtos;
using Auth_IdentityService.Application.Dtos.UserDtos;
using Auth_IdentityService.Application.Services;
using Auth_IdentityService.Domain.Entities;
using Auth_IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth_IdentityService.Infrastructure.Services
{
	public class AuthService : IAuthService
	{
		private readonly IConfiguration _configuration;
		private readonly AppDbContext _context;

		public AuthService(IConfiguration configuration, AppDbContext context)
		{
			_configuration = configuration;
			_context = context;
		}

		public string GenerateAccessToken(Guid userId, string email, List<string> roles)
		{
			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
				new Claim(JwtRegisteredClaimNames.Email, email ?? ""),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role ?? ""));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationInMinutes"] ?? "30")),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public string GenerateRefreshToken()
		{
			var randomNumber = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			return Convert.ToBase64String(randomNumber);
		}

		public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
		{
			// Bu metodun implementasyonu UserService'e taşındı
			// Çünkü kullanıcı doğrulama işlemi orada yapılacak
			throw new NotImplementedException();
		}

		public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto refreshTokenRequest)
		{
			// Refresh token'ı veritabanında bul
			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenRequest.RefreshToken);

			if (user == null)
			{
				throw new Exception("Invalid refresh token.");
			}

			// Refresh token'ın süresi dolmuş mu kontrol et
			if (user.RefreshTokenExpiryTime <= DateTime.Now)
			{
				throw new Exception("Refresh token has expired.");
			}

			// Kullanıcının rollerini al
			var userRoles = await _context.UserRoles
				.Where(ur => ur.UserId == user.Id)
				.Include(ur => ur.Role)
				.Select(ur => ur.Role.Name)
				.ToListAsync();

			// Yeni access token oluştur
			string newAccessToken = GenerateAccessToken(user.Id, user.Email, userRoles);

			// Yeni refresh token oluştur
			string newRefreshToken = GenerateRefreshToken();

			// Kullanıcının refresh token'ını güncelle
			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // 7 gün geçerli

			await _context.SaveChangesAsync();

			// Response döndür
			return new RefreshTokenResponseDto
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken,
				Expiration = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationInMinutes"] ?? "30"))
			};
		}

		public bool ValidateToken(string token)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = true,
					ValidIssuer = _configuration["Jwt:Issuer"],
					ValidateAudience = true,
					ValidAudience = _configuration["Jwt:Audience"],
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero
				}, out SecurityToken validatedToken);

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}