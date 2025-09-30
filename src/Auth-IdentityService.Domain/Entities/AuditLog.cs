using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth_IdentityService.Domain.Entities
{
	public class AuditLog
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public Guid? UserId { get; set; }
		public string Action { get; set; } = string.Empty;
		public string EntityType { get; set; } = string.Empty;
		public string EntityId { get; set; } = string.Empty;
		public string OldValues { get; set; } = string.Empty;
		public string NewValues { get; set; } = string.Empty;
		public string IpAddress { get; set; } = string.Empty;
		public string UserAgent { get; set; } = string.Empty;
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;
	}
}