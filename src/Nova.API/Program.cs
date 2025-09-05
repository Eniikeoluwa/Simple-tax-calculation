using Microsoft.EntityFrameworkCore;
using Nova.API.Extensions;
using Nova.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddApplicationServices();

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

// Add Swagger with JWT
builder.Services.AddSwaggerWithJwt();

// Add CORS
builder.Services.AddCorsPolicy();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
