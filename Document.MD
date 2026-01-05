OnlineCourses – API REST + Frontend

Proyecto Fullstack para gestión de cursos y lecciones con autenticación JWT.
Incluye:

Backend: API REST en .NET 9 con arquitectura por capas.

Frontend: App en HTML/CSS/JavaScript que consume la API (Login/Register + CRUD + paginación + filtros).

Persistencia: Base de datos MySQL (en Aiven o local).

Reglas de negocio: publicación de cursos solo si tienen al menos una lección activa, orden único de lecciones, soft delete.

Tests: 5 tests unitarios enfocados en lógica de negocio.

Tabla de contenido

Características

Arquitectura

Tecnologías

Backend

Estructura de proyectos

Reglas de negocio

Endpoints principales

Cómo levantar el backend

Ejecutar tests

Frontend

Funciones

Cómo levantar el frontend

Cómo se autentica

Notas importantes

Características
Cursos

✅ Listado con paginación

✅ Filtro por estado: Draft / Published

✅ Crear curso

✅ Editar curso

✅ Eliminar curso (Soft Delete)

✅ Publicar / despublicar curso

Lecciones

✅ Listar lecciones por curso ordenadas por campo Order

✅ Crear lección

✅ Editar lección

✅ Eliminar lección (Soft Delete)

✅ Reordenar lecciones (subir/bajar posición)

Autenticación

✅ Pantalla simple de Login y Register

✅ Guarda JWT en localStorage

✅ Usa Authorization: Bearer <token> en llamadas protegidas

Arquitectura

El backend está dividido en capas para separar responsabilidades:

Domain: Entidades, enums, reglas de negocio y excepciones.

Application: DTOs, interfaces (contratos) y servicios.

Infrastructure: EF Core DbContext, migraciones y repositorios.

Api: Controladores HTTP, configuración, Swagger, CORS, Auth.

Tecnologías

.NET 9

ASP.NET Core Web API

Entity Framework Core

MySQL (Aiven o local)

JWT Authentication

xUnit (tests)

Frontend: HTML + CSS + JavaScript (Fetch API)

Backend
Estructura de proyectos

Solución: OnlineCourses.sln

OnlineCourses.Api

Controllers: CoursesController, LessonsController, AuthController

Swagger + configuración del pipeline

OnlineCourses.Application

DTOs: CourseDto, LessonDto, PagedResultDto, AuthDto, etc.

Interfaces: ICourseRepository, ILessonRepository, IAuthService

Services: lógica de autenticación (JWT)

OnlineCourses.Domain

Entities: Course, Lesson

Enums: CourseStatus

Exceptions: BusinessRuleException (reglas de negocio)

OnlineCourses.Infrastructure

ApplicationDbContext

Repositories: CourseRepository, LessonRepository

Migrations

Reglas de negocio

Estas reglas se validan en el dominio/servicios (no en controllers):

Publicar un curso

✅ Solo se puede publicar si el curso tiene al menos una lección activa (no eliminada).

Crear lección

✅ El campo Order debe ser único dentro del curso.

Soft Delete

✅ Al eliminar un curso/lección no se borra físicamente, se marca como IsDeleted = true.

Endpoints principales
Auth

POST /api/Auth/register

POST /api/Auth/login

Courses (protegidos)

GET /api/Courses/search?q=&status=&page=&pageSize=

GET /api/Courses/{id}

GET /api/Courses/{id}/summary

POST /api/Courses

PUT /api/Courses/{id}

DELETE /api/Courses/{id} (soft delete)

PATCH /api/Courses/{id}/publish

PATCH /api/Courses/{id}/unpublish

Lessons (protegidos)

GET /api/courses/{courseId}/Lessons

POST /api/courses/{courseId}/Lessons

PUT /api/courses/{courseId}/Lessons/{id}

DELETE /api/courses/{courseId}/Lessons/{id} (soft delete)

PATCH /api/courses/{courseId}/Lessons/{id}/reorder

Cómo levantar el backend
Requisitos

.NET SDK 9

Base de datos MySQL (Aiven o local)

1) Configurar conexión a BD

Define tu connection string en tu appsettings.json (ignorado por git).

1) Crear una migración
   dotnet ef migrations add InitialCreate \
   --project ./OnlineCourses.Infrastructure/OnlineCourses.Infrastructure.csproj \
   --startup-project ./OnlineCourses.Api/OnlineCourses.Api.csproj


Si ya tienes una migración, cambia InitialCreate por otro nombre, por ejemplo: AddSoftDeleteFields, FixLessonOrder, etc.

2) Aplicar la migración a la base de datos
   dotnet ef database update \
   --project ./OnlineCourses.Infrastructure/OnlineCourses.Infrastructure.csproj \
   --startup-project ./OnlineCourses.Api/OnlineCourses.Api.csproj

3) Ejecutar API
   dotnet run --project ./OnlineCourses.Api/OnlineCourses.Api.csproj


Swagger:

http://localhost:5258/swagger/index.html

Ejecutar tests
dotnet test


Incluye mínimo 5 tests unitarios:

PublishCourse_WithLessons_ShouldSucceed

PublishCourse_WithoutLessons_ShouldFail

CreateLesson_WithUniqueOrder_ShouldSucceed

CreateLesson_WithDuplicateOrder_ShouldFail

DeleteCourse_ShouldBeSoftDelete

Frontend

El frontend es una SPA sencilla en HTML/CSS/JS.

Funciones

Register con confirmación de password

Login

Guardado del token JWT en localStorage

Listado de cursos con paginación

Filtro Draft/Published

Crear/editar/eliminar cursos

Seleccionar curso para ver lecciones

Crear/editar/eliminar lecciones

Reordenar lecciones (↑ / ↓)

Publicar / despublicar curso

Cómo levantar el frontend
Requisitos

Extensión Live Server en VSCode (recomendado)

Pasos

Abre la carpeta frontend/ en VSCode

Click derecho en index.html

Open with Live Server

Se abrirá algo como:

http://127.0.0.1:5500

o http://127.0.0.1:5501

Asegúrate de que el backend esté corriendo en http://localhost:5258.

Cómo se autentica

Register → crea usuario

Login → recibe token

Se guarda en:

localStorage.setItem("jwt", token)

Cada request protegida usa:

Authorization: Bearer <token>

Notas importantes
CORS

Si el frontend (Live Server) no puede consumir la API, habilita CORS en el backend.

Ejemplo en Program.cs:

builder.Services.AddCors(opt =>
{
opt.AddPolicy("Front", p =>
p.AllowAnyHeader()
.AllowAnyMethod()
.SetIsOriginAllowed(_ => true));
});

app.UseCors("Front");

Soft Delete

Los endpoints DELETE no eliminan registros físicamente. Solo marcan IsDeleted = true.

Autor / Proyecto

OnlineCourses – Proyecto académico para evaluación de consumo de API REST + reglas de negocio + tests + frontend funcional.