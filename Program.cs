
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Pharmacy_API.Context;
using Pharmacy_API.MapperProfiles.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Repositories.Brand;
using Pharmacy_API.Repositories.Category;
using Pharmacy_API.Repositories.Country;
using Pharmacy_API.Repositories.Unit;
using Pharmacy_API.Services.Account;
using Pharmacy_API.Services.Brand;
using Pharmacy_API.Services.Category;
using Pharmacy_API.Services.Country;
using Pharmacy_API.Services.Product;
using Pharmacy_API.Services.Unit;
using Pharmacy_API.Supports;
using Resend;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.StaticFiles;  // Thêm dòng này
using Pharmacy_API.Services.Redis;
using Microsoft.Extensions.Caching.Distributed;
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

            ValidAudience = jwtAudience,
            ValidIssuer = jwtIssuer,

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

//#region Email Config

//void AddEmailConfig(IServiceCollection services, IConfiguration configuration)
//{
//    services.AddScoped<ISmtpClient>(provider =>
//    {
//        var port =
//            configuration.GetValue<int>("MailSettings:Port", 587);

//        var smtpClient = new SmtpClient(
//            configuration["MailSettings:Host"],
//            port
//        )
//        {
//            UseDefaultCredentials = false,

//            Credentials = new NetworkCredential(
//                configuration["MailSettings:Mail"],
//                configuration["MailSettings:Password"]
//            ),

//            EnableSsl =
//                configuration.GetValue<bool>(
//                    "MailSettings:EnableSsl",
//                    true
//                )
//        };

//        return new SmtpClientWrapper(smtpClient);
//    });
//}

//AddEmailConfig(builder.Services, builder.Configuration);

//#endregion

#region Email Config - Brevo

// Đăng ký HttpClient và EmailSenderService
builder.Services.AddHttpClient<IEmailSenderService, EmailSenderService>();

#endregion


#region Seed Data Service

// ✅ Thêm SeedDataService
builder.Services.AddScoped<SeedDataService>();

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

builder.Services.AddHttpClient();  // ✅ Thêm HttpClient
builder.Services.AddSingleton<IDistributedCache>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
    return new RedisCacheService(httpClient, configuration, logger);
});

builder.Services.AddAutoMapper(
    typeof(AutoMapperProfile).Assembly
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

// Catalog
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();

#endregion

#region Services

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

#endregion

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

#region Middleware

//app.UseCors("AllowAll");

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseCors("AllowAll");

// Luôn bật Swagger cho cả Development và Production
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pharmacy API V1");
    c.RoutePrefix = "swagger";
});
app.UseStaticFiles();

// Cấu hình phục vụ file upload với xử lý thư mục không tồn tại
try
{
    var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    var uploadsPath = Path.Combine(wwwrootPath, "uploads");
    var brandsPath = Path.Combine(uploadsPath, "brands");

    // Tạo thư mục nếu chưa tồn tại
    if (!Directory.Exists(brandsPath))
    {
        Directory.CreateDirectory(brandsPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(brandsPath),
        RequestPath = "/uploads/brands",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800");
        }
    });
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not configure static files: {ex.Message}");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandler>();

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

#endregion

// ✅ Chạy Seed Data sau khi app khởi động
using (var seedScope = app.Services.CreateScope())
{
    var seedService = seedScope.ServiceProvider.GetRequiredService<SeedDataService>();
    await seedService.SeedAsync();
}


app.Run();

