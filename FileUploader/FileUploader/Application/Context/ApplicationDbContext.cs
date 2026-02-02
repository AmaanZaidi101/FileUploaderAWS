using FileUploader.Application.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FileUploader.Application.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			// Customize the ASP.NET Identity model if needed
			builder.Entity<ApplicationUser>(entity =>
			{
				entity.Property(e => e.GoogleId).HasMaxLength(100);
			});
		}
	}
}
