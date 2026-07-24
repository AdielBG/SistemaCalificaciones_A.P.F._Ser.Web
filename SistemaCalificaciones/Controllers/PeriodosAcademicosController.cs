using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/periodos-academicos")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class PeriodosAcademicosController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public PeriodosAcademicosController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todos los períodos académicos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Recupera todos los períodos académicos almacenados en la base de datos.
            var periodos = await _context.PeriodosAcademicos.ToListAsync();
            // Devuelve los datos en formato JSON con código de estado 200 (OK).
            return Ok(periodos);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener un período académico por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var periodo = await _context.PeriodosAcademicos.FindAsync(id);

            if (periodo == null)
            {
                return NotFound(new { mensaje = "No se encontró un período académico con el Id " + id });
            }

            return Ok(periodo);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear un nuevo período académico
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] PeriodoAcademico periodo)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo PeriodoAcademico.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que la fecha de fin sea posterior a la de inicio
            // Regla de negocio: un período académico no puede terminar antes
            // (o el mismo día) de cuando empieza.
            if (periodo.FechaFin <= periodo.FechaInicio)
            {
                return BadRequest(new { mensaje = "La fecha de fin debe ser posterior a la fecha de inicio." });
            }

            _context.PeriodosAcademicos.Add(periodo);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = periodo.Id }, periodo);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar un período académico existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] PeriodoAcademico periodoActualizado)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo PeriodoAcademico.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Misma regla de negocio que en Crear: se valida antes de tocar la base de datos,
            // para no hacer una consulta innecesaria si los datos ya son inválidos.
            if (periodoActualizado.FechaFin <= periodoActualizado.FechaInicio)
            {
                return BadRequest(new { mensaje = "La fecha de fin debe ser posterior a la fecha de inicio." });
            }

            var periodo = await _context.PeriodosAcademicos.FindAsync(id);

            if (periodo == null)
            {
                return NotFound(new { mensaje = "No se encontró un período académico con el Id " + id });
            }

            // Reemplazar todos los campos
            periodo.Nombre = periodoActualizado.Nombre;
            periodo.FechaInicio = periodoActualizado.FechaInicio;
            periodo.FechaFin = periodoActualizado.FechaFin;
            periodo.Activo = periodoActualizado.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar un período académico
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var periodo = await _context.PeriodosAcademicos.FindAsync(id);

            if (periodo == null)
            {
                return NotFound(new { mensaje = "No se encontró un período académico con el Id " + id });
            }

            // Verificar que no tenga calificaciones asociadas
            // Se evita eliminar un período académico si ya existen calificaciones
            // registradas sobre él, para no dejar registros huérfanos o inconsistentes.
            bool tieneCalificaciones = await _context.Calificaciones
                .AnyAsync(c => c.PeriodoAcademicoId == id);

            if (tieneCalificaciones)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el período porque tiene calificaciones registradas." });
            }

            _context.PeriodosAcademicos.Remove(periodo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}