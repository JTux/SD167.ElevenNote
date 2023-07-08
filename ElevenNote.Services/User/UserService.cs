using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElevenNote.Services.User;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;

    public UserService(ApplicationDbContext context, UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> RegisterUserAsync(UserRegister model)
    {
        if (await GetUserByEmailAsync(model.Email) != null || await GetUserByUsernameAsync(model.Username) != null)
            return false;

        UserEntity entity = new()
        {
            Email = model.Email,
            Username = model.Username,
            DateCreated = DateTime.Now
        };

        var passwordHasher = new PasswordHasher<UserEntity>();
        entity.Password = passwordHasher.HashPassword(entity, model.Password);

        var createResult = await _userManager.CreateAsync(entity);
        return createResult.Succeeded;
    }

    public async Task<bool> LoginAsync(UserLogin model)
    {
        var userEntity = await _userManager.FindByNameAsync(model.Username);
        if (userEntity is null)
            return false;

        var passwordHasher = new PasswordHasher<UserEntity>();
        var verifyPasswordResult = passwordHasher.VerifyHashedPassword(userEntity, userEntity.Password, model.Password);

        if (verifyPasswordResult == PasswordVerificationResult.Failed)
            return false;

        await _signInManager.SignInAsync(userEntity, true);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<UserDetail?> GetUserByIdAsync(int userId)
    {
        var entity = await _context.Users.FindAsync(userId);
        if (entity is null)
            return null;

        UserDetail model = new()
        {
            Id = entity.Id,
            Email = entity.Email,
            Username = entity.Username,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateCreated = entity.DateCreated
        };

        return model;
    }

    private async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == email.ToLower());
    }
    private async Task<UserEntity?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Username.ToLower() == username.ToLower());
    }
}