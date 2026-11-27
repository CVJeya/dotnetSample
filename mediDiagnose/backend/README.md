# MediDiagnose backend

The backend is a .NET 10 microservice solution:

- Identity and Access owns accounts and issues JWTs.
- Directory owns doctor/patient profiles and doctor shifts.
- Appointments owns booking requests, decisions, and shift capacity.
- The YARP gateway is the frontend-facing API at `http://localhost:8080`.

Each data service reads its own PostgreSQL connection from `DB_URL`. The Compose setup creates separate databases for Identity, Directory, and Appointments. Services apply their EF Core migrations when they start; migrations are not applied by this repository's tests.

## Run locally with Docker Compose

1. Copy `.env.example` to `.env`.
2. Replace the PostgreSQL password, `JWT_KEY`, and `INTERNAL_SERVICES_API_KEY` with local values. Use random values of at least 32 characters for both keys. If the PostgreSQL password contains URI-reserved characters, percent-encode it in each `*_DB_URL`.
3. Start the stack from this directory:

   ```powershell
   docker compose up --build
   ```

4. Open `http://localhost:8080` as the API base URL. Backend services are private to the Compose network; the gateway preserves bearer authorization headers and forwards requests to the owning service.

To create the first administrator, set `INITIAL_ADMIN_EMAIL` and `INITIAL_ADMIN_PASSWORD` in `.env`, then run:

```powershell
docker compose run --rm identity IdentityService.dll --bootstrap-admin
```

The bootstrap operation refuses to create a second administrator. Change or remove the bootstrap credentials after use.

Set `HOSPITAL_TIME_ZONE` to the hospital's Windows or IANA time-zone ID. Appointment slots that fall into nonexistent, ambiguous, or offset-transition intervals are not generated.

## Run backend tests and build

```powershell
dotnet test mediDiagnose.slnx
dotnet build mediDiagnose.slnx
```

Appointment capacity integration tests use an in-memory SQLite database. They do not replace verification against PostgreSQL; run the Compose stack and exercise the migrations in a PostgreSQL-backed environment before deployment.
