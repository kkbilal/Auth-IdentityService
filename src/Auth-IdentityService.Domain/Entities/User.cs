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
		public string? RefreshToken { get; set; }
		public DateTime RefreshTokenExpiryTime { get; set; }

		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
	}
}