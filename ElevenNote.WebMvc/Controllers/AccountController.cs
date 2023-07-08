using ElevenNote.Models.User;
using ElevenNote.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace ElevenNote.WebMvc.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLogin model)
    {
        var result = await _userService.LoginAsync(model);
        if (result == false)
        {
            return View(model);
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
}