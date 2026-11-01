# MyCheckpoint

Red social de seguimiento y crítica de videojuegos. Cada usuario puede llevar su biblioteca de juegos, puntuarlos y reseñarlos, crear listas, seguir a otros usuarios y consultar rankings. La información de los videojuegos procede de la API de [RAWG](https://rawg.io/apidocs).

Trabajo de Fin de Grado de Rodrigo Lozano Fraile, Grado en Ingeniería Informática, Universidad de Málaga.

## Tecnologías

- **Backend:** ASP.NET Core (.NET 10), Entity Framework Core, ASP.NET Core Identity y JWT.
- **Base de datos:** PostgreSQL.
- **Frontend:** Angular 21 con Tailwind CSS.
- **Pruebas:** xUnit y Moq.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 22 o superior (probado con la 24)
- [PostgreSQL](https://www.postgresql.org/download/) (probado con la 18)
- Una clave gratuita de la API de RAWG, que se obtiene registrándose en https://rawg.io/apidocs

## Puesta en marcha

### 1. Clonar el repositorio

```bash
git clone https://github.com/rodrigoLFuma/MyCheckpoint.git
cd MyCheckpoint
```

### 2. Crear la base de datos

Crea una base de datos vacía en PostgreSQL, por ejemplo con `psql`:

```bash
psql -U postgres -c "CREATE DATABASE mycheckpoint;"
```

### 3. Configurar el backend

Los datos sensibles no están en el repositorio. Se guardan en los secretos de usuario de .NET, desde la carpeta del proyecto de la API:

```bash
cd Backend/MyCheckpoint
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=mycheckpoint;Username=postgres;Password=TU_CONTRASEÑA"
dotnet user-secrets set "Jwt:Key" "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES"
dotnet user-secrets set "Rawg:ApiKey" "TU_CLAVE_DE_RAWG"
```

Sustituye los tres valores por los tuyos. La clave de JWT puede ser cualquier cadena larga y aleatoria.

### 4. Crear las tablas

Sube a la carpeta `Backend` y aplica las migraciones. Si no tienes la herramienta `dotnet-ef`, instálala antes:

```bash
dotnet tool install --global dotnet-ef
cd ..
dotnet ef database update --project MyCheckpoint.Data --startup-project MyCheckpoint
```

### 5. Arrancar el backend

Desde la carpeta `Backend`, confía una vez en el certificado de desarrollo de HTTPS y arranca la API con el perfil `https`:

```bash
dotnet dev-certs https --trust
cd MyCheckpoint
dotnet run --launch-profile https
```

La API queda disponible en `https://localhost:7018` y su documentación en `https://localhost:7018/swagger`.

En el primer arranque, y solo si la base de datos está vacía, la aplicación crea los roles, una cuenta de administrador y un conjunto de datos de demostración (usuarios, juegos, reseñas, listas, seguimientos y notificaciones).

### 6. Arrancar el frontend

En otra terminal, desde la raíz del repositorio:

```bash
cd Frontend
npm install
npm start
```

La aplicación queda disponible en `http://localhost:4200`. En desarrollo se conecta a la API en `https://localhost:7018`, por lo que el backend debe estar arrancado.

## Cuentas de demostración

| Usuario | Contraseña | Rol |
|---|---|---|
| `Admin` | `Admin1234` | Administrador |
| `NeonRider87` | `NeonRider871234` | Administrador |
| `PixelQueen` | `PixelQueen1234` | Usuario |

Todos los usuarios de demostración usan como contraseña su nombre de usuario seguido de `1234`. Se puede iniciar sesión con el nombre de usuario o con el correo.

## Pruebas

Las pruebas automatizadas de la lógica de negocio del backend se ejecutan desde la carpeta `Backend`:

```bash
dotnet test
```

## Estructura del repositorio

```
Backend/
  MyCheckpoint/        API: controladores, servicios, DTO y configuración
  MyCheckpoint.Data/   Entidades, DbContext, repositorios y migraciones
  MyCheckpoint.Tests/  Pruebas automatizadas
Frontend/              Aplicación Angular
docs/                  Documentación del proyecto
```

## Notas

- Sin la clave de RAWG la aplicación arranca y muestra los datos de demostración, pero no funcionan la búsqueda de juegos nuevos ni su importación.
- Las fotos de perfil se guardan en `Backend/MyCheckpoint/wwwroot/uploads`, que no se versiona.
- El cliente de la API del frontend se genera a partir del Swagger del backend. Para regenerarlo, con el backend en marcha, ejecuta `npm run generate-api` desde la carpeta `Frontend`.
