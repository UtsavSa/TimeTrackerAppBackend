//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using System.Text;
//using TimeTrackerApi.Data;
//using TimeTrackerApi.Models;
//using TimeTrackerApi.Services;
//using Microsoft.AspNetCore.HttpOverrides;

//var builder = WebApplication.CreateBuilder(args);
//var env = builder.Environment;

//// ============================
////        Service Setup
//// ============================

//// Add EF Core with SQLite

//if (env.IsProduction())
//{
//    var pg = builder.Configuration.GetConnectionString("DefaultConnection")
//        ?? throw new InvalidOperationException("Missing Postgres connection string. ");
//    builder.Services.AddDbContext<TimeTrackerContext>(options => options.UseNpgsql(pg));

//}
//else
//{


//    var sqlite = builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=timetracker.db";
//    builder.Services.AddDbContext<TimeTrackerContext>(opt => opt.UseSqlite(sqlite));
//}




//// Add Identity for user auth
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<TimeTrackerContext>()
//    .AddDefaultTokenProviders();

//// Bind JwtSettings from config
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
//var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
//var jwtAudience = builder.Configuration["JwtSettings:Audience"];
////var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

//var jwtSecret = builder.Configuration["JwtSettings:Secret"];

//if (string.IsNullOrWhiteSpace(jwtSecret))
//    throw new InvalidOperationException("Missing JwtSettings:Secret. In Production, set In Production, set env var JwtSettings__Secret.");
////var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
//var key = Encoding.UTF8.GetBytes(jwtSecret);

//// ✅ Add HttpContextAccessor so you can get User from HttpContext in services
//builder.Services.AddHttpContextAccessor();

//// Add JWT Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = true;
//    options.SaveToken = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtIssuer,
//        ValidAudience = jwtAudience,
//        IssuerSigningKey = new SymmetricSecurityKey(key)
//    };
//});

//// Register custom services
//builder.Services.AddScoped<TimeEntryService>();

//// Add CORS for frontend
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("ui", policy =>
//    {
//       var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
//        policy.WithOrigins(allowed)
//              .AllowAnyMethod()
//              .AllowAnyHeader()
//              .AllowCredentials();
//    });
//});

//// Add Controllers + Swagger
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
////builder.Services.AddSwaggerGen();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeTracker API", Version = "v1" });

//    // 🔐 Add JWT auth to Swagger
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer {token}' (without quotes). Example: Bearer abcdef12345"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});

//// ============================
////        Build & Run App
//// ============================


//var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,

//});

//app.UseHttpsRedirection();

//app.UseCors("ui");

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapGet("/healthz", () => Results.Ok("ok"));

//// Serve Angular static files
////app.UseDefaultFiles();
////app.UseStaticFiles();

//// 🔁 Fallback routing for Angular (MUST be before MapControllers)
////app.Use(async (context, next) =>
////{
////    await next();

////    if (context.Response.StatusCode == 404 &&
////        !Path.HasExtension(context.Request.Path.Value) &&
////        !context.Request.Path.Value.StartsWith("/api"))
////    {
////        context.Request.Path = "/index.html";
////        await next();
////    }
////});

//app.MapControllers();


//if (env.IsProduction())
//{
//    using var scope = app.Services.CreateScope();
//    var db = scope.ServiceProvider.GetRequiredService<TimeTrackerContext>();
//    db.Database.Migrate();
//}
//app.Run();


//-----------------------------


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TimeTrackerApi.Data;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Optional but helpful if you deal with DateTime kinds
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ============================
//        Data / EF
// ============================
if (env.IsProduction())
{
    // Try both config connection string and env var fallback
    var pg = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
             ?? throw new InvalidOperationException("Missing Postgres connection string (ConnectionStrings__DefaultConnection).");

    builder.Services.AddDbContext<TimeTrackerContext>(opt => opt.UseNpgsql(pg));
}
else
{
    var sqlite = builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=timetracker.db";
    builder.Services.AddDbContext<TimeTrackerContext>(opt => opt.UseSqlite(sqlite));
}

// ============================
//       Identity + JWT
// ============================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<TimeTrackerContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSecret = builder.Configuration["JwtSettings:Secret"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("Missing JwtSettings:Secret. In Production, set env var JwtSettings__Secret.");

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
var validateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer);
var validateAudience = !string.IsNullOrWhiteSpace(jwtAudience);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,

        ValidateIssuer = validateIssuer,
        ValidIssuer = jwtIssuer,

        ValidateAudience = validateAudience,
        ValidAudience = jwtAudience,

        ValidateLifetime = true
    };
});

builder.Services.AddHttpContextAccessor();

// ============================
//            CORS
// ============================
// If Cors:AllowedOrigins is provided (comma-separated), we’ll use it.
// Otherwise we allow timetrackerapp.pages.dev and its preview subdomains.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
    {
        var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (allowed != null && allowed.Length > 0)
        {
            policy.WithOrigins(allowed)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
            // .AllowCredentials(); // only if you use cookies
        }
        else
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                var host = new Uri(origin).Host;
                return host == "timetrackerapp.pages.dev" ||
                       host.EndsWith(".timetrackerapp.pages.dev");
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
            // .AllowCredentials(); // only if you use cookies
        }
    });
});

// ============================
//    Controllers + Swagger
// ============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeTracker API", Version = "v1" });

    // JWT in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Custom services
builder.Services.AddScoped<TimeEntryService>();

// ============================
//         Build & Run
// ============================
var app = builder.Build();

// Swagger in prod is fine for now
app.UseSwagger();
app.UseSwaggerUI();

// Trust Render’s proxy headers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    // Accept any proxy (Render). If you want to lock this down later, configure KnownProxies/Networks.
    KnownNetworks = { },
    KnownProxies = { }
});

app.UseHttpsRedirection();
app.UseCors("ui");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok("ok"));
app.MapControllers();

// Auto-migrate in Production
if (env.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TimeTrackerContext>();
    db.Database.Migrate();
}

app.Run();
