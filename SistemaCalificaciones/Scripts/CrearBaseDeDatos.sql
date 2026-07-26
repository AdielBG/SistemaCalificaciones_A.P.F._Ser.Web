IF DB_ID('SistemaCalificacionesDb') IS NULL
BEGIN
    CREATE DATABASE SistemaCalificacionesDb;
END
GO

USE SistemaCalificacionesDb;
GO

-- Eliminar tablas en orden inverso por las dependencias
IF OBJECT_ID('Calificaciones', 'U') IS NOT NULL DROP TABLE Calificaciones;
IF OBJECT_ID('Estudiantes', 'U') IS NOT NULL DROP TABLE Estudiantes;
IF OBJECT_ID('Materias', 'U') IS NOT NULL DROP TABLE Materias;
IF OBJECT_ID('Profesores', 'U') IS NOT NULL DROP TABLE Profesores;
IF OBJECT_ID('ProgramasAcademicos', 'U') IS NOT NULL DROP TABLE ProgramasAcademicos;
IF OBJECT_ID('PeriodosAcademicos', 'U') IS NOT NULL DROP TABLE PeriodosAcademicos;
IF OBJECT_ID('Secciones', 'U') IS NOT NULL DROP TABLE Secciones;
IF OBJECT_ID('TiposEvaluacion', 'U') IS NOT NULL DROP TABLE TiposEvaluacion;
IF OBJECT_ID('EstadosAcademicos', 'U') IS NOT NULL DROP TABLE EstadosAcademicos;
IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DROP TABLE Usuarios;
GO

CREATE TABLE Usuarios
(
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre          NVARCHAR(100)     NOT NULL,
    NombreUsuario   NVARCHAR(50)      NOT NULL,
    Correo          NVARCHAR(150)     NOT NULL,
    HashContrasena  NVARCHAR(MAX)     NOT NULL,
    Rol             NVARCHAR(20)      NOT NULL,
    FechaRegistro   DATETIME2         NOT NULL
);
GO

CREATE UNIQUE INDEX IX_Usuarios_NombreUsuario ON Usuarios(NombreUsuario);
CREATE UNIQUE INDEX IX_Usuarios_Correo ON Usuarios(Correo);
GO

CREATE TABLE ProgramasAcademicos
(
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre      NVARCHAR(100)     NOT NULL,
    Descripcion NVARCHAR(250)     NOT NULL
);
GO

CREATE TABLE EstadosAcademicos
(
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre      NVARCHAR(50)      NOT NULL,
    Descripcion NVARCHAR(150)     NOT NULL
);
GO

CREATE TABLE Secciones
(
    Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre    NVARCHAR(50)      NOT NULL,
    Capacidad INT               NOT NULL
);
GO

CREATE TABLE TiposEvaluacion
(
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre      NVARCHAR(50)      NOT NULL,
    Descripcion NVARCHAR(150)     NOT NULL
);
GO

CREATE TABLE PeriodosAcademicos
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre       NVARCHAR(50)      NOT NULL,
    FechaInicio  DATETIME2         NOT NULL,
    FechaFin     DATETIME2         NOT NULL,
    Activo       BIT               NOT NULL
);
GO

CREATE TABLE Profesores
(
    Id       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre   NVARCHAR(100)     NOT NULL,
    Apellido NVARCHAR(100)     NOT NULL,
    Correo   NVARCHAR(150)     NOT NULL,
    Telefono NVARCHAR(20)      NOT NULL
);
GO

CREATE TABLE Materias
(
    Id         INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre     NVARCHAR(100)     NOT NULL,
    Codigo     NVARCHAR(20)      NOT NULL,
    Creditos   INT               NOT NULL,
    ProfesorId INT               NOT NULL,
    CONSTRAINT FK_Materias_Profesores FOREIGN KEY (ProfesorId) REFERENCES Profesores(Id)
);
GO

CREATE UNIQUE INDEX IX_Materias_Codigo ON Materias(Codigo);
GO

CREATE TABLE Estudiantes
(
    Id                  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nombre              NVARCHAR(100)     NOT NULL,
    Apellido            NVARCHAR(100)     NOT NULL,
    Correo              NVARCHAR(150)     NOT NULL,
    Matricula           NVARCHAR(20)      NOT NULL,
    ProgramaAcademicoId INT               NOT NULL,
    SeccionId           INT               NOT NULL,
    EstadoAcademicoId   INT               NOT NULL,
    CONSTRAINT FK_Estudiantes_Programas  FOREIGN KEY (ProgramaAcademicoId) REFERENCES ProgramasAcademicos(Id),
    CONSTRAINT FK_Estudiantes_Secciones  FOREIGN KEY (SeccionId)           REFERENCES Secciones(Id),
    CONSTRAINT FK_Estudiantes_Estados    FOREIGN KEY (EstadoAcademicoId)   REFERENCES EstadosAcademicos(Id)
);
GO

CREATE UNIQUE INDEX IX_Estudiantes_Matricula ON Estudiantes(Matricula);
CREATE UNIQUE INDEX IX_Estudiantes_Correo    ON Estudiantes(Correo);
GO

CREATE TABLE Calificaciones
(
    Id                 INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EstudianteId       INT               NOT NULL,
    MateriaId          INT               NOT NULL,
    PeriodoAcademicoId INT               NOT NULL,
    TipoEvaluacionId   INT               NOT NULL,
    Calificacion1      DECIMAL(5,2)      NOT NULL,
    Calificacion2      DECIMAL(5,2)      NOT NULL,
    Calificacion3      DECIMAL(5,2)      NOT NULL,
    Calificacion4      DECIMAL(5,2)      NOT NULL,
    Examen             DECIMAL(5,2)      NOT NULL,
    TotalCalificacion  DECIMAL(5,2)      NOT NULL,
    Clasificacion      NVARCHAR(20)      NOT NULL,
    Estado             NVARCHAR(20)      NOT NULL,
    CONSTRAINT FK_Calificaciones_Estudiantes FOREIGN KEY (EstudianteId)       REFERENCES Estudiantes(Id),
    CONSTRAINT FK_Calificaciones_Materias    FOREIGN KEY (MateriaId)           REFERENCES Materias(Id),
    CONSTRAINT FK_Calificaciones_Periodos    FOREIGN KEY (PeriodoAcademicoId)  REFERENCES PeriodosAcademicos(Id),
    CONSTRAINT FK_Calificaciones_Tipos       FOREIGN KEY (TipoEvaluacionId)    REFERENCES TiposEvaluacion(Id)
);
GO

-- Datos de prueba
INSERT INTO EstadosAcademicos (Nombre, Descripcion) VALUES
('Regular',    'Estudiante en condición académica normal'),
('Irregular',  'Estudiante con materias pendientes'),
('Suspendido', 'Estudiante suspendido temporalmente'),
('Graduado',   'Estudiante que completó el programa');
GO

INSERT INTO ProgramasAcademicos (Nombre, Descripcion) VALUES
('Ingeniería de Software',   'Carrera enfocada en el desarrollo de software'),
('Administración de Empresas', 'Carrera enfocada en gestión empresarial'),
('Contabilidad',             'Carrera enfocada en ciencias contables');
GO

INSERT INTO Secciones (Nombre, Capacidad) VALUES
('Sección A', 30),
('Sección B', 25),
('Sección C', 35);
GO

INSERT INTO TiposEvaluacion (Nombre, Descripcion) VALUES
('Práctica',      'Evaluación práctica en el aula'),
('Tarea',         'Asignación para realizar fuera del aula'),
('Participación', 'Evaluación de participación en clase'),
('Proyecto',      'Trabajo de investigación o desarrollo'),
('Examen',        'Evaluación escrita formal');
GO

INSERT INTO PeriodosAcademicos (Nombre, FechaInicio, FechaFin, Activo) VALUES
('Primer Cuatrimestre 2026',  '2026-01-10', '2026-04-30', 1),
('Segundo Cuatrimestre 2026', '2026-05-10', '2026-08-31', 0);
GO

INSERT INTO Profesores (Nombre, Apellido, Correo, Telefono) VALUES
('Carlos',  'Ramírez', 'carlos.ramirez@ufhec.edu.do',  '809-555-0101'),
('María',   'González','maria.gonzalez@ufhec.edu.do',  '809-555-0102'),
('Roberto', 'Méndez',  'roberto.mendez@ufhec.edu.do',  '809-555-0103');
GO

INSERT INTO Materias (Nombre, Codigo, Creditos, ProfesorId) VALUES
('Ingeniería de Servicios Web', 'INF-4318', 4, 1),
('Base de Datos',               'INF-2210', 4, 2),
('Programación Orientada a Objetos', 'INF-2105', 3, 3);
GO

INSERT INTO Estudiantes (Nombre, Apellido, Correo, Matricula, ProgramaAcademicoId, SeccionId, EstadoAcademicoId) VALUES
('Adiel',   'Batista',  'adiel.batista@ufhec.edu.do',  '2021-0001', 1, 1, 1),
('Ana',     'Pérez',    'ana.perez@ufhec.edu.do',      '2021-0002', 1, 1, 1),
('Luis',    'García',   'luis.garcia@ufhec.edu.do',    '2021-0003', 2, 2, 1);
GO

SELECT * FROM EstadosAcademicos;
SELECT * FROM ProgramasAcademicos;
SELECT * FROM Secciones;
SELECT * FROM TiposEvaluacion;
SELECT * FROM PeriodosAcademicos;
SELECT * FROM Profesores;
SELECT * FROM Materias;
SELECT * FROM Estudiantes;
GO