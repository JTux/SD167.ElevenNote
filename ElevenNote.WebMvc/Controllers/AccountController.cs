using System.Security.Claims;
using ElevenNote.Models.User;
using ElevenNote.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElevenNote.WebMvc.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AccountController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLogin model, [FromQuery] string? returnUrl)
    {
        var result = await _userService.LoginAsync(model);
        if (result == false)
        {
            return View(model);
        }

        Console.WriteLine(returnUrl);
        if (returnUrl is not null)
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserRegister model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var registerResult = await _userService.RegisterUserAsync(model);
        if (registerResult)
        {
            var loginModel = new UserLogin()
            {
                Username = model.Username,
                Password = model.Password
            };

            var loginResult = await _userService.LoginAsync(loginModel);
            if (loginResult)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _userService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var identifierClaimType = _configuration["ClaimTypes:Id"] ?? "Id";
        var requestingUser = User.Identity as ClaimsIdentity;
        var idClaim = requestingUser?.FindFirst(identifierClaimType);
        var validId = int.TryParse(idClaim?.Value, out int id);
        if (validId)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return View(user);
        }

        return RedirectToAction("Index", "Home");
    }
}