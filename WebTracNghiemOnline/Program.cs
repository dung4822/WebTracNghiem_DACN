﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebTracNghiemOnline.Access;
using WebTracNghiemOnline.Configuration;
using WebTracNghiemOnline.Mappings;
using WebTracNghiemOnline.Models;
using WebTracNghiemOnline.Repository;
using WebTracNghiemOnline.Service;
using WebTracNghiemOnline.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http.Features;
using OfficeOpenXml;
using WebTracNghiemOnline.MoMo.Config;
using WebTracNghiemOnline.DTO;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Không phân biệt hoa/thường
    options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString; // Hỗ trợ đọc số từ chuỗi
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Đăng ký JwtConfig
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

// Lấy khóa bí mật từ cấu hình
var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
var key = Encoding.UTF8.GetBytes(jwtConfig.SecretKey);

builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("Momo"));
//để lây được đường dẫn truy cập tới server trực tiếp thì phải. -> đọc kĩ sau
builder.Services.AddHttpContextAccessor();


// Cấu hình Authentication
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
        };*/

// Đọc token từ header hoặc cookie
/*options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        // Lấy token từ header Authorization
       *//* if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
            }
        }*//*
       Console.WriteLine($"Token received: {context.Token}");
        // Fallback: Lấy token từ cookie nếu không có trong header
        if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.ContainsKey("jwt"))
        {
            context.Token = context.Request.Cookies["jwt"];
        }
        Console.WriteLine($"Token received: {context.Token}");
        return Task.CompletedTask;
    }
};*/

/*    });
*/

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt"];
            Console.WriteLine("ContextToken" + context.Token);
            return Task.CompletedTask;
        }
    };
}).AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    var googleAuth = builder.Configuration.GetSection("GoogleAuth");
    options.ClientId = googleAuth["ClientId"];
    options.ClientSecret = googleAuth["ClientSecret"];
    options.CallbackPath = googleAuth["CallbackPath"];
});



builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false; // Không yêu cầu phải có chữ số
    options.Password.RequiredLength = 6; // Độ dài tối thiểu là 6 ký tự
    options.Password.RequireNonAlphanumeric = false; // Không yêu cầu ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không yêu cầu chữ in hoa
    options.Password.RequireLowercase = false; // Không yêu cầu chữ thường
    options.Password.RequiredUniqueChars = 1; // Yêu cầu tối thiểu 1 ký tự khác nhau
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://localhost:5173") // Chỉ định nguồn được phép
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Cho phép gửi cookie
    });
});

// Configure EPPlus for reading Excel files
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Configure FormOptions for large file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // Allow files up to 50MB
});
//place add DI
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
/*builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();*/
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpClient<IMomoService, MomoService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IVNPAYService, VNPAYService>();
builder.Services.AddScoped<IOnlineRoomRepository, OnlineRoomRepository>();
builder.Services.AddScoped<IOnlineRoomService, OnlineRoomService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddSingleton<IEmailService>(sp =>
{
    var emailConfig = builder.Configuration.GetSection("EmailSettings");
    return new EmailService(
        smtpServer: emailConfig["SmtpServer"],
        port: int.Parse(emailConfig["Port"]),
        username: emailConfig["Username"],
        password: emailConfig["Password"]
    );
});




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication(); // Thêm dòng này
app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await CreateRoles(serviceProvider);
}

app.Run();

async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Các role mặc định
    string[] roles = { "User", "Admin" };
    foreach (var role in roles)
    {
        var roleExist = await roleManager.RoleExistsAsync(role);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Role '{role}' đã được tạo thành công.");
        }
    }
}
