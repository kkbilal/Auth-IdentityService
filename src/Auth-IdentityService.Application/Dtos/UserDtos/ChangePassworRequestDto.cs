using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.UserDtos
{
	public class ChangePasswordRequestDto
	{
		public Guid UserId { get; set; }
		public string? CurrentPassword { get; set; }
		public string? NewPassword { get; set; }
	}
}