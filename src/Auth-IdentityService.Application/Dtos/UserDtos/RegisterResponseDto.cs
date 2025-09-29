using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.UserDtos
{
	public class UserResponseDto
	{
		public Guid Id { get; set; }
		public string? Email { get; set; }
		public bool IsActive { get; set; }
		public List<string> Roles { get; set; } = new();
	}
}