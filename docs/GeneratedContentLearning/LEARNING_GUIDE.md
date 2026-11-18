# Full Stack Web Development Learning Guide
## ASP.NET + Angular + React Demo

A comprehensive guide to understanding modern web application architecture with zero prior experience.

---

## Table of Contents

1. [High-Level Architecture](#high-level-architecture)
2. [Core Concepts](#core-concepts)
3. [ASP.NET Backend](#aspnet-backend)
4. [Angular Frontend](#angular-frontend)
5. [React Frontend](#react-frontend)
6. [How They Work Together](#how-they-work-together)
7. [Key Patterns & Concepts](#key-patterns--concepts)

---

## High-Level Architecture

### What is a Three-Tier Application?

Think of a restaurant:
- **Frontend (Waiter)**: Takes your order and displays the menu — this is Angular or React
- **Backend (Kitchen)**: Processes your order, prepares the food — this is ASP.NET
- **Database (Pantry)**: Stores ingredients — in this demo, stored in memory

```
┌─────────────────────────────────────────────────────────────┐
│                    USER'S BROWSER                            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Angular / React Frontend (Port 4200 or 5173)       │   │
│  │  - Login page, Product list, Forms                   │   │
│  │  - Beautiful UI, responsive design                   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           ↕ (HTTP Requests)
┌─────────────────────────────────────────────────────────────┐
│           ASP.NET Web API Backend (Port 5104)                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Program.cs - All endpoint definitions                │   │
│  │ - POST /api/auth/login - validate user               │   │
│  │ - POST /api/auth/register - create new user          │   │
│  │ - GET /api/products - list all products              │   │
│  │ - POST /api/products - create product (protected)    │   │
│  │ - PUT /api/products/{id} - update (protected)        │   │
│  │ - DELETE /api/products/{id} - delete (protected)     │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           ↕ (In-Memory Storage)
┌─────────────────────────────────────────────────────────────┐
│              In-Memory Data Store (No Database)              │
│  - List<User> with Alice Johnson, Bob Smith                 │
│  - List<Product> with Laptop, Mouse                         │
│  - Data persists only while server is running               │
└─────────────────────────────────────────────────────────────┘
```

---

## Core Concepts

### 1. **HTTP — The Language of the Web**

HTTP = HyperText Transfer Protocol. It's how your browser talks to servers.

**The 4 Main Methods:**

| Method | What it does | Example |
|--------|------------|---------|
| **GET** | Retrieve data | `GET /api/products` → fetch all products |
| **POST** | Create new data | `POST /api/auth/login` → send email + password, get JWT token |
| **PUT** | Update existing data | `PUT /api/products/5` → update product with ID 5 |
| **DELETE** | Remove data | `DELETE /api/products/5` → delete product with ID 5 |

**Status Codes:**
- `200` = Success ✓
- `400` = Bad request (you sent wrong data)
- `401` = Unauthorized (you need to log in)
- `404` = Not found (the resource doesn't exist)
- `500` = Server error (something broke)

### 2. **Authentication & JWT — Proving Who You Are**

**The Problem:** How does the server know that you are really Alice Johnson?

**The Old Way (Sessions):**
- You log in → Server creates a session file on disk
- Server gives you a session ID cookie
- Browser stores the cookie and sends it with every request
- Server looks up the session file to verify you

**The Modern Way (JWT):**
- You log in with email + password
- Server checks your credentials and creates a JWT token (a special encoded string)
- Token contains: who you are + when it expires + a signature to prove it's real
- Browser stores the token in localStorage
- Browser sends token in the `Authorization` header with every request
- Server decodes the token to see who you are — no session file needed

**JWT Token Example (simplified):**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
eyJzdWIiOiIxIiwibmFtZSI6IkFsaWNlIEpvaG5zb24iLCJleHAiOjE3MjUwMDAwMDB9.
signature_here_proves_token_is_real
```

**Why JWT?**
- Scales better (no server-side session storage)
- Works great for mobile apps
- Stateless (server doesn't store session state)

### 3. **Password Hashing — Never Store Plain Passwords**

**The Problem:** If hackers steal your database, they see all passwords.

**The Solution:** Hash passwords before storing.

**Hashing = One-Way Encryption:**
- `"mypassword123"` → hash function → `$2b$12$abcd1234xyz...` (unreadable)
- When you log in, hash the password you typed and compare with stored hash
- If someone steals the hash, they can't reverse it back to the password

**BCrypt:** The industry standard password hashing algorithm.

```csharp
// When user registers
string passwordHash = BCrypt.Net.BCrypt.HashPassword("mypassword123");
// Store passwordHash in database

// When user logs in
bool isValid = BCrypt.Net.BCrypt.Verify("mypassword123", passwordHash);
// Returns true only if password matches
```

### 4. **CORS — Cross-Origin Resource Sharing**

**The Problem:** For security, browsers block requests between different origins.

**Origin = protocol + host + port**
- `http://localhost:4200` (frontend)
- `http://localhost:5104` (backend)
- These are *different* origins → CORS protection blocks it

**The Solution:** Backend explicitly allows certain origins.

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.SetIsOriginAllowed(origin => 
            new Uri(origin).Host == "localhost")  // Allow localhost
        .AllowAnyMethod()     // GET, POST, PUT, DELETE all allowed
        .AllowAnyHeader()     // Any header allowed
        .AllowCredentials();  // Cookies & Authorization headers allowed
    });
});
```

---

## ASP.NET Backend

### What is ASP.NET?

ASP.NET is a **server framework** for building web APIs (and web apps).

**C# (pronounced "C Sharp")** is the language. It's modern, type-safe, and powerful.

### Architecture Choice: Minimal API vs Controllers

#### Minimal API (What We Use)

Endpoints are defined directly in `Program.cs` as lambdas:

```csharp
// Register a new user
app.MapPost("/api/auth/register", async (RegisterRequest req) =>
{
    // 1. Validate input
    if (string.IsNullOrEmpty(req.Email)) 
        return Results.BadRequest("Email required");
    
    // 2. Check if user already exists
    var existing = users.FirstOrDefault(u => u.Email == req.Email);
    if (existing != null) 
        return Results.BadRequest("Email already registered");
    
    // 3. Hash the password
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
    
    // 4. Create the user
    var user = new User 
    { 
        Id = users.Count + 1,
        Name = req.Name, 
        Email = req.Email, 
        PasswordHash = passwordHash,
        CreatedAt = DateTime.UtcNow
    };
    users.Add(user);
    
    // 5. Generate JWT token
    var token = GenerateJwtToken(user);
    
    // 6. Return the token
    return Results.Ok(new { token, user = new { user.Id, user.Name, user.Email } });
});
```

**Advantages:**
- Simple and concise
- Everything in one file (easy to see the whole API)
- Perfect for small projects, microservices, learning

**When NOT to use:**
- If you have 30+ endpoints → use Controllers for organization
- If different teams manage different endpoints → use Controllers

#### Controllers (Traditional Approach)

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        // Same logic, but in a separate file
    }
}
```

**Advantages:**
- Better organization for large APIs
- One controller = one resource (e.g., ProductsController for products)
- Industry standard for enterprise apps

### Key Backend Components

#### 1. **Models** — Data Structures

```csharp
public class User
{
    public int Id { get; set; }           // Unique identifier
    public string Name { get; set; }      // User's name
    public string Email { get; set; }     // Email (used for login)
    public string PasswordHash { get; set; } // Hashed password (NOT plain text!)
    public DateTime CreatedAt { get; set; } // When account was created
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }      // "Laptop", "Mouse"
    public string Description { get; set; }  // Details about the product
    public decimal Price { get; set; }    // Price in dollars
    public string CreatedBy { get; set; }    // Email of who created it
    public DateTime CreatedAt { get; set; }
}
```

#### 2. **In-Memory Data Storage** — No Database

```csharp
var users = new List<User>
{
    new User 
    { 
        Id = 1, 
        Name = "Alice Johnson", 
        Email = "alice@example.com",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
        CreatedAt = DateTime.UtcNow
    }
};

var products = new List<Product>();
```

When the server restarts, all data is lost. This is fine for demos, but production apps use databases (SQL Server, PostgreSQL, etc.).

#### 3. **JWT Token Generation**

```csharp
private static string GenerateJwtToken(User user)
{
    // 1. Read JWT secret from appsettings.json
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey)
    );
    
    // 2. Create signing credentials
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    // 3. Create claims (data inside the token)
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Name)
    };
    
    // 4. Create the token
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),  // Expires in 7 days
        signingCredentials: creds
    );
    
    // 5. Write it as a string
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**What's inside the token:**
- **Issuer:** Who created it ("AngularDotnetDemo")
- **Audience:** Who should use it ("AngularDotnetDemoClient")
- **Claims:** Data about the user (ID, email, name)
- **Expiration:** When it expires (7 days from now)
- **Signature:** Proof that it's real and wasn't tampered with

#### 4. **Protected Endpoints** — Require JWT

```csharp
// This endpoint requires authentication
app.MapPost("/api/products", async (CreateProductRequest req) =>
{
    var user = HttpContext.User;  // Current logged-in user
    var email = user.FindFirst(ClaimTypes.Email)?.Value;
    
    var product = new Product
    {
        Id = products.Count + 1,
        Name = req.Name,
        Description = req.Description,
        Price = req.Price,
        CreatedBy = email,  // Who created it
        CreatedAt = DateTime.UtcNow
    };
    products.Add(product);
    return Results.Ok(product);
}).RequireAuthorization();  // ← Forces login
```

When you call this endpoint:
- If no JWT token is sent → 401 Unauthorized
- If token is expired → 401 Unauthorized
- If token is invalid → 401 Unauthorized
- If token is valid → proceeds and you can use `HttpContext.User`

#### 5. **appsettings.json** — Configuration

```json
{
  "Jwt": {
    "Key": "ThisIsAVeryLongSecretKeyForDevelopmentPurposesOnly123!",
    "Issuer": "AngularDotnetDemo",
    "Audience": "AngularDotnetDemoClient"
  }
}
```

Never hardcode secrets! Use configuration files or environment variables.

---

## Angular Frontend

### What is Angular?

Angular is a **frontend framework** built by Google. It lets you build interactive UIs with components, services, and reactive programming.

### Core Concepts

#### 1. **Components** — Reusable UI Blocks

A component = HTML template + TypeScript logic + CSS styles

```typescript
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',                    // HTML tag: <app-login></app-login>
  imports: [CommonModule, FormsModule, RouterLink],  // Dependencies
  template: `                               // HTML template (inline)
    <form (ngSubmit)="login()">
      <input [(ngModel)]="email" name="email" type="email" />
      <input [(ngModel)]="password" name="password" type="password" />
      <button type="submit">Login</button>
    </form>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  error: string | null = null;
  submitting = false;

  login(): void {
    this.submitting = true;
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => { this.error = 'Invalid credentials'; this.submitting = false; }
    });
  }
}
```

**Key Concepts:**
- `selector`: HTML tag name
- `imports`: Required modules
- `template`: HTML code
- Properties (`email`, `password`, `error`) = data
- Methods (`login()`) = what happens when user clicks

#### 2. **Two-Way Data Binding** — Automatic Synchronization

```html
<input [(ngModel)]="email" />
```

The `[(ngModel)]` syntax = two-way binding:
- **One way:** When you type in the input, `email` property updates automatically
- **Other way:** When `email` changes in code, the input displays the new value

**Without binding (harder):**
```typescript
const emailInput = document.querySelector('input');
emailInput.addEventListener('change', (e) => {
  this.email = e.target.value;
});
```

**With binding (easy):**
```html
<input [(ngModel)]="email" />
<!-- That's it! Automatic sync in both directions -->
```

#### 3. **Services & Dependency Injection** — Reusable Logic

A service = a class with business logic that can be shared across components.

```typescript
// auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })  // Available everywhere
export class AuthService {
  private readonly http = inject(HttpClient);  // DI: inject HttpClient
  private readonly apiUrl = '/api/auth';

  login(email: string, password: string): Observable<AuthResult> {
    return this.http.post<AuthResult>(
      `${this.apiUrl}/login`, 
      { email, password }
    ).pipe(
      tap(res => {  // When response arrives, also do this:
        localStorage.setItem('jwt_token', res.token);
        localStorage.setItem('auth_user', JSON.stringify(res.user));
      })
    );
  }

  get token(): string | null {
    return localStorage.getItem('jwt_token');
  }

  logout(): void {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('auth_user');
  }
}

// In LoginComponent:
constructor(private auth: AuthService) {}

login(): void {
  this.auth.login(this.email, this.password).subscribe(...);
}
```

**Dependency Injection (DI):**
- `@Injectable({ providedIn: 'root' })` = "register this service globally"
- `inject(HttpClient)` = "give me an instance of HttpClient"
- Why? Angular manages the lifecycle, ensures single instance, easy to test

#### 4. **HTTP Interceptors** — Auto-Add JWT Token

Without interceptor:
```typescript
this.http.get('/api/products', {
  headers: new HttpHeaders({
    'Authorization': `Bearer ${this.auth.token}`
  })
});
```

Repeated every single API call. Tedious!

**With Interceptor:**
```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private readonly auth = inject(AuthService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.auth.token;
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next.handle(req);
  }
}

// Register in app.config.ts:
{ provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
```

Now **every HTTP request** automatically includes the JWT token. No manual header setting!

#### 5. **Route Guards** — Protect Pages

```typescript
// auth.guard.ts
export const authGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn) {
    return true;  // Allow access
  }
  return router.parseUrl('/login');  // Redirect to login
};

// app.routes.ts
export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { 
    path: 'products', 
    component: ProductListComponent,
    canActivate: [authGuard]  // ← Only accessible if logged in
  },
];
```

Without guard: Anyone can access `/products` even if not logged in (bad!)
With guard: If you try to access `/products` without logging in, you're redirected to `/login`.

#### 6. **Observable & Reactive Programming** — Handle Async Data

**What's an Observable?**
Think of it as a **stream of events over time**.

```typescript
// Without Observable:
let products: Product[] = [];
fetch('/api/products')
  .then(res => res.json())
  .then(data => products = data);
// products is empty for a moment, then fills in later

// With Observable:
products$: Observable<Product[]> = this.productService.getProducts();

// In template:
<div *ngIf="products$ | async as products">
  <div *ngFor="let p of products">{{ p.name }}</div>
</div>
```

The `| async` pipe:
- Subscribes automatically
- Updates when data arrives
- Unsubscribes when component is destroyed
- Triggers change detection

**RxJS Operators:**

```typescript
this.productService.getProducts()
  .pipe(
    map(products => products.filter(p => p.price < 100)),  // Transform
    catchError(err => {                                     // Handle errors
      console.error(err);
      return [];
    })
  )
  .subscribe(filtered => console.log(filtered));
```

#### 7. **Routing** — Navigate Between Pages

```typescript
const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'products', component: ProductListComponent, canActivate: [authGuard] },
  { path: 'products/new', component: ProductFormComponent, canActivate: [authGuard] },
  { path: 'products/:id', component: ProductFormComponent, canActivate: [authGuard] },
];
```

**Dynamic Routes:**
- `products/:id` = `:id` is a parameter
- In component: `this.route.snapshot.paramMap.get('id')`
- Access via `<a routerLink="/products/5">Edit Product 5</a>`

---

## React Frontend

### What is React?

React is a **frontend framework** built by Meta. It focuses on components and state management with a simpler learning curve than Angular.

### Core Concepts

#### 1. **Components** — Functions That Return JSX

```javascript
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { login, selectAuth } from '../features/auth/authSlice';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { status, error } = useSelector(selectAuth);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const result = await dispatch(login({ email, password }));
    if (result.meta.requestStatus === 'fulfilled') {
      navigate('/products');
    }
  };

  return (
    <div className="login-container">
      <h1>Login</h1>
      {error && <p className="error">{error}</p>}
      <form onSubmit={handleSubmit}>
        <input 
          value={email} 
          onChange={(e) => setEmail(e.target.value)} 
          type="email" 
          placeholder="Email"
        />
        <input 
          value={password} 
          onChange={(e) => setPassword(e.target.value)} 
          type="password" 
          placeholder="Password"
        />
        <button disabled={status === 'loading'}>
          {status === 'loading' ? 'Logging in...' : 'Login'}
        </button>
      </form>
    </div>
  );
}
```

**Key Differences from Angular:**
- Components are **functions**, not classes
- JSX = HTML-like syntax inside JavaScript
- Simpler, more JavaScript-like

#### 2. **Hooks** — Add Features to Functions

Hooks let you add state and side effects to functional components.

**useState — State Management:**
```javascript
const [email, setEmail] = useState('');  // "email" state, "setEmail" to update

// When user types:
<input value={email} onChange={(e) => setEmail(e.target.value)} />

// Update state:
setEmail('alice@example.com');
// Component re-renders with new value
```

**useEffect — Side Effects:**
```javascript
useEffect(() => {
  // This runs after component renders
  dispatch(fetchProducts());
}, [dispatch]);  // Run once when component mounts

// Cleanup:
useEffect(() => {
  const timer = setInterval(() => console.log('tick'), 1000);
  return () => clearInterval(timer);  // Cleanup when component unmounts
}, []);
```

**useSelector — Read Redux State:**
```javascript
const isAuthenticated = useSelector(selectIsAuthenticated);
const user = useSelector(selectAuthUser);
```

**useDispatch — Trigger Redux Actions:**
```javascript
const dispatch = useDispatch();

dispatch(login({ email, password }));
```

#### 3. **Redux & Redux Toolkit** — State Management

**The Problem:** As your app grows, components have deeply nested state. Passing data through 5 levels of components is painful.

**The Solution:** Redux = a central store for all state.

```
┌─────────────────────────────────────────────┐
│         Redux Store (Central State)          │
│  ┌──────────────────────────────────────┐   │
│  │ auth: {                              │   │
│  │   isAuthenticated: true,             │   │
│  │   user: { id: 1, name: "Alice" },    │   │
│  │   token: "jwt..."                    │   │
│  │ }                                    │   │
│  │ products: {                          │   │
│  │   items: [...],                      │   │
│  │   status: 'loading'                  │   │
│  │ }                                    │   │
│  └──────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
         ↑                            ↓
    Components                   Components
   (read state)                 (dispatch actions)
```

**Redux Toolkit Pattern (Modern):**

```javascript
// features/auth/authSlice.js
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { authAPI } from './authAPI';

// Define what the API should do
export const login = createAsyncThunk('auth/login', async ({ email, password }) => {
  const data = await authAPI.login(email, password);
  return data;  // Redux automatically stores this in payload
});

// Define state shape and how it changes
const authSlice = createSlice({
  name: 'auth',
  initialState: {
    isAuthenticated: false,
    user: null,
    token: null,
    status: 'idle',  // 'idle' | 'loading' | 'succeeded' | 'failed'
    error: null,
  },
  reducers: {
    logout(state) {
      state.isAuthenticated = false;
      state.token = null;
      state.user = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.isAuthenticated = true;
        state.user = action.payload.user;
        state.token = action.payload.token;
        localStorage.setItem('jwt_token', action.payload.token);
      })
      .addCase(login.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message;
      });
  },
});

export const { logout } = authSlice.actions;
export const selectIsAuthenticated = (state) => state.auth.isAuthenticated;
export default authSlice.reducer;
```

**In a Component:**

```javascript
const dispatch = useDispatch();
const isAuthenticated = useSelector(selectIsAuthenticated);
const { status, error } = useSelector(selectAuth);

// Dispatch an action
dispatch(login({ email, password }));

// Subscribe to state
if (isAuthenticated) {
  // User is logged in
}
```

**Flow:**
1. User clicks "Login" button
2. Component dispatches `login` action
3. `login` thunk calls `authAPI.login()` (API request)
4. While waiting, `login.pending` updates state (`status: 'loading'`)
5. Response arrives → `login.fulfilled` updates state (`isAuthenticated: true`, `token: ...`)
6. Component re-renders with new state
7. Navigation redirects to `/products`

#### 4. **API Layer** — Fetch Data

```javascript
// features/auth/authAPI.js
const BASE_URL = '/api/auth';

export const authAPI = {
  async login(email, password) {
    const response = await fetch(`${BASE_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });
    if (!response.ok) throw new Error('Login failed');
    return response.json();
  },

  async register(name, email, password) {
    const response = await fetch(`${BASE_URL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, email, password }),
    });
    if (!response.ok) throw new Error('Registration failed');
    return response.json();
  },
};
```

**Protected API Calls (with JWT token):**

```javascript
// features/products/productsAPI.js
function getAuthHeaders() {
  const token = localStorage.getItem('jwt_token');
  const headers = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return headers;
}

export const productsAPI = {
  async getAll() {
    const response = await fetch('/api/products', {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch');
    return response.json();
  },

  async create(product) {
    const response = await fetch('/api/products', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(product),
    });
    if (!response.ok) throw new Error('Failed to create');
    return response.json();
  },
};
```

#### 5. **Protected Routes** — Restrict Pages

```javascript
// components/ProtectedRoute.jsx
import { Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { selectIsAuthenticated } from '../features/auth/authSlice';

export default function ProtectedRoute({ children }) {
  const isAuthenticated = useSelector(selectIsAuthenticated);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return children;
}

// App.jsx
<Routes>
  <Route path="/login" element={<Login />} />
  <Route 
    path="/products" 
    element={<ProtectedRoute><ProductList /></ProtectedRoute>} 
  />
</Routes>
```

#### 6. **useNavigate — Programmatic Navigation**

```javascript
import { useNavigate } from 'react-router-dom';

function Login() {
  const navigate = useNavigate();

  const handleSuccess = () => {
    navigate('/products');  // Go to /products after login
  };

  return <form onSubmit={handleSuccess}>...</form>;
}
```

---

## How They Work Together

### Complete Login Flow

#### 1. User Opens Browser → Angular/React Frontend

```
Browser: http://localhost:4200 (Angular)
         http://localhost:5173 (React)
```

Frontend loads → User sees login form.

#### 2. User Submits Email & Password → Frontend Makes API Request

**Angular:**
```typescript
this.auth.login(email, password).subscribe({
  next: (result) => { /* store token */ },
  error: (err) => { /* show error */ }
});
```

**React:**
```javascript
dispatch(login({ email, password }));
```

Both send:
```
POST http://localhost:5104/api/auth/login
Content-Type: application/json

{
  "email": "alice@example.com",
  "password": "password123"
}
```

#### 3. Backend Validates Credentials

```csharp
var user = users.FirstOrDefault(u => u.Email == email);
if (user == null) 
  return Results.Unauthorized();

bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
if (!isValid) 
  return Results.Unauthorized();

// Password is correct!
var token = GenerateJwtToken(user);
return Results.Ok(new { token, user });
```

#### 4. Backend Sends JWT Token → Frontend Stores It

```json
200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": { "id": 1, "name": "Alice Johnson", "email": "alice@example.com" }
}
```

**Angular stores:**
```typescript
localStorage.setItem('jwt_token', token);
localStorage.setItem('auth_user', JSON.stringify(user));
```

**React stores (via Redux):**
```javascript
state.token = token;
state.user = user;
localStorage.setItem('jwt_token', token);
```

#### 5. User Navigates to `/products` → Protected Route

**Angular Route Guard:**
```typescript
canActivate: [authGuard]

// authGuard checks:
if (auth.isLoggedIn)  // Check localStorage for token
  return true;
else
  return router.parseUrl('/login');  // Redirect if no token
```

**React Protected Route:**
```javascript
const isAuthenticated = useSelector(selectIsAuthenticated);
if (!isAuthenticated)
  return <Navigate to="/login" />;
```

#### 6. Frontend Fetches Products with JWT Token

**Angular:**
```typescript
this.http.get('/api/products')  // Interceptor auto-adds token
```

**React:**
```javascript
fetch('/api/products', {
  headers: { 'Authorization': `Bearer ${token}` }
})
```

Both send:
```
GET http://localhost:5104/api/products
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### 7. Backend Validates Token & Returns Products

```csharp
var user = HttpContext.User;  // Auto-validated by middleware
var email = user.FindFirst(ClaimTypes.Email)?.Value;

return Results.Ok(products);  // Return all products
```

#### 8. Frontend Displays Products

Both Angular and React render the product list to the user.

### Complete Add Product Flow

```
User fills form (Name, Description, Price)
        ↓
Frontend validates (required fields, valid price)
        ↓
Frontend sends POST with JWT token
        ↓
Backend validates token (is user logged in?)
        ↓
Backend creates product (sets CreatedBy = user's email)
        ↓
Backend returns product with ID
        ↓
Frontend adds product to list (no page refresh needed)
        ↓
User sees new product in the list
```

---

## Key Patterns & Concepts

### 1. **Separation of Concerns**

Each layer has one responsibility:

| Layer | Responsibility |
|-------|-----------------|
| **Frontend (Angular/React)** | User interface, input validation, displaying data |
| **Backend (ASP.NET)** | Business logic, authentication, database |
| **Database** | Storing data |

### 2. **Stateless Authentication (JWT)**

- Server doesn't remember who you are
- Each request proves who you are with a token
- Scales easily (no session storage needed)

### 3. **Reactive Programming (Angular) vs Imperative (React)**

**Angular (Reactive):**
- Think in streams of data
- Observables emit values over time
- `| async` pipe handles subscription automatically
- More powerful for complex apps

**React (Imperative):**
- Tell the computer what to do step-by-step
- useState updates state, component re-renders
- Simpler mental model
- Easier to learn

### 4. **State Management**

**Frontend State:**
- Is user logged in?
- What products are displayed?
- Form input values
- Loading/error status

**Backend State:**
- User database
- Product database
- Session state (in this demo, JWT tokens)

### 5. **Error Handling**

**Frontend:**
```typescript
// Angular
.subscribe({
  next: (data) => { /* success */ },
  error: (err) => { /* show error message */ }
});

// React
.catch(err => {
  setError(err.message);
});
```

**Backend:**
```csharp
if (string.IsNullOrEmpty(email))
  return Results.BadRequest("Email required");

if (user == null)
  return Results.NotFound();

if (!isValid)
  return Results.Unauthorized();
```

---

## Common Gotchas & How to Avoid Them

### 1. **Forgot to Send JWT Token**

**Wrong:**
```typescript
// Forgot Authorization header
this.http.get('/api/products')
```

**Right:**
```typescript
// Auto-added by interceptor (Angular)
this.http.get('/api/products')

// Or explicit (React)
fetch('/api/products', {
  headers: { 'Authorization': `Bearer ${token}` }
})
```

### 2. **Storing Passwords in Plain Text**

**Wrong:**
```csharp
user.Password = password;  // NEVER do this!
```

**Right:**
```csharp
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
```

### 3. **Forgetting to Validate on the Backend**

**Wrong:**
```csharp
app.MapPost("/api/products", (CreateProductRequest req) =>
{
    // Trust frontend validation — BAD!
    products.Add(req);
});
```

**Right:**
```csharp
app.MapPost("/api/products", (CreateProductRequest req) =>
{
    if (string.IsNullOrEmpty(req.Name))
        return Results.BadRequest("Name required");
    
    if (req.Price <= 0)
        return Results.BadRequest("Price must be positive");
    
    // Now safe to create
    products.Add(new Product { Name = req.Name, Price = req.Price });
});
```

Frontend validation is for UX (instant feedback). Backend validation is for security!

### 4. **CORS Errors**

**Error:**
```
Access to XMLHttpRequest at 'http://localhost:5104/api/products'
from origin 'http://localhost:4200' has been blocked by CORS policy
```

**Solution:** Backend must enable CORS:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

app.UseCors();  // Must be called!
```

### 5. **JWT Token Not Being Sent**

Check:
- Is token stored in localStorage/Redux?
- Is interceptor (Angular) or getAuthHeaders (React) being used?
- Is token included in Authorization header?
- Is token valid (not expired)?

---

## Learning Path for Beginners

### Week 1: Fundamentals
1. Understand HTTP & REST (GET, POST, PUT, DELETE)
2. Understand JWT authentication
3. Build a simple ASP.NET API

### Week 2: Angular
1. Understand components & services
2. Build login form
3. Add route guards
4. Make API calls

### Week 3: React
1. Understand hooks & functional components
2. Build login form with Redux
3. Add protected routes
4. Make API calls

### Week 4: Integration
1. Run all 3 solutions together
2. Test authentication flow
3. Test product CRUD
4. Debug issues

---

## Useful Commands

### ASP.NET Backend
```bash
cd backend/AngularDotnetDemo.Api
dotnet build              # Compile
dotnet run                # Run the server
```

### Angular Frontend
```bash
cd frontend
npm install               # Install dependencies
npm start                 # Dev server on port 4200
npx ng build              # Production build
```

### React Frontend
```bash
cd ReactDotNetDemo
npm install               # Install dependencies
npm run dev               # Dev server on port 5173
npm run build             # Production build
```

---

## Resources for Learning

- **HTTP Basics:** [MDN - HTTP](https://developer.mozilla.org/en-US/docs/Web/HTTP)
- **JWT:** [jwt.io](https://jwt.io)
- **C# & ASP.NET:** [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/)
- **Angular:** [Angular Official Docs](https://angular.io/docs)
- **React:** [React Official Docs](https://react.dev)
- **Redux:** [Redux Official Docs](https://redux.js.org/)

---

## Summary

| Component | What it does | Technologies |
|-----------|------------|--------------|
| **Backend** | Validates, authenticates, stores data | ASP.NET, C#, JWT, BCrypt |
| **Angular Frontend** | Beautiful, reactive UI | TypeScript, Angular, RxJS, HttpClient |
| **React Frontend** | Simple, interactive UI | JavaScript, React, Redux, Hooks |
| **Communication** | Exchange data securely | HTTPS, JWT, CORS, HTTP |

All three work together to create a complete, secure web application!

---

**Remember:** You don't need to understand everything at once. Start with one concept, practice it, then move to the next. Build small things and gradually increase complexity. Happy learning! 🚀
