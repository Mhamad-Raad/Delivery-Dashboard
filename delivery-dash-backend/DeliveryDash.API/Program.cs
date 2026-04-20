using AspNetCoreRateLimit;
using FluentValidation;
using DeliveryDash.API.Extensions;
using DeliveryDash.API.Handlers;
using DeliveryDash.API.Hubs;
using DeliveryDash.API.Services;
using DeliveryDash.Application.Abstracts;
using DeliveryDash.Application.Abstracts.IRepository;
using DeliveryDash.Application.Abstracts.IService;
using DeliveryDash.Application.Options;
using DeliveryDash.Application.Services;
using DeliveryDash.Application.Validators;
using DeliveryDash.Domain.Entities;
using DeliveryDash.infrastructure.Options;
using DeliveryDash.infrastructure.Processors;
using DeliveryDash.Infrastructure;
using DeliveryDash.Infrastructure.Audit;
using DeliveryDash.Infrastructure.Data;
using DeliveryDash.Infrastructure.Repositories;
using DeliveryDash.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddValidatorsFromAssembly(typeof(IValidatorMarker).Assembly);

// Disable automatic ModelState validation to prevent ASP.NET from returning 400 before your handler
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true; // Important!
    }
);

builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.JwtOptionsKey));

builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DbConnectionString");
    options.UseNpgsql(connectionString);

    var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
    options.AddInterceptors(auditInterceptor);
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.JwtOptionsKey)
        .Get<JwtOptions>() ?? throw new ArgumentException(nameof(JwtOptions));

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
    };

    // SINGLE options.Events assignment with ALL handlers
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Try to get token from Authorization header first
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
            }
            // Fallback to cookie for backward compatibility
            else
            {
                context.Token = context.Request.Cookies["ACCESS_TOKEN"];
            }

            // For SignalR connections, also check query string
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Override the default 401 challenge response
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Unauthorized access. Please provide a valid authentication token.",
                statusCode = 401
            };

            return context.Response.WriteAsJsonAsync(errorResponse);
        },
        OnForbidden = context =>
        {
            // Override the default 403 forbidden response
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Access forbidden. You don't have permission to access this resource.",
                statusCode = 403
            };

            return context.Response.WriteAsJsonAsync(errorResponse);
        }
    };
});

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Add SignalR (keep this - used by both hubs)
builder.Services.AddSignalR();

// Processors
builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorCategoryRepository, VendorCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IVendorStaffRepository, VendorStaffRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IEntityImageRepository, EntityImageRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDriverShiftRepository, DriverShiftRepository>();
builder.Services.AddScoped<IOrderAssignmentRepository, OrderAssignmentRepository>();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IVendorCategoryService, VendorCategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IVendorStaffService, VendorStaffService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// options
builder.Services.Configure<DashboardCacheOptions>(
    builder.Configuration.GetSection(DashboardCacheOptions.SectionName));

// Notification services
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ===== Driver Dispatch Services =====
builder.Services.AddDriverDispatchServices(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
           .WithOrigins(
                "https://delivery-dashboard-sepia.vercel.app",
                "https://delivery-dashboard-v362.vercel.app",
                "https://localhost:5173",
                "http://localhost:5173",
                "https://localhost:5174",
                "http://localhost:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===== AspNetCoreRateLimit setup =====
builder.Services.AddMemoryCache();

// Bind configuration from appsettings.json
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(
    builder.Configuration.GetSection("IpRateLimitPolicies"));

// Add in-memory rate limiting store
builder.Services.AddInMemoryRateLimiting();

// Required by the library for resolving configuration
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add this with your other service configurations
builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection(FileStorageOptions.SectionName));

// Register the service
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// ===== Redis Configuration =====
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName));

var redisOptions = builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();

if (redisOptions != null && !string.IsNullOrEmpty(redisOptions.ConnectionString))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse(redisOptions.ConnectionString);
        configuration.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
        configuration.ConnectTimeout = redisOptions.ConnectTimeout;
        configuration.SyncTimeout = redisOptions.SyncTimeout;

        return ConnectionMultiplexer.Connect(configuration);
    });

    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}

var app = builder.Build();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.Saturn)
        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    });
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();

            // Apply migrations
            await context.Database.MigrateAsync();

            // Seed the database
            var initializer = new DatabaseInitializer(context, userManager);
            await initializer.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

if (app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.Saturn)
        .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    });
}

app.UseExceptionHandler(options => { }); // Empty options = use registered IExceptionHandler

// Trust X-Forwarded-* from reverse proxy (Render/Fly/etc.) so cookies set Secure correctly
// and ASP.NET Core sees the real scheme and client IP.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

app.UseCors();

// Inside a container the platform terminates TLS; redirecting to HTTPS here breaks health checks.
var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.UseIpRateLimiting();

app.MapControllers();

// Map SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notifications");  // for general notifications
app.MapHub<DispatchHub>("/hubs/dispatch");           // for driver order dispatching

// Configure static files - this is OK in the API layer
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

app.Run();