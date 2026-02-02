using Microsoft.AspNetCore.Identity;

namespace FileUploader.Application.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? ProfilePictureUrl { get; set; }
		public string? GoogleId { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
