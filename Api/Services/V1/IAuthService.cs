using System.ComponentModel;
using System.Reflection.Metadata;

public interface IAuthService
{
    Task<RegisterResponseDto?> RegisterAsync(UserDto request);
    Task<TokenResponseDto?> LoginAsync(UserDto request);
    Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
}