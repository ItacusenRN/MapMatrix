using MapMatrix.Data;
using MapMatrix.Entities;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
   }));

//Jwt Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key manquant");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
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
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TrailService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AssignmentService>();

var app = builder.Build();

//Configure the HTTP request pipeline.
//if(app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
