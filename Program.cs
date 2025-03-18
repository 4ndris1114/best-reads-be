using DotNetEnv;
using BestReads.Database;
using BestReads.Repositories;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
var mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")!;

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => 
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

//TODO: add repos
builder.Services.AddScoped<BookRepository>();

// Add MongoDB connection service (singleton)
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext(mongoDbConnectionString)
);

//TODO: add swagger docs and config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//TODO: add auth

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//TODO: add authentication
//TODO: add authorization

app.MapControllers();

Console.WriteLine("Starting application");
app.Run();