using backend.Data;
using backend.Models;
using backend.Repositories;
using backend.Repositories.Interfaces;
using backend.Services;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Đăng Ký DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DBConnection")));

// Đăng ký sử dụng interface
//builder.Services.AddScoped<IFruitRepository, FruitRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IFoodService, FoodService>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();


builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<ISystemCategoryRepository, SystemCategoryRepository>();
builder.Services.AddScoped<ISystemCategoryService, SystemCategoryService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();


builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddHttpContextAccessor(); 

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập nguyên văn chuỗi Token của bạn vào đây (Không gõ chữ Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer", // 🛠️ SỬA THÀNH CHỮ "Bearer" VIẾT HOA CHUẨN
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Thêm Authentication & JWT
var key = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Roles.Any())
    {
        context.Roles.AddRange(
            new Role { Name = "Admin" },
            new Role { Name = "Owner" },
            new Role { Name = "Customer" }
        );

        context.SaveChanges();
    }
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User
            {
                FullName = "Admin",
                Email = "admin@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true
            },
            new User
            {
                FullName = "Restaurant Owner",
                Email = "owner@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true
            },
            new User
            {
                FullName = "Customer",
                Email = "customer@gmail.com",
                Password = BCrypt.Net.BCrypt.HashPassword("123456"),
                IsActive = true
            }
        );

        context.SaveChanges();
    }
    if (!context.UserRoles.Any())
    {
        var admin = context.Users.First(x => x.Email == "admin@gmail.com");
        var owner = context.Users.First(x => x.Email == "owner@gmail.com");
        var customer = context.Users.First(x => x.Email == "customer@gmail.com");

        var adminRole = context.Roles.First(x => x.Name == "Admin");
        var ownerRole = context.Roles.First(x => x.Name == "Owner");
        var customerRole = context.Roles.First(x => x.Name == "Customer");

        context.UserRoles.AddRange(
            new UserRole
            {
                UserId = admin.Id,
                RoleId = adminRole.Id
            },
            new UserRole
            {
                UserId = owner.Id,
                RoleId = ownerRole.Id
            },
            new UserRole
            {
                UserId = customer.Id,
                RoleId = customerRole.Id
            }
        );

        context.SaveChanges();
    }
    if (!context.Restaurants.Any())
    {
        var owner = context.Users.First(x => x.Email == "owner@gmail.com");

        context.Restaurants.AddRange(
            new Restaurant
            {
                Name = "Pizza House",
                Address = "Thu Dau Mot",
                Description = "Italian Pizza",
                OwnerId = owner.Id,
                IsActive = true
            },
            new Restaurant
            {
                Name = "KFC",
                Address = "Binh Duong",
                Description = "Fast Food",
                OwnerId = owner.Id,
                IsActive = true
            }
        );

        context.SaveChanges();
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();
app.Run();
