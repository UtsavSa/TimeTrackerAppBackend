using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TimeTrackerApi.Data;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// ============================
//        Service Setup
// ============================

// Add EF Core with SQLite

if (env.IsProduction())
{
    var pg = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Missing Postgres connection string. ");
    builder.Services.AddDbContext<TimeTrackerContext>(options => options.UseNpgsql(pg));

}
else
{
   

    var sqlite = builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=timetracker.db";
    builder.Services.AddDbContext<TimeTrackerContext>(opt => opt.UseSqlite(sqlite));
}


    

// Add Identity for user auth
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<TimeTrackerContext>()
    .AddDefaultTokenProviders();

// Bind JwtSettings from config
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

// ✅ Add HttpContextAccessor so you can get User from HttpContext in services
builder.Services.AddHttpContextAccessor();

// Add JWT Authentication
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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Register custom services
builder.Services.AddScoped<TimeEntryService>();

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
    {
       var allowed = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowed)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TimeTracker API", Version = "v1" });

    // 🔐 Add JWT auth to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}' (without quotes). Example: Bearer abcdef12345"
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
            Array.Empty<string>()
        }
    });
});

// ============================
//        Build & Run App
// ============================


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("ui");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok("ok"));

// Serve Angular static files
app.UseDefaultFiles();
app.UseStaticFiles();

// 🔁 Fallback routing for Angular (MUST be before MapControllers)
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 &&
        !Path.HasExtension(context.Request.Path.Value) &&
        !context.Request.Path.Value.StartsWith("/api"))
    {
        context.Request.Path = "/index.html";
        await next();
    }
});

app.MapControllers();

app.Run();
