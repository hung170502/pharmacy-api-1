using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using Pharmacy_API.Context;
using Pharmacy_API.MapperProfiles.Account;
using Pharmacy_API.MapperProfiles.Question;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Repositories.Brand;
using Pharmacy_API.Repositories.Category;
using Pharmacy_API.Repositories.Country;
using Pharmacy_API.Repositories.Unit;
using Pharmacy_API.Repositories.Question;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Services.Brand;
using Pharmacy_API.Services.Category;
using Pharmacy_API.Services.Country;
using Pharmacy_API.Services.Product;
using Pharmacy_API.Services.Unit;
using Pharmacy_API.Services.Question;
using Pharmacy_API.Services.Redis;
using Pharmacy_API.Supports;
using Microsoft.Extensions.Caching.Distributed;
using Pharmacy_API.Repositories;
using Pharmacy_API.Services; // THÊM: để dùng CloudinaryService

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<AppSettings>(builder.Configuration);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AccountContext>(options =>
    options.UseNpgsql(connectionString));


using var scope = builder.Services.BuildServiceProvider().CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AccountContext>();
dbContext.Database.Migrate();

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins =
            builder.Configuration["AllowOrigins"]?.Split(",")
            ?? Array.Empty<string>();

        policy.WithOrigins(allowedOrigins)
              .WithMethods("POST", "GET", "PUT", "DELETE")
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#endregion

#region JWT

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;

    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuers = new[]
            {
                jwtIssuer,
                "http://localhost:5188",
                "http://127.0.0.1:5188",
                "http://192.168.1.5:5188",
                "https://localhost:5001",
            },
            ValidAudiences = new[]
            {
                jwtAudience,
                "http://localhost:5188",
                "http://127.0.0.1:5188",
                "http://192.168.1.5:5188",
                "https://localhost:5001",
            },

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey ?? string.Empty)
                ),

            ClockSkew = TimeSpan.FromMinutes(5)
        };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger =
                context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            logger.LogError(
                "Authentication failed: {Exception}",
                context.Exception
            );

            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            var logger =
                context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            logger.LogWarning(
                "Token validation failed. Error: {Error}, Description: {ErrorDescription}",
                context.Error,
                context.ErrorDescription
            );

            return Task.CompletedTask;
        }
    };
});

#endregion

#region Identity

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<Role>()
.AddEntityFrameworkStores<AccountContext>()
.AddSignInManager()
.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(
    builder.Configuration["Jwt:AppName"] ?? string.Empty
);

#endregion

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pharmacy API",
        Version = "v1",
        Description = "API Pharmacy"
    });

    options.AddSecurityDefinition(
        JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Description =
                @"JWT Authorization header using the Bearer scheme.
                Enter 'Bearer' [space] and then your token below.",

            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = JwtBearerDefaults.AuthenticationScheme
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                new List<string>()
            }
        });
});

#endregion

#region Email Config - Brevo

builder.Services.AddHttpClient<IEmailSenderService, EmailSenderService>();

#endregion



#region Compression

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
});

#endregion

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IDistributedCache>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();

    httpClient.BaseAddress = new Uri(configuration["Upstash:RestUrl"] ?? "https://honest-bat-74659.upstash.io");

    return new RedisCacheService(httpClient, configuration, logger);
});

// AutoMapper - Thêm QuestionProfile
builder.Services.AddAutoMapper(
    typeof(AutoMapperProfile).Assembly,
    typeof(QuestionProfile).Assembly
);

#region Repositories

// Account
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPolicyPermissionRepository, PolicyPermissionRepository>();
builder.Services.AddScoped<IRolePolicyRepository, RolePolicyRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

// Phone Verification
builder.Services.AddScoped<IPhoneVerificationService, PhoneVerificationService>();
builder.Services.AddScoped<IPhoneOtpService, PhoneOtpService>();

// Catalog
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();

// Q&A Repositories
builder.Services.AddScoped<IQARepository, QARepository>();




// Account
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtAuthManagerService, JwtAuthManagerService>();
builder.Services.AddScoped<IAuthManagerService, AuthManagerService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IUpdateUserService, UpdateUserService>();

// Catalog
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Q&A Services
builder.Services.AddScoped<IQAService, QAService>();

// Cloudinary Service - THÊM VÀO ĐÂY
builder.Services.AddScoped<CloudinaryService>();

#endregion

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());

        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

#region Middleware

app.UseCors("AllowAll");

// Luôn bật Swagger cho cả Development và Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pharmacy API V1");
    c.RoutePrefix = "swagger";
});

// Vì giờ ảnh đã được lưu trên Cloudinary, không cần local nữa
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandler>();

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

#endregion


app.Run();