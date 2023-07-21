using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.User;
using Microsoft.AspNetCore.Identity;

namespace ElevenNote.Services.User;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UserEntity> _userManager;

    public UserService(ApplicationDbContext context, UserManager<UserEntity> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> RegisterUserAsync(UserRegister model)
    {
        var userExists = await _userManager.FindByEmailAsync(model.Email) is not null ||
                         await _userManager.FindByNameAsync(model.UserName) is not null;

        if (userExists)
            return false;

        UserEntity entity = new()
        {
            Email = model.Email,
            UserName = model.UserName,
            DateCreated = DateTime.Now
        };

        var createResult = await _userManager.CreateAsync(entity, model.Password);
        return createResult.Succeeded;
    }

    public async Task<UserDetail?> GetUserByIdAsync(int userId)
    {
        var entity = await _context.Users.FindAsync(userId);
        if (entity is null)
            return null;

        UserDetail model = new()
        {
            Id = entity.Id,
            Email = entity.Email!,
            UserName = entity.UserName!,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateCreated = entity.DateCreated
        };

        return model;
    }
}
