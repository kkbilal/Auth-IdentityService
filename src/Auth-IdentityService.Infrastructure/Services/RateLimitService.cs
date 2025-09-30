using Auth_IdentityService.Application.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Infrastructure.Services
{
	public class RateLimitService : IRateLimitService
	{
		private readonly ConcurrentDictionary<string, List<DateTime>> _requestHistory;
		
		public RateLimitService()
		{
			_requestHistory = new ConcurrentDictionary<string, List<DateTime>>();
		}

		public bool IsRateLimited(string key, int maxRequests, TimeSpan period)
		{
			var now = DateTime.UtcNow;
			var cutoff = now - period;

			if (_requestHistory.TryGetValue(key, out var timestamps))
			{
				// Süresi dolmuş istekleri temizle
				timestamps.RemoveAll(t => t < cutoff);
				
				// İstek sayısı sınırı kontrolü
				return timestamps.Count >= maxRequests;
			}

			return false;
		}

		public void RecordRequest(string key)
		{
			var now = DateTime.UtcNow;
			
			_requestHistory.AddOrUpdate(
				key,
				new List<DateTime> { now },
				(k, list) =>
				{
					list.Add(now);
					return list;
				});
		}
	}
}