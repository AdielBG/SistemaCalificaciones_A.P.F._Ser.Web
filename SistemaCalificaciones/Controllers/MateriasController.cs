using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/materias")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class MateriasController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public MateriasController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todas las materias, incluyendo el nombre del profesor
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            // Incluimos el nombre del profesor en la respuesta
            // En lugar de traer la entidad completa, se proyecta un objeto anónimo
            // que combina los datos de Materia con una subconsulta a Profesores,
            // concatenando Nombre + Apellido en un solo campo (NombreProfesor).
            var materias = await _context.Materias
                .Select(m => new
                {
                    m.Id,
                    m.Nombre,
                    m.Codigo,
                    m.Creditos,
                    m.ProfesorId,
                    NombreProfesor = _context.Profesores
                        .Where(p => p.Id == m.ProfesorId)
                        .Select(p => p.Nombre + " " + p.Apellido)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(materias);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener una materia por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var materia = await _context.Materias.FindAsync(id);

            if (materia == null)
            {
                return NotFound(new { mensaje = "No se encontró una materia con el Id " + id });
            }

            return Ok(materia);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Crear una nueva materia
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Materia materia)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Materia.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el código no esté duplicado
            // Se asume que el código de materia (ej. "MAT101") debe ser único.
            bool codigoExiste = await _context.Materias
                .AnyAsync(m => m.Codigo == materia.Codigo);

            if (codigoExiste)
            {
                return Conflict(new { mensaje = "Ya existe una materia con ese código." });
            }

            // Verificar que el profesor exista
            // Se valida el ProfesorId recibido antes de permitir la creación de la materia,
            // igual que se hizo con AutorId en LibrosController.
            var profesorExiste = await _context.Profesores.FindAsync(materia.ProfesorId);
            if (profesorExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un profesor con el Id " + materia.ProfesorId });
            }

            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = materia.Id }, materia);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Actualizar una materia existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Materia materiaActualizada)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Materia.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var materia = await _context.Materias.FindAsync(id);

            if (materia == null)
            {
                return NotFound(new { mensaje = "No se encontró una materia con el Id " + id });
            }

            // Verificar código duplicado excluyendo la misma materia
            // Se excluye el propio Id de la materia que se está actualizando para
            // no marcar como duplicado su código actual.
            bool codigoRepetido = await _context.Materias
                .AnyAsync(m => m.Codigo == materiaActualizada.Codigo && m.Id != id);

            if (codigoRepetido)
            {
                return Conflict(new { mensaje = "Ya existe una materia con ese código." });
            }

            // Verificar que el nuevo profesor exista
            var profesorExiste = await _context.Profesores.FindAsync(materiaActualizada.ProfesorId);
            if (profesorExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un profesor con el Id " + materiaActualizada.ProfesorId });
            }

            // Reemplazar todos los campos
            materia.Nombre = materiaActualizada.Nombre;
            materia.Codigo = materiaActualizada.Codigo;
            materia.Creditos = materiaActualizada.Creditos;
            materia.ProfesorId = materiaActualizada.ProfesorId;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Eliminar una materia
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var materia = await _context.Materias.FindAsync(id);

            if (materia == null)
            {
                return NotFound(new { mensaje = "No se encontró una materia con el Id " + id });
            }

            // Verificar que no tenga calificaciones asociadas
            // Se evita eliminar una materia si ya existen calificaciones registradas
            // sobre ella, para no dejar registros huérfanos o inconsistentes.
            bool tieneCalificaciones = await _context.Calificaciones
                .AnyAsync(c => c.MateriaId == id);

            if (tieneCalificaciones)
            {
                return BadRequest(new { mensaje = "No se puede eliminar la materia porque tiene calificaciones registradas." });
            }

            _context.Materias.Remove(materia);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}