using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        // Utilidad de ASP.NET Identity para encriptar y verificar contraseñas de forma segura.
        private readonly PasswordHasher<Usuario> _hasher = new();

        public UsuariosController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los usuarios
        [HttpGet]
        // Solo los usuarios con rol "Admin" pueden listar a todos los usuarios del sistema.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Se proyecta el resultado con Select para excluir campos sensibles,
            // como el hash de la contraseña, y devolver solo los datos necesarios.
            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.NombreUsuario,
                    u.Correo,
                    u.Rol,
                    u.FechaRegistro
                })
                .ToListAsync();

            return Ok(usuarios);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un usuario por Id
        [HttpGet("{id:int}")]
        // Solo los usuarios con rol "Admin" pueden consultar el detalle de un usuario específico.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensaje = "No se encontró un usuario con el Id " + id });
            }

            // Se devuelve un objeto anónimo con únicamente los campos necesarios,
            // evitando exponer el hash de la contraseña almacenado en el usuario.
            return Ok(new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.NombreUsuario,
                usuario.Correo,
                usuario.Rol,
                usuario.FechaRegistro
            });
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Actualizar un usuario existente
        [HttpPut("{id:int}")]
        // Solo los usuarios con rol "Admin" pueden modificar los datos de un usuario.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Usuario usuarioActualizado)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensaje = "No se encontró un usuario con el Id " + id });
            }

            // Verificar que el nuevo nombre de usuario no lo tenga otro usuario
            // Se excluye el propio Id del usuario que se está actualizando para
            // no marcar como duplicado su nombre de usuario actual.
            bool nombreRepetido = await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == usuarioActualizado.NombreUsuario && u.Id != id);

            if (nombreRepetido)
            {
                return Conflict(new { mensaje = "El nombre de usuario ya está en uso." });
            }

            // Verificar que el nuevo correo no lo tenga otro usuario
            bool correoRepetido = await _context.Usuarios
                .AnyAsync(u => u.Correo == usuarioActualizado.Correo && u.Id != id);

            if (correoRepetido)
            {
                return Conflict(new { mensaje = "El correo ya está registrado." });
            }

            // Reemplazar los campos editables
            // Nota: no se modifica el hash de la contraseña aquí, ya que este endpoint
            // no recibe ni procesa una nueva contraseña.
            usuario.Nombre = usuarioActualizado.Nombre;
            usuario.NombreUsuario = usuarioActualizado.NombreUsuario;
            usuario.Correo = usuarioActualizado.Correo;
            usuario.Rol = usuarioActualizado.Rol;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Eliminar un usuario
        [HttpDelete("{id:int}")]
        // Solo los usuarios con rol "Admin" pueden eliminar usuarios del sistema.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensaje = "No se encontró un usuario con el Id " + id });
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}