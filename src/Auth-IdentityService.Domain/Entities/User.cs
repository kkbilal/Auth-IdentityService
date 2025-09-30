using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Domain.Entities
{
	public class User
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public bool IsActive { get; set; } = true;
		public bool IsLockedOut { get; set; } = false;
		public int FailedLoginAttempts { get; set; } = 0;
		public DateTime? LastFailedLoginAttempt { get; set; }
		public DateTime? LockoutEndDate { get; set; }
		public string? RefreshToken { get; set; }
		public DateTime RefreshTokenExpiryTime { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? LastLoginAt { get; set; }
		public string? TwoFactorSecret { get; set; }
		public bool IsTwoFactorEnabled { get; set; } = false;
		public List<string>? RecoveryCodes { get; set; }

		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
	}
}