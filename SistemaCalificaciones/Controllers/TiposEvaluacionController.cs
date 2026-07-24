using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/tipos-evaluacion")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class TiposEvaluacionController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public TiposEvaluacionController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los tipos de evaluación
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Recupera todos los tipos de evaluación almacenados en la base de datos.
            var tipos = await _context.TiposEvaluacion.ToListAsync();
            // Devuelve los datos en formato JSON con código de estado 200 (OK).
            return Ok(tipos);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un tipo de evaluación por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var tipo = await _context.TiposEvaluacion.FindAsync(id);

            if (tipo == null)
            {
                return NotFound(new { mensaje = "No se encontró un tipo de evaluación con el Id " + id });
            }

            return Ok(tipo);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear un nuevo tipo de evaluación
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] TipoEvaluacion tipo)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo TipoEvaluacion.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TiposEvaluacion.Add(tipo);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = tipo.Id }, tipo);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar un tipo de evaluación existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] TipoEvaluacion tipoActualizado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo TipoEvaluacion.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipo = await _context.TiposEvaluacion.FindAsync(id);

            if (tipo == null)
            {
                return NotFound(new { mensaje = "No se encontró un tipo de evaluación con el Id " + id });
            }

            // Reemplazar los campos editables
            tipo.Nombre = tipoActualizado.Nombre;
            tipo.Descripcion = tipoActualizado.Descripcion;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar un tipo de evaluación
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tipo = await _context.TiposEvaluacion.FindAsync(id);

            if (tipo == null)
            {
                return NotFound(new { mensaje = "No se encontró un tipo de evaluación con el Id " + id });
            }

            // Verificar que no tenga calificaciones asociadas
            // Se evita eliminar un tipo de evaluación si ya existen calificaciones
            // registradas con él, para no dejar registros huérfanos o inconsistentes.
            bool tieneCalificaciones = await _context.Calificaciones
                .AnyAsync(c => c.TipoEvaluacionId == id);

            if (tieneCalificaciones)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el tipo de evaluación porque tiene calificaciones asociadas." });
            }

            _context.TiposEvaluacion.Remove(tipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}