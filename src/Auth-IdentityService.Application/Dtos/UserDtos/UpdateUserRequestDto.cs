using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.UserDtos
{
	public class UpdateUserRequestDto
	{
		public Guid Id { get; set; }
		public string Email { get; set; }
		public bool IsActive { get; set; }
	}
}
