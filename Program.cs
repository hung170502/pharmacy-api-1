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
using Pharmacy_API.Services;
using Supabase;
using Supabase.Core;
using Supabase.Interfaces;
using Pharmacy_API.Services.Order;

var builder = WebApplication.CreateBuilder(args);

// ✅ QUAN TRỌNG: Load configuration từ Environment Variables
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<AppSettings>(builder.Configuration);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=db.wnvtlloluziuvjmxbkmm.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=NKcZLRhaLTzLXEjt;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Maximum Pool Size=10;";

builder.Services.AddDbContext<AccountContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(60);
    }));

// ✅ CHỈNH SỬA: Bỏ Migrate ở đây vì chưa có scope hợp lệ
// ❌ KHÔNG DÙNG: using var scope = builder.Services.BuildServiceProvider().CreateScope();

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins =
            builder.Configuration["AllowOrigins"]?.Split(",")
            ?? new[] { "https://pharmacy-api.onrender.com", "http://localhost:8081", "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods("POST", "GET", "PUT", "DELETE", "OPTIONS")
              .AllowAnyHeader()
              .AllowCredentials();
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

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? "PharmacyAPI2024SuperSecretKeyAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? "https://pharmacy-api.onrender.com";
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? "https://pharmacy-api.onrender.com";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
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
            "https://pharmacy-api.onrender.com"
        },
        ValidAudiences = new[]
        {
            jwtAudience,
            "http://localhost:5188",
            "http://127.0.0.1:5188",
            "http://192.168.1.5:5188",
            "https://localhost:5001",
            "https://pharmacy-api.onrender.com"
        },

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        ),

        ClockSkew = TimeSpan.FromMinutes(5)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            logger.LogError("Authentication failed: {Exception}", context.Exception);
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices
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
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<Role>()
.AddEntityFrameworkStores<AccountContext>()
.AddSignInManager()
.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(
    builder.Configuration["Jwt:AppName"] ?? "Pharmacy"
);

#endregion

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pharmacy API",
        Version = "v1",
        Description = "API Pharmacy - Deployed on Render"
    });

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.
                Enter 'Bearer' [space] and then your token below.",
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
                }
            },
            new List<string>()
        }
    });
});

#endregion

#region Email Config

builder.Services.AddHttpClient<IEmailSenderService, EmailSenderService>();

#endregion

#region Seed Data Service

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
builder.Services.AddHttpClient();

// ✅ FIX: Cấu hình Redis với điều kiện
var upstashUrl = builder.Configuration["Upstash:RestUrl"];
if (!string.IsNullOrEmpty(upstashUrl))
{
    builder.Services.AddSingleton<IDistributedCache>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        var configuration = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();

        httpClient.BaseAddress = new Uri(upstashUrl);

        return new RedisCacheService(httpClient, configuration, logger);
    });
}
else
{
    // Fallback: Dùng Memory Cache nếu không có Redis
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<IDistributedCache, MemoryDistributedCache>();
}

// AutoMapper
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
builder.Services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();

// Catalog
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();

// Q&A Repositories
builder.Services.AddScoped<IQARepository, QARepository>();

#endregion

#region Services

// Supabase Client
builder.Services.AddSingleton<Supabase.Client>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var supabaseUrl = configuration["Supabase:Url"]
        ?? "https://wnvtlloluziuvjmxbkmm.supabase.co";
    var supabaseKey = configuration["Supabase:Key"]
        ?? "sb_publishable_LXbs7Y2KB2bfCuGWqQjU2w_cFr7RvyR";

    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
    };

    return new Supabase.Client(supabaseUrl, supabaseKey, options);
});

// Account Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IJwtAuthManagerService, JwtAuthManagerService>();
builder.Services.AddScoped<IAuthManagerService, AuthManagerService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IUpdateUserService, UpdateUserService>();

// Catalog Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Q&A Services
builder.Services.AddScoped<IQAService, QAService>();

// Cloudinary Service
builder.Services.AddScoped<CloudinaryService>();

// Order Services
builder.Services.AddScoped<IOrderService, OrderService>();

#endregion

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());

        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

#region Middleware

// ✅ FIX: Chọn CORS policy phù hợp
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowSpecificOrigins");
}

// ✅ FIX: Swagger chỉ bật trong Development hoặc luôn bật
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Vẫn bật Swagger trên Production để test
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pharmacy API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ✅ THÊM: Xử lý lỗi
app.UseMiddleware<ExceptionHandler>();
app.UseMiddleware<JwtMiddleware>();

// ✅ THÊM: Health Check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

#endregion

// ✅ FIX: Chạy Migrate và Seed Data sau khi app đã build
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AccountContext>();

        // Chạy migration
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Database migration completed successfully!");

        // Seed data
        var seedService = services.GetRequiredService<SeedDataService>();
        await seedService.SeedAsync();
        Console.WriteLine("✅ Seed data completed successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during migration/seed: {ex.Message}");
        // Không throw exception để app vẫn chạy
    }
}

app.Run();