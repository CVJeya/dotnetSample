# Teaching Graduating Students to Build a Startup Application

## A practical instructor's guide using ASP.NET Core, C#, a relational database, and a modern frontend

**Audience:** Final-year students and recent graduates\
**Outcome:** Students can work as a small product team and deliver a
usable, secure, tested, deployable MVP for a startup-style problem.\
**Suggested duration:** 8 weeks (adaptable to 6--10 weeks)\
**Teaching principle:** Teach the product-development process first.
Introduce frameworks, architecture, and design patterns when students
encounter a real problem that needs them.

------------------------------------------------------------------------

## 1. What students should be able to do

By the end of the programme, students should be able to:

1.  Understand a business problem and identify its users.
2.  Convert a problem into user stories and acceptance criteria.
3.  Plan a small MVP instead of trying to build every possible feature.
4.  Design a database and explain relationships and constraints.
5.  Build REST APIs using ASP.NET Core Web API.
6.  Build a frontend using Angular or React and connect it to the API.
7.  Use Git and collaborate through branches, pull requests, and code
    reviews.
8.  Validate input, handle errors, authenticate users, and protect data.
9.  Write unit and integration tests.
10. Deploy the application and diagnose basic production issues.
11. Explain their design decisions, trade-offs, and what they would
    improve next.

Students do not need to master every technology before starting. They
need to learn how to make progress safely, ask good questions, test
their work, and deliver in small increments.

------------------------------------------------------------------------

## 2. The teaching approach

Use a single realistic project throughout the course. Avoid teaching
each technology as an isolated subject with unrelated exercises.

A useful example is a **small appointment-booking platform**:

-   Customers can register and sign in.
-   Customers can view available services and time slots.
-   Customers can book, view, and cancel appointments.
-   Staff can manage services and availability.
-   An administrator can manage staff and view bookings.

Other suitable projects include a campus placement portal, local-service
marketplace, inventory application, expense tracker, or
learning-management application.

### Teach in this order

1.  Problem and users
2.  MVP and requirements
3.  User interface sketches and user journeys
4.  Database and API design
5.  A complete vertical feature from frontend to database
6.  Validation, security, and error handling
7.  Testing and code review
8.  Deployment and monitoring
9.  Refactoring and design patterns

**Important:** Do not begin with all 30 design patterns, microservices,
Kubernetes, or advanced cloud architecture. A startup MVP usually
benefits more from a simple, understandable application that can be
changed quickly.

------------------------------------------------------------------------

## 3. The project students will build

### Project brief: Appointment Booking MVP

**Problem statement**

Customers currently need to contact a service provider manually to find
an available appointment. The startup wants a web application that makes
booking easier and allows staff to manage availability.

### User roles

  -----------------------------------------------------------------------
  Role                                Main capabilities
  ----------------------------------- -----------------------------------
  Customer                            Register, sign in, browse services,
                                      book, view and cancel own
                                      appointments

  Staff                               View assigned appointments and
                                      manage availability

  Administrator                       Manage services, staff, and
                                      bookings
  -----------------------------------------------------------------------

### MVP scope

Build only the features needed to prove the core workflow:

-   User registration and login
-   View active services
-   View available time slots
-   Create an appointment
-   View the customer's appointments
-   Cancel an appointment subject to a simple business rule
-   Staff view of daily appointments

### Keep these out of the first release

-   Online payments
-   SMS and WhatsApp notifications
-   Recommendation engines
-   Multiple branches and complex scheduling
-   Microservices
-   Advanced analytics

These can become future enhancements after the core booking flow works.

### Define acceptance criteria

Example user story:

> As a customer, I want to book an available appointment so that I can
> reserve a time without calling the service provider.

Acceptance criteria:

-   The customer must be signed in.
-   The selected service must be active.
-   The selected slot must be in the future.
-   The slot must still be available when the booking is saved.
-   A successful booking must appear in the customer's appointment list.
-   If another customer has already booked the slot, the API must return
    a clear conflict response.

Students should learn that acceptance criteria are testable statements,
not vague descriptions such as "booking should work properly."

------------------------------------------------------------------------

## 4. Suggested 8-week teaching plan

Each week should include a short explanation, a live instructor
demonstration, a student task, code review, and a working deliverable.

### Week 1 --- Think like a product team

**Teach**

-   What a startup product is and who its users are
-   Problem statements, assumptions, and user personas
-   MVP versus a complete product
-   User journeys and simple wireframes
-   User stories and acceptance criteria
-   Prioritisation: must-have, should-have, could-have
-   Basic Scrum/Kanban concepts and task boards

**Class activity**

Ask students to interview or role-play a customer. Have them write the
top five problems and choose one problem to solve.

**Deliverable**

-   One-page product brief
-   User roles
-   MVP feature list
-   User stories with acceptance criteria
-   Basic wireframes

**Check understanding**

Can a student explain who has the problem, why it matters, and what the
first version will do?

### Week 2 --- Git, development setup, and database design

**Teach**

-   Git repository, commit, branch, merge, pull request
-   Meaningful commit messages
-   `.gitignore` and why secrets must not be committed
-   Relational database concepts
-   Tables, primary keys, foreign keys, unique constraints
-   One-to-one, one-to-many, and many-to-many relationships
-   Basic SQL: `SELECT`, `INSERT`, `UPDATE`, `DELETE`, joins
-   Entity Framework Core and migrations

**Suggested initial tables**

-   `Users`
-   `Roles` or the chosen identity model
-   `Services`
-   `Staff`
-   `AvailabilitySlots`
-   `Appointments`

Discuss which data belongs in each table and why an appointment should
refer to a customer, service, staff member, and slot.

**Student task**

Create the database schema, add sample data, and practise common
queries.

**Deliverable**

-   Shared Git repository
-   Database diagram
-   Initial migrations
-   Seed/sample data

### Week 3 --- Backend fundamentals with ASP.NET Core Web API

**Teach**

-   What HTTP and REST APIs are
-   Request and response
-   GET, POST, PUT/PATCH, DELETE
-   Status codes: 200, 201, 204, 400, 401, 403, 404, 409, 500
-   Controllers or Minimal APIs (choose one approach for the course)
-   DTOs and why database entities should not automatically become API
    contracts
-   Dependency injection and constructor injection
-   Configuration and connection strings
-   EF Core queries and asynchronous methods
-   Model validation and consistent error responses
-   Swagger/OpenAPI

**Build**

Start with the Services feature:

1.  `GET /api/services`
2.  `GET /api/services/{id}`
3.  `POST /api/services` (admin-only later)
4.  `PUT /api/services/{id}`
5.  `DELETE /api/services/{id}` or deactivate it

Explain the flow:

`HTTP request → Controller → Application/service logic → Repository or DbContext → Database → HTTP response`

Do not introduce repository and unit-of-work abstractions automatically
just because they are patterns. First explain the data-access problem
and decide whether an abstraction adds value for this project.

**Deliverable**

A running API with service CRUD, validation, Swagger, and database
persistence.

### Week 4 --- Frontend and end-to-end feature development

Choose **one** frontend framework for the cohort: Angular or React. Do
not require students to learn both during the same short programme.

**Teach**

-   Components and component composition
-   Routing
-   Forms and validation
-   Calling APIs
-   Loading, success, empty, and error states
-   State management basics
-   Reusable UI components
-   Accessibility basics and responsive layouts

**Build**

Create the customer screens:

-   Service list
-   Service details
-   Available slots
-   Booking form
-   My appointments

Connect each screen to the real API. Avoid hard-coded sample data once
the API is available.

**Deliverable**

A user can browse services from the database through the frontend.

### Week 5 --- Complete the core business workflow

**Teach**

-   Business rules and where they belong
-   Preventing duplicate bookings
-   Database transactions and constraints
-   Race conditions: two users attempting to book the same slot
-   DTO mapping and validation
-   Pagination, filtering, and sorting basics
-   Logging and exception handling

**Build**

Complete the booking workflow from browser to database.

A crucial teaching point: checking that a slot is free in the UI is not
enough. The backend and database must protect the booking operation
against simultaneous requests. Discuss unique constraints or another
appropriate concurrency strategy.

**Deliverable**

A customer can book, view, and cancel an appointment according to the
business rules.

### Week 6 --- Authentication, authorisation, security, and testing

**Teach security**

-   Authentication: who are you?
-   Authorisation: what are you allowed to do?
-   Password hashing and established identity libraries
-   Secure token/cookie handling appropriate to the chosen architecture
-   Role- or policy-based authorisation
-   Input validation and output handling
-   HTTPS
-   CORS and its purpose
-   SQL injection and parameterised queries/ORM usage
-   Secrets in configuration or a secret store, not source control
-   Least privilege and protection of personal data

Do not ask students to invent their own password hashing or
authentication protocol.

**Teach testing**

-   Unit tests for business rules
-   Integration tests for API/database behaviour
-   Testing validation and error cases
-   Test data and test isolation
-   Manual API testing with Swagger or an API client
-   Frontend component tests where time permits

**Student task**

Write tests for booking rules, including: - Booking a free slot
succeeds. - Booking an already reserved slot fails. - An unauthorised
user cannot view another customer's bookings. - Invalid input is
rejected. - A customer cannot cancel someone else's appointment.

**Deliverable**

Authentication and role-based access, plus a meaningful automated test
suite.

### Week 7 --- Deployment and operational readiness

**Teach**

-   Development, test/staging, and production environments
-   Build versus run
-   Environment-specific configuration
-   Database migrations during deployment
-   Hosting choices for frontend, API, and database
-   CI basics: restore, build, test
-   CD basics: deploy after checks pass
-   Application logs and health checks
-   Basic monitoring and error diagnosis
-   Backups and recovery
-   Rollback strategy

Use a simple hosting setup first. A single deployable ASP.NET Core
application with a managed database is often a sensible MVP
architecture. Explain that hosting choices depend on budget, expected
traffic, data sensitivity, and team capability.

**Deliverable**

A deployed application with a working public test URL, production-like
configuration, and a deployment README.

### Week 8 --- Product demo, review, and next steps

**Teach**

-   Demo preparation
-   Explaining architecture and design decisions
-   Gathering user feedback
-   Prioritising bugs and feature requests
-   Technical debt
-   Release checklist
-   Basic product metrics

**Final presentation**

Each team should demonstrate:

1.  The problem and intended users
2.  The MVP scope
3.  A complete user journey
4.  The architecture and database
5.  API examples
6.  Security and testing
7.  Deployment
8.  Known limitations and next improvements

**Deliverable**

A working MVP, source code, tests, deployment instructions, and a short
technical presentation.

------------------------------------------------------------------------

## 5. How to teach application architecture

Start with a **modular monolith**: one deployable backend application
organised into clear modules. It is easier for new graduates to
understand, test, debug, and deploy than a collection of microservices.

A possible backend structure:

``` text
AppointmentBooking.Api/
├── Controllers/
├── Application/
│   ├── Appointments/
│   ├── Services/
│   └── Users/
├── Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Rules/
├── Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   └── ExternalServices/
├── Contracts/
│   ├── Requests/
│   └── Responses/
├── Middleware/
├── Tests/
└── Program.cs
```

Explain the responsibilities:

-   **API layer:** receives HTTP requests, validates request shape, and
    returns HTTP responses.
-   **Application layer:** coordinates use cases such as booking an
    appointment.
-   **Domain layer:** holds important business rules and domain
    concepts.
-   **Infrastructure layer:** handles database access, email providers,
    file storage, and other external systems.
-   **Contracts:** defines what enters and leaves the API.

The exact structure can vary. The important rule is to keep
responsibilities understandable and avoid putting the entire application
inside controllers.

### Teach one vertical slice at a time

A vertical slice is a small feature implemented end to end.

For example, "Book an appointment" includes:

1.  A booking form in the frontend.
2.  A POST request to the API.
3.  Request validation.
4.  Business-rule checks.
5.  A database write.
6.  A response with the booking identifier.
7.  A frontend success or error message.
8.  Automated tests.

This teaches students how real application work is delivered.

------------------------------------------------------------------------

## 6. How to teach design patterns without overwhelming students

Students should understand the **problem first**, then the pattern, then
the trade-offs. A pattern is a reusable way of organising a solution; it
is not a requirement to add more classes to every feature.

Use the 30 patterns in the existing learning solution as a reference
catalogue. Do not require students to implement all 30 in the startup
MVP.

### A. Creational patterns

  -----------------------------------------------------------------------
  Pattern                 Teaching example        When it is useful
  ----------------------- ----------------------- -----------------------
  Singleton               Shared stateless        One shared instance is
                          application service or  genuinely appropriate;
                          framework-managed       discuss thread safety
                          singleton               

  Factory Method          Create a notification   Creation logic varies
                          sender based on a       by type
                          chosen channel          

  Abstract Factory        Create a family of      Multiple compatible
                          related provider        product families are
                          objects                 needed

  Builder                 Construct a complex     Many optional
                          report/export request   construction steps
                                                  exist

  Prototype               Clone a configurable    Copying an existing
                          template                configured object is
                                                  useful
  -----------------------------------------------------------------------

### B. Structural patterns

  -----------------------------------------------------------------------
  Pattern                 Teaching example        When it is useful
  ----------------------- ----------------------- -----------------------
  Adapter                 Wrap a third-party      External API does not
                          SMS/email API behind an match the application's
                          application interface   interface

  Bridge                  Separate notification   Two dimensions should
                          abstraction from        vary independently
                          delivery mechanism      

  Composite               Represent nested        Treat a group and an
                          service categories or   individual item
                          menus                   uniformly

  Decorator               Add logging or caching  Add behaviour without
                          around a service        changing the original
                                                  class

  Facade                  Provide one simple      Hide complex subsystem
                          operation for a         interactions
                          multi-step booking      
                          workflow                
  -----------------------------------------------------------------------

### C. Behavioral patterns

  -----------------------------------------------------------------------
  Pattern                 Teaching example        When it is useful
  ----------------------- ----------------------- -----------------------
  Observer                Publish an              Multiple subscribers
                          appointment-created     react to an event
                          event to notification   
                          listeners               

  Strategy                Different cancellation  An algorithm varies by
                          or pricing policies     context

  Command                 Represent a             Actions need queuing,
                          booking/cancellation    logging, retry, or undo
                          action as an object     

  Chain of Responsibility Run a sequence of       Several handlers may
                          booking validations     process or reject a
                                                  request

  State                   Appointment lifecycle:  Behaviour depends on
                          Pending, Confirmed,     lifecycle state
                          Cancelled               

  Template Method         Common                  Overall steps are fixed
                          report-generation       but some steps vary
                          workflow with different 
                          formats                 

  Memento                 Restore an edited draft Previous state must be
                          or configuration        captured and restored

  Iterator                Traverse a custom       Collection traversal
                          collection              should be separated
                                                  from storage

  Visitor                 Apply different         New operations are
                          operations to a stable  added more often than
                          object structure        object types

  Interpreter             Evaluate a small rule   A small language or
                          expression              rule grammar is
                                                  genuinely required
  -----------------------------------------------------------------------

### D. Architectural and enterprise patterns

  ------------------------------------------------------------------------------
  Pattern                 Teaching example        Important note
  ----------------------- ----------------------- ------------------------------
  Repository              Isolate persistence     EF Core `DbContext` already
                          operations behind an    provides
                          interface               repository/unit-of-work-like
                                                  behaviour; add a custom
                                                  repository when it creates
                                                  value

  Unit of Work            Coordinate changes that Explain transaction boundaries
                          must be saved together  and EF Core `SaveChanges`

  CQRS                    Separate booking        Useful when read/write models
                          commands from           or requirements differ; not
                          appointment queries     mandatory for every CRUD app

  Mediator                Send a use case request Helps decouple callers and
                          to its handler          handlers; avoid unnecessary
                                                  indirection

  Specification           Reusable appointment    Keep specifications
                          search/filter rules     understandable and testable

  Proxy                   Cache or protect access Explain cache expiry and
                          to an underlying        invalidation
                          service                 

  Dependency Injection    Register services and   Teach constructor injection
  Container               inject dependencies     and lifetimes

  Middleware / Pipeline   Request logging,        Explain request/response
                          exception handling,     pipeline order
                          correlation IDs         

  Interceptor             Add cross-cutting       Distinguish decorators,
                          behaviour around calls  framework interceptors, and
                                                  middleware

  Event Sourcing          Store events and        Advanced; not the default
                          rebuild aggregate state choice for an ordinary MVP
  ------------------------------------------------------------------------------

### Recommended teaching sequence

1.  Dependency Injection and constructor injection
2.  Strategy
3.  Factory Method
4.  Adapter
5.  Decorator
6.  Observer
7.  Repository and Unit of Work
8.  Middleware
9.  State
10. Specification
11. CQRS and Mediator
12. Proxy and Interceptor
13. Remaining patterns as optional workshops
14. Event Sourcing as an advanced architecture session

For every pattern, ask students: - What problem are we solving? - What
would the simplest implementation look like? - What becomes difficult as
the application grows? - How does the pattern address that difficulty? -
What extra complexity does the pattern introduce? - Would we use it in
this MVP? Why or why not?

------------------------------------------------------------------------

## 7. Dependency injection and service lifetimes

Teach this with small examples before students build a larger
application.

``` csharp
var builder = WebApplication.CreateBuilder(args);

// One instance for the lifetime of the application container.
// Use for suitable shared, thread-safe services.
builder.Services.AddSingleton<IClock, SystemClock>();

// One instance per HTTP request scope.
// Common for application services and repositories.
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// A new instance each time the service is requested.
builder.Services.AddTransient<IEmailFormatter, EmailFormatter>();

// EF Core DbContext is scoped by default when registered this way.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

Explain:

-   **Singleton:** one instance shared by requests. It must be safe for
    concurrent use. Do not inject a scoped DbContext into a singleton.
-   **Scoped:** one instance within a request scope. Common for
    DbContext and request-oriented services.
-   **Transient:** a new instance each time it is resolved. Suitable for
    lightweight, stateless services.

Ask students to predict how many instances are created in a request and
then verify with a small demonstration. Teach that lifetime is a design
decision, not something to choose randomly.

------------------------------------------------------------------------

## 8. Coding standards and team habits

Introduce these habits from the first week:

-   Use meaningful names for classes, methods, variables, and API
    routes.
-   Keep methods focused and reasonably small.
-   Avoid duplicated business rules.
-   Keep controllers thin; move use-case logic into application
    services/handlers.
-   Use asynchronous I/O for database and network operations.
-   Validate input at the application boundary and enforce important
    rules on the server.
-   Use DTOs for API request/response contracts.
-   Do not return stack traces or sensitive details to clients.
-   Do not commit passwords, API keys, connection secrets, or tokens.
-   Add comments to explain **why** a non-obvious decision exists, not
    to repeat what the code already says.
-   Write tests for important business rules.
-   Use pull requests and review code respectfully.
-   Make small commits that explain the change.

### Example of a useful comment

``` csharp
// A unique database constraint is also required here because
// two requests may try to reserve the same slot at the same time.
```

### Example of an unhelpful comment

``` csharp
// Set the name.
product.Name = request.Name;
```

------------------------------------------------------------------------

## 9. Suggested team roles

For teams of four or five, rotate responsibilities so everyone learns
more than one area.

-   **Product coordinator:** clarifies requirements and maintains the
    backlog.
-   **Backend developer:** API, business logic, and persistence.
-   **Frontend developer:** screens, forms, and API integration.
-   **Quality engineer:** test cases, exploratory testing, and defect
    tracking.
-   **Deployment/review owner:** CI checks, deployment notes, and
    code-review coordination.

These are temporary responsibilities, not permanent silos. Every student
should be able to explain the complete application flow.

------------------------------------------------------------------------

## 10. Assessment rubric

  Area                                          Weight
  ----------------------------------------- ----------
  Understanding the problem and MVP scope          10%
  User experience and frontend                     15%
  API design and backend implementation            20%
  Database design and data integrity               15%
  Security and authorisation                       10%
  Automated testing and quality                    10%
  Git collaboration and code review                 5%
  Deployment and documentation                     10%
  Demo and explanation of decisions                 5%
  **Total**                                   **100%**

Do not grade only on the number of screens or lines of code. A smaller
application with correct business rules, useful tests, and reliable
deployment is a stronger result than a large unfinished application.

------------------------------------------------------------------------

## 11. Definition of Done

A feature is not complete merely because it works on the developer's
machine.

A feature is done when:

-   The acceptance criteria are met.
-   The frontend and API are connected.
-   Input validation is present.
-   Important business rules are enforced on the server.
-   Authorisation is checked where required.
-   Success and error states are handled.
-   Relevant automated tests pass.
-   The code has been reviewed and merged.
-   Configuration and secrets are handled safely.
-   The feature is deployed to the shared test environment.
-   The team can demonstrate it.

------------------------------------------------------------------------

## 12. Instructor's weekly classroom rhythm

A repeatable rhythm helps students learn by doing:

  Session     Activity
  ----------- ---------------------------------------------------
  Session 1   Explain the concept and show a small example
  Session 2   Build the concept live, explaining decisions
  Session 3   Students implement a related feature in teams
  Session 4   Review code, test behaviour, and discuss mistakes
  Session 5   Demo progress and plan the next small increment

Aim for more student implementation time than lecture time. Ask students
to explain their reasoning before showing the final code.

### Questions to ask during reviews

-   What user problem does this change solve?
-   What happens if the user submits invalid data?
-   What happens if the API or database is unavailable?
-   What happens if two users perform this action simultaneously?
-   Who is allowed to perform this operation?
-   How would you test the failure path?
-   What would you change if usage increased tenfold?
-   Is this abstraction helping, or is it adding unnecessary complexity?

------------------------------------------------------------------------

## 13. Common mistakes to prevent

1.  **Building too much:** Students add chat, payment, analytics, and
    admin dashboards before the core workflow works. Bring them back to
    the MVP.
2.  **Starting with microservices:** A distributed system introduces
    deployment, network, observability, and data-consistency complexity.
    Start with a modular monolith unless there is a clear reason not to.
3.  **Putting business logic in controllers:** Controllers should
    coordinate HTTP input/output, not contain every business rule.
4.  **Trusting frontend validation:** The backend must validate and
    enforce rules too.
5.  **Ignoring concurrency:** A UI availability check cannot guarantee a
    slot remains available.
6.  **No tests:** Require tests for critical business rules, not just
    happy-path manual demos.
7.  **Hard-coded secrets:** Teach environment configuration and secret
    management early.
8.  **Overusing design patterns:** Patterns solve recurring problems;
    they are not a checklist.
9.  **Deploying only at the end:** Deploy a thin working version early
    and keep it deployable.
10. **Copying code without understanding it:** Ask students to trace a
    request from the browser to the database and explain each step.

------------------------------------------------------------------------

## 14. Optional advanced topics after the MVP

Once the core application is stable, introduce these topics based on
actual needs:

-   Background jobs for reminders
-   Email/SMS provider integration using an adapter
-   Caching and cache invalidation
-   Rate limiting
-   File uploads and object storage
-   Search and pagination improvements
-   Message queues such as RabbitMQ
-   Event-driven integration
-   Redis
-   Docker
-   CI/CD pipelines
-   Cloud hosting and managed databases
-   Observability: structured logs, metrics, traces
-   Microservices and service boundaries
-   Event Sourcing and CQRS at scale

Explain the problem each technology solves before asking students to use
it. For example, a message broker is useful when work needs to happen
asynchronously or services need decoupled communication; it is not
automatically required for every API.

------------------------------------------------------------------------

## 15. Final instructor checklist

Before the course starts:

-   [ ] Select one project brief and define the MVP.
-   [ ] Prepare a sample user journey and wireframe.
-   [ ] Prepare a Git repository template.
-   [ ] Prepare the .NET SDK, IDE, database, and frontend tooling.
-   [ ] Decide whether the cohort will use Angular or React.
-   [ ] Prepare a basic API and database starter, but do not give away
    the entire solution.
-   [ ] Create a task board and agree on a branching/pull-request
    workflow.
-   [ ] Prepare acceptance criteria and test scenarios.

During the course:

-   [ ] Make students demonstrate working software every week.
-   [ ] Review code and ask students to explain their decisions.
-   [ ] Keep the MVP scope under control.
-   [ ] Require automated tests for critical rules.
-   [ ] Deploy early and regularly.
-   [ ] Teach patterns only when a concrete problem makes them useful.

At the end:

-   [ ] Confirm that the core user journey works.
-   [ ] Review security and data integrity.
-   [ ] Run automated tests.
-   [ ] Verify deployment and configuration.
-   [ ] Collect a short retrospective: what worked, what failed, and
    what the team would do next.

------------------------------------------------------------------------

## Closing advice

The most valuable lesson for graduating students is not memorising
frameworks or design-pattern definitions. It is learning to turn an
unclear business problem into a small, working product; collaborate with
others; make sensible technical decisions; test the important
behaviours; and deploy something that real users can try.

Teach them to build one complete feature well before asking them to
build ten features. Teach them to understand a simple architecture
before introducing a distributed one. Teach them to explain **why** they
chose a solution, not merely **how** they copied it.

That is a much stronger foundation for joining a startup engineering
team.
