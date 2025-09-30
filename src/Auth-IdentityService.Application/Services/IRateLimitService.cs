using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Application.Services
{
	public interface IRateLimitService
	{
		bool IsRateLimited(string key, int maxRequests, TimeSpan period);
		void RecordRequest(string key);
	}
}