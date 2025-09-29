using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.RoleDtos
{
	public class UpdateRoleRequestDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}
