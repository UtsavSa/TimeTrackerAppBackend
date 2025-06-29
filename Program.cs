using TimeTrackerApi.Services;
using TimeTrackerApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
//  Register services
builder.Services.AddScoped<TimeEntryService>();

// Add controller support
builder.Services.AddControllers();

//  Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//  Add EF Core + SQLite
builder.Services.AddDbContext<TimeTrackerContext>(options =>
    options.UseSqlite("Data Source=timetracker.db"));

//  Add CORS BEFORE builder.Build()
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Your Angular app
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

//  Use Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Use CORS (after app is built, but before controllers)
app.UseCors("AllowLocalhost4200");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
