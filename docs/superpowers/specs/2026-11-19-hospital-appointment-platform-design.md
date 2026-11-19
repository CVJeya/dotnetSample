# Hospital Appointment Platform Design

## Intent

Create a single-hospital appointment platform for patients, doctors, and hospital staff. Patients should be able to find doctors and available appointment times; hospital admins approve requests; doctors see the patients booked with them.

## Goals

- Provide one responsive Angular Progressive Web App (PWA) for desktop and mobile browsers.
- Let anyone browse doctor profiles and availability; allow patients to self-register and require sign-in to request or view appointments.
- Let doctors define their working days, morning/evening shift hours, and appointment duration. Generate appointment times from that schedule.
- Enforce a maximum of 20 patients per doctor, per date, per shift. A pending appointment request reserves capacity. Declining a request releases it.
- Require a hospital admin to approve or decline every request. Let doctors view their own appointments and their patient list.
- Provide role-based access for patients, doctors, and admins, enforced by the ASP.NET APIs.
- Implement the backend as independently deployable ASP.NET Core microservices with clear ownership of data and business rules.

## Non-goals

- Clinical records, diagnoses, prescriptions, triage, pharmacy, inpatient care, payments, or billing.
- A separate native iOS/Android application or app-store release in the first version.
- A multi-hospital or multi-tenant system.
- Patient cancellation or rescheduling workflows unless added after this scope is approved.

## Anti-goals

- Do not let the Angular app access databases directly or rely on client-side guards for authorization.
- Do not allow pending requests to cause a doctor’s shift to exceed its 20-patient capacity.
- Do not split the backend into many tiny services or share database tables across services.
- Do not collect or store clinical details that are not required to schedule and manage appointments.

## Core workflows

1. A visitor browses doctor profiles and published availability without signing in.
2. A patient self-registers or signs in, chooses a doctor and an available generated time, and submits a request.
3. The appointment service records the request as pending and reserves one place in that doctor’s shift. It rejects a request if the time is unavailable or the shift has reached 20 patients.
4. An admin reviews pending requests and approves or declines them. Declining releases the reserved place.
5. A patient can view their appointment requests and whether each is pending, approved, or declined.
6. A doctor views appointments and patient details needed for those appointments, limited to that doctor’s own schedule.
7. Doctors maintain their working days, shift hours, and appointment duration. The system generates available appointment times and does not offer a time already reserved or approved. Schedule edits affect unbooked availability only; existing appointments remain intact.

## Proposed architecture

### Frontend

Use Angular as a responsive PWA for web and mobile browser use. Patients, doctors, and admins use the same application with role-appropriate pages. Use Angular services for API access and route guards for navigation; the backend remains authoritative for all access checks.

### Backend services

Use three services as the initial boundary:

- **Identity and Access:** accounts, sign-in, and patient/doctor/admin roles; issues access tokens.
- **Hospital Directory:** doctor and patient profiles, plus doctor working days and shift definitions.
- **Appointments:** generated availability, appointment requests, approval status, capacity reservations, and doctor-specific appointment lists.

Use PostgreSQL for persistence. Each service reads its database connection string from the `DB_URL` environment variable; do not hard-code or commit connection credentials. Each service owns its data and exposes APIs; services must not read or write another service’s database directly. The appointment service is the source of truth for reservations and the 20-patient capacity rule. Use a single frontend-facing API entry point to route requests to services. The concrete gateway and service-to-service communication mechanism are implementation choices.

Within each service, use a small layered structure that separates HTTP/API concerns, application use cases, and domain rules from persistence. Use dependency injection, request/response DTOs, validation, and automated tests. Apply repository abstractions only where they improve isolation or testability; avoid adding patterns without a concrete need.

## Security and privacy constraints

- Require authentication and role checks on protected API operations; a frontend route guard is not an authorization boundary.
- Hash passwords with an established password-hashing library; keep signing keys and database credentials out of source control.
- Validate all client input on the server and use parameterized ORM/database operations.
- Return only the patient information required for a doctor’s own appointments and the admin’s scheduling work.
- Record who approved or declined an appointment and when.
- Do not claim compliance with a particular healthcare regulation until the deployment jurisdiction and applicable requirements are known.

## Approaches considered

### Responsive Angular PWA — selected

One Angular application adapts to desktop and phone layouts and can be installed from a browser where supported. This meets the described booking and staff workflows with one frontend to build and maintain.

### Ionic Angular with Capacitor

This would package an Angular-based mobile experience for iOS and Android and support deeper native integrations. It adds mobile-specific build, testing, and release work that is not needed for the workflows currently described.

### Microservice boundaries

The proposed three-service split groups profiles and schedules in the Hospital Directory while keeping appointment reservations and approvals in a dedicated service. Splitting doctor profiles, patient profiles, schedules, and appointments into separate services could allow more independent scaling, but adds deployment and cross-service coordination overhead. The three-service split is the recommended starting point; it is a design recommendation, not a confirmed user constraint.

## Constraints and decisions

- Capacity is 20 patients per doctor, date, and shift; morning and evening are separate shifts.
- PostgreSQL is the database engine, and services use the `DB_URL` environment variable for their connection string.
- The doctor defines working days, shift start/end times, and appointment duration; the system generates times from those values.
- A pending request immediately reserves capacity; a declined request releases it.
- Patients can browse publicly, self-register, and sign in to request and view appointments.
- An admin manually approves or declines requests.
- Doctors can view only their own appointments.
- Schedule edits affect unbooked availability only; existing appointments remain intact.
- The app is for one hospital in the first version; its time zone must be configured rather than hard-coded.

## Left to the implementation team

- Select the database engine and concrete API gateway/service communication technology based on deployment constraints.
- Define the minimum fields needed for doctor and patient profiles; do not add clinical data.
- Define appointment slot generation when the configured shift duration does not divide evenly by the appointment duration.
- Configure the hospital’s time zone and define its policy for daylight-saving transitions.
- Decide how doctor accounts are provisioned and how the first admin is created securely.
- Keep cancellation, rescheduling, notifications, and appointment reminders out of scope until their rules are specified.
