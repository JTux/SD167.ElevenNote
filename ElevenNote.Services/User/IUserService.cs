using ElevenNote.Models.User;

namespace ElevenNote.Services.User;

public interface IUserService
{
    Task<bool> LoginAsync(UserLogin model);
    Task LogoutAsync();
    Task<bool> RegisterUserAsync(UserRegister model);
    Task<UserDetail?> GetUserByIdAsync(int userId);
}