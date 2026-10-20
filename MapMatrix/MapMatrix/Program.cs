using MapMatrix.Data;
using MapMatrix.Entities;
using MapMatrix.Hubs;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

//DbContext + PostGIS
builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("defaultConnection"), o => 
   {
       o.UseNetTopologySuite();
       o.MapEnum<UserRole>("user_role");
       o.MapEnum<TrailStatus>("trail_status");
       o.MapEnum<TransportMode>("transport_mode");
       o.MapEnum<SessionStatus>("session_status");
       o.MapEnum<AssignmentStatus>("assignment_status");
       o.MapEnum<EmergencyStatus>("emergency_status");
   }));

//Jwt Authentication + SignalR
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key manquant");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/tracking"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }); ;

builder.Services.AddAuthorization();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TrailService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AssignmentService>();
builder.Services.AddScoped<EmergencyService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<BeaconService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<FileUploadService>();

var app = builder.Build();

//Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<TrackingHub>("/hubs/tracking");

app.Run();
