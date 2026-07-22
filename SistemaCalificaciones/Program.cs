using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;
using SistemaCalificaciones.Seguridad;

var builder = WebApplication.CreateBuilder(args);

// Registra los controladores de la API para que ASP.NET Core
// pueda detectar y exponer los endpoints definidos en ellos.
builder.Services.AddControllers();

// Configura Entity Framework Core para utilizar SQL Server
// como proveedor de base de datos, empleando la cadena de
// conexión definida en appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra JwtService en el contenedor de inyección de dependencias.
// Se crea una nueva instancia por cada solicitud HTTP.
builder.Services.AddScoped<JwtService>();

// Obtiene la configuración de JWT desde appsettings.json,
// donde se encuentran el emisor, la audiencia y la clave secreta.
var jwt = builder.Configuration.GetSection("Jwt");

// Configura el esquema de autenticación de la aplicación
// para utilizar JSON Web Tokens (JWT).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Evita que ASP.NET Core cambie automáticamente los nombres
        // originales de los claims definidos en el token.
        options.MapInboundClaims = false;

        // Define las reglas que se utilizarán para validar
        // cada token recibido en las solicitudes.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Verifica que el emisor del token sea el esperado.
            ValidateIssuer = true,

            // Verifica que el token esté destinado a esta aplicación.
            ValidateAudience = true,

            // Comprueba que el token no haya expirado.
            ValidateLifetime = true,

            // Valida la firma digital para garantizar que el token
            // no haya sido alterado.
            ValidateIssuerSigningKey = true,

            // Emisor permitido.
            ValidIssuer = jwt["Issuer"],

            // Audiencia permitida.
            ValidAudience = jwt["Audience"],

            // Clave secreta utilizada para validar la firma del token.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),

            // Elimina el margen de tolerancia en la expiración,
            // haciendo que el token expire exactamente a la hora indicada.
            ClockSkew = TimeSpan.Zero,

            // Indica qué claim representa el nombre del usuario.
            NameClaimType = JwtRegisteredClaimNames.UniqueName,

            // Indica qué claim contiene el rol del usuario para
            // la autorización basada en roles.
            RoleClaimType = "role"
        };
    });

// Habilita el sistema de autorización para controlar el acceso
// a los recursos mediante atributos como [Authorize].
builder.Services.AddAuthorization();

// Registra los servicios necesarios para generar la documentación
// automática de la API mediante Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define la información general que se mostrará
    // en la interfaz de Swagger.
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sistema de Calificaciones",
        Version = "v1",
        Description = "API REST para gestión de calificaciones estudiantiles con autenticación JWT."
    });

    // Agrega el esquema de autenticación Bearer para que
    // Swagger permita enviar el token JWT en las pruebas.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa el token JWT. Ejemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Indica que los endpoints protegidos utilizarán
    // el esquema de autenticación Bearer.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Construye la aplicación con todos los servicios
// y configuraciones registrados previamente.
var app = builder.Build();

// Crea un alcance de servicios para acceder al contexto
// de la base de datos durante el inicio de la aplicación.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Crea la base de datos si aún no existe.
    context.Database.EnsureCreated();

    // Verifica si existen usuarios registrados.
    // Si no hay ninguno, crea automáticamente un administrador inicial.
    if (!context.Usuarios.Any())
    {
        // Servicio encargado de generar el hash seguro
        // de la contraseña del usuario.
        var hasher = new PasswordHasher<Usuario>();

        var admin = new Usuario
        {
            Nombre = "Administrador",
            NombreUsuario = "admin",
            Correo = "admin@sistema.edu.do",
            Rol = "Admin",
            FechaRegistro = DateTime.UtcNow
        };

        // Convierte la contraseña en un hash antes de almacenarla,
        // evitando guardar contraseñas en texto plano.
        admin.HashContrasena = hasher.HashPassword(admin, "Admin123!");

        // Guarda el usuario administrador en la base de datos.
        context.Usuarios.Add(admin);
        context.SaveChanges();
    }
}

// Si la aplicación se ejecuta en modo Desarrollo,
// habilita la interfaz de Swagger para probar la API.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirige automáticamente las solicitudes HTTP hacia HTTPS.
app.UseHttpsRedirection();

// Habilita el middleware encargado de autenticar
// los usuarios mediante el token JWT.
app.UseAuthentication();

// Habilita el middleware de autorización para verificar
// los permisos de acceso a cada recurso.
app.UseAuthorization();

// Mapea los controladores para que respondan
// a las solicitudes HTTP correspondientes.
app.MapControllers();

// Inicia la aplicación y comienza a escuchar solicitudes.
app.Run();