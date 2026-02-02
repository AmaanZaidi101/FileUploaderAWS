using FileUploader.Application.Context;
using FileUploader.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Controllers ----------------
builder.Services.AddControllers();

// ---------------- Database ----------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ---------------- Identity (DO NOT ADD COOKIES MANUALLY) ----------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ---------------- Google Authentication ----------------
builder.Services.AddAuthentication()
	.AddGoogle("Google", options =>
	{
		options.ClientId =
			builder.Configuration["Authentication:Google:ClientId"]
			?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
			?? throw new InvalidOperationException("Google ClientId missing");

		options.ClientSecret =
			builder.Configuration["Authentication:Google:ClientSecret"]
			?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
			?? throw new InvalidOperationException("Google ClientSecret missing");

		// MUST match controller callback
		options.CallbackPath = "/signin-google";

		// Identity external cookie
		options.SignInScheme = IdentityConstants.ExternalScheme;

		options.Scope.Add("email");
		options.Scope.Add("profile");

		options.SaveTokens = true;
	});

// ---------------- Cookie Settings (LOCALHOST SAFE) ----------------
builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.HttpOnly = true;
	options.Cookie.SameSite = SameSiteMode.Lax;
	options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
	options.LoginPath = "/api/auth/google-login";
});

// ---------------- CORS ----------------
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy.WithOrigins("https://localhost:5174").WithOrigins("https://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials());
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
