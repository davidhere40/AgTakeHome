using System.Reflection;
using System.Text.Json.Serialization;
using FileStorageAPI.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "FileStorage API",
        Description = "This is a simple file storage API service using .Net Core 6, SQL Lite, and Entity Framework. It allows binary files to be saved with a name and keeps track of previous versions.",
        Contact = new OpenApiContact
        {
            Name = "David Jones",
            Email = "davidhere40@gmail.com"
        }
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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
}

app.Run("http://localhost:5000");
