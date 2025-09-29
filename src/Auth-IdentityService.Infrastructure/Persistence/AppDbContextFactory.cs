using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth_IdentityService.Infrastructure.Persistence
{
	public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
	{
		public AppDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
			optionsBuilder.UseSqlServer("Server=DESKTOP-82RLRPM\\SQLEXPRESS;Initial Catalog=IdentityServiceDb;integrated Security=true;TrustServerCertificate=True;");

			return new AppDbContext(optionsBuilder.Options);
		}
	}
}