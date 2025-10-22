using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using System.Security.Claims;

public class AuthServiceTests
{
    private readonly DbContextOptions<MediaBrowserDbContext> _dbOptions;
    private readonly IConfiguration _config;

    public AuthServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<MediaBrowserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var configValues = new Dictionary<string, string?>
        {
            {"AppSettings:Token", "XK7z9Hf2Qp8Ym3Vb5Wd6Jk1Lm4Nn7Pp0Qq3Rr6Ss9Tt2Uu5Vv8Ww1Xx4Yy7Zz0Aa3Bb6Cc9Dd2Ee5678=="},
            {"AppSettings:Issuer", "TestIssuer"},
            {"AppSettings:Audience", "TestAudience"}
        };

        _config = new ConfigurationBuilder()
        .AddInMemoryCollection(configValues)
        .Build();
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        //Arrange
        using var context = new MediaBrowserDbContext(_dbOptions);
        var passwordHasher = new PasswordHasher<User>();
        var user = new User { Username = "testificate" };
        user.PasswordHash = passwordHasher.HashPassword(user, "testpass123");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var authService = new AuthService(context, _config);
        var request = new UserDto
        {
            Username = "testificate",
            Password = "testpass123"
        };

        // Act
        var result = await authService.LoginAsync(request);
        var jwtHandler = new JwtSecurityTokenHandler();

        //Assert
        Assert.NotNull(result);
        Assert.True(!string.IsNullOrEmpty(result.AccessToken));
        Assert.True(!string.IsNullOrEmpty(result.RefreshToken));
        Assert.True(jwtHandler.CanReadToken(result.AccessToken));

        if (jwtHandler.CanReadToken(result.AccessToken))
        {
            var jwtToken = jwtHandler.ReadJwtToken(result.AccessToken); 

            Assert.Equal("testificate", jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier));
            Assert.Equal("TestIssuer", jwtToken.Issuer);
            Assert.Equal("TestAudience", jwtToken.Audiences.First());  
        }
    }
}