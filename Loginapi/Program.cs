using Loginapi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddAuthentication(
    
    o => { o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
        o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
        o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; }
    ).AddCookie().AddGoogleOpenIdConnect(options =>{
    options.ClientId = "127115882562-o1ncfvcnq2cuoj6qt7n8mqs627fspru4.apps.googleusercontent.com";
        options.ClientSecret = "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";
});

builder.Services.AddDbContext<ApplcationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseCors(options => options.WithOrigins(new[] { "http://localhost:8000", "http://localhost:4200" }).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();
