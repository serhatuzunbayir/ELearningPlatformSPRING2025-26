using System.Text;
using LearningPlatform.API.Data;
using LearningPlatform.API.Models;
using LearningPlatform.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var dbConnectionString = DatabasePathHelper.BuildConnectionString(builder.Environment.ContentRootPath);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(dbConnectionString));

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
builder.Services.AddSingleton<CourseRecommendationService>();

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
            Role = UserRole.Admin,
            TwoFactorEnabled = true
        });
        db.SaveChanges();
    }
    else
    {
        var admin = db.Users.First(u => u.Role == UserRole.Admin);
        if (!admin.TwoFactorEnabled)
        {
            admin.TwoFactorEnabled = true;
            db.SaveChanges();
        }
    }

    var sampleCourse = db.Courses.FirstOrDefault(c => c.Title == "Müzik ve Bilgisayarlar");
    if (sampleCourse is null)
    {
        sampleCourse = new Course
        {
            Title = "Müzik ve Bilgisayarlar",
            Description = "Bilgisayar destekli müzik üretimi ve algoritmik kompozisyon.",
            Category = "Programlama",
            Difficulty = DifficultyLevel.Intermediate,
            EctsCredit = 5
        };
        db.Courses.Add(sampleCourse);
        db.SaveChanges();
    }

    if (!db.CourseModules.Any(m => m.CourseId == sampleCourse.Id))
    {
        db.CourseModules.AddRange(
            new CourseModule
            {
                CourseId = sampleCourse.Id,
                Title = "Introduction to Digital Audio",
                Content = "Samples, waveforms, and basic synthesis concepts.",
                Order = 1
            },
            new CourseModule
            {
                CourseId = sampleCourse.Id,
                Title = "MIDI and Sequencing",
                Content = "MIDI protocol, sequencing, and DAW workflow.",
                Order = 2
            },
            new CourseModule
            {
                CourseId = sampleCourse.Id,
                Title = "Algorithmic Composition",
                Content = "Rule-based and generative composition techniques.",
                Order = 3
            });
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
