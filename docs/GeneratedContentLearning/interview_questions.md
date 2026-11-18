# Technical Interview Questions
## Angular + React + Razor + .NET Core + DevOps + Microservices

---

## ANGULAR (15 Questions)

### Fundamentals
1. **What is Angular and how does it differ from AngularJS?**
   - Explain the key architectural differences between Angular (2+) and AngularJS (1.x).
   - What are the benefits of using TypeScript in Angular?

2. **Explain the Angular component lifecycle. What are the main lifecycle hooks?**
   - Describe the order of lifecycle hooks: ngOnInit, ngOnDestroy, ngAfterViewInit, etc.
   - When would you use ngOnInit vs. ngAfterViewInit?

3. **What is dependency injection and how is it used in Angular?**
   - How does Angular's DI container work?
   - What are the differences between different provider scopes (root, component, any)?

4. **Explain the concept of observables in Angular. How do they differ from Promises?**
   - What operators have you used? (map, filter, switchMap, etc.)
   - How do you handle memory leaks with subscriptions?

### Advanced
5. **What is change detection in Angular and how does it work?**
   - Explain OnPush change detection strategy.
   - How would you optimize performance in a large Angular application?

6. **Describe Angular routing and lazy loading.**
   - How do you implement route guards?
   - What is the difference between eager and lazy loading?

7. **What are services and how do they promote code reusability?**
   - How do you share data between components using services?
   - Explain the singleton pattern in Angular services.

8. **How do you handle forms in Angular?**
   - Explain the difference between template-driven and reactive forms.
   - What is FormBuilder and how is it used?

9. **What is RxJS and which operators have you used?**
   - Describe switchMap, mergeMap, and concatMap differences.
   - How do you handle error handling in observables?

10. **Explain Angular modules and their purpose.**
    - What is a feature module vs. a shared module?
    - How do you organize modules in a large application?

11. **How do you implement authentication in Angular?**
    - How would you store and refresh JWT tokens?
    - How do you protect routes with authentication guards?

12. **What is the difference between @Input, @Output, and @ViewChild?**
    - How do you communicate between parent and child components?
    - Explain when to use each decorator.

13. **How would you optimize an Angular application for production?**
    - What is tree-shaking and how does it help?
    - Explain the role of AOT compilation.

14. **What are directives and how do you create custom directives?**
    - Explain structural vs. attribute directives.
    - Give an example of a custom directive you've built.

15. **How do you test Angular components?**
    - Explain unit testing with Jasmine and Karma.
    - How do you mock services in tests?

---

## REACT (15 Questions)

### Fundamentals
1. **What is React and what are the core concepts?**
   - Explain the virtual DOM and its benefits.
   - What is JSX and why is it used?

2. **Explain class components vs. functional components.**
   - What are the advantages of functional components?
   - When would you use class components?

3. **What are React Hooks and which ones have you used?**
   - Explain useState, useEffect, and useContext.
   - What is the rules of hooks?

4. **How does state management work in React?**
   - What is lifting state up?
   - When should you use context vs. prop drilling?

5. **Explain props and prop drilling.**
   - What are controlled and uncontrolled components?
   - How do you validate props using PropTypes or TypeScript?

### Advanced
6. **What is the useEffect hook and how does it handle side effects?**
   - Explain dependencies array and its purpose.
   - How do you cleanup side effects?

7. **Describe React Router and client-side routing.**
   - How do you implement route parameters and query strings?
   - What are route guards or protected routes?

8. **What is Redux and how does it manage state?**
   - Explain actions, reducers, and the store.
   - What are middleware and have you used Redux Thunk or Redux Saga?

9. **What alternatives to Redux exist for state management?**
   - Explain Context API vs. Redux.
   - Have you used Zustand, Recoil, or Jotai?

10. **How do you optimize React performance?**
    - Explain useMemo and useCallback.
    - What is code splitting and lazy loading?

11. **What is React.memo and when should you use it?**
    - How does shallow comparison work?
    - When would you use useCallback with React.memo?

12. **Explain conditional rendering in React.**
    - What are different patterns for conditional rendering?
    - What is the ternary operator pattern in JSX?

13. **How do you handle forms in React?**
    - Explain controlled components.
    - Have you used form libraries like Formik or React Hook Form?

14. **What is the key prop and why is it important in lists?**
    - Why shouldn't you use array index as key?
    - How does React use keys for reconciliation?

15. **How do you test React components?**
    - Explain testing with React Testing Library.
    - How do you mock API calls in tests?

---

## RAZOR (10 Questions)

### Fundamentals
1. **What is Razor and how does it differ from traditional ASP.NET views?**
   - Explain the syntax and how C# is embedded in HTML.
   - What are the advantages of Razor?

2. **Explain Razor Pages and how they differ from MVC views.**
   - What is the PageModel class?
   - How does the routing work in Razor Pages?

3. **What is the @model directive and how is it used?**
   - How do you pass data from the PageModel to the view?
   - Explain strongly-typed views.

4. **How do you implement form submission in Razor Pages?**
   - Explain binding and model binding.
   - What is the [BindProperty] attribute?

### Advanced
5. **What are Tag Helpers and how do you use them?**
   - Explain form, validation, and input tag helpers.
   - How do you create custom tag helpers?

6. **How do you handle validation in Razor Pages?**
   - Explain data annotations and validation attributes.
   - How do you display validation messages?

7. **What is the difference between @Html.Partial and @Html.RenderPartial?**
   - When would you use each?
   - What are partial views used for?

8. **How do you work with layout pages in Razor?**
   - Explain _Layout.cshtml and its purpose.
   - How do you override layout in specific views?

9. **Explain routing in Razor Pages.**
   - How do you configure custom routes?
   - What is the @page directive?

10. **How do you handle AJAX and asynchronous operations in Razor Pages?**
    - How do you return JSON from a handler?
    - Explain OnPostAsync and OnGetAsync methods.

---

## .NET CORE (20 Questions)

### Fundamentals
1. **What is .NET Core and how does it differ from .NET Framework?**
   - Explain cross-platform capabilities.
   - What are the benefits of .NET Core?

2. **What is the structure of a .NET Core project?**
   - Explain the project file (.csproj).
   - What is the purpose of appsettings.json?

3. **Explain the Startup class and the configuration pipeline.**
   - What are ConfigureServices and Configure methods?
   - How does middleware work?

4. **What is dependency injection and how does it work in .NET Core?**
   - Explain IServiceCollection and IServiceProvider.
   - What are the differences between Transient, Scoped, and Singleton?

5. **What is ASP.NET Core MVC and how does it work?**
   - Explain the MVC pattern.
   - What are controllers and actions?

### Middleware & Configuration
6. **What is middleware and how do you create custom middleware?**
   - Explain the middleware pipeline.
   - When would you use UseMiddleware?

7. **How do you configure application settings?**
   - Explain appsettings.json and environment-specific settings.
   - How do you access configuration in code?

8. **What is logging in .NET Core and which providers have you used?**
   - Explain ILogger and LoggerFactory.
   - How do you implement different log levels?

### Data Access
9. **What is Entity Framework Core and how does it work?**
   - Explain the DbContext and DbSet classes.
   - What are migrations and how do you use them?

10. **Explain LINQ and how it simplifies data queries.**
    - What are LINQ operators you've used? (Where, Select, Join, etc.)
    - What is deferred execution?

11. **What are the different ways to query data in EF Core?**
    - Explain LINQ to Entities, raw SQL, and stored procedures.
    - How do you handle complex queries efficiently?

12. **How do you handle relationships in EF Core?**
    - Explain one-to-one, one-to-many, and many-to-many relationships.
    - What is lazy loading vs. eager loading?

### API Development
13. **What is REST and how do you design RESTful APIs in ASP.NET Core?**
    - Explain HTTP methods (GET, POST, PUT, DELETE).
    - What are status codes and their meanings?

14. **How do you implement API versioning?**
    - Explain different versioning strategies.
    - Have you used API versioning libraries?

15. **What is authentication and authorization in .NET Core?**
    - Explain JWT tokens and how they work.
    - How do you implement role-based authorization?

### Advanced
16. **What is async/await and how does it work?**
    - Explain Task and Task<T>.
    - When should you use async programming?

17. **How do you handle caching in .NET Core?**
    - Explain IMemoryCache and IDistributedCache.
    - When would you use each?

18. **What is the repository pattern and unit of work?**
    - How does this pattern improve code testability?
    - Have you implemented these patterns?

19. **How do you implement error handling and validation?**
    - Explain exception handling middleware.
    - How do you return consistent error responses?

20. **What is the difference between IEnumerable and IQueryable?**
    - When should you use each?
    - How does LINQ to SQL differ from LINQ to Objects?

---

## DEVOPS (15 Questions)

### Fundamentals
1. **What is DevOps and what are its core principles?**
   - Explain the relationship between development and operations.
   - What are the benefits of DevOps culture?

2. **What is CI/CD and why is it important?**
   - Explain continuous integration and continuous deployment.
   - What are the benefits of CI/CD pipelines?

3. **What is version control and have you used Git?**
   - Explain branching strategies (Git Flow, GitHub Flow).
   - What are best practices for commit messages?

4. **What is Docker and how does containerization work?**
   - Explain images and containers.
   - What are the benefits of Docker?

5. **What is Docker Compose and when would you use it?**
   - Explain service orchestration.
   - How do you define multi-container applications?

### Infrastructure & Deployment
6. **What is Kubernetes and how does it manage containers?**
   - Explain pods, services, and deployments.
   - What are the benefits of container orchestration?

7. **What is Infrastructure as Code (IaC)?**
   - Explain declarative and imperative approaches.
   - Have you used Terraform, CloudFormation, or Ansible?

8. **What is a build pipeline and how do you create one?**
   - Explain stages: source, build, test, deploy.
   - Have you used Jenkins, GitLab CI, GitHub Actions, or Azure Pipelines?

9. **How do you implement automated testing in CI/CD?**
   - Explain unit tests, integration tests, and end-to-end tests.
   - How do you measure code coverage?

10. **What is monitoring and logging in production?**
    - Explain application performance monitoring (APM).
    - Have you used tools like ELK stack, Prometheus, or Grafana?

### Cloud & Platforms
11. **What are cloud platforms and have you used any?**
    - Explain AWS, Azure, and GCP differences.
    - What are the benefits of cloud computing?

12. **What is serverless computing and when is it useful?**
    - Explain AWS Lambda, Azure Functions, and Google Cloud Functions.
    - What are pros and cons of serverless?

13. **How do you manage secrets and sensitive data in DevOps?**
    - Explain secret management tools.
    - How do you prevent secrets in version control?

14. **What is load balancing and auto-scaling?**
    - Explain horizontal and vertical scaling.
    - How do you implement auto-scaling?

15. **How do you handle rollbacks and zero-downtime deployments?**
    - Explain blue-green deployment.
    - What is canary deployment?

---

## MICROSERVICES (15 Questions)

### Fundamentals
1. **What is microservices architecture and how does it differ from monolithic architecture?**
   - Explain the benefits and challenges.
   - When should you use microservices?

2. **What are the key characteristics of microservices?**
   - Explain service independence and autonomy.
   - What is service coupling?

3. **How do microservices communicate with each other?**
   - Explain synchronous (REST, gRPC) and asynchronous (message queues) communication.
   - What are the pros and cons of each?

4. **What is service discovery and how does it work?**
   - Explain client-side and server-side service discovery.
   - Have you used tools like Consul, Eureka, or Kubernetes DNS?

5. **What is an API Gateway and why is it important?**
   - Explain its responsibilities.
   - Have you used API Gateway tools (Kong, AWS API Gateway, etc.)?

### Data Management
6. **How do you manage data in microservices architecture?**
   - Explain database per service pattern.
   - What are the challenges of distributed data?

7. **What is eventual consistency and how does it work?**
   - Explain the CAP theorem.
   - How do you handle inconsistencies?

8. **What is the saga pattern and how do you implement it?**
   - Explain choreography vs. orchestration.
   - How do you handle saga failures?

### Communication & Integration
9. **What is event-driven architecture?**
   - Explain event producers and consumers.
   - Have you used message brokers (RabbitMQ, Kafka, Azure Service Bus)?

10. **What is message queuing and when is it used?**
    - Explain point-to-point and publish-subscribe patterns.
    - What are benefits of asynchronous messaging?

11. **How do you implement resilience in microservices?**
    - Explain circuit breaker pattern.
    - What are other resilience patterns (retry, timeout, bulkhead)?

### Deployment & Observability
12. **How do you deploy microservices?**
    - Explain containerization and orchestration.
    - What are deployment challenges with microservices?

13. **What is distributed tracing and why is it important?**
    - Explain trace propagation across services.
    - Have you used tools like Jaeger, Zipkin, or Application Insights?

14. **How do you log and monitor microservices?**
    - Explain centralized logging.
    - How do you correlate logs across services?

15. **What are the operational challenges of microservices?**
    - Explain increased complexity.
    - How do you manage versioning and backwards compatibility?

---

## SCENARIO-BASED QUESTIONS (10 Questions)

1. **You have a .NET Core microservices architecture with 5 services. One service is slow and causing cascading failures. How would you diagnose and fix this?**

2. **Design a real-time notification system using Angular frontend, .NET Core backend, and microservices.**

3. **How would you implement authentication and authorization across multiple microservices?**

4. **You need to migrate a monolithic ASP.NET Core application to microservices. What strategy would you use?**

5. **Design a CI/CD pipeline for deploying a Docker-based microservices application to Kubernetes.**

6. **How would you implement a distributed transaction across multiple microservices?**

7. **You have a React application that needs to consume data from multiple .NET Core APIs. How would you handle this efficiently?**

8. **Design a caching strategy for a Razor Pages application with multiple data sources.**

9. **How would you implement feature flags/toggles in a microservices architecture?**

10. **Design a disaster recovery and business continuity plan for a cloud-based microservices application.**

---

## FOLLOW-UP DISCUSSION TOPICS

- **System Design**: Ask candidates to design a system (e.g., e-commerce platform, social media feed).
- **Problem-Solving**: Discuss how they approach problems and learn new technologies.
- **Team Collaboration**: Ask about their experience working in teams and code review processes.
- **Performance**: Discuss optimization techniques and profiling tools they've used.
- **Testing Strategy**: Ask about testing methodologies and test coverage goals.
- **Security**: Discuss security best practices and vulnerabilities they've encountered.
- **Continuous Learning**: Ask about recent technologies they've learned and why.
- **Project Experience**: Request detailed walkthroughs of past projects and their contributions.
