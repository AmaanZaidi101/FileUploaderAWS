using System.Security.Claims;
using FileUploader.Application.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FileUploader.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;

	public AuthController(
		SignInManager<ApplicationUser> signInManager,
		UserManager<ApplicationUser> userManager)
	{
		_signInManager = signInManager;
		_userManager = userManager;
	}

	// ---------------- START GOOGLE LOGIN ----------------
	[HttpGet("google-login")]
	public IActionResult GoogleLogin([FromQuery] string? returnUrl = "/")
	{
		var properties = _signInManager.ConfigureExternalAuthenticationProperties(
			"Google",
			Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl })
		);

		return Challenge(properties, "Google");
	}

	// ---------------- GOOGLE CALLBACK ----------------
	[HttpGet("google-callback")]
	public async Task<IActionResult> GoogleCallback(string? returnUrl = "/")
	{
		var info = await _signInManager.GetExternalLoginInfoAsync();
		if (info == null)
			return BadRequest("External login failed");

		var signInResult = await _signInManager.ExternalLoginSignInAsync(
			info.LoginProvider,
			info.ProviderKey,
			isPersistent: false,
			bypassTwoFactor: true
		);

		if (signInResult.Succeeded)
			return Redirect(returnUrl!);

		var email = info.Principal.FindFirstValue(ClaimTypes.Email);
		if (email == null)
			return BadRequest("Email not provided by Google");

		var user = await _userManager.FindByEmailAsync(email);
		if (user == null)
		{
			user = new ApplicationUser
			{
				UserName = email,
				Email = email
			};

			var createResult = await _userManager.CreateAsync(user);
			if (!createResult.Succeeded)
				return BadRequest("User creation failed");
		}

		await _userManager.AddLoginAsync(user, info);
		await _signInManager.SignInAsync(user, isPersistent: false);

		return Redirect(returnUrl!);
	}

	// ---------------- LOGOUT ----------------
	[HttpGet("logout")]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();
		return Ok();
	}

	// ---------------- ME ----------------
	[HttpGet("me")]
	public async Task<IActionResult> Me()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Unauthorized();

		return Ok(new
		{
			user.Id,
			user.Email,
			user.UserName
		});
	}
}
