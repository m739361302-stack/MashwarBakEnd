using Application.Interfaces;
using Application.Interfaces.Admin;
using Application.Interfaces.IBooking;
using Application.Interfaces.ICity;
using Application.Interfaces.ICustomer;
using Application.Interfaces.IDriver;
using Application.Interfaces.TripsSearch;
using Application.Services;
using Application.Services.Admin;
using Application.Services.BookingsService;
using Application.Services.CityS;
using Application.Services.CustomerService;
using Application.Services.DriverService;
using Application.Services.helper;
using Application.Services.Security;
using Application.Services.TripsSearch;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins("http://localhost:8083")   // فرونت
              .AllowAnyHeader()
              .AllowAnyMethod()
    // .AllowCredentials() // فعّلها فقط إذا تستخدم Cookies
    );
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddDbContext<MashwarDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// +++ ADD: JWT Authentication (Bearer)
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt section is missing in appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };
    });


// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICityRouteService, CityRouteService>();

builder.Services.AddScoped<IAdminDriverApprovalsService, AdminDriverApprovalsService>();

builder.Services.AddScoped<IDriverAvailabilityService, DriverAvailabilityService>();
builder.Services.AddScoped<IDriverTripsService, DriverTripsService>();

builder.Services.AddScoped<ICustomerBookingsService, CustomerBookingsService>();
builder.Services.AddScoped<IDriverBookingsService, DriverBookingsService>();

builder.Services.AddScoped<ITripsSearchService, TripsSearchService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();

builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

builder.Services.AddScoped<AdminTripsService>();
builder.Services.AddScoped<AdminBookingsService>();
builder.Services.AddScoped<AdminReportsService>();

builder.Services.AddScoped<AdminDashboardService>();
builder.Services.AddScoped<AdminLayoutService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();


// Swagger
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
// +++ REPLACE AddSwaggerGen() with JWT support
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mashwar API", Version = "v1" });

//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter: Bearer {token}"
//    });

//    // ✅ net10: AddSecurityRequirement يتوقع Func<OpenApiDocument, OpenApiSecurityRequirement>
//    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
//    {
//        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
//    });
//});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mashwar API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste ONLY the token (no Bearer)"
    });

    // ✅ .NET 10 / OpenAPI 2.x: لازم تربطها بالـ document
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
    });
});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy(Policies.AdminOnly, policy =>
//        policy.RequireClaim("userType", ((byte)UserType.Admin).ToString()));

//    // لاحقًا نستخدمها لصفحات السائق
//    options.AddPolicy(Policies.DriverApproved, policy =>
//    {
//        policy.RequireClaim("userType", ((byte)UserType.Driver).ToString());
//        policy.RequireClaim("approvalStatus", ((byte)ApprovalStatus.Approved).ToString());
//        policy.RequireClaim("isActive", "1");
//    });
//});
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("ActiveUser", p =>
        p.RequireAssertion(ctx =>
            (ctx.User.FindFirstValue(AppClaims.IsActive) ?? "0") == "1"));

    opt.AddPolicy("AdminOnly", p =>
        p.RequireRole("Admin"));

    opt.AddPolicy("DriverApprovedOnly", p =>
        p.RequireAssertion(ctx =>
        {
            var userType = ctx.User.FindFirstValue(AppClaims.UserType);
            var approval = ctx.User.FindFirstValue(AppClaims.ApprovalStatus);
            return userType == ((byte)UserType.Driver).ToString()
                   && approval == ((byte)ApprovalStatus.Approved).ToString();
        }));
});



var app = builder.Build();

app.UseStaticFiles(); // wwwroot

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseCors("DevCors");
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
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
