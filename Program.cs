using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppSettings>(builder.Configuration);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//builder.Services.AddDbContext<AccountContext>(options =>
//    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<AccountContext>(options =>
    options.UseNpgsql(connectionString));
using var scope = builder.Services.BuildServiceProvider().CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AccountContext>();
dbContext.Database.Migrate();

// ✅ FIX CORS: AllowAll cho dev (điện thoại + Swagger đều dùng được)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration["AllowOrigins"]?.Split(",") ?? Array.Empty<string>();
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

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // ✅ Cho phép HTTP
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = jwtAudience,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Authentication failed: {Exception}", context.Exception);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Token validation failed. Error: {Error}, Description: {ErrorDescription}",
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddRoles<Role>()
.AddEntityFrameworkStores<AccountContext>()
.AddSignInManager()
.AddDefaultTokenProviders();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ho Ngoc Hung",
        Version = "v1",
        Description = "API Pharmacy"
    });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                },
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddScoped<System.Net.Mail.SmtpClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var smtpClient = new System.Net.Mail.SmtpClient(configuration["Smtp:Host"])
    {
        Port = int.Parse(configuration["Smtp:Port"] ?? "25"),
        Credentials = new System.Net.NetworkCredential(
            configuration["Smtp:Username"],
            configuration["Smtp:Password"]
        ),
        EnableSsl = bool.Parse(configuration["Smtp:EnableSsl"] ?? "false")
    };
    return smtpClient;
});

void AddEmailConfig(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<ISmtpClient, SmtpClientWrapper>((provider) =>
    {
        var port = configuration.GetValue<int>("MailSettings:Port", 587);
        var smtpClient = new SmtpClient(configuration["MailSettings:Host"], port)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(configuration["MailSettings:Mail"], configuration["MailSettings:Password"]),
            EnableSsl = configuration.GetValue<bool>("MailSettings:EnableSsl", true)
        };
        return new SmtpClientWrapper(smtpClient);
    });
}

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDistributedMemoryCache();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Repositories - Account
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPolicyPermissionRepository, PolicyPermissionRepository>();
builder.Services.AddScoped<IRolePolicyRepository, RolePolicyRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

// Repositories - Catalog
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();

// Services - Account
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtAuthManagerService, JwtAuthManagerService>();
builder.Services.AddScoped<IAuthManagerService, AuthManagerService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IUpdateUserService, UpdateUserService>();
builder.Services.AddScoped<ISmtpClient, SmtpClientWrapper>();
builder.Services.AddScoped<RoleManager<Role>>();

// Services - Catalog
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//if (builder.Environment.IsDevelopment())
//{
//    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
//}

AddEmailConfig(builder.Services, builder.Configuration);

var app = builder.Build();

// ✅ FIX: Dùng AllowAll cho dev để điện thoại gọi được
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// ✅ FIX: BỎ UseHttpsRedirection — đang dùng HTTP thuần, redirect này gây lỗi trên điện thoại
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandler>();
app.UseMiddleware<JwtMiddleware>();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();