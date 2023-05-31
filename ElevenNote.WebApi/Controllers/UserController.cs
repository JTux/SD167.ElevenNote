using ElevenNote.Models.Responses;
using ElevenNote.Models.User;
using ElevenNote.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace ElevenNote.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    public UserController(IUserService service)
    {
        _service = service;
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
}
