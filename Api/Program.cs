using AccesoDatos.Configuracion;
using AccesoDatos.Contratos;
using AccesoDatos.Repositorios;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURACIÓN DE DATABASE ====================

// Configurar DatabaseSettings desde appsettings.json
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(DatabaseSettings.SectionName)
);

// Validar configuración al inicio de la aplicación
builder.Services.AddOptions<DatabaseSettings>()
    .Bind(builder.Configuration.GetSection(DatabaseSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ==================== INYECCIÓN DE DEPENDENCIAS ====================

// Registrar UnitOfWork como Scoped (una instancia por request HTTP)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==================== CONFIGURACIÓN DE CONTROLLERS ====================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar serialización JSON
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ==================== CONFIGURACIÓN DE SWAGGER ====================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema de Urgencias Hospitalarias API",
        Version = "v1",
        Description = "API para el sistema de gestión de urgencias hospitalarias (IS2025-001 a IS2025-005)",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "dev@urgencias.com"
        }
    });

    // Agregar documentación XML si existe
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ==================== CONFIGURACIÓN DE CORS (Opcional) ====================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==================== BUILD APP ====================

var app = builder.Build();

// ==================== VALIDACIÓN DE CONFIGURACIÓN ====================

// Validar que las connection strings estén configuradas correctamente
try
{
    var testUow = app.Services.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
    app.Logger.LogInformation("✅ Configuración de base de datos validada correctamente");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ Error en la configuración de base de datos");
    throw;
}

// ==================== MIDDLEWARE PIPELINE ====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Urgencias API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz (/)
    });
    
    app.Logger.LogInformation("🚀 Ejecutando en modo DESARROLLO");
    app.Logger.LogInformation($"📊 Swagger UI disponible en: https://localhost:{builder.Configuration["Kestrel:Endpoints:Https:Port"] ?? "7000"}");
}
else
{
    app.Logger.LogInformation("🚀 Ejecutando en modo PRODUCCIÓN");
}

app.UseHttpsRedirection();

// Habilitar CORS (si lo configuraste)
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();



app.Run();
