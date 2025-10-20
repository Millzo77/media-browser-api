using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// 1. Core API services
builder.Services.AddControllers();
builder.Services.AddRouting(options => { options.LowercaseUrls = true; });

// 2. API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // Returns supported versions in response headers

    // Choose your versioning strategy (pick one or combine):
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),           // /api/v1/products
        new HeaderApiVersionReader("X-Api-Version"), // Header: X-Api-Version: 1.0
        new QueryStringApiVersionReader("api-version") // ?api-version=1.0
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Format: v1, v2, v3
    options.SubstituteApiVersionInUrl = true;
});

// 4. Database
builder.Services.AddDbContext<MediaBrowserDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MediaBrowserDB"),
    new MySqlServerVersion(new Version(8, 0, 43)))
);

// 5. Authentication/Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });

// 6. Services/Dependencies
builder.Services.AddScoped<IAuthService, AuthService>();

// 7. Http Clients
builder.Services.AddHttpClient<MovieService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// app.UseCors("AllowFrontend"); // Apply CORS policy

// Optional: Enable CORS if your Next.js frontend is on a different domain
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend",
//         policy => policy.WithOrigins("http://localhost:3000") // Replace with your Next.js URL
//                         .AllowAnyHeader()
//                         .AllowAnyMethod());
// });