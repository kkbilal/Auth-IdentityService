using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.UserDtos
{
	public class RegisterUserRequestDto
	{
		public string? Email { get; set; }
		public string? Password { get; set; }
	}
}