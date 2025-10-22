using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public abstract class AuthServiceTestBase : IDisposable
{
    protected readonly DbContextOptions<MediaBrowserDbContext> _dbOptions;
    protected readonly IConfiguration _config;
    protected readonly AuthService _authService;
    protected readonly User _testUser;
    protected readonly string _testUsername;
    protected readonly string _testPassword;
    protected readonly MediaBrowserDbContext _context;
    protected readonly PasswordHasher<User> _passwordHasher;

    protected AuthServiceTestBase()
    {
        _dbOptions = new DbContextOptionsBuilder<MediaBrowserDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new MediaBrowserDbContext(_dbOptions);

        var configValues = new Dictionary<string, string?>
        {
            {"AppSettings:Token", "XK7z9Hf2Qp8Ym3Vb5Wd6Jk1Lm4Nn7Pp0Qq3Rr6Ss9Tt2Uu5Vv8Ww1Xx4Yy7Zz0Aa3Bb6Cc9Dd2Ee5678=="},
            {"AppSettings:Issuer", "TestIssuer"},
            {"AppSettings:Audience", "TestAudience"}
        };

        _config = new ConfigurationBuilder()
        .AddInMemoryCollection(configValues)
        .Build();

        _passwordHasher = new PasswordHasher<User>();
        
        // Setting up a default existing user for the tests
        _testUsername = "username";
        _testPassword = "password";
        _testUser = new User();

        SeedUser();
        
        _authService = new AuthService(_context, _config);
    }

    private async void SeedUser()
    {
        _testUser.Username = _testUsername;
        _testUser.PasswordHash = _passwordHasher.HashPassword(_testUser, _testPassword);
        _context.Users.Add(_testUser);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public class LoginAsyncTests : AuthServiceTestBase
    {
        [Trait("Category", "LoginAsync")]
        [Fact]
        public async Task ReturnsToken_WhenCredentialsAreValid()
        {
            //Arrange
            var request = new UserDto
            {
                Username = _testUsername,
                Password = _testPassword
            };

            // Act
            var result = await _authService.LoginAsync(request);
            var jwtHandler = new JwtSecurityTokenHandler();

            //Assert
            Assert.NotNull(result);
            Assert.True(!string.IsNullOrEmpty(result.AccessToken));
            Assert.True(!string.IsNullOrEmpty(result.RefreshToken));
            Assert.True(jwtHandler.CanReadToken(result.AccessToken));

            if (jwtHandler.CanReadToken(result.AccessToken))
            {
                var jwtToken = jwtHandler.ReadJwtToken(result.AccessToken);

                Assert.Equal(_testUser.Username, jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
                Assert.NotNull(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier));
                Assert.Equal("TestIssuer", jwtToken.Issuer);
                Assert.Equal("TestAudience", jwtToken.Audiences.First());
            }
        }

        [Trait("Category", "LoginAsync")]
        [Fact]
        public async Task ReturnsNull_WhenUsernameIsNotValid()
        {
            //Arrange
            var request = new UserDto
            {
                Username = "wrongusername",
                Password = _testPassword
            };

            // Act
            var result = await _authService.LoginAsync(request);

            //Assert
            Assert.Null(result);
        }

        [Trait("Category", "LoginAsync")]
        [Fact]
        public async Task ReturnsNull_WhenPasswordIsNotValid()
        {
            //Arrange
            var request = new UserDto
            {
                Username = _testUsername,
                Password = "wrongpassword"
            };

            // Act
            var result = await _authService.LoginAsync(request);

            //Assert
            Assert.Null(result);
        }
    }

    public class RegisterAsyncTests : AuthServiceTestBase
    {
        [Trait("Category", "RegisterAsync")]
        [Fact]
        public async Task ReturnsValidRegisterResponseDto_WhenInputsAreValid()
        {
            //Arrange
            var request = new UserDto
            {
                Username = "registerusername",
                Password = _testPassword
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(request.Username, result?.Username);
            Assert.NotEqual(DateTime.MinValue, result?.CreatedOn);
        }

        [Trait("Category", "RegisterAsync")]
        [Fact]
        public async Task ReturnsNull_WhenUserAlreadyExists()
        {
            //Arrange
            var request = new UserDto
            {
                Username = _testUsername,
                Password = _testPassword
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            Assert.Null(result);
        }

        [Trait("Category", "RegisterAsync")]
        [Theory]
        [InlineData("username", "")]
        [InlineData("", "password")]
        [InlineData("", "")]
        public async Task ReturnsNull_WhenOneOrMoreInputsAreEmpty(
            string username, string password)
        {
            //Arrange
            var request = new UserDto
            {
                Username = username,
                Password = password
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            //Assert
            Assert.Null(result);
        }
    }
}