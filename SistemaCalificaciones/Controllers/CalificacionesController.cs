using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaCalificaciones.Data;
using SistemaCalificaciones.Models;

namespace SistemaCalificaciones.Controllers
{
    [ApiController]
    [Route("api/calificaciones")]
    // Requiere que el usuario esté autenticado (token JWT válido) para acceder
    // a cualquier endpoint de este controlador.
    [Authorize]
    public class CalificacionesController : ControllerBase
    {
        // Contexto de base de datos utilizado para acceder y manipular la información.
        private readonly AppDbContext _context;

        public CalificacionesController(AppDbContext context)
        {
            // Inyección de dependencias del contexto de base de datos.
            _context = context;
        }


        // ENDPOINT 1: Obtener todas las calificaciones, incluyendo los nombres de sus relaciones
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            //Se utilizan subconsultas correlacionadas dentro del Select para resolver los nombres
            // del estudiante, la materia, el período académico y el tipo de evaluación,
            // evitando el problema N+1.
            var calificaciones = await _context.Calificaciones
                .Select(c => new
                {
                    c.Id,
                    c.EstudianteId,
                    NombreEstudiante = _context.Estudiantes
                        .Where(e => e.Id == c.EstudianteId)
                        .Select(e => e.Nombre + " " + e.Apellido)
                        .FirstOrDefault(),
                    c.MateriaId,
                    NombreMateria = _context.Materias
                        .Where(m => m.Id == c.MateriaId)
                        .Select(m => m.Nombre)
                        .FirstOrDefault(),
                    c.PeriodoAcademicoId,
                    NombrePeriodo = _context.PeriodosAcademicos
                        .Where(p => p.Id == c.PeriodoAcademicoId)
                        .Select(p => p.Nombre)
                        .FirstOrDefault(),
                    c.TipoEvaluacionId,
                    NombreTipoEvaluacion = _context.TiposEvaluacion
                        .Where(t => t.Id == c.TipoEvaluacionId)
                        .Select(t => t.Nombre)
                        .FirstOrDefault(),
                    c.Calificacion1,
                    c.Calificacion2,
                    c.Calificacion3,
                    c.Calificacion4,
                    c.Examen,
                    c.TotalCalificacion,
                    c.Clasificacion,
                    c.Estado
                })
                .ToListAsync();

            return Ok(calificaciones);
        }
        // -------------------------------------------------------


        // ENDPOINT 2: Obtener una calificación por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var calificacion = await _context.Calificaciones.FindAsync(id);

            if (calificacion == null)
            {
                return NotFound(new { mensaje = "No se encontró una calificación con el Id " + id });
            }

            return Ok(calificacion);
        }
        // -------------------------------------------------------


        // ENDPOINT 3: Obtener todas las calificaciones de un estudiante específico
        [HttpGet("estudiante/{estudianteId:int}")]
        public async Task<IActionResult> ObtenerPorEstudiante(int estudianteId)
        {
            // Primero se valida que el estudiante exista antes de buscar sus calificaciones.
            var estudianteExiste = await _context.Estudiantes.FindAsync(estudianteId);

            if (estudianteExiste == null)
            {
                return NotFound(new { mensaje = "No se encontró un estudiante con el Id " + estudianteId });
            }

            // Se filtra por EstudianteId y se proyecta un objeto reducido:
            // no se repite NombreEstudiante (ya sabemos de quién son las calificaciones),
            // pero sí se incluye el nombre de la materia para dar contexto a cada nota.
            var calificaciones = await _context.Calificaciones
                .Where(c => c.EstudianteId == estudianteId)
                .Select(c => new
                {
                    c.Id,
                    c.MateriaId,
                    NombreMateria = _context.Materias
                        .Where(m => m.Id == c.MateriaId)
                        .Select(m => m.Nombre)
                        .FirstOrDefault(),
                    c.Calificacion1,
                    c.Calificacion2,
                    c.Calificacion3,
                    c.Calificacion4,
                    c.Examen,
                    c.TotalCalificacion,
                    c.Clasificacion,
                    c.Estado
                })
                .ToListAsync();

            return Ok(calificaciones);
        }
        // -------------------------------------------------------


        // ENDPOINT 4: Crear una nueva calificación
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Calificacion calificacion)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Calificacion.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el estudiante exista
            // Se validan las cuatro llaves foráneas del modelo Calificacion antes de
            // crearla, ya que conecta Estudiante, Materia, PeriodoAcademico y TipoEvaluacion.
            var estudianteExiste = await _context.Estudiantes.FindAsync(calificacion.EstudianteId);
            if (estudianteExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un estudiante con el Id " + calificacion.EstudianteId });
            }

            // Verificar que la materia exista
            var materiaExiste = await _context.Materias.FindAsync(calificacion.MateriaId);
            if (materiaExiste == null)
            {
                return BadRequest(new { mensaje = "No existe una materia con el Id " + calificacion.MateriaId });
            }

            // Verificar que el período académico exista
            var periodoExiste = await _context.PeriodosAcademicos.FindAsync(calificacion.PeriodoAcademicoId);
            if (periodoExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un período académico con el Id " + calificacion.PeriodoAcademicoId });
            }

            // Verificar que el tipo de evaluación exista
            var tipoExiste = await _context.TiposEvaluacion.FindAsync(calificacion.TipoEvaluacionId);
            if (tipoExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un tipo de evaluación con el Id " + calificacion.TipoEvaluacionId });
            }

            // Verificar que no exista ya una calificación para ese estudiante
            // en esa materia y período
            // Regla de negocio: un estudiante solo puede tener UNA calificación
            // por combinación de Materia + PeriodoAcademico (evita registros duplicados).
            bool calificacionDuplicada = await _context.Calificaciones
                .AnyAsync(c => c.EstudianteId == calificacion.EstudianteId
                            && c.MateriaId == calificacion.MateriaId
                            && c.PeriodoAcademicoId == calificacion.PeriodoAcademicoId);

            if (calificacionDuplicada)
            {
                return Conflict(new { mensaje = "Ya existe una calificación registrada para ese estudiante en esa materia y período." });
            }

            // Calcular automáticamente el total, clasificación y estado
            // CalcularResultados() es un método del propio modelo Calificacion que,
            // a partir de Calificacion1-4 y Examen, calcula TotalCalificacion,
            // Clasificacion (ej. "Excelente", "Bueno") y Estado (ej. "Aprobado"/"Reprobado").
            calificacion.CalcularResultados();

            _context.Calificaciones.Add(calificacion);
            await _context.SaveChangesAsync();

            // Retorna una respuesta 201 (Created) indicando que el recurso fue creado.
            // Además, incluye la ubicación del nuevo recurso mediante el método ObtenerPorId.
            return CreatedAtAction(nameof(ObtenerPorId), new { id = calificacion.Id }, calificacion);
        }
        // -------------------------------------------------------


        // ENDPOINT 5: Actualizar una calificación existente
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Calificacion calificacionActualizada)
        {
            // Valida que los datos enviados en el cuerpo de la petición cumplan
            // con las anotaciones de validación definidas en el modelo Calificacion.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var calificacion = await _context.Calificaciones.FindAsync(id);

            if (calificacion == null)
            {
                return NotFound(new { mensaje = "No se encontró una calificación con el Id " + id });
            }

            // Verificar que el estudiante exista
            var estudianteExiste = await _context.Estudiantes
                .FindAsync(calificacionActualizada.EstudianteId);

            if (estudianteExiste == null)
            {
                return BadRequest(new { mensaje = "No existe un estudiante con el Id " + calificacionActualizada.EstudianteId });
            }

            // Verificar que la materia exista
            var materiaExiste = await _context.Materias
                .FindAsync(calificacionActualizada.MateriaId);

            if (materiaExiste == null)
            {
                return BadRequest(new { mensaje = "No existe una materia con el Id " + calificacionActualizada.MateriaId });
            }

            // Verificar duplicado excluyendo la misma calificación
            bool calificacionDuplicada = await _context.Calificaciones
                .AnyAsync(c => c.EstudianteId == calificacionActualizada.EstudianteId
                            && c.MateriaId == calificacionActualizada.MateriaId
                            && c.PeriodoAcademicoId == calificacionActualizada.PeriodoAcademicoId
                            && c.Id != id);

            if (calificacionDuplicada)
            {
                return Conflict(new { mensaje = "Ya existe una calificación registrada para ese estudiante en esa materia y período." });
            }

            // Actualizar los campos
            calificacion.EstudianteId = calificacionActualizada.EstudianteId;
            calificacion.MateriaId = calificacionActualizada.MateriaId;
            calificacion.PeriodoAcademicoId = calificacionActualizada.PeriodoAcademicoId;
            calificacion.TipoEvaluacionId = calificacionActualizada.TipoEvaluacionId;
            calificacion.Calificacion1 = calificacionActualizada.Calificacion1;
            calificacion.Calificacion2 = calificacionActualizada.Calificacion2;
            calificacion.Calificacion3 = calificacionActualizada.Calificacion3;
            calificacion.Calificacion4 = calificacionActualizada.Calificacion4;
            calificacion.Examen = calificacionActualizada.Examen;

            // Recalcular automáticamente con los nuevos valores
            // Se vuelve a invocar CalcularResultados() porque las notas cambiaron,
            // así que el total, la clasificación y el estado deben recalcularse.
            calificacion.CalcularResultados();

            await _context.SaveChangesAsync();

            return NoContent();
        }
        // -------------------------------------------------------


        // ENDPOINT 6: Eliminar una calificación
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var calificacion = await _context.Calificaciones.FindAsync(id);

            if (calificacion == null)
            {
                return NotFound(new { mensaje = "No se encontró una calificación con el Id " + id });
            }

            _context.Calificaciones.Remove(calificacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}