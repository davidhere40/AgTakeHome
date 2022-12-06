using System.Text.Json.Serialization;
using FileStorageAPI.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    services.AddDbContext<FileStorageAPIDBContext>();
    services.AddCors();
    services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // ignore omitted parameters on models to enable optional params (e.g. User update)
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    // configure DI for application services
    services.AddScoped<IFileService, FileService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// configure HTTP request pipeline
{
    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    // global error handler
    //app.UseMiddleware<ErrorHandlerMiddleware>();

    app.MapControllers();
}

//Make sure the database is created
using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<FileStorageAPIDBContext>())
{
    context.Database.Migrate();
    //Force the CustomerFile table to be created. I'm sure there's a better way.
    string result = context.Database.GenerateCreateScript();
}

app.Run("http://localhost:5000");
