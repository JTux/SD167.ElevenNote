using ElevenNote.Models.Responses;
using ElevenNote.Models.Token;
using ElevenNote.Models.User;
using ElevenNote.Services.Token;
using ElevenNote.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElevenNote.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ITokenService _tokenService;
    public UserController(IUserService service, ITokenService tokenService)
    {
        _service = service;
        _tokenService = tokenService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegister model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var registerResult = await _service.RegisterUserAsync(model);
        if (registerResult)
        {
            return Ok(new TextResponse("User was registered."));
        }

        return BadRequest(new TextResponse("User could not be registered"));
    }

    [Authorize]
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetById([FromRoute] int userId)
    {
        var userDetail = await _service.GetUserByIdAsync(userId);
        if (userDetail is null)
        {
            return NotFound();
        }

        return Ok(userDetail);
    }

    [HttpPost("~/api/Token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tokenResponse = await _tokenService.GetTokenAsync(request);
        if (tokenResponse is null)
            return BadRequest(new TextResponse("Invalid username or password."));

        return Ok(tokenResponse);
    }
}
