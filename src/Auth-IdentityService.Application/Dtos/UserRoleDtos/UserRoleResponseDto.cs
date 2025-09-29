using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.UserRoleDtos
{
	public class UserRoleResponseDto
	{
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
		public string RoleName { get; set; }
	}
}
