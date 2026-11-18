# Angular v22 Learning Guide: From Zero to Production

A comprehensive guide for learning modern Angular (v2-v22), from fundamentals through advanced concepts, with ASP.NET Web API integration and deployment.

---

## Table of Contents

1. [Introduction & Setup](#introduction--setup)
2. [Components & Templates](#components--templates)
3. [Data Binding & Events](#data-binding--events)
4. [Services & Dependency Injection](#services--dependency-injection)
5. [HTTP Client & API Integration](#http-client--api-integration)
6. [Routing](#routing)
7. [Forms (Template-Driven & Reactive)](#forms-template-driven--reactive)
8. [RxJS & Observables](#rxjs--observables)
9. [Directives & Pipes](#directives--pipes)
10. [State Management](#state-management)
11. [Testing](#testing)
12. [Deployment](#deployment)

---

# PART 1: INTRODUCTION & SETUP

## 1.1 What is Modern Angular?

**Definition:** Angular (v2+) is a complete TypeScript-based framework for building dynamic web applications. It's a complete rewrite of Angular.js with modern architecture, built-in CLI, and powerful tooling.

**Key Characteristics:**
- **TypeScript First** — All code in TypeScript (not optional)
- **Component-Based** — Everything is a component
- **RxJS Reactive** — Uses Observables for data flow
- **Built-in CLI** — Angular CLI handles all tooling
- **Type Safety** — Full compile-time type checking
- **Dependency Injection** — Built into the framework
- **Testing Ready** — Jasmine + Karma/Cypress built-in

---

## 1.2 Angular vs Angular.js

**Key Differences:**

```
┌─────────────────────────────────────┐
│        Angular.js (1.x - Legacy)    │
├─────────────────────────────────────┤
│ JavaScript (not TypeScript)         │
│ Controllers + Views                 │
│ Two-way $scope binding              │
│ Manual module setup                 │
│ No CLI (manual tooling)             │
│ Declining community                 │
│ ~50KB bundle size                   │
│ For legacy/maintenance projects     │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│      Angular (2+ - Modern)          │
├─────────────────────────────────────┤
│ TypeScript (required)               │
│ Components only                     │
│ Property binding + Observables      │
│ Angular CLI setup                   │
│ Full CLI with tooling               │
│ Large active community              │
│ ~500KB+ bundle size                 │
│ For new & enterprise projects       │
└─────────────────────────────────────┘
```

---

## 1.3 Installation & Project Setup

**Prerequisites:**
```bash
# Install Node.js (v20+)
node --version  # v20.x or higher

# Install npm (v10+)
npm --version   # v10.x or higher
```

**Create New Angular Project:**

```bash
# Install Angular CLI globally
npm install -g @angular/cli

# Verify installation
ng version

# Create new project
ng new my-app

# Follow prompts:
# - Add routing? Yes
# - Stylesheet format? CSS (or SCSS)
# - Enable SSR? No (for now)

# Navigate to project
cd my-app

# Start development server
ng serve

# Open browser: http://localhost:4200
```

**Project Structure:**

```
my-app/
├── src/
│   ├── app/
│   │   ├── app.component.ts         # Root component
│   │   ├── app.component.html       # Root template
│   │   ├── app.component.css        # Root styles
│   │   ├── app.routes.ts            # Route configuration
│   │   ├── services/
│   │   │   └── user.service.ts      # Business logic
│   │   ├── components/
│   │   │   ├── user-list/
│   │   │   ├── user-detail/
│   │   │   └── user-form/
│   │   └── models/
│   │       └── user.ts             # TypeScript interfaces
│   ├── main.ts                      # Entry point
│   ├── styles.css                   # Global styles
│   └── index.html                   # HTML template
├── angular.json                     # CLI configuration
├── tsconfig.json                    # TypeScript configuration
├── package.json                     # Dependencies
└── README.md
```

---

# PART 2: COMPONENTS & TEMPLATES

## 2.1 Component Basics

**Definition:** A component is a reusable piece of UI consisting of a TypeScript class, HTML template, and CSS styles.

**Syntax:**

```typescript
// user.component.ts
import { Component } from '@angular/core';

@Component({
  selector: 'app-user',                    // HTML tag name
  templateUrl: './user.component.html',    // External template
  styleUrls: ['./user.component.css']      // External styles
})
export class UserComponent {
  name: string = 'John Doe';
  email: string = 'john@example.com';
}
```

**Inline Template & Styles:**

```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-user',
  template: `
    <div>
      <h1>{{ name }}</h1>
      <p>{{ email }}</p>
    </div>
  `,
  styles: [`
    h1 {
      color: blue;
    }
  `]
})
export class UserComponent {
  name: string = 'John Doe';
  email: string = 'john@example.com';
}
```

**Using Component in HTML:**

```html
<!-- app.component.html -->
<app-user></app-user>

<!-- Output:
<div>
  <h1>John Doe</h1>
  <p>john@example.com</p>
</div>
-->
```

---

## 2.2 Component Lifecycle Hooks

**Definition:** Lifecycle hooks are methods that Angular calls at specific stages of a component's life.

**Syntax:**

```typescript
import { Component, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-user',
  template: '<div>{{ name }}</div>'
})
export class UserComponent implements OnInit, OnDestroy, OnChanges {
  name: string = '';
  
  // 1. After component instance is created
  constructor() {
    console.log('constructor: Component created');
  }
  
  // 2. After input properties are set
  ngOnChanges(changes: SimpleChanges) {
    console.log('ngOnChanges:', changes);
  }
  
  // 3. After component is initialized (MOST USED)
  ngOnInit() {
    console.log('ngOnInit: Component initialized');
    this.loadUser();
  }
  
  // 4. After view is initialized
  ngAfterViewInit() {
    console.log('ngAfterViewInit: View ready');
  }
  
  // 5. When component is about to be destroyed
  ngOnDestroy() {
    console.log('ngOnDestroy: Component destroyed');
    // Cleanup: unsubscribe, timers, etc.
  }
  
  loadUser() {
    console.log('Loading user...');
  }
}
```

**Lifecycle Order:**
```
1. constructor()
2. ngOnChanges()
3. ngOnInit()        ← Most used for initialization
4. ngAfterViewInit()
5. ngOnDestroy()     ← Cleanup here
```

---

## 2.3 Standalone Components (Angular 14+)

**Definition:** Standalone components don't require NgModule and are self-contained.

**Syntax:**

```typescript
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css'],
  standalone: true,  // Mark as standalone
  imports: [
    CommonModule,           // For ngIf, ngFor, etc.
    HttpClientModule        // For HTTP calls
  ]
})
export class UserListComponent {
  users: any[] = [];
}
```

**Main.ts with Standalone Components:**

```typescript
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient()  // Provide HTTP client globally
  ]
});
```

---

# PART 3: DATA BINDING & EVENTS

## 3.1 Property Binding

**Definition:** One-way binding from component to template.

**Syntax:**

```typescript
// component.ts
export class UserComponent {
  title: string = 'My App';
  imageUrl: string = 'https://example.com/image.jpg';
  isDisabled: boolean = false;
  user = {
    name: 'John',
    age: 30
  };
}
```

**Template Binding:**

```html
<!-- Property binding: [] -->
<h1 [textContent]="title"></h1>
<img [src]="imageUrl" alt="User" />
<button [disabled]="isDisabled">Click Me</button>

<!-- String interpolation: {{ }} -->
<h1>{{ title }}</h1>
<p>User: {{ user.name }}, Age: {{ user.age }}</p>

<!-- HTML attribute binding: [attr.attributeName] -->
<img [attr.alt]="title" />
<button [attr.data-id]="user.id">Delete</button>

<!-- CSS class binding: [class] or [ngClass] -->
<div [class.active]="isActive">Active item</div>
<div [class]="isActive ? 'active' : 'inactive'">Dynamic class</div>
<div [ngClass]="{'active': isActive, 'disabled': isDisabled}">
  Multiple classes
</div>

<!-- CSS style binding: [style] or [ngStyle] -->
<div [style.color]="textColor">Colored text</div>
<div [style.width.px]="width">Dynamic width</div>
<div [ngStyle]="{'color': textColor, 'font-size.px': fontSize}">
  Multiple styles
</div>
```

---

## 3.2 Event Binding

**Definition:** Listening to DOM events and triggering component methods.

**Syntax:**

```typescript
// component.ts
export class UserComponent {
  count: number = 0;
  message: string = '';
  
  // Click handler
  onClick() {
    this.count++;
    console.log('Button clicked');
  }
  
  // Input handler
  onInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.message = value;
  }
  
  // Form submission
  onSubmit(form: any) {
    console.log('Form submitted:', form.value);
  }
  
  // Key events
  onKeyDown(event: KeyboardEvent) {
    console.log('Key pressed:', event.key);
  }
  
  // Mouse events
  onMouseEnter() {
    console.log('Mouse entered');
  }
}
```

**Template Event Binding:**

```html
<!-- Event binding: (eventName)="handler()" -->

<!-- Click event -->
<button (click)="onClick()">Increment</button>
<p>Count: {{ count }}</p>

<!-- Input event -->
<input (input)="onInput($event)" />
<p>Message: {{ message }}</p>

<!-- Form submission -->
<form (ngSubmit)="onSubmit(form)" #form="ngForm">
  <input name="email" ngModel required />
  <button type="submit">Submit</button>
</form>

<!-- Key events -->
<input (keydown)="onKeyDown($event)" />
<input (keyup.enter)="handleEnter()" />
<input (keyup.escape)="handleEscape()" />

<!-- Mouse events -->
<div (mouseenter)="onMouseEnter()" (mouseleave)="onMouseLeave()">
  Hover me
</div>

<!-- Double click -->
<button (dblclick)="handleDoubleClick()">Double click</button>

<!-- Focus & blur -->
<input (focus)="onFocus()" (blur)="onBlur()" />
```

---

## 3.3 Two-Way Data Binding

**Definition:** Binding that updates both component property and DOM automatically.

**Syntax:**

```typescript
// component.ts
export class UserComponent {
  firstName: string = 'John';
  lastName: string = 'Doe';
  user = {
    email: 'john@example.com',
    age: 30
  };
}
```

**Template:**

```html
<!-- Two-way binding: [(ngModel)] -->
<!-- Requires FormsModule in imports -->

<input [(ngModel)]="firstName" />
<p>First name: {{ firstName }}</p>

<input [(ngModel)]="lastName" />
<p>Last name: {{ lastName }}</p>

<!-- Two-way binding with objects -->
<input [(ngModel)]="user.email" />
<input [(ngModel)]="user.age" type="number" />

<!-- This is equivalent to: -->
<!-- <input [ngModel]="firstName" (ngModelChange)="firstName = $event" /> -->
```

**Module Import:**

```typescript
// app.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [FormsModule]  // Required for ngModel
})
export class AppComponent {}
```

---

# PART 4: SERVICES & DEPENDENCY INJECTION

## 4.1 Services Basics

**Definition:** Services are singleton objects that provide reusable business logic and data access.

**Syntax:**

```typescript
// user.service.ts
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'  // Make available app-wide
})
export class UserService {
  private users = [
    { id: 1, name: 'Alice', email: 'alice@example.com' },
    { id: 2, name: 'Bob', email: 'bob@example.com' }
  ];
  
  // Get all users
  getUsers() {
    return this.users;
  }
  
  // Get user by ID
  getUserById(id: number) {
    return this.users.find(u => u.id === id);
  }
  
  // Add user
  addUser(user: any) {
    this.users.push(user);
  }
  
  // Update user
  updateUser(id: number, user: any) {
    const index = this.users.findIndex(u => u.id === id);
    if (index !== -1) {
      this.users[index] = user;
    }
  }
  
  // Delete user
  deleteUser(id: number) {
    this.users = this.users.filter(u => u.id !== id);
  }
}
```

**Using Service in Component:**

```typescript
// user-list.component.ts
import { Component, OnInit } from '@angular/core';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  standalone: true
})
export class UserListComponent implements OnInit {
  users: any[] = [];
  
  // Inject service via constructor
  constructor(private userService: UserService) {}
  
  ngOnInit() {
    this.users = this.userService.getUsers();
  }
  
  deleteUser(id: number) {
    this.userService.deleteUser(id);
    this.users = this.userService.getUsers();
  }
}
```

---

## 4.2 Dependency Injection Container

**Definition:** Angular's DI system automatically provides instances of services to components.

**How it Works:**

```typescript
// Step 1: Create service with @Injectable decorator
@Injectable({ providedIn: 'root' })
export class LoggerService {
  log(message: string) {
    console.log(message);
  }
}

// Step 2: Inject into component via constructor
@Component({...})
export class MyComponent {
  constructor(private logger: LoggerService) {}
  
  ngOnInit() {
    this.logger.log('Component initialized');
  }
}

// Step 3: Angular automatically:
// 1. Creates LoggerService instance
// 2. Passes it to MyComponent constructor
// 3. Stores as singleton (same instance everywhere)
```

**Provider Scopes:**

```typescript
// providedIn: 'root' - App-wide singleton (RECOMMENDED)
@Injectable({ providedIn: 'root' })
export class GlobalService {}

// providedIn: 'platform' - Shared between tabs/windows
@Injectable({ providedIn: 'platform' })
export class SharedService {}

// No providedIn - Manual registration (legacy)
@Injectable()
export class ManualService {}

// In module:
@NgModule({
  providers: [ManualService]
})
export class MyModule {}
```

---

# PART 5: HTTP CLIENT & API INTEGRATION

## 5.1 HTTP Client Setup

**Syntax:**

```typescript
// app.config.ts (standalone app)
import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig } from '@angular/core';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient()
  ]
};

// main.ts
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, appConfig);

// OR in module (non-standalone)
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  imports: [HttpClientModule]
})
export class AppModule {}
```

---

## 5.2 Making API Calls

**Definition:** Using HttpClient to call ASP.NET Web API endpoints.

**Service with API Calls:**

```typescript
// user.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface User {
  id: number;
  name: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private apiUrl = 'http://localhost:5000/api/users';
  
  constructor(private http: HttpClient) {}
  
  // GET all users
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }
  
  // GET user by ID
  getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }
  
  // POST create user
  createUser(user: Omit<User, 'id'>): Observable<User> {
    return this.http.post<User>(this.apiUrl, user);
  }
  
  // PUT update user
  updateUser(id: number, user: User): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, user);
  }
  
  // DELETE user
  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

**Component Using Service:**

```typescript
// user-list.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService, User } from '../services/user.service';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class UserListComponent implements OnInit {
  users: User[] = [];
  loading: boolean = false;
  error: string | null = null;
  
  constructor(private userService: UserService) {}
  
  ngOnInit() {
    this.loadUsers();
  }
  
  loadUsers() {
    this.loading = true;
    this.error = null;
    
    this.userService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error(err);
      }
    });
  }
  
  deleteUser(id: number) {
    if (confirm('Are you sure?')) {
      this.userService.deleteUser(id).subscribe({
        next: () => {
          this.users = this.users.filter(u => u.id !== id);
        },
        error: (err) => {
          this.error = 'Failed to delete user';
        }
      });
    }
  }
}
```

**Template:**

```html
<!-- user-list.component.html -->
<div class="container">
  <h1>Users</h1>
  
  <!-- Loading indicator -->
  <div *ngIf="loading" class="alert alert-info">
    Loading users...
  </div>
  
  <!-- Error message -->
  <div *ngIf="error" class="alert alert-danger">
    {{ error }}
  </div>
  
  <!-- Users table -->
  <table *ngIf="!loading && users.length > 0" class="table">
    <thead>
      <tr>
        <th>ID</th>
        <th>Name</th>
        <th>Email</th>
        <th>Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let user of users">
        <td>{{ user.id }}</td>
        <td>{{ user.name }}</td>
        <td>{{ user.email }}</td>
        <td>
          <button (click)="deleteUser(user.id)" class="btn btn-danger">
            Delete
          </button>
        </td>
      </tr>
    </tbody>
  </table>
  
  <!-- Empty state -->
  <p *ngIf="!loading && users.length === 0" class="text-muted">
    No users found.
  </p>
</div>
```

---

## 5.3 HTTP Interceptors

**Definition:** Interceptors allow you to intercept HTTP requests/responses globally.

**Auth Interceptor:**

```typescript
// auth.interceptor.ts
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Get auth token from service
    const token = this.authService.getToken();
    
    // Clone request and add authorization header
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return next.handle(req);
  }
}

// Register interceptor (standalone)
import { HTTP_INTERCEPTORS } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),  // Angular 15+
    // OR legacy:
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ]
};
```

**Functional Interceptor (Angular 15+):**

```typescript
// auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }
  
  return next(req);
};

// app.config.ts
import { withInterceptors } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```

---

# PART 6: ROUTING

## 6.1 Router Setup (Standalone Components)

**Syntax:**

```typescript
// app.routes.ts
import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home.component';
import { UserListComponent } from './pages/user-list.component';
import { UserDetailComponent } from './pages/user-detail.component';
import { LoginComponent } from './pages/login.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'home',
    component: HomeComponent
  },
  {
    path: 'users',
    component: UserListComponent
  },
  {
    path: 'users/:id',
    component: UserDetailComponent
  },
  {
    path: '**',  // Catch-all
    redirectTo: 'home'
  }
];

// main.ts
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes)
  ]
});
```

**App Component with Router Outlet:**

```typescript
// app.component.ts
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [RouterOutlet]
})
export class AppComponent {}
```

**Template:**

```html
<!-- app.component.html -->
<nav>
  <a routerLink="/home">Home</a>
  <a routerLink="/users">Users</a>
  <a routerLink="/login">Login</a>
</nav>

<router-outlet></router-outlet>
```

---

## 6.2 Route Parameters & Nested Routes

**Syntax:**

```typescript
// app.routes.ts
export const routes: Routes = [
  {
    path: 'users',
    children: [
      {
        path: '',
        component: UserListComponent
      },
      {
        path: ':id',
        component: UserDetailComponent
      },
      {
        path: ':id/edit',
        component: UserEditComponent
      }
    ]
  }
];

// user-detail.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-user-detail',
  templateUrl: './user-detail.component.html',
  standalone: true
})
export class UserDetailComponent implements OnInit {
  userId!: number;
  user: any;
  
  constructor(
    private route: ActivatedRoute,
    private userService: UserService
  ) {}
  
  ngOnInit() {
    // Get route parameter
    this.route.params.subscribe(params => {
      this.userId = params['id'];
      this.loadUser();
    });
  }
  
  loadUser() {
    this.userService.getUserById(this.userId).subscribe(user => {
      this.user = user;
    });
  }
}
```

---

## 6.3 Route Guards

**Definition:** Guards protect routes based on conditions (authentication, authorization, etc.).

**Syntax:**

```typescript
// auth.guard.ts
import { Injectable } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from './services/auth.service';
import { inject } from '@angular/core';

// Functional guard (Angular 15+) - RECOMMENDED
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  if (authService.isAuthenticated()) {
    return true;
  } else {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
};

// Class-based guard (legacy)
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}
  
  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    } else {
      this.router.navigate(['/login']);
      return false;
    }
  }
}
```

**Using Guard in Routes:**

```typescript
// app.routes.ts
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]  // Protect this route
  },
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [authGuard]
  }
];
```

---

# PART 7: FORMS (TEMPLATE-DRIVEN & REACTIVE)

## 7.1 Template-Driven Forms

**Definition:** Forms defined in the template with ngForm directive.

**Syntax:**

```typescript
// user-form.component.ts
import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  standalone: true,
  imports: [FormsModule, CommonModule]
})
export class UserFormComponent {
  user = {
    name: '',
    email: '',
    age: null as number | null
  };
  
  submitted = false;
  error: string | null = null;
  
  constructor(private userService: UserService) {}
  
  onSubmit(form: NgForm) {
    this.submitted = true;
    
    if (form.invalid) {
      return;
    }
    
    this.userService.createUser(this.user).subscribe({
      next: () => {
        alert('User created successfully!');
        form.reset();
        this.submitted = false;
      },
      error: (err) => {
        this.error = 'Failed to create user';
      }
    });
  }
}
```

**Template:**

```html
<!-- user-form.component.html -->
<form (ngSubmit)="onSubmit(form)" #form="ngForm" novalidate>
  <!-- Name field -->
  <div class="form-group">
    <label>Name:</label>
    <input
      type="text"
      name="name"
      [(ngModel)]="user.name"
      required
      minlength="3"
      #nameControl="ngModel"
    />
    <div *ngIf="nameControl.invalid && nameControl.touched" class="error">
      <small *ngIf="nameControl.errors?.['required']">Name is required</small>
      <small *ngIf="nameControl.errors?.['minlength']">
        Name must be at least 3 characters
      </small>
    </div>
  </div>
  
  <!-- Email field -->
  <div class="form-group">
    <label>Email:</label>
    <input
      type="email"
      name="email"
      [(ngModel)]="user.email"
      required
      email
      #emailControl="ngModel"
    />
    <div *ngIf="emailControl.invalid && emailControl.touched" class="error">
      <small *ngIf="emailControl.errors?.['required']">Email is required</small>
      <small *ngIf="emailControl.errors?.['email']">Invalid email</small>
    </div>
  </div>
  
  <!-- Age field -->
  <div class="form-group">
    <label>Age:</label>
    <input
      type="number"
      name="age"
      [(ngModel)]="user.age"
      min="0"
      max="120"
    />
  </div>
  
  <!-- Error message -->
  <div *ngIf="error" class="alert alert-danger">
    {{ error }}
  </div>
  
  <!-- Submit button -->
  <button type="submit" [disabled]="form.invalid">
    Submit
  </button>
</form>
```

---

## 7.2 Reactive Forms

**Definition:** Forms created programmatically with FormBuilder and FormGroup.

**Syntax:**

```typescript
// user-form.component.ts
import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})
export class UserFormComponent implements OnInit {
  form!: FormGroup;
  submitted = false;
  error: string | null = null;
  
  constructor(
    private fb: FormBuilder,
    private userService: UserService
  ) {}
  
  ngOnInit() {
    this.initializeForm();
  }
  
  initializeForm() {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      age: [null, [Validators.min(0), Validators.max(120)]]
    });
  }
  
  onSubmit() {
    this.submitted = true;
    
    if (this.form.invalid) {
      return;
    }
    
    this.userService.createUser(this.form.value).subscribe({
      next: () => {
        alert('User created successfully!');
        this.form.reset();
        this.submitted = false;
      },
      error: (err) => {
        this.error = 'Failed to create user';
      }
    });
  }
  
  get name() {
    return this.form.get('name');
  }
  
  get email() {
    return this.form.get('email');
  }
  
  get age() {
    return this.form.get('age');
  }
}
```

**Template:**

```html
<!-- user-form.component.html -->
<form [formGroup]="form" (ngSubmit)="onSubmit()" novalidate>
  <!-- Name field -->
  <div class="form-group">
    <label>Name:</label>
    <input type="text" formControlName="name" />
    <div *ngIf="name?.invalid && name?.touched" class="error">
      <small *ngIf="name?.errors?.['required']">Name is required</small>
      <small *ngIf="name?.errors?.['minlength']">
        Name must be at least 3 characters
      </small>
    </div>
  </div>
  
  <!-- Email field -->
  <div class="form-group">
    <label>Email:</label>
    <input type="email" formControlName="email" />
    <div *ngIf="email?.invalid && email?.touched" class="error">
      <small *ngIf="email?.errors?.['required']">Email is required</small>
      <small *ngIf="email?.errors?.['email']">Invalid email</small>
    </div>
  </div>
  
  <!-- Age field -->
  <div class="form-group">
    <label>Age:</label>
    <input type="number" formControlName="age" />
  </div>
  
  <!-- Error message -->
  <div *ngIf="error" class="alert alert-danger">
    {{ error }}
  </div>
  
  <!-- Submit button -->
  <button type="submit" [disabled]="form.invalid">
    Submit
  </button>
</form>
```

---

# PART 8: RXJS & OBSERVABLES

## 8.1 Observables Basics

**Definition:** Observables are lazy, reusable collections of values that emit over time.

**Syntax:**

```typescript
import { Observable, of, from } from 'rxjs';

// Create observable from values
const obs1: Observable<number> = of(1, 2, 3);

// Subscribe to observable
obs1.subscribe({
  next: (value) => console.log(value),      // 1, 2, 3
  error: (err) => console.error(err),
  complete: () => console.log('Done')
});

// Create observable from array
const obs2 = from([1, 2, 3]);
obs2.subscribe(value => console.log(value)); // 1, 2, 3

// Create custom observable
const custom = new Observable(subscriber => {
  subscriber.next(1);
  subscriber.next(2);
  subscriber.next(3);
  subscriber.complete();
});

custom.subscribe(console.log); // 1, 2, 3
```

---

## 8.2 Operators

**Definition:** Functions that manipulate observable streams.

**Common Operators:**

```typescript
import { map, filter, take, tap, catchError, switchMap } from 'rxjs/operators';

// map: Transform values
of(1, 2, 3)
  .pipe(map(x => x * 2))
  .subscribe(console.log); // 2, 4, 6

// filter: Filter values
of(1, 2, 3, 4, 5)
  .pipe(filter(x => x > 2))
  .subscribe(console.log); // 3, 4, 5

// take: Take first N values
of(1, 2, 3, 4, 5)
  .pipe(take(3))
  .subscribe(console.log); // 1, 2, 3

// tap: Side effects (logging)
of(1, 2, 3)
  .pipe(tap(x => console.log('emitted:', x)))
  .subscribe();

// catchError: Handle errors
this.http.get('/api/users').pipe(
  catchError(err => {
    console.error('Error:', err);
    return of([]); // Return empty array on error
  })
);

// switchMap: Switch to new observable
this.route.params.pipe(
  switchMap(params => this.userService.getUserById(params.id))
);
```

---

## 8.3 Subject

**Definition:** Subject is an Observable that can also emit values (multicast).

**Syntax:**

```typescript
import { Subject, BehaviorSubject } from 'rxjs';

// Subject (basic)
const subject = new Subject<string>();

// Subscribe
subject.subscribe(value => console.log('Subscriber 1:', value));
subject.subscribe(value => console.log('Subscriber 2:', value));

// Emit
subject.next('Hello');  // Both subscribers receive

// BehaviorSubject (remembers last value)
const behaviorSubject = new BehaviorSubject<string>('initial');

// New subscriber gets initial value immediately
behaviorSubject.subscribe(value => console.log(value)); // initial

behaviorSubject.next('new value');  // All subscribers get new value
```

**Using Subject in Service:**

```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isLoggedIn = new BehaviorSubject<boolean>(false);
  
  get isLoggedIn$(): Observable<boolean> {
    return this.isLoggedIn.asObservable();
  }
  
  login(email: string, password: string) {
    // Call API, on success:
    this.isLoggedIn.next(true);
  }
  
  logout() {
    this.isLoggedIn.next(false);
  }
}

// Component
export class AppComponent implements OnInit {
  isLoggedIn$: Observable<boolean>;
  
  constructor(private authService: AuthService) {
    this.isLoggedIn$ = this.authService.isLoggedIn$;
  }
  
  // In template: *ngIf="isLoggedIn$ | async"
}
```

---

# PART 9: DIRECTIVES & PIPES

## 9.1 Built-in Directives

**Syntax:**

```html
<!-- Structural directives: Change DOM structure -->

<!-- *ngIf: Add/remove from DOM -->
<div *ngIf="isVisible">Visible</div>
<div *ngIf="isLoading; then loading else content"></div>
<ng-template #loading>Loading...</ng-template>
<ng-template #content>Content</ng-template>

<!-- *ngFor: Loop through array -->
<li *ngFor="let user of users; let i = index">
  {{ i }}: {{ user.name }}
</li>

<!-- *ngSwitch: Switch statement -->
<div [ngSwitch]="status">
  <div *ngSwitchCase="'loading'">Loading...</div>
  <div *ngSwitchCase="'error'">Error occurred</div>
  <div *ngSwitchDefault>Content</div>
</div>

<!-- Attribute directives: Change element behavior/appearance -->

<!-- [ngClass]: Add/remove classes -->
<div [ngClass]="{'active': isActive, 'disabled': isDisabled}">
  Styled div
</div>

<!-- [ngStyle]: Inline styles -->
<div [ngStyle]="{'color': textColor, 'font-size.px': fontSize}">
  Styled text
</div>

<!-- [ngModel]: Two-way binding -->
<input [(ngModel)]="userName" />

<!-- [class.*]: Bind specific class -->
<div [class.active]="isActive">Active</div>

<!-- [style.*]: Bind specific style -->
<div [style.color]="textColor">Colored</div>
```

---

## 9.2 Custom Directives

**Attribute Directive:**

```typescript
// highlight.directive.ts
import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appHighlight]',
  standalone: true
})
export class HighlightDirective {
  @Input() highlightColor: string = 'yellow';
  
  constructor(private el: ElementRef) {}
  
  @HostListener('mouseenter')
  onMouseEnter() {
    this.el.nativeElement.style.backgroundColor = this.highlightColor;
  }
  
  @HostListener('mouseleave')
  onMouseLeave() {
    this.el.nativeElement.style.backgroundColor = '';
  }
}

// Usage
// <div appHighlight [highlightColor]="'pink'">Hover me</div>
```

**Structural Directive:**

```typescript
// unless.directive.ts
import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
  selector: '[appUnless]',
  standalone: true
})
export class UnlessDirective {
  @Input()
  set appUnless(condition: boolean) {
    if (!condition) {
      this.vcr.createEmbeddedView(this.templateRef);
    } else {
      this.vcr.clear();
    }
  }
  
  constructor(
    private templateRef: TemplateRef<any>,
    private vcr: ViewContainerRef
  ) {}
}

// Usage
// <div *appUnless="isDisabled">Show when NOT disabled</div>
```

---

## 9.3 Pipes

**Definition:** Pipes format data for display.

**Built-in Pipes:**

```html
<!-- Currency pipe -->
<p>{{ price | currency }}</p>                    <!-- $100.00 -->
<p>{{ price | currency:'€' }}</p>                <!-- €100.00 -->

<!-- Date pipe -->
<p>{{ date | date }}</p>                         <!-- Jan 1, 2024 -->
<p>{{ date | date:'yyyy-MM-dd' }}</p>            <!-- 2024-01-01 -->

<!-- Number pipe -->
<p>{{ 1234.5678 | number:'1.2-2' }}</p>          <!-- 1,234.57 -->

<!-- Uppercase/Lowercase pipes -->
<p>{{ 'hello' | uppercase }}</p>                 <!-- HELLO -->
<p>{{ 'HELLO' | lowercase }}</p>                 <!-- hello -->

<!-- JSON pipe -->
<pre>{{ user | json }}</pre>

<!-- Slice pipe -->
<p>{{ 'Hello World' | slice:0:5 }}</p>           <!-- Hello -->

<!-- Async pipe (unwrap Observable) -->
<p>{{ users$ | async | json }}</p>

<!-- Chaining pipes -->
<p>{{ price | currency | uppercase }}</p>
```

**Custom Pipe:**

```typescript
// capitalize.pipe.ts
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'capitalize',
  standalone: true
})
export class CapitalizePipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';
    return value.charAt(0).toUpperCase() + value.slice(1);
  }
}

// Usage
// {{ 'hello' | capitalize }}  → Hello

// In component
import { CapitalizePipe } from '../pipes/capitalize.pipe';

@Component({
  imports: [CapitalizePipe]
})
export class MyComponent {}
```

---

# PART 10: STATE MANAGEMENT

## 10.1 Service-Based State Management

**Definition:** Using services with BehaviorSubject for shared state.

**Syntax:**

```typescript
// app-state.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AppState {
  user: any | null;
  isLoading: boolean;
  notification: any | null;
}

@Injectable({ providedIn: 'root' })
export class AppStateService {
  private readonly initialState: AppState = {
    user: null,
    isLoading: false,
    notification: null
  };
  
  private state = new BehaviorSubject<AppState>(this.initialState);
  
  // Public observables
  state$: Observable<AppState> = this.state.asObservable();
  user$: Observable<any | null> = this.state$.pipe(map(s => s.user));
  isLoading$: Observable<boolean> = this.state$.pipe(map(s => s.isLoading));
  
  // Getters
  getState(): AppState {
    return this.state.value;
  }
  
  getUser(): any {
    return this.state.value.user;
  }
  
  // Setters
  setUser(user: any) {
    this.setState({ ...this.state.value, user });
  }
  
  setLoading(isLoading: boolean) {
    this.setState({ ...this.state.value, isLoading });
  }
  
  setNotification(notification: any) {
    this.setState({ ...this.state.value, notification });
  }
  
  private setState(state: AppState) {
    this.state.next(state);
  }
  
  resetState() {
    this.setState(this.initialState);
  }
}
```

**Using Service:**

```typescript
// app.component.ts
export class AppComponent implements OnInit {
  user$ = this.appState.user$;
  isLoading$ = this.appState.isLoading$;
  
  constructor(private appState: AppStateService) {}
  
  ngOnInit() {
    this.appState.setLoading(true);
    // Load data...
    this.appState.setLoading(false);
  }
}
```

---

## 10.2 NgRx (Advanced)

**Definition:** RxJS-based state management library for complex apps.

**Installation:**

```bash
ng add @ngrx/store
```

**Actions:**

```typescript
// user.actions.ts
import { createAction, props } from '@ngrx/store';
import { User } from '../models/user';

export const loadUsers = createAction('[Users Page] Load Users');

export const loadUsersSuccess = createAction(
  '[Users API] Load Users Success',
  props<{ users: User[] }>()
);

export const loadUsersFailure = createAction(
  '[Users API] Load Users Failure',
  props<{ error: string }>()
);
```

**Reducer:**

```typescript
// user.reducer.ts
import { createReducer, on } from '@ngrx/store';
import * as UserActions from './user.actions';

export interface UserState {
  users: User[];
  loading: boolean;
  error: string | null;
}

const initialState: UserState = {
  users: [],
  loading: false,
  error: null
};

export const userReducer = createReducer(
  initialState,
  on(UserActions.loadUsers, state => ({
    ...state,
    loading: true
  })),
  on(UserActions.loadUsersSuccess, (state, { users }) => ({
    ...state,
    users,
    loading: false,
    error: null
  })),
  on(UserActions.loadUsersFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error
  }))
);
```

---

# PART 11: TESTING

## 11.1 Unit Testing with Jasmine

**Component Test:**

```typescript
// user.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserComponent } from './user.component';
import { UserService } from '../services/user.service';

describe('UserComponent', () => {
  let component: UserComponent;
  let fixture: ComponentFixture<UserComponent>;
  let userService: UserService;
  
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserComponent],
      providers: [UserService]
    }).compileComponents();
    
    fixture = TestBed.createComponent(UserComponent);
    component = fixture.componentInstance;
    userService = TestBed.inject(UserService);
    fixture.detectChanges();
  });
  
  it('should create', () => {
    expect(component).toBeTruthy();
  });
  
  it('should load users on init', () => {
    spyOn(userService, 'getUsers').and.returnValue(of([
      { id: 1, name: 'Alice' }
    ]));
    
    component.ngOnInit();
    
    expect(userService.getUsers).toHaveBeenCalled();
    expect(component.users.length).toBe(1);
  });
  
  it('should delete user', () => {
    component.users = [{ id: 1, name: 'Alice' }];
    spyOn(userService, 'deleteUser').and.returnValue(of(void 0));
    
    component.deleteUser(1);
    
    expect(userService.deleteUser).toHaveBeenCalledWith(1);
    expect(component.users.length).toBe(0);
  });
});
```

**Service Test:**

```typescript
// user.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UserService } from './user.service';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;
  
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService]
    });
    
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });
  
  afterEach(() => {
    httpMock.verify();
  });
  
  it('should fetch users', () => {
    const mockUsers = [
      { id: 1, name: 'Alice' },
      { id: 2, name: 'Bob' }
    ];
    
    service.getUsers().subscribe(users => {
      expect(users.length).toBe(2);
      expect(users).toEqual(mockUsers);
    });
    
    const req = httpMock.expectOne('/api/users');
    expect(req.request.method).toBe('GET');
    req.flush(mockUsers);
  });
});
```

---

# PART 12: DEPLOYMENT

## 12.1 Building for Production

**Syntax:**

```bash
# Build optimized production bundle
ng build --configuration production

# Output in dist/ directory
# - Main bundle
# - Vendor bundle
# - CSS bundles
# - Source maps (optional)

# Build with optimizations
ng build --configuration production --optimization

# Build without source maps (smaller)
ng build --configuration production --source-map=false

# Output files
dist/
├── index.html
├── main-HASH.js          # Main app bundle
├── polyfills-HASH.js     # Browser compatibility
├── runtime-HASH.js       # Angular runtime
├── styles-HASH.css       # Global styles
└── assets/               # Static files
```

---

## 12.2 Environment Configuration

**Syntax:**

```typescript
// environment.ts (development)
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  logLevel: 'debug'
};

// environment.prod.ts (production)
export const environment = {
  production: true,
  apiUrl: 'https://api.myapp.com/api',
  logLevel: 'error'
};

// app.config.ts
import { environment } from '../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConfigService {
  apiUrl = environment.apiUrl;
}

// angular.json configuration
"configurations": {
  "development": {
    "fileReplacements": [
      {
        "replace": "src/environments/environment.ts",
        "with": "src/environments/environment.development.ts"
      }
    ]
  },
  "production": {
    "fileReplacements": [
      {
        "replace": "src/environments/environment.ts",
        "with": "src/environments/environment.prod.ts"
      }
    ]
  }
}

// Build with specific environment
ng build --configuration development
ng build --configuration production
```

---

## 12.3 Docker Deployment

**Dockerfile:**

```dockerfile
# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**nginx.conf (SPA routing):**

```nginx
server {
    listen 80;
    root /usr/share/nginx/html;
    
    location / {
        try_files $uri $uri/ /index.html;
    }
    
    location /api/ {
        proxy_pass http://backend:5000;
        proxy_set_header Host $host;
    }
}
```

---

## 12.4 Azure DevOps Pipeline

**azure-pipelines.yml:**

```yaml
trigger:
  - main
  - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  nodeVersion: '20.x'
  buildConfiguration: 'production'

stages:
  - stage: Build
    displayName: Build Angular App
    jobs:
      - job: BuildAngular
        displayName: Build & Test
        steps:
          - task: NodeTool@0
            inputs:
              versionSpec: $(nodeVersion)

          - task: Npm@1
            displayName: Install Dependencies
            inputs:
              command: 'install'

          - task: Npm@1
            displayName: Run Tests
            inputs:
              command: 'custom'
              customCommand: 'run test -- --watch=false --browsers=ChromeHeadless'

          - task: Npm@1
            displayName: Build Production
            inputs:
              command: 'custom'
              customCommand: 'run build -- --configuration production'

          - task: PublishBuildArtifacts@1
            displayName: Publish Artifacts
            inputs:
              pathToPublish: 'dist'
              artifactName: 'angular-build'

  - stage: Deploy
    displayName: Deploy to Azure
    dependsOn: Build
    condition: succeeded()
    jobs:
      - deployment: DeployApp
        displayName: Deploy App Service
        environment: 'production'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebApp@1
                  displayName: Deploy to App Service
                  inputs:
                    azureSubscription: 'AzureConnection'
                    appName: 'my-angular-app'
                    package: '$(Pipeline.Workspace)/angular-build'
```

---

## 12.5 ASP.NET Hosting (Serve as Static Files)

**ASP.NET Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins("https://myapp.azurewebsites.net")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AngularApp");
app.UseHttpsRedirection();

// Serve Angular static files
app.UseStaticFiles();

// Map API routes
app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
```

---

## End of Angular v22 Learning Guide

**Complete Coverage:**
✅ Angular setup with Angular CLI
✅ Components & templates
✅ Data binding (property, event, two-way)
✅ Services & dependency injection
✅ HTTP Client with ASP.NET Web API
✅ Routing (routes, parameters, guards)
✅ Forms (template-driven & reactive)
✅ RxJS Observables & operators
✅ Directives (built-in & custom)
✅ Pipes (built-in & custom)
✅ State management (services & NgRx)
✅ Testing (Jasmine & unit tests)
✅ Deployment (Docker, Azure, ASP.NET hosting)

---

### Angular Version Comparison

| Feature | Angular.js (1.x) | Angular 2-16 | Angular 17+ |
|---------|------------------|--------------|------------|
| **Language** | JavaScript | TypeScript | TypeScript |
| **Modules** | Manual @NgModule | @NgModule | Standalone (default) |
| **CLI** | Manual setup | Angular CLI | Angular CLI (better) |
| **Rendering** | Direct DOM | Change Detection | Signals (new) |
| **Zoneless** | N/A | N/A | Possible (v14+) |
| **Bundle Size** | ~50KB | ~500KB+ | ~200KB+ (with optimizations) |
| **RxJS** | No | Required | Required |
| **Community** | Declining | Large | Very large |

---

**This guide is production-ready for Angular v22 developers integrating with ASP.NET Web API and deploying to Azure or Docker.**
