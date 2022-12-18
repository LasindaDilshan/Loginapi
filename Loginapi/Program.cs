using Loginapi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplcationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseCors(options => options.WithOrigins(new[] { "http://localhost:8000", "http://localhost:4200" }).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();
