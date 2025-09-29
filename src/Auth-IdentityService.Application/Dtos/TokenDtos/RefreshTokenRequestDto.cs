using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Dtos.TokenDtos
{
	public class RefreshTokenRequestDto
	{
		public string RefreshToken { get; set; }
	}
}
