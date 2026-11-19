# Hospital Appointment Platform Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a responsive Angular PWA and ASP.NET Core microservices appointment platform under `mediDiagnose\frontend` and `mediDiagnose\backend`.

**Architecture:** The frontend is a single Angular PWA that calls an ASP.NET Core API gateway. The gateway routes to Identity, Hospital Directory, and Appointments services; each service owns its data in PostgreSQL and reads its connection string from `DB_URL`. Appointment requests reserve shift capacity immediately, and the Appointments service enforces the 20-patient limit transactionally.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core, Npgsql, PostgreSQL, YARP, Angular 22, Angular Router, HttpClient, Vitest, xUnit.

**Spec:** `docs/superpowers/specs/2026-11-19-hospital-appointment-platform-design.md`

## Global Constraints

- Use PostgreSQL; each service reads its connection string from the `DB_URL` environment variable. Do not hard-code or commit connection credentials.
- Each service owns its data; services do not read or write another service's database directly.
- The frontend is a responsive Angular PWA. A native iOS/Android app is out of scope for the first version.
- The first version serves one hospital; its time zone is configurable and must not be hard-coded.
- Doctors define working days, shift start/end times, and appointment duration; the system generates available appointment times.
- Capacity is 20 patients per doctor, date, and shift; morning and evening are separate shifts.
- Pending requests reserve capacity. Declining a request releases capacity.
- Patients can browse publicly, self-register, and sign in to request and view appointments.
- Admins approve or decline requests; doctors can view only their own appointments.
- Schedule edits affect unbooked availability only; existing appointments remain intact.
- Do not add clinical records, diagnoses, prescriptions, triage, pharmacy, inpatient care, payments, billing, cancellation, or rescheduling flows.

## Review Focus

- **Concurrent requests for the last places in a shift:** the database must never reserve more than 20; Task 4 pins this with `ConcurrentRequestsForFinalCapacity_NeverExceedsTwenty`.
- **Concurrent requests for the same generated time:** only one request can reserve it; Task 4 pins this with `ConcurrentRequestsForSameSlot_OnlyOneSucceeds`.
- **A shift whose duration is not divisible by appointment duration:** do not generate a slot that ends after shift close; Task 4 pins this with `GeneratedSlots_EndNoLaterThanShiftClose`.
- **A doctor edits a shift after appointments are booked:** existing appointments remain unchanged and no longer-bookable times are not offered for new requests; Task 4 pins this with `ScheduleEdit_PreservesExistingAppointments`.
- **A patient attempts staff or cross-patient operations:** deny approval and access to another patient's appointments; Task 4 pins this with `PatientCannotApproveOrReadAnotherPatientsAppointments`.

---

### Task 1: Creating the microservice workspace and PostgreSQL configuration

**Files:**
- Create: `mediDiagnose\backend\mediDiagnose.slnx`
- Create: `mediDiagnose\backend\Directory.Build.props`
- Create: `mediDiagnose\backend\src\BuildingBlocks\BuildingBlocks.csproj`
- Create: `mediDiagnose\backend\src\BuildingBlocks\Configuration\DatabaseConnectionSettings.cs`
- Create: `mediDiagnose\backend\src\IdentityService\IdentityService.csproj`
- Create: `mediDiagnose\backend\src\IdentityService\Program.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\DirectoryService.csproj`
- Create: `mediDiagnose\backend\src\DirectoryService\Program.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\AppointmentService.csproj`
- Create: `mediDiagnose\backend\src\AppointmentService\Program.cs`
- Create: `mediDiagnose\backend\src\Gateway\Gateway.csproj`
- Create: `mediDiagnose\backend\src\Gateway\Program.cs`
- Create: `mediDiagnose\backend\tests\BuildingBlocks.Tests\BuildingBlocks.Tests.csproj`
- Create: `mediDiagnose\backend\tests\BuildingBlocks.Tests\DatabaseConnectionSettingsTests.cs`
- Create: `mediDiagnose\backend\compose.yaml`
- Create: `mediDiagnose\backend\docker\init-databases.sql`
- Create: `mediDiagnose\backend\.gitignore`

**Interfaces:**
- Produces: `DatabaseConnectionSettings.RequireDbUrl(IConfiguration configuration) -> string`. It accepts either a PostgreSQL URI or an Npgsql connection string from `DB_URL`, returns a normalized Npgsql connection string, or throws `InvalidOperationException` naming `DB_URL` without logging its value.
- Each service reads its own `DB_URL`; local Compose may use one PostgreSQL server with separate databases for Identity, Directory, and Appointments.

- [ ] **Step 1: Write the failing configuration tests** in `DatabaseConnectionSettingsTests.cs`: assert missing and blank `DB_URL` throw an exception that names the variable, an Npgsql connection string is accepted, and a PostgreSQL URI is normalized to the expected host, database, and decoded credentials.
- [ ] **Step 2: Run the tests to verify they fail.**

Run: `dotnet test mediDiagnose\backend\tests\BuildingBlocks.Tests\BuildingBlocks.Tests.csproj --filter FullyQualifiedName~DatabaseConnectionSettingsTests`
Expected: FAIL because `DatabaseConnectionSettings` does not exist.

- [ ] **Step 3: Implement `RequireDbUrl` and scaffold the four .NET 10 applications plus the BuildingBlocks library.** Add nullable reference types, implicit usings, EF Core/Npgsql to the three data services, and YARP to Gateway. Add all projects to the solution and reference BuildingBlocks from each service.
- [ ] **Step 4: Add local PostgreSQL Compose configuration.** Create separate databases for the three services; read the local PostgreSQL password from an untracked environment file and document `DB_URL` for each service without storing credentials in source.
- [ ] **Step 5: Run the tests and build the solution.**

Run: `dotnet test mediDiagnose\backend\tests\BuildingBlocks.Tests\BuildingBlocks.Tests.csproj`
Expected: PASS.

Run: `dotnet build mediDiagnose\backend\mediDiagnose.slnx`
Expected: Build succeeds.

- [ ] **Step 6: Commit the workspace foundation.**

```powershell
git add mediDiagnose\backend
git commit -m "feat: scaffold hospital microservices"
```

### Task 2: Implementing patient authentication and access roles

**Files:**
- Create: `mediDiagnose\backend\src\IdentityService\Domain\UserAccount.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Infrastructure\IdentityDbContext.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Application\RegisterPatient.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Application\AuthenticateUser.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Application\JwtTokenIssuer.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Api\AuthEndpoints.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Api\DoctorAccountEndpoints.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Application\BootstrapAdmin.cs`
- Modify: `mediDiagnose\backend\src\IdentityService\Program.cs`
- Create: `mediDiagnose\backend\src\IdentityService\Migrations\`
- Create: `mediDiagnose\backend\tests\IdentityService.Tests\IdentityService.Tests.csproj`
- Create: `mediDiagnose\backend\tests\IdentityService.Tests\RegistrationTests.cs`
- Create: `mediDiagnose\backend\tests\IdentityService.Tests\AuthenticationTests.cs`
- Create: `mediDiagnose\backend\tests\IdentityService.Tests\AdminProvisioningTests.cs`

**Interfaces:**
- Produces: `POST /api/auth/register` accepts patient name, email, and password; it creates a Patient account and returns `{ accessToken, userId, role }`.
- Produces: `POST /api/auth/login` accepts email and password and returns the same response shape.
- Produces: JWT claims `sub` (user ID) and `role` (`Patient`, `Doctor`, or `Admin`). Patient self-registration must not accept a role from the request. Configure issuer, audience, and the signing key through environment-backed settings shared with the protected services.
- Produces: admin-only `POST /api/admin/doctor-accounts` to create a Doctor account; never allow public Doctor or Admin self-registration.
- Produces: a one-time `--bootstrap-admin` command that reads `INITIAL_ADMIN_EMAIL` and `INITIAL_ADMIN_PASSWORD` from the process environment and refuses to create another Admin if one already exists.

- [ ] **Step 1: Write failing tests** for patient registration, duplicate-email rejection, password hashing (the stored value must not equal the submitted password), invalid-login rejection, inability to self-register as Admin or Doctor, admin-only Doctor account creation, and one-time Admin bootstrap.
- [ ] **Step 2: Run the Identity tests to verify they fail.**

Run: `dotnet test mediDiagnose\backend\tests\IdentityService.Tests\IdentityService.Tests.csproj`
Expected: FAIL because registration and authentication endpoints do not exist.

- [ ] **Step 3: Implement the Identity domain, EF Core persistence, and patient registration.** Normalize email and enforce uniqueness in PostgreSQL; hash passwords with BCrypt.
- [ ] **Step 4: Implement login and JWT issuance.** Read signing keys from environment-backed configuration, require a 32-character minimum, and do not log credentials or token values.
- [ ] **Step 5: Add and apply the Identity EF migration against the Identity database.**
- [ ] **Step 6: Run the Identity tests and commit.**

Run: `dotnet test mediDiagnose\backend\tests\IdentityService.Tests\IdentityService.Tests.csproj`
Expected: PASS.

```powershell
git add mediDiagnose\backend
git commit -m "feat: add patient authentication"
```

### Task 3: Managing doctor and patient profiles and doctor schedules

**Files:**
- Create: `mediDiagnose\backend\src\DirectoryService\Domain\DoctorProfile.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Domain\PatientProfile.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Domain\DoctorShift.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Infrastructure\DirectoryDbContext.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Application\DoctorDirectory.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Application\ManageDoctorSchedule.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Api\DoctorEndpoints.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Api\PatientEndpoints.cs`
- Modify: `mediDiagnose\backend\src\DirectoryService\Program.cs`
- Create: `mediDiagnose\backend\src\DirectoryService\Migrations\`
- Create: `mediDiagnose\backend\tests\DirectoryService.Tests\DirectoryService.Tests.csproj`
- Create: `mediDiagnose\backend\tests\DirectoryService.Tests\DirectoryAccessTests.cs`
- Create: `mediDiagnose\backend\tests\DirectoryService.Tests\DoctorScheduleTests.cs`

**Interfaces:**
- Consumes: `sub` and `role` claims from Task 2.
- Produces: public `GET /api/doctors` and `GET /api/doctors/{doctorId}`; patient-owned `GET/PUT /api/patients/me`; admin-only `POST /api/admin/doctors`, `PUT /api/admin/doctors/{doctorId}`, and `PUT /api/admin/patients/{patientId}`; doctor-owned `GET/PUT /api/doctors/me/shifts`.
- Admins deactivate profiles with `isActive=false`; do not hard-delete profiles or appointment history.
- `PUT /api/patients/me` creates or updates the profile for the authenticated patient. The Angular registration flow calls it after successful Identity registration and makes profile completion retryable if the Directory request fails.
- Doctor profiles store the Identity `userId` as an external identifier; do not create cross-service database foreign keys.
- Admin doctor creation stores the Identity account ID in the Directory profile after the Identity service creates the Doctor account.
- A shift contains weekday, `Morning` or `Evening`, local start and end times, and appointment duration in minutes.
- Produces internal `GET /internal/doctors/{doctorId}/shifts?date=YYYY-MM-DD`, `GET /internal/doctors/by-user/{userId}`, and `GET /internal/patients/{patientId}` for the Appointments service. All internal routes require the `X-Internal-Service-Key` header, configured by `InternalServices__ApiKey` and compared in constant time. Do not include patient profile data in public doctor responses.
- Patient profile creation is available only through the authenticated patient's `PUT /api/patients/me`; admins may list, update, and deactivate those profiles but cannot create a profile without a verified Identity account.

- [ ] **Step 1: Write failing tests** for public doctor browsing, exclusion of private patient data, admin-only profile management, profile deactivation without deleting appointment history, patient ownership of `/patients/me`, Doctor account/profile linkage, and doctor ownership of shift updates.
- [ ] **Step 2: Run the Directory tests to verify they fail.**

Run: `dotnet test mediDiagnose\backend\tests\DirectoryService.Tests\DirectoryService.Tests.csproj`
Expected: FAIL because Directory endpoints and models do not exist.

- [ ] **Step 3: Implement doctor and patient profile persistence and endpoints.** Use role policies and derive the profile owner from the authenticated `sub` claim rather than accepting an owner ID from the caller. Configure JWT validation to interpret the `role` claim as the role claim type.
- [ ] **Step 4: Implement doctor shift creation and update.** Validate weekday, shift, start/end ordering, and positive appointment duration; validate the configured hospital time zone rather than hard-coding one.
- [ ] **Step 5: Add and apply the Directory EF migration and run the Directory tests.**

Run: `dotnet test mediDiagnose\backend\tests\DirectoryService.Tests\DirectoryService.Tests.csproj`
Expected: PASS.

- [ ] **Step 6: Commit the Directory service.**

```powershell
git add mediDiagnose\backend
git commit -m "feat: add hospital directory and schedules"
```

### Task 4: Generating availability and enforcing appointment capacity

**Files:**
- Create: `mediDiagnose\backend\src\AppointmentService\Domain\AppointmentRequest.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Domain\ShiftCapacity.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Application\GenerateAvailability.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Application\RequestAppointment.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Application\DecideAppointment.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Infrastructure\AppointmentsDbContext.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Infrastructure\DirectoryScheduleClient.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Api\AvailabilityEndpoints.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Api\AppointmentEndpoints.cs`
- Modify: `mediDiagnose\backend\src\AppointmentService\Program.cs`
- Create: `mediDiagnose\backend\src\AppointmentService\Migrations\`
- Create: `mediDiagnose\backend\tests\AppointmentService.Tests\AppointmentService.Tests.csproj`
- Create: `mediDiagnose\backend\tests\AppointmentService.Tests\AvailabilityGenerationTests.cs`
- Create: `mediDiagnose\backend\tests\AppointmentService.Tests\AppointmentCapacityTests.cs`
- Create: `mediDiagnose\backend\tests\AppointmentService.Tests\AppointmentAuthorizationTests.cs`

**Interfaces:**
- Consumes: the internal doctor shift, doctor lookup, and patient-profile responses from Task 3 and identity claims from Task 2. Configure `InternalServices__ApiKey` for service-to-service lookups.
- Produces: public `GET /api/availability/doctors/{doctorId}?date=YYYY-MM-DD&shift=Morning|Evening`; `POST /api/appointments` with `{ doctorId, startsAt }` where `startsAt` is an offset-bearing timestamp returned by availability; `GET /api/appointments/mine`; admin-only `GET /api/admin/appointments?status=Pending` and separate `POST /api/admin/appointments/{id}/approve` and `/decline` endpoints; doctor-only `GET /api/doctors/me/appointments`.
- Appointment statuses are `Pending`, `Approved`, and `Declined`. Derive `patientId` from `sub`; never accept it from a booking request body.
- Require an active Directory patient profile before accepting a booking; return patient profile PII only in admin/doctor appointment responses via the authenticated internal Directory route.
- The capacity key is `(doctorId, hospital-local date, shift)`. Count `Pending` and `Approved`; exclude `Declined`.

- [ ] **Step 1: Write failing domain tests** named `GeneratedSlots_EndNoLaterThanShiftClose`, `ReservesPendingRequestsAndCapsEachShiftAtTwenty`, `ConcurrentRequestsForFinalCapacity_NeverExceedsTwenty`, `ConcurrentRequestsForSameSlot_OnlyOneSucceeds`, `DeclinedRequest_ReleasesCapacityAndSlot`, `ScheduleEdit_PreservesExistingAppointments`, `RequestAppointment_DerivesPatientFromClaims`, and `PatientCannotApproveOrReadAnotherPatientsAppointments`.
- [ ] **Step 2: Run the Appointment tests to verify they fail.**

Run: `dotnet test mediDiagnose\backend\tests\AppointmentService.Tests\AppointmentService.Tests.csproj`
Expected: FAIL because slot generation, capacity reservation, and appointment workflows do not exist.

- [ ] **Step 3: Implement slot generation from the Directory shift response.** Generate only complete appointment intervals within the shift and in the configured hospital time zone. Skip slots whose local interval is invalid, ambiguous, or crosses a daylight-saving offset transition.
- [ ] **Step 4: Implement capacity reservation and unique-slot protection in PostgreSQL.** Atomically reserve a shift place and the selected start time in the same transaction before creating a Pending request. Enforce uniqueness only for Pending or Approved requests so a declined slot can be requested again. Return a conflict when the slot is taken or the 20-place limit is reached.
- [ ] **Step 5: Implement patient status, admin decisions, and doctor-specific appointment queries.** Declining a request releases its reservation; schedule edits never rewrite existing appointment records.
- [ ] **Step 6: Add and apply the Appointments EF migration; run the Appointment tests against the test PostgreSQL database.**

Run: `dotnet test mediDiagnose\backend\tests\AppointmentService.Tests\AppointmentService.Tests.csproj`
Expected: PASS, including concurrent-capacity and authorization tests.

- [ ] **Step 7: Commit the Appointments service.**

```powershell
git add mediDiagnose\backend
git commit -m "feat: add appointment booking and approval"
```

### Task 5: Routing services through the API gateway

**Files:**
- Modify: `mediDiagnose\backend\src\Gateway\Program.cs`
- Create: `mediDiagnose\backend\src\Gateway\appsettings.json`
- Create: `mediDiagnose\backend\tests\Gateway.Tests\Gateway.Tests.csproj`
- Create: `mediDiagnose\backend\tests\Gateway.Tests\GatewayRoutingTests.cs`
- Modify: `mediDiagnose\backend\compose.yaml`
- Create: `mediDiagnose\backend\README.md`

**Interfaces:**
- Consumes: the public API routes from Tasks 2-4.
- Produces: a single `/api` entry point. Route `/api/auth/*` to Identity, `/api/doctors/*` and `/api/patients/*` to Directory, `/api/availability/*` and `/api/appointments/*` to Appointments, `/api/admin/appointments/*` to Appointments, `/api/admin/doctors` and `/api/admin/doctors/*` to Directory, and `/api/admin/patients` and `/api/admin/patients/*` to Directory.
- Route `/api/admin/doctor-accounts/*` to Identity. Route the exact `/api/doctors/me/appointments` path to Appointments before the broader `/api/doctors/*` route to Directory.

- [ ] **Step 1: Write failing gateway routing tests** for each route group, admin doctor/patient routes, the exact doctor-appointments route taking precedence over the Directory wildcard, and preserving Authorization headers through the gateway.
- [ ] **Step 2: Run the Gateway tests to verify they fail.**

Run: `dotnet test mediDiagnose\backend\tests\Gateway.Tests\Gateway.Tests.csproj`
Expected: FAIL because the gateway routes are not configured.

- [ ] **Step 3: Configure YARP routes and destinations** using environment-backed service URLs; configure JWT authentication and authorization on service endpoints, not only at the gateway.
- [ ] **Step 4: Complete local Compose configuration** so all services use their own PostgreSQL database and `DB_URL`; keep local credentials in an untracked environment file.
- [ ] **Step 5: Document local startup, required environment variables, migrations, and the gateway base URL in `mediDiagnose\backend\README.md`.**
- [ ] **Step 6: Run gateway tests and build all backend projects.**

Run: `dotnet test mediDiagnose\backend\mediDiagnose.slnx`
Expected: PASS.

Run: `dotnet build mediDiagnose\backend\mediDiagnose.slnx`
Expected: Build succeeds.

- [ ] **Step 7: Commit the gateway and local run configuration.**

```powershell
git add mediDiagnose\backend
git commit -m "feat: route hospital APIs through gateway"
```

### Task 6: Building the Angular PWA and patient booking flow

**Files:**
- Create: `mediDiagnose\frontend\package.json`
- Create: `mediDiagnose\frontend\angular.json`
- Create: `mediDiagnose\frontend\src\app\core\auth\auth.service.ts`
- Create: `mediDiagnose\frontend\src\app\core\auth\auth.interceptor.ts`
- Create: `mediDiagnose\frontend\src\app\core\auth\auth.guard.ts`
- Create: `mediDiagnose\frontend\src\app\core\api\doctor.service.ts`
- Create: `mediDiagnose\frontend\src\app\core\api\patient.service.ts`
- Create: `mediDiagnose\frontend\src\app\core\api\appointment.service.ts`
- Create: `mediDiagnose\frontend\src\app\app.routes.ts`
- Create: `mediDiagnose\frontend\src\app\features\doctors\doctor-list.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\doctors\doctor-availability.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\auth\register.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\auth\login.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\appointments\booking.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\appointments\my-appointments.component.ts`
- Create: corresponding `*.spec.ts` files for the services and feature components.

**Interfaces:**
- Consumes: the gateway routes from Task 5.
- Produces: public doctor browsing/detail/availability pages, patient self-registration/login and profile completion, appointment requests, and a patient appointment-status page showing Pending, Approved, or Declined.
- Use Angular standalone components, Router, HttpClient, injectable services, and typed request/response models. Protect patient routes with a guard for navigation while relying on the API for authorization.

- [ ] **Step 1: Create the Angular application and write failing tests** for patient registration/login service calls, profile completion after registration, public doctor browsing, booking submission, and rendering each appointment status.
- [ ] **Step 2: Run the Angular tests to verify they fail.**

Run: `npm test -- --watch=false`
Expected: FAIL because the patient feature services and pages do not exist.

- [ ] **Step 3: Implement the Angular app shell and PWA configuration.** Add a manifest and service worker; cache only the application shell, never appointment or patient API responses.
- [ ] **Step 4: Implement auth and API services.** Attach bearer tokens to protected API calls, handle 401/403 responses visibly, and avoid logging credentials or tokens.
- [ ] **Step 5: Implement public doctor/availability pages and patient booking/status pages.** After registration, create the Directory patient profile; show a retryable profile-completion state if that request fails. Make the layout responsive for desktop and phone browsers.
- [ ] **Step 6: Run Angular tests and production build.**

Run: `npm test -- --watch=false`
Expected: PASS.

Run: `npm run build`
Expected: Production build succeeds.

- [ ] **Step 7: Commit the patient PWA flow.**

```powershell
git add mediDiagnose\frontend
git commit -m "feat: add patient appointment PWA"
```

### Task 7: Building the admin and doctor workspaces

**Files:**
- Create: `mediDiagnose\frontend\src\app\features\admin\appointment-queue.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\admin\doctor-management.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\admin\patient-management.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\doctor\doctor-schedule.component.ts`
- Create: `mediDiagnose\frontend\src\app\features\doctor\doctor-appointments.component.ts`
- Create: `mediDiagnose\frontend\src\app\core\api\admin.service.ts`
- Create: `mediDiagnose\frontend\src\app\core\api\staff.service.ts`
- Modify: `mediDiagnose\frontend\src\app\app.routes.ts`
- Create: corresponding `*.spec.ts` files for admin and doctor feature components.

**Interfaces:**
- Consumes: role claims and staff API routes from Tasks 2-5.
- Produces: an Admin workspace for doctor-account/profile creation, patient-profile management, and appointment approval/decline, and a Doctor workspace for editing shifts and viewing only that doctor's appointment/patient list.

- [ ] **Step 1: Write failing component tests** proving Admin users see the approval and directory management views, Doctor users see their schedule and appointments, and Patient users cannot access either staff workspace.
- [ ] **Step 2: Run the frontend tests to verify they fail.**

Run: `npm test -- --watch=false`
Expected: FAIL because the staff routes and components do not exist.

- [ ] **Step 3: Implement role-specific routes and admin pages.** Create the Doctor account through Identity before creating its Directory profile; expose a retry path if profile creation fails. Show patient-profile forms, allow profile deactivation without deleting history, and require server confirmation for approval or decline.
- [ ] **Step 4: Implement doctor schedule and appointment pages.** Display morning/evening schedule controls and the doctor's own booked patients; keep existing appointments visible after future schedule edits.
- [ ] **Step 5: Run all frontend tests and the production build.**

Run: `npm test -- --watch=false`
Expected: PASS.

Run: `npm run build`
Expected: Production build succeeds.

- [ ] **Step 6: Verify the full local workflow** using Compose and the PWA: public browsing, patient registration, generated availability, capacity conflict, pending status, admin approval/decline, patient status, and doctor-specific appointment list.
- [ ] **Step 7: Commit the staff workspaces.**

```powershell
git add mediDiagnose\frontend
git commit -m "feat: add hospital staff workspaces"
```
