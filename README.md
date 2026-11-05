# Servicio de Identidad SmartPalm

Microservicio independiente .NET 10 dueño de los usuarios, roles, planes de suscripción, suscripciones, pagos, emisión de JWT y outbox de eventos de integración de Identity.

## Rol en la plataforma

Identity es la fuente de confianza y suscripciones de toda la plataforma. Crea las cuentas de Administrator y PalmGrower, asigna roles y planes, activa suscripciones mediante pagos y emite JWT para los clientes humanos. Device, Crop, Ingestion, Alert, Agronomy y Field dependen de sus eventos de integración o claims proyectados; no comparten su base de datos.

La demostración física comienza aquí: un Administrator inicia sesión, crea al PalmGrower y activa un plan antes de registrar una plantación, gateway y sensor. Un plan inactivo es una regla de negocio que Ingestion debe respetar.

## Endpoints de compatibilidad

- `POST /api/v1/Authentication/sign-in`
- `GET /api/v1/users` and `GET /api/v1/users/{id}`
- `GET /api/v1/subscriptions/plans`
- `GET|DELETE /api/v1/subscriptions`
- `GET /api/v1/subscriptions/payments`
- `/api/v1/admin/users/*`
- `/api/v1/admin/subscriptions/*`

## Ejecución local

1. Copia `.env.example` como `.env` y reemplaza todos los valores de ejemplo.
2. Ejecuta `docker compose --env-file .env -f compose.local.yaml up --build`.
3. Abre `http://localhost:5101/swagger` o `http://localhost:5101/health/ready`.

RabbitMQ es opcional para el desarrollo aislado. Configura `RABBITMQ_ENABLED=true` y ejecuta
`docker compose --env-file .env -f compose.local.yaml --profile messaging up --build`
para iniciar un broker local y despachar los mensajes pendientes del outbox.

La API aplica las migraciones de EF Core al iniciar. La entrega mediante RabbitMQ utiliza un outbox transaccional; los mensajes permanecen pendientes cuando el broker no está disponible.

## Comandos de desarrollo

```powershell
dotnet restore SmartPalm.IdentityService.slnx
dotnet build SmartPalm.IdentityService.slnx --no-restore
dotnet test SmartPalm.IdentityService.slnx --no-build
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName --project SmartPalm.IdentityService.API --startup-project SmartPalm.IdentityService.API --output-dir Infrastructure/Persistence/EFC/Migrations
```

Las pruebas unitarias y de arquitectura se ejecutan sin contenedores. Para ejecutar también
la prueba de integración de PostgreSQL, RabbitMQ y outbox transaccional, inicia Docker y ejecuta:

```powershell
$env:RUN_TESTCONTAINERS = "true"
dotnet test SmartPalm.IdentityService.slnx --configuration Debug
```

