using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Services
{
	public interface IPasswordValidator
	{
		bool IsValid(string password, out List<string> errors);
	}
}