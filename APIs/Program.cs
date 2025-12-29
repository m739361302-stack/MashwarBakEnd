using Application.Interfaces;
using Application.Services;
using Application.Services.Security;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<MashwarDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireClaim("userType", ((byte)UserType.Admin).ToString()));

    // لاحقًا نستخدمها لصفحات السائق
    options.AddPolicy(Policies.DriverApproved, policy =>
    {
        policy.RequireClaim("userType", ((byte)UserType.Driver).ToString());
        policy.RequireClaim("approvalStatus", ((byte)ApprovalStatus.Approved).ToString());
        policy.RequireClaim("isActive", "1");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}
    app.UseAuthorization();

app.MapControllers();

app.Run();
