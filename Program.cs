using FhirApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with In-Memory Database
builder.Services.AddDbContext<FhirDbContext>(options =>
    options.UseInMemoryDatabase("FhirDb"));

// Add controllers with NewtonsoftJson support (NO NEED FOR FhirJsonConverter)
builder.Services.AddControllers()
    .AddNewtonsoftJson();  // This line is enough for JSON serialization

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FHIR API",
        Version = "v1",
        Description = "A simple FHIR backend API using ASP.NET Core 8",
        Contact = new OpenApiContact
        {
            Name = "Daniel Newman",
            Email = "contact@newman.com"
        }
    });
});

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FHIR API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapControllers();
app.Run();
