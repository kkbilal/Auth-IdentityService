using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.UserRoleDtos
{
	public class AssignRoleRequestDto
	{
		public Guid UserId { get; set; }
		public Guid RoleId { get; set; }
	}
}
