using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/secciones")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class SeccionesController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public SeccionesController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todas las secciones
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Recupera todas las secciones almacenadas en la base de datos.
            var secciones = await _context.Secciones.ToListAsync();
            // Devuelve los datos en formato JSON con código de estado 200 (OK).
            return Ok(secciones);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener una sección por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var seccion = await _context.Secciones.FindAsync(id);

            if (seccion == null)
            {
                return NotFound(new { mensaje = "No se encontró una sección con el Id " + id });
            }

            return Ok(seccion);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear una nueva sección
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Seccion seccion)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Seccion.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Secciones.Add(seccion);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = seccion.Id }, seccion);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar una sección existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Seccion seccionActualizada)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Seccion.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var seccion = await _context.Secciones.FindAsync(id);

            if (seccion == null)
            {
                return NotFound(new { mensaje = "No se encontró una sección con el Id " + id });
            }

            // Reemplazar los campos editables
            seccion.Nombre = seccionActualizada.Nombre;
            seccion.Capacidad = seccionActualizada.Capacidad;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar una sección
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var seccion = await _context.Secciones.FindAsync(id);

            if (seccion == null)
            {
                return NotFound(new { mensaje = "No se encontró una sección con el Id " + id });
            }

            // Verificar que no tenga estudiantes asociados
            // Se evita eliminar una sección si ya existen estudiantes
            // asignados a ella, para no dejar registros huérfanos o inconsistentes.
            bool tieneEstudiantes = await _context.Estudiantes
                .AnyAsync(e => e.SeccionId == id);

            if (tieneEstudiantes)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la sección porque tiene estudiantes asociados." });
            }

            _context.Secciones.Remove(seccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}