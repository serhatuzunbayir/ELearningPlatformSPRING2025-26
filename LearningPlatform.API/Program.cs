using System.Text;
using LearningPlatform.API.Data;
using LearningPlatform.API.Models;
using LearningPlatform.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<EnrollmentService>();
builder.Services.AddSingleton<StudyPlanService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Role == UserRole.Admin))
    {
        db.Users.Add(new User
        {
            Name = "Admin",
            Email = "admin@platform.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
            Role = UserRole.Admin
        });
        db.SaveChanges();
    }

    if (!db.Courses.Any(c => c.Title == "Müzik ve Bilgisayarlar"))
    {
        db.Courses.Add(new Course
        {
            Title = "Müzik ve Bilgisayarlar",
            Description = "Bilgisayar destekli müzik üretimi ve algoritmik kompozisyon.",
            Category = "Programlama",
            Difficulty = DifficultyLevel.Intermediate,
            EctsCredit = 5
        });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
