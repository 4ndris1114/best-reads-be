using DotNetEnv;
using BestReads.Database;

var builder = WebApplication.CreateBuilder(args);

Env.Load();
var mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")!;

// Add environment variables to the configuration
builder.Configuration.AddEnvironmentVariables();

//TODO: add controllers
//TODO: add CORS
//TODO: add repos - maybe come up with a way to automatically add them

// Add MongoDB connection service (singleton)
builder.Services.AddSingleton<MongoDbContext>(sp =>
    new MongoDbContext(mongoDbConnectionString)
);

//TODO: add swagger docs and config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//TODO: add auth

var app = builder.Build();

//TODO: add cors

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//TODO: add authentication
//TODO: add authorization

//TODO: map controllers

Console.WriteLine("Starting application");
app.Run();