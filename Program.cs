using DotNetEnv;
using BestReads.Database;
using BestReads.Services;
using BestReads.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using BestReads.Hubs;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
var mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")!;

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<CloudinaryService>();

builder.Services.AddControllers();

builder.Services.AddSignalR();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var assembly = typeof(Program).Assembly;
foreach (var type in assembly.GetTypes()
    .Where(t => t.IsClass && t.Name.EndsWith("Repository")))
{
    builder.Services.AddScoped(type);
}

// Add MongoDB connection service (singleton)
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext(mongoDbConnectionString)
);

//TODO: add swagger docs and config
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "localhost",
            ValidAudience = "localhost",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!))
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BestReads API", Version = "v1", Description = "The API for the BestReads application" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"

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
            new string[] {}
        }
    });
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "best-reads-be.xml");
    c.IncludeXmlComments(xmlFile);
});

builder.Services.AddAuthorization();  // To use authorization

//TODO: add auth

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BestReads API v1");
        options.RoutePrefix = string.Empty;  // Swagger UI at root URL
    });
}

app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();  // Add authorization middleware

app.MapControllers();
app.MapHub<ActivityHub>("/hubs/activity");

Console.WriteLine("Starting application...");

app.Run();