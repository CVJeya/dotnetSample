# Angular.js Learning Guide: From Zero to Mastery

A comprehensive guide for beginners to learn Angular.js, from fundamentals through advanced concepts, with integration patterns for ASP.NET Web API.

---

## Table of Contents

1. [Angular.js Fundamentals](#angularjs-fundamentals)
2. [Core Concepts & Syntax](#core-concepts--syntax)
3. [Services & Dependency Injection](#services--dependency-injection)
4. [Controllers & Scopes](#controllers--scopes)
5. [Directives](#directives)
6. [Filters](#filters)
7. [Routing](#routing)
8. [ASP.NET Web API Integration](#aspnet-web-api-integration)
9. [Advanced Concepts](#advanced-concepts)
10. [Best Practices](#best-practices)

---

# PART 1: ANGULAR.JS FUNDAMENTALS

## 1.1 What is Angular.js?

**Definition:** Angular.js (also called AngularJS or Angular 1.x) is a JavaScript framework for building dynamic web applications. It uses the **MVC/MVVM** pattern and provides two-way data binding.

**Key Concepts:**

```
┌─────────────────────────────────────┐
│        AngularJS Application        │
├─────────────────────────────────────┤
│  View (HTML)                        │ ← User sees this
│    ↕ (Two-way data binding)         │
│  Controller (JavaScript Logic)      │ ← Updates model
│    ↕ (Dependency Injection)         │
│  Services (Business Logic)          │ ← Reusable logic
│    ↕                                │
│  API (ASP.NET Web API)              │ ← Backend
└─────────────────────────────────────┘
```

---

## 1.2 Setting Up Angular.js

**Installation Methods:**

```html
<!-- Method 1: CDN (Easiest for learning) -->
<script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.8.2/angular.min.js"></script>

<!-- Method 2: NPM -->
npm install angular@1.8.2

<!-- Basic HTML structure -->
<!DOCTYPE html>
<html ng-app="myApp">
<head>
    <title>My Angular App</title>
    <script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.8.2/angular.min.js"></script>
    <script src="app.js"></script>
</head>
<body>
    <div ng-controller="MainController">
        <!-- Content here -->
    </div>
</body>
</html>
```

**Create Your First App (app.js):**

```javascript
// Define the Angular application module
var app = angular.module('myApp', []);

// Define a controller
app.controller('MainController', function($scope) {
    $scope.name = 'John Doe';
    $scope.message = 'Hello from Angular!';
});
```

**HTML Using the App:**

```html
<div ng-app="myApp">
    <div ng-controller="MainController">
        <h1>{{ message }}</h1>
        <p>Name: {{ name }}</p>
    </div>
</div>
```

**Output:**
```
Hello from Angular!
Name: John Doe
```

---

## 1.3 MVC vs MVVM Architecture

**MVC (Model-View-Controller):**

```
Model (Data) ← Controller → View (Display)
              (Logic)
```

**MVVM (Model-View-ViewModel):**

```
Model (Data) ↔ ViewModel ↔ View (Display)
              (Two-way binding)
```

**Angular.js uses MVVM pattern:**

```javascript
// Model (Data)
app.controller('UserController', function($scope) {
    $scope.user = {
        id: 1,
        name: 'Alice',
        email: 'alice@example.com'
    };
    
    // ViewModel (Logic)
    $scope.updateUser = function() {
        // Update logic
    };
});
```

```html
<!-- View (Display) - binds to ViewModel -->
<div ng-controller="UserController">
    <input ng-model="user.name" />
    <input ng-model="user.email" />
    <p>Name: {{ user.name }}</p>
    <p>Email: {{ user.email }}</p>
    <button ng-click="updateUser()">Save</button>
</div>
```

---

# PART 2: CORE CONCEPTS & SYNTAX

## 2.1 Two-Way Data Binding

**Definition:** Changes in the view automatically update the model, and changes in the model automatically update the view.

**Syntax:**

```html
<!-- ng-model creates two-way binding -->
<input type="text" ng-model="firstName" />
<input type="text" ng-model="lastName" />

<p>Hello, {{ firstName }} {{ lastName }}!</p>

<script>
app.controller('NameController', function($scope) {
    $scope.firstName = 'John';
    $scope.lastName = 'Doe';
    
    // Any change in input automatically updates firstName/lastName
    // Any change to $scope.firstName automatically updates view
});
</script>
```

**Behind the scenes:**

```javascript
// View changes trigger model update
// Model changes trigger view update (through $digest cycle)
// This is automatic - no manual refresh needed
```

---

## 2.2 Expressions & Interpolation

**Definition:** Expressions bind data to HTML using `{{ }}` syntax.

**Syntax:**

```html
<!-- Simple variable -->
<p>{{ name }}</p>

<!-- Math operations -->
<p>{{ 5 + 3 }}</p>
<p>{{ quantity * price }}</p>

<!-- String concatenation -->
<p>{{ firstName + ' ' + lastName }}</p>

<!-- Method calls -->
<p>{{ getCurrentDate() }}</p>

<!-- Ternary operator -->
<p>{{ isActive ? 'Active' : 'Inactive' }}</p>

<!-- Array/Object access -->
<p>{{ user.name }}</p>
<p>{{ users[0].email }}</p>

<!-- Filters -->
<p>{{ price | currency }}</p>
<p>{{ date | date:'yyyy-MM-dd' }}</p>

<script>
app.controller('ExpressionController', function($scope) {
    $scope.name = 'Alice';
    $scope.firstName = 'John';
    $scope.lastName = 'Doe';
    $scope.quantity = 10;
    $scope.price = 50;
    $scope.isActive = true;
    $scope.user = { name: 'Bob', email: 'bob@example.com' };
    $scope.users = [
        { email: 'user1@example.com' },
        { email: 'user2@example.com' }
    ];
    
    $scope.getCurrentDate = function() {
        return new Date().toDateString();
    };
});
</script>
```

---

## 2.3 ng-directives (Built-in)

**Definition:** Directives are markers on DOM elements (attribute, class, comment) that tell Angular to attach behavior or transform DOM.

**Common Directives:**

```html
<!-- ng-app: Initialize AngularJS application -->
<html ng-app="myApp">

<!-- ng-controller: Attach controller class to view -->
<div ng-controller="MyController">

<!-- ng-bind: One-way binding (same as {{ }}) -->
<p ng-bind="name"></p>

<!-- ng-model: Two-way binding -->
<input ng-model="user.name" />

<!-- ng-show/ng-hide: Toggle display -->
<div ng-show="isVisible">Visible</div>
<div ng-hide="isHidden">Not hidden</div>

<!-- ng-if: Remove/add from DOM (not just hide) -->
<div ng-if="user.isAdmin">Admin only content</div>

<!-- ng-class: Conditionally apply CSS classes -->
<div ng-class="{active: isActive, disabled: isDisabled}">
    Styled element
</div>

<!-- ng-style: Inline styles -->
<div ng-style="{color: textColor, 'font-size': fontSize + 'px'}">
    Styled text
</div>

<!-- ng-repeat: Loop through arrays -->
<ul>
    <li ng-repeat="user in users">
        {{ user.name }} - {{ user.email }}
    </li>
</ul>

<!-- ng-click: Click handler -->
<button ng-click="handleClick()">Click me</button>

<!-- ng-submit: Form submission -->
<form ng-submit="submitForm()">
    <input ng-model="formData.name" />
    <button type="submit">Submit</button>
</form>

<!-- ng-change: Change handler -->
<input ng-model="quantity" ng-change="updateTotal()" />

<!-- ng-disabled: Disable form elements -->
<button ng-disabled="!isFormValid">Submit</button>

<!-- ng-value: Set element value -->
<option ng-repeat="option in options" ng-value="option">
    {{ option.label }}
</option>

<!-- ng-options: Generate select options -->
<select ng-model="selectedUser" ng-options="user as user.name for user in users">
</select>

<!-- ng-checked: Checkbox state -->
<input type="checkbox" ng-model="isChecked" />

<!-- ng-readonly: Make input readonly -->
<input ng-readonly="!isEditable" />

<!-- ng-disabled: Disable input -->
<input ng-disabled="!isEnabled" />

<script>
app.controller('DirectiveController', function($scope) {
    $scope.name = 'John';
    $scope.isVisible = true;
    $scope.isHidden = false;
    $scope.user = { isAdmin: true };
    $scope.isActive = true;
    $scope.isDisabled = false;
    $scope.textColor = 'blue';
    $scope.fontSize = 16;
    
    $scope.users = [
        { name: 'Alice', email: 'alice@example.com' },
        { name: 'Bob', email: 'bob@example.com' },
        { name: 'Charlie', email: 'charlie@example.com' }
    ];
    
    $scope.handleClick = function() {
        alert('Button clicked!');
    };
    
    $scope.submitForm = function() {
        console.log('Form submitted:', $scope.formData);
    };
    
    $scope.updateTotal = function() {
        console.log('Quantity changed to:', $scope.quantity);
    };
    
    $scope.isFormValid = true;
    $scope.options = [
        { label: 'Option 1', value: 1 },
        { label: 'Option 2', value: 2 }
    ];
});
</script>
```

---

## 2.4 Event Handling

**Definition:** Attach event listeners to DOM elements.

**Syntax:**

```html
<!-- Click events -->
<button ng-click="count = count + 1">Increment</button>
<p>Count: {{ count }}</p>

<!-- Form events -->
<input type="text" ng-keyup="onKeyUp($event)" />
<input type="text" ng-keydown="onKeyDown($event)" />
<input type="text" ng-blur="onBlur()" />
<input type="text" ng-focus="onFocus()" />

<!-- Mouse events -->
<div ng-mouseenter="onMouseEnter()" ng-mouseleave="onMouseLeave()">
    Hover me
</div>

<!-- Form submission -->
<form ng-submit="submitForm()">
    <button type="submit">Submit</button>
</form>

<!-- Prevent default -->
<a ng-click="handleLinkClick($event)" href="#">Link</a>

<script>
app.controller('EventController', function($scope) {
    $scope.count = 0;
    
    $scope.onKeyUp = function(event) {
        console.log('Key up:', event.key);
    };
    
    $scope.onKeyDown = function(event) {
        console.log('Key down:', event.key);
    };
    
    $scope.onBlur = function() {
        console.log('Input blurred');
    };
    
    $scope.onFocus = function() {
        console.log('Input focused');
    };
    
    $scope.onMouseEnter = function() {
        console.log('Mouse entered');
    };
    
    $scope.onMouseLeave = function() {
        console.log('Mouse left');
    };
    
    $scope.submitForm = function() {
        console.log('Form submitted');
    };
    
    $scope.handleLinkClick = function(event) {
        event.preventDefault();
        console.log('Link clicked (default prevented)');
    };
});
</script>
```

---

# PART 3: SERVICES & DEPENDENCY INJECTION

## 3.1 What is Dependency Injection (DI)?

**Definition:** Dependency Injection is a software design pattern that deals with how components get hold of their dependencies. The Angular injector subsystem is in charge of creating instances, invoking types, invoking methods, and loading modules.

**Without DI (Hard to test):**

```javascript
function UserController() {
    var userService = new UserService(); // Controller creates its dependency
    this.users = userService.getUsers();
}
```

**With DI (Easy to test):**

```javascript
function UserController(UserService) { // Service injected
    this.users = UserService.getUsers();
}

// Angular automatically provides UserService
app.controller('UserController', UserController);
```

---

## 3.2 Creating Services

**Definition:** Services are singleton objects that handle business logic and share data across controllers.

**Service Types:**

```javascript
// 1. Factory (Most common)
app.factory('UserService', function($http) {
    return {
        getUsers: function() {
            return $http.get('/api/users');
        },
        getUserById: function(id) {
            return $http.get('/api/users/' + id);
        },
        createUser: function(user) {
            return $http.post('/api/users', user);
        }
    };
});

// 2. Service (Class-like)
app.service('UserService', function($http) {
    this.getUsers = function() {
        return $http.get('/api/users');
    };
    
    this.getUserById = function(id) {
        return $http.get('/api/users/' + id);
    };
});

// 3. Provider (Most flexible)
app.provider('UserService', function() {
    this.$get = function($http) {
        return {
            getUsers: function() {
                return $http.get('/api/users');
            }
        };
    };
});
```

**Using Services in Controllers:**

```javascript
app.controller('UserListController', function($scope, UserService) {
    UserService.getUsers().then(function(response) {
        $scope.users = response.data;
    }, function(error) {
        $scope.error = 'Failed to load users';
    });
});
```

---

## 3.3 $http Service (API Calls)

**Definition:** $http is Angular's service for making XMLHttpRequest (AJAX) calls.

**Syntax:**

```javascript
app.controller('ApiController', function($scope, $http) {
    // GET request
    $http.get('/api/users').then(
        function(response) {
            console.log('Success:', response.data);
        },
        function(error) {
            console.log('Error:', error);
        }
    );
    
    // POST request
    $http.post('/api/users', {
        name: 'John',
        email: 'john@example.com'
    }).then(function(response) {
        console.log('User created:', response.data);
    });
    
    // PUT request (update)
    $http.put('/api/users/1', {
        name: 'John Updated',
        email: 'john.updated@example.com'
    }).then(function(response) {
        console.log('User updated:', response.data);
    });
    
    // DELETE request
    $http.delete('/api/users/1').then(function(response) {
        console.log('User deleted');
    });
    
    // Custom headers
    $http({
        method: 'POST',
        url: '/api/users',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer token123'
        },
        data: { name: 'John' }
    }).then(function(response) {
        console.log(response);
    });
});
```

---

## 3.4 $q Service (Promises)

**Definition:** $q is Angular's promise library for handling asynchronous operations.

**Syntax:**

```javascript
app.factory('DataService', function($q, $timeout) {
    return {
        // Return a promise
        fetchData: function() {
            var deferred = $q.defer();
            
            $timeout(function() {
                // Simulate async operation
                deferred.resolve(['Item 1', 'Item 2']);
            }, 2000);
            
            return deferred.promise;
        },
        
        // Chain promises
        getDataWithTransform: function() {
            return this.fetchData()
                .then(function(data) {
                    // Transform data
                    return data.map(function(item) {
                        return item.toUpperCase();
                    });
                })
                .catch(function(error) {
                    console.log('Error:', error);
                    return $q.reject('Custom error message');
                });
        }
    };
});

app.controller('PromiseController', function($scope, DataService) {
    DataService.getDataWithTransform()
        .then(function(transformedData) {
            $scope.items = transformedData;
        });
});
```

---

# PART 4: CONTROLLERS & SCOPES

## 4.1 Understanding $scope

**Definition:** $scope is an object that refers to the application model. It is the glue between the view and the controller.

**Syntax:**

```javascript
app.controller('ScopeController', function($scope) {
    // Properties (bind to view)
    $scope.message = 'Hello World';
    $scope.count = 0;
    $scope.user = {
        name: 'John',
        email: 'john@example.com'
    };
    
    // Methods (callable from view)
    $scope.increment = function() {
        $scope.count++;
    };
    
    $scope.updateUser = function() {
        $scope.user.name = 'Updated Name';
    };
    
    // Watch for changes
    $scope.$watch('user.name', function(newValue, oldValue) {
        console.log('User name changed from', oldValue, 'to', newValue);
    });
    
    // One-time binding
    $scope.$watchOnce('message', function(newValue) {
        console.log('Message changed to:', newValue);
    });
});
```

**HTML:**

```html
<div ng-controller="ScopeController">
    <p>{{ message }}</p>
    <p>Count: {{ count }}</p>
    <button ng-click="increment()">Increment</button>
    
    <p>Name: {{ user.name }}</p>
    <button ng-click="updateUser()">Update Name</button>
</div>
```

---

## 4.2 Scope Lifecycle

**Definition:** Scope has a well-defined lifecycle that includes Creation, Watcher registration, Model mutation, Observation, and Destruction.

**Phases:**

```javascript
app.controller('LifecycleController', function($scope) {
    // 1. Creation phase
    console.log('Controller created');
    
    $scope.data = 'Initial value';
    
    // 2. Watcher registration phase
    $scope.$watch('data', function(newValue, oldValue) {
        console.log('Data changed');
    });
    
    // 3. Model mutation phase
    $scope.updateData = function() {
        $scope.data = 'New value';
        // Triggers $digest cycle which updates watchers
    };
    
    // 4. Observation phase
    // Angular runs $digest cycle automatically
    
    // 5. Cleanup
    $scope.$on('$destroy', function() {
        console.log('Scope destroyed - cleanup here');
    });
});
```

---

## 4.3 Scope Inheritance

**Definition:** Scopes inherit from parent scopes in a prototypal manner.

**Syntax:**

```html
<div ng-controller="ParentController">
    <p>Parent name: {{ parentName }}</p>
    
    <div ng-controller="ChildController">
        <p>Parent name in child: {{ parentName }}</p>
        <p>Child name: {{ childName }}</p>
        <button ng-click="updateParentName()">Update Parent</button>
    </div>
</div>

<script>
app.controller('ParentController', function($scope) {
    $scope.parentName = 'Parent';
});

app.controller('ChildController', function($scope) {
    $scope.childName = 'Child';
    
    // Access parent scope property
    console.log($scope.parentName); // 'Parent'
    
    $scope.updateParentName = function() {
        $scope.parentName = 'Parent Updated'; // Modifies parent's scope
    };
});
</script>
```

**Important:** Avoid writing to parent scope directly. Use a property object instead:

```javascript
// Bad: Direct property assignment (can break binding)
$scope.name = 'value'; // Only updates child scope

// Good: Object property (works with inheritance)
$scope.data = { name: 'value' }; // Both can access/modify
```

---

# PART 5: DIRECTIVES

## 5.1 Built-in Directives Review

Already covered in Part 2.3, but here's a quick reference:

```html
<!-- Binding & Data -->
{{ expression }} <!-- Interpolation -->
ng-bind="property" <!-- One-way binding -->
ng-model="property" <!-- Two-way binding -->

<!-- Conditional -->
ng-if="condition" <!-- Remove from DOM -->
ng-show="condition" <!-- Hide with display:none -->
ng-hide="condition" <!-- Hide with display:none -->

<!-- Looping -->
ng-repeat="item in collection" <!-- Loop -->

<!-- Events -->
ng-click="handler()" <!-- Click -->
ng-submit="handler()" <!-- Form submit -->
ng-change="handler()" <!-- Input change -->

<!-- CSS -->
ng-class="{'className': condition}" <!-- Apply class -->
ng-style="{'prop': value}" <!-- Inline style -->

<!-- Misc -->
ng-app="appName" <!-- Initialize app -->
ng-controller="CtrlName" <!-- Attach controller -->
ng-disabled="condition" <!-- Disable element -->
ng-readonly="condition" <!-- Make readonly -->
```

---

## 5.2 Creating Custom Directives

**Definition:** Create reusable, encapsulated components with custom directives.

**Simple Directive:**

```javascript
// Define directive
app.directive('helloWorld', function() {
    return {
        restrict: 'E', // Element
        template: '<h1>Hello World</h1>'
    };
});

// Use directive
// <hello-world></hello-world>
```

**Directive with Attribute:**

```javascript
app.directive('highlight', function() {
    return {
        restrict: 'A', // Attribute
        link: function(scope, element, attrs) {
            element.css('backgroundColor', 'yellow');
        }
    };
});

// <div highlight>Highlighted text</div>
```

**Directive with Data Binding:**

```javascript
app.directive('greetingCard', function() {
    return {
        restrict: 'E',
        scope: {
            name: '@', // One-way binding from attribute
            greeting: '=', // Two-way binding
            onGreet: '&' // Function binding
        },
        template: `
            <div class="card">
                <h2>{{ greeting }} {{ name }}!</h2>
                <button ng-click="onGreet()">Say Hi</button>
            </div>
        `
    };
});

// <greeting-card name="Alice" greeting="Hello" on-greet="handleGreet()"></greeting-card>
```

**Directive with Controller:**

```javascript
app.directive('todoList', function() {
    return {
        restrict: 'E',
        template: `
            <div>
                <ul>
                    <li ng-repeat="todo in todos">{{ todo }}</li>
                </ul>
                <input ng-model="newTodo" />
                <button ng-click="addTodo()">Add</button>
            </div>
        `,
        controller: function($scope) {
            $scope.todos = ['Item 1', 'Item 2'];
            $scope.newTodo = '';
            
            $scope.addTodo = function() {
                if ($scope.newTodo) {
                    $scope.todos.push($scope.newTodo);
                    $scope.newTodo = '';
                }
            };
        }
    };
});

// <todo-list></todo-list>
```

---

# PART 6: FILTERS

## 6.1 Built-in Filters

**Definition:** Filters format data for display to the user.

**Syntax:**

```html
<!-- Currency filter -->
<p>{{ price | currency }}</p>              <!-- $100.00 -->
<p>{{ price | currency:'€' }}</p>          <!-- €100.00 -->

<!-- Number filter -->
<p>{{ 1234.5678 | number:2 }}</p>          <!-- 1,234.57 -->

<!-- Date filter -->
<p>{{ date | date }}</p>                   <!-- Jan 1, 2024 -->
<p>{{ date | date:'yyyy-MM-dd' }}</p>      <!-- 2024-01-01 -->
<p>{{ date | date:'HH:mm:ss' }}</p>        <!-- 12:30:45 -->

<!-- Uppercase/Lowercase -->
<p>{{ 'hello' | uppercase }}</p>           <!-- HELLO -->
<p>{{ 'HELLO' | lowercase }}</p>           <!-- hello -->

<!-- Filter (search) -->
<ul>
    <li ng-repeat="user in users | filter:searchText">
        {{ user.name }}
    </li>
</ul>

<!-- OrderBy (sort) -->
<ul>
    <li ng-repeat="user in users | orderBy:'name'">
        {{ user.name }}
    </li>
</ul>

<!-- Reverse -->
<ul>
    <li ng-repeat="item in items | reverse">{{ item }}</li>
</ul>

<!-- Slice (limit) -->
<p>{{ 'Hello World' | slice:0:5 }}</p>     <!-- Hello -->

<!-- Limit to -->
<ul>
    <li ng-repeat="item in items | limitTo:5">{{ item }}</li>
</ul>

<!-- JSON -->
<pre>{{ user | json }}</pre>

<!-- Chain filters -->
<p>{{ price | currency | uppercase }}</p>

<script>
app.controller('FilterController', function($scope) {
    $scope.price = 100;
    $scope.date = new Date();
    $scope.users = [
        { name: 'Charlie' },
        { name: 'Alice' },
        { name: 'Bob' }
    ];
    $scope.searchText = '';
    $scope.items = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
});
</script>
```

---

## 6.2 Creating Custom Filters

**Definition:** Create reusable filters for custom formatting.

**Syntax:**

```javascript
// Simple filter
app.filter('capitalize', function() {
    return function(input) {
        if (!input) return input;
        return input.charAt(0).toUpperCase() + input.slice(1);
    };
});

// Usage: {{ 'hello' | capitalize }} → Hello

// Filter with parameters
app.filter('repeat', function() {
    return function(input, times) {
        times = times || 1;
        return new Array(times + 1).join(input);
    };
});

// Usage: {{ '*' | repeat:5 }} → *****

// Complex filter
app.filter('highlightSearch', function() {
    return function(text, search) {
        if (!search) return text;
        return text.replace(
            new RegExp(search, 'gi'),
            '<strong>$&</strong>'
        );
    };
});

// Usage: {{ paragraph | highlightSearch:searchTerm }}
```

---

# PART 7: ROUTING

## 7.1 ngRoute Module

**Definition:** ngRoute enables Single-Page Application (SPA) routing using URL fragments (#).

**Setup:**

```html
<!DOCTYPE html>
<html ng-app="myApp">
<head>
    <script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.8.2/angular.min.js"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/angularjs/1.8.2/angular-route.min.js"></script>
    <script src="app.js"></script>
</head>
<body>
    <!-- Navigation -->
    <nav>
        <a href="#!/home">Home</a>
        <a href="#!/about">About</a>
        <a href="#!/users">Users</a>
    </nav>
    
    <!-- View container -->
    <div ng-view></div>
</body>
</html>
```

**Configure Routes (app.js):**

```javascript
var app = angular.module('myApp', ['ngRoute']);

app.config(function($routeProvider, $locationProvider) {
    $routeProvider
        .when('/home', {
            templateUrl: 'views/home.html',
            controller: 'HomeController'
        })
        .when('/about', {
            templateUrl: 'views/about.html',
            controller: 'AboutController'
        })
        .when('/users', {
            templateUrl: 'views/users.html',
            controller: 'UserListController'
        })
        .when('/users/:id', {
            templateUrl: 'views/user-detail.html',
            controller: 'UserDetailController'
        })
        .otherwise({
            redirectTo: '/home'
        });
});

app.controller('HomeController', function($scope) {
    $scope.title = 'Home';
});

app.controller('UserDetailController', function($scope, $routeParams) {
    $scope.userId = $routeParams.id; // Get route parameter
});
```

---

## 7.2 ui-router Module (Advanced)

**Definition:** ui-router provides nested states and nested views.

**Setup:**

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/angular-ui-router/1.0.30/angular-ui-router.min.js"></script>
```

**Configuration:**

```javascript
var app = angular.module('myApp', ['ui.router']);

app.config(function($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('home', {
            url: '/home',
            templateUrl: 'views/home.html',
            controller: 'HomeController'
        })
        .state('users', {
            url: '/users',
            templateUrl: 'views/users.html',
            controller: 'UserListController'
        })
        .state('users.detail', {
            url: '/:id',
            templateUrl: 'views/user-detail.html',
            controller: 'UserDetailController'
        });
    
    $urlRouterProvider.otherwise('/home');
});

// Navigation
app.controller('NavController', function($scope, $state) {
    $scope.goTo = function(state) {
        $state.go(state);
    };
});
```

**HTML with ui-view:**

```html
<div ng-controller="NavController">
    <button ng-click="goTo('home')">Home</button>
    <button ng-click="goTo('users')">Users</button>
    
    <div ui-view></div>
</div>
```

---

# PART 8: ASP.NET WEB API INTEGRATION

## 8.1 HTTP Requests to ASP.NET Web API

**ASP.NET Web API Endpoint (C#):**

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        var user = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var success = await _userService.UpdateAsync(id, dto);
        if (!success)
            return NotFound();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var success = await _userService.DeleteAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }
}
```

**Angular Service to Call API:**

```javascript
app.factory('UserService', function($http, $q) {
    var apiBase = 'http://localhost:5000/api/users';
    
    return {
        // GET all users
        getAll: function() {
            return $http.get(apiBase).then(
                function(response) {
                    return response.data;
                },
                function(error) {
                    return $q.reject('Failed to load users: ' + error.statusText);
                }
            );
        },
        
        // GET user by ID
        getById: function(id) {
            return $http.get(apiBase + '/' + id).then(
                function(response) {
                    return response.data;
                },
                function(error) {
                    return $q.reject('User not found');
                }
            );
        },
        
        // POST create user
        create: function(user) {
            return $http.post(apiBase, user).then(
                function(response) {
                    return response.data;
                },
                function(error) {
                    return $q.reject('Failed to create user');
                }
            );
        },
        
        // PUT update user
        update: function(id, user) {
            return $http.put(apiBase + '/' + id, user).then(
                function(response) {
                    return response.data;
                },
                function(error) {
                    return $q.reject('Failed to update user');
                }
            );
        },
        
        // DELETE user
        delete: function(id) {
            return $http.delete(apiBase + '/' + id).then(
                function(response) {
                    return response.data;
                },
                function(error) {
                    return $q.reject('Failed to delete user');
                }
            );
        }
    };
});
```

---

## 8.2 Complete CRUD Example with ASP.NET Web API

**HTML Template (users.html):**

```html
<div ng-controller="UserListController">
    <h1>Users</h1>
    
    <!-- Error message -->
    <div ng-if="errorMessage" class="alert alert-danger">
        {{ errorMessage }}
    </div>
    
    <!-- Loading indicator -->
    <div ng-if="loading" class="alert alert-info">
        Loading users...
    </div>
    
    <!-- Add form -->
    <form ng-submit="addUser()" ng-if="!editingId">
        <h3>Add New User</h3>
        <input type="text" ng-model="newUser.name" placeholder="Name" required />
        <input type="email" ng-model="newUser.email" placeholder="Email" required />
        <button type="submit">Add User</button>
    </form>
    
    <!-- Edit form -->
    <form ng-submit="updateUser()" ng-if="editingId">
        <h3>Edit User</h3>
        <input type="text" ng-model="editingUser.name" placeholder="Name" required />
        <input type="email" ng-model="editingUser.email" placeholder="Email" required />
        <button type="submit">Save</button>
        <button type="button" ng-click="cancelEdit()">Cancel</button>
    </form>
    
    <!-- Users list -->
    <table>
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Email</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            <tr ng-repeat="user in users">
                <td>{{ user.id }}</td>
                <td>{{ user.name }}</td>
                <td>{{ user.email }}</td>
                <td>
                    <button ng-click="editUser(user.id)">Edit</button>
                    <button ng-click="deleteUser(user.id)">Delete</button>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

**Controller (app.js):**

```javascript
app.controller('UserListController', function($scope, UserService) {
    $scope.users = [];
    $scope.newUser = {};
    $scope.editingId = null;
    $scope.editingUser = {};
    $scope.loading = false;
    $scope.errorMessage = '';
    
    // Load all users
    $scope.loadUsers = function() {
        $scope.loading = true;
        $scope.errorMessage = '';
        
        UserService.getAll()
            .then(function(users) {
                $scope.users = users;
            })
            .catch(function(error) {
                $scope.errorMessage = error;
            })
            .finally(function() {
                $scope.loading = false;
            });
    };
    
    // Add new user
    $scope.addUser = function() {
        if (!$scope.newUser.name || !$scope.newUser.email) {
            $scope.errorMessage = 'Please fill in all fields';
            return;
        }
        
        UserService.create($scope.newUser)
            .then(function(user) {
                $scope.users.push(user);
                $scope.newUser = {};
                $scope.errorMessage = '';
            })
            .catch(function(error) {
                $scope.errorMessage = error;
            });
    };
    
    // Edit user
    $scope.editUser = function(id) {
        var user = $scope.users.find(function(u) { return u.id === id; });
        $scope.editingId = id;
        $scope.editingUser = angular.copy(user);
    };
    
    // Update user
    $scope.updateUser = function() {
        UserService.update($scope.editingId, $scope.editingUser)
            .then(function() {
                var index = $scope.users.findIndex(function(u) { return u.id === $scope.editingId; });
                $scope.users[index] = $scope.editingUser;
                $scope.editingId = null;
                $scope.editingUser = {};
            })
            .catch(function(error) {
                $scope.errorMessage = error;
            });
    };
    
    // Cancel edit
    $scope.cancelEdit = function() {
        $scope.editingId = null;
        $scope.editingUser = {};
    };
    
    // Delete user
    $scope.deleteUser = function(id) {
        if (confirm('Are you sure?')) {
            UserService.delete(id)
                .then(function() {
                    $scope.users = $scope.users.filter(function(u) { return u.id !== id; });
                })
                .catch(function(error) {
                    $scope.errorMessage = error;
                });
        }
    };
    
    // Initial load
    $scope.loadUsers();
});
```

---

## 8.3 Error Handling

**Syntax:**

```javascript
app.factory('UserService', function($http, $q) {
    return {
        getUser: function(id) {
            return $http.get('/api/users/' + id)
                .then(function(response) {
                    return response.data;
                })
                .catch(function(error) {
                    // Handle different error types
                    if (error.status === 404) {
                        return $q.reject('User not found');
                    } else if (error.status === 401) {
                        return $q.reject('Unauthorized');
                    } else if (error.status === 500) {
                        return $q.reject('Server error');
                    } else {
                        return $q.reject('An error occurred: ' + error.statusText);
                    }
                });
        }
    };
});

app.controller('UserController', function($scope, UserService) {
    UserService.getUser(1)
        .then(function(user) {
            $scope.user = user;
        })
        .catch(function(error) {
            $scope.error = error;
            console.error(error);
        });
});
```

---

# PART 9: ADVANCED CONCEPTS

## 9.1 Interceptors

**Definition:** HTTP interceptors are used to intercept HTTP requests and responses globally.

**Syntax:**

```javascript
// Auth interceptor
app.factory('AuthInterceptor', function($q, $window) {
    return {
        request: function(config) {
            // Add auth token to every request
            var token = $window.localStorage.getItem('token');
            if (token) {
                config.headers = config.headers || {};
                config.headers.Authorization = 'Bearer ' + token;
            }
            return config;
        },
        
        response: function(response) {
            return response;
        },
        
        responseError: function(response) {
            if (response.status === 401) {
                // Handle unauthorized
                $window.localStorage.removeItem('token');
                // Redirect to login
            }
            return $q.reject(response);
        }
    };
});

// Register interceptor
app.config(function($httpProvider) {
    $httpProvider.interceptors.push('AuthInterceptor');
});
```

---

## 9.2 Authentication & Authorization

**Login Service:**

```javascript
app.factory('AuthService', function($http, $q, $window) {
    return {
        login: function(credentials) {
            return $http.post('/api/auth/login', credentials)
                .then(function(response) {
                    $window.localStorage.setItem('token', response.data.token);
                    return response.data;
                });
        },
        
        logout: function() {
            $window.localStorage.removeItem('token');
        },
        
        getToken: function() {
            return $window.localStorage.getItem('token');
        },
        
        isAuthenticated: function() {
            return !!$window.localStorage.getItem('token');
        }
    };
});

app.controller('LoginController', function($scope, $location, AuthService) {
    $scope.login = function() {
        AuthService.login($scope.credentials)
            .then(function() {
                $location.path('/dashboard');
            })
            .catch(function(error) {
                $scope.error = 'Login failed';
            });
    };
});
```

**Route Protection:**

```javascript
app.config(function($routeProvider) {
    $routeProvider
        .when('/dashboard', {
            templateUrl: 'views/dashboard.html',
            controller: 'DashboardController',
            resolve: {
                auth: function(AuthService) {
                    if (AuthService.isAuthenticated()) {
                        return true;
                    } else {
                        return Promise.reject('Not authenticated');
                    }
                }
            }
        });
});

app.run(function($rootScope, $location) {
    $rootScope.$on('$routeChangeError', function() {
        $location.path('/login');
    });
});
```

---

## 9.3 Form Validation

**HTML:**

```html
<form ng-submit="submitForm()" name="userForm">
    <div>
        <label>Name:</label>
        <input type="text" ng-model="user.name" name="name" required minlength="3" />
        <span ng-if="userForm.name.$error.required">Name is required</span>
        <span ng-if="userForm.name.$error.minlength">Name must be at least 3 characters</span>
    </div>
    
    <div>
        <label>Email:</label>
        <input type="email" ng-model="user.email" name="email" required />
        <span ng-if="userForm.email.$error.required">Email is required</span>
        <span ng-if="userForm.email.$error.email">Invalid email</span>
    </div>
    
    <div>
        <label>Age:</label>
        <input type="number" ng-model="user.age" name="age" min="0" max="120" />
        <span ng-if="userForm.age.$error.min">Age must be at least 0</span>
        <span ng-if="userForm.age.$error.max">Age must be at most 120</span>
    </div>
    
    <button type="submit" ng-disabled="!userForm.$valid">Submit</button>
</form>
```

**Controller:**

```javascript
app.controller('FormController', function($scope) {
    $scope.user = {};
    
    $scope.submitForm = function() {
        if ($scope.userForm.$valid) {
            console.log('Form is valid, submitting...', $scope.user);
        }
    };
    
    // Watch form validity
    $scope.$watch('userForm.$valid', function(newVal) {
        console.log('Form valid:', newVal);
    });
});
```

---

## 9.4 Testing

**Unit Testing with Jasmine & Karma:**

```javascript
describe('UserService', function() {
    var UserService, $httpBackend;
    
    beforeEach(module('myApp'));
    
    beforeEach(inject(function(_UserService_, _$httpBackend_) {
        UserService = _UserService_;
        $httpBackend = _$httpBackend_;
    }));
    
    afterEach(function() {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });
    
    it('should fetch users', function() {
        var mockUsers = [
            { id: 1, name: 'Alice' },
            { id: 2, name: 'Bob' }
        ];
        
        $httpBackend.expectGET('/api/users').respond(mockUsers);
        
        var result;
        UserService.getAll().then(function(users) {
            result = users;
        });
        
        $httpBackend.flush();
        expect(result).toEqual(mockUsers);
    });
});

describe('UserController', function() {
    var $scope, controller, UserService;
    
    beforeEach(module('myApp'));
    
    beforeEach(inject(function($controller, $rootScope, _UserService_) {
        $scope = $rootScope.$new();
        UserService = _UserService_;
        
        spyOn(UserService, 'getAll').and.returnValue(Promise.resolve([
            { id: 1, name: 'Alice' }
        ]));
        
        controller = $controller('UserListController', {
            $scope: $scope,
            UserService: UserService
        });
    }));
    
    it('should load users on init', function() {
        $scope.$apply();
        expect(UserService.getAll).toHaveBeenCalled();
    });
});
```

---

# PART 10: BEST PRACTICES

## 10.1 Code Organization

**Folder Structure:**

```
app/
├── controllers/
│   ├── user-list.controller.js
│   ├── user-detail.controller.js
│   └── dashboard.controller.js
├── services/
│   ├── user.service.js
│   ├── auth.service.js
│   └── api.service.js
├── directives/
│   ├── user-card.directive.js
│   └── form-validator.directive.js
├── filters/
│   └── custom-filters.js
├── views/
│   ├── users/
│   │   ├── list.html
│   │   └── detail.html
│   └── dashboard.html
├── app.config.js
├── app.routes.js
└── app.js
```

**Load Order (index.html):**

```html
<!-- Framework -->
<script src="angular.min.js"></script>
<script src="angular-route.min.js"></script>

<!-- App Module -->
<script src="app.js"></script>
<script src="app.config.js"></script>
<script src="app.routes.js"></script>

<!-- Services -->
<script src="services/api.service.js"></script>
<script src="services/auth.service.js"></script>
<script src="services/user.service.js"></script>

<!-- Controllers -->
<script src="controllers/user-list.controller.js"></script>
<script src="controllers/user-detail.controller.js"></script>

<!-- Directives -->
<script src="directives/user-card.directive.js"></script>

<!-- Filters -->
<script src="filters/custom-filters.js"></script>
```

---

## 10.2 Naming Conventions

**Syntax:**

```javascript
// Files: kebab-case
// user-service.js
// user-list.controller.js
// user-card.directive.js

// Angular modules: camelCase
var app = angular.module('myApp', []);

// Controllers: PascalCase + 'Controller'
app.controller('UserListController', function() {});

// Services: camelCase
app.service('userService', function() {});
app.factory('authFactory', function() {});

// Directives: camelCase
app.directive('userCard', function() {});

// Functions: camelCase
function getUserById(id) {}

// Constants: UPPER_SNAKE_CASE
app.constant('API_BASE_URL', 'http://api.example.com');
```

---

## 10.3 Performance Tips

**1. Use $applyAsync for batch updates:**

```javascript
$scope.$applyAsync(function() {
    // Multiple $scope updates batched into single digest cycle
    $scope.data1 = value1;
    $scope.data2 = value2;
    $scope.data3 = value3;
});
```

**2. Use one-time binding (::):**

```html
<!-- Data won't change - use one-time binding -->
<p>{{ ::user.createdAt }}</p>

<!-- Instead of -->
<p>{{ user.createdAt }}</p>
```

**3. Unsubscribe from watchers:**

```javascript
var unwatchFn = $scope.$watch('data', function(newVal) {
    // Handle change
});

// In $destroy event
$scope.$on('$destroy', function() {
    unwatchFn();
});
```

**4. Use trackBy with ng-repeat:**

```html
<div ng-repeat="user in users track by user.id">
    {{ user.name }}
</div>
```

**5. Limit API calls:**

```javascript
// Use caching
app.factory('UserService', function($http, $q) {
    var cache = {};
    
    return {
        getUser: function(id) {
            if (cache[id]) {
                return $q.when(cache[id]);
            }
            
            return $http.get('/api/users/' + id).then(function(response) {
                cache[id] = response.data;
                return response.data;
            });
        }
    };
});
```

---

## 10.4 Common Mistakes to Avoid

**1. Directly mutating parent scope:**

```javascript
// BAD - breaks inheritance
$scope.name = 'value';

// GOOD - use object
$scope.data = { name: 'value' };
```

**2. Not handling promises:**

```javascript
// BAD - undefined result
var users = UserService.getUsers();

// GOOD - use .then()
UserService.getUsers().then(function(users) {
    $scope.users = users;
});
```

**3. Creating services in controllers:**

```javascript
// BAD - not reusable
app.controller('Ctrl', function($http) {
    $http.get('/api/users');
});

// GOOD - shared service
app.factory('UserService', function($http) {
    return { getUsers: function() { return $http.get('/api/users'); } };
});
```

**4. Using ng-repeat on large lists:**

```html
<!-- BAD - performance issue -->
<div ng-repeat="item in items">{{ item }}</div>

<!-- GOOD - with track by -->
<div ng-repeat="item in items track by item.id">{{ item }}</div>
```

**5. Not unsubscribing from events:**

```javascript
// BAD - memory leak
$scope.$on('event', function() { });

// GOOD - cleanup
var unsubscribe = $scope.$on('event', function() { });
$scope.$on('$destroy', unsubscribe);
```

---

# PART 11: ADVANCED CONCEPTS (CONTINUED)

## 11.1 State Management with Angular Modules

**Definition:** Managing application state using services and shared data patterns.

**Syntax:**

```javascript
// Global state service
app.factory('StateService', function($rootScope) {
    var state = {
        user: null,
        notification: null,
        isLoading: false
    };
    
    return {
        getState: function() {
            return state;
        },
        
        setState: function(key, value) {
            state[key] = value;
            $rootScope.$broadcast('stateChanged', { key: key, value: value });
        },
        
        getUser: function() {
            return state.user;
        },
        
        setUser: function(user) {
            this.setState('user', user);
        },
        
        showNotification: function(message, type) {
            this.setState('notification', { message: message, type: type });
        },
        
        setLoading: function(loading) {
            this.setState('isLoading', loading);
        }
    };
});

// Usage in controller
app.controller('AppController', function($scope, StateService) {
    $scope.$watch(function() {
        return StateService.getState();
    }, function(newState) {
        $scope.appState = newState;
    }, true);
});
```

---

## 11.2 Advanced $scope Features

**$apply & $digest:**

```javascript
app.controller('AdvancedScopeController', function($scope, $timeout) {
    // Manual digest cycle trigger
    $scope.manualUpdate = function() {
        // Code outside Angular
        setTimeout(function() {
            $scope.data = 'Updated outside Angular';
            $scope.$apply(); // Manually trigger digest cycle
        }, 1000);
    };
    
    // $digest only for specific scope tree
    $scope.localUpdate = function() {
        $scope.localData = 'Local update';
        $scope.$digest(); // Only update this scope and children
    };
    
    // Multiple watchers with aggregation
    var dataChanges = 0;
    $scope.$watch('property1', function() { dataChanges++; });
    $scope.$watch('property2', function() { dataChanges++; });
    $scope.$watch('property3', function() { dataChanges++; });
    
    // Watch object by reference vs deep watch
    $scope.$watch('user', function(newVal) {
        console.log('Shallow watch triggered');
    }); // Only when reference changes
    
    $scope.$watch('user', function(newVal) {
        console.log('Deep watch triggered');
    }, true); // Watch all nested properties
});
```

**$broadcast & $emit:**

```javascript
// Parent controller broadcasts to children
app.controller('ParentController', function($scope) {
    $scope.sendMessageToChildren = function() {
        $scope.$broadcast('messageFromParent', { message: 'Hello children' });
    };
});

// Child controller listens
app.controller('ChildController', function($scope) {
    $scope.$on('messageFromParent', function(event, data) {
        console.log('Received message:', data.message);
    });
    
    // Child sends message to parent
    $scope.sendToParent = function() {
        $scope.$emit('messageFromChild', { message: 'Hello parent' });
    };
});

// Root scope broadcast (to all controllers)
app.run(function($rootScope) {
    $rootScope.$broadcast('appInitialized');
});
```

---

## 11.3 Middleware & Request/Response Processing

**Request/Response Transformation:**

```javascript
// Transform request data before sending
app.factory('DataTransformService', function($http, $q) {
    return {
        transformRequest: function(data) {
            // Convert camelCase to snake_case
            return Object.keys(data).reduce(function(acc, key) {
                var snakeKey = key.replace(/([A-Z])/g, '_$1').toLowerCase();
                acc[snakeKey] = data[key];
                return acc;
            }, {});
        },
        
        transformResponse: function(data) {
            // Convert snake_case to camelCase
            return Object.keys(data).reduce(function(acc, key) {
                var camelKey = key.replace(/_([a-z])/g, function(m) {
                    return m[1].toUpperCase();
                });
                acc[camelKey] = data[key];
                return acc;
            }, {});
        }
    };
});

// Use in service
app.factory('UserService', function($http, DataTransformService) {
    return {
        createUser: function(user) {
            var transformedData = DataTransformService.transformRequest(user);
            return $http.post('/api/users', transformedData)
                .then(function(response) {
                    return DataTransformService.transformResponse(response.data);
                });
        }
    };
});
```

---

## 11.4 Advanced Routing Patterns

**Nested Views with ui-router:**

```javascript
app.config(function($stateProvider) {
    $stateProvider
        // Parent state with abstract: true (no direct route)
        .state('app', {
            abstract: true,
            templateUrl: 'views/app.html',
            controller: 'AppController'
        })
        
        // Nested states
        .state('app.dashboard', {
            url: '/dashboard',
            views: {
                '': {
                    templateUrl: 'views/dashboard.html',
                    controller: 'DashboardController'
                },
                'sidebar@app': {
                    templateUrl: 'views/sidebar.html',
                    controller: 'SidebarController'
                }
            }
        })
        
        .state('app.users', {
            url: '/users',
            templateUrl: 'views/users/list.html',
            controller: 'UserListController'
        })
        
        .state('app.users.detail', {
            url: '/:userId',
            templateUrl: 'views/users/detail.html',
            controller: 'UserDetailController',
            resolve: {
                user: function($stateParams, UserService) {
                    return UserService.getById($stateParams.userId);
                }
            }
        });
});

// Resolve data before state transition
app.controller('UserDetailController', function($scope, user) {
    $scope.user = user; // user is guaranteed to exist
});
```

---

# PART 11.5: OPTIMIZATION & PERFORMANCE

## Lazy Loading Modules

**Syntax:**

```javascript
// Main app with core modules
var app = angular.module('myApp', ['ngRoute']);

// Feature module (lazy loaded)
var featuresModule = angular.module('features', []);

featuresModule.controller('FeatureController', function($scope) {
    $scope.data = 'Feature data';
});

// Load feature module dynamically
app.controller('MainController', function($scope, $ocLazyLoad) {
    $scope.loadFeature = function() {
        $ocLazyLoad.load('features')
            .then(function() {
                console.log('Feature module loaded');
            });
    };
});
```

---

# PART 12: ANGULAR DEPLOYMENT

## 12.1 Building Angular.js Application

**Minification & Bundling:**

```bash
# Install build tools
npm install -g grunt-cli
npm install grunt grunt-contrib-uglify grunt-contrib-concat grunt-contrib-copy

# Gruntfile.js
module.exports = function(grunt) {
    grunt.initConfig({
        // Concatenate files
        concat: {
            app: {
                src: [
                    'app/app.js',
                    'app/**/*.js'
                ],
                dest: 'dist/app.js'
            }
        },
        
        // Minify JavaScript
        uglify: {
            app: {
                files: {
                    'dist/app.min.js': ['dist/app.js']
                }
            }
        },
        
        // Copy static files
        copy: {
            static: {
                files: [
                    { expand: true, src: ['index.html'], dest: 'dist/' },
                    { expand: true, src: ['views/**'], dest: 'dist/' },
                    { expand: true, src: ['css/**'], dest: 'dist/' }
                ]
            }
        }
    });
    
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-copy');
    
    grunt.registerTask('build', ['concat', 'uglify', 'copy']);
};
```

**Production HTML (index.html):**

```html
<!DOCTYPE html>
<html ng-app="myApp">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>My App</title>
    <link rel="stylesheet" href="css/style.min.css">
</head>
<body>
    <div ng-view></div>
    
    <!-- All scripts minified & concatenated -->
    <script src="js/app.min.js"></script>
    
    <!-- Google Analytics -->
    <script>
        (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
        (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
        m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
        })(window,document,'script','https://www.google-analytics.com/analytics.js','ga');
        ga('create', 'UA-XXXXX-Y', 'auto');
        ga('send', 'pageview');
    </script>
</body>
</html>
```

---

## 12.2 Deploying to Azure App Service

**Create ASP.NET Core API + Angular App:**

**Project Structure:**

```
MyApp/
├── backend/
│   ├── MyApp.Api/
│   │   ├── Controllers/
│   │   ├── Program.cs
│   │   └── MyApp.Api.csproj
│   └── MyApp.sln
├── frontend/
│   ├── app/
│   │   ├── controllers/
│   │   ├── services/
│   │   ├── views/
│   │   ├── app.js
│   │   └── index.html
│   ├── dist/
│   ├── Gruntfile.js
│   └── package.json
└── README.md
```

**Dockerfile:**

```dockerfile
# Stage 1: Build Angular
FROM node:20-alpine AS angular-build
WORKDIR /app/frontend

COPY frontend/package*.json ./
RUN npm ci

COPY frontend/ .
RUN npm run build

# Stage 2: Build ASP.NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src

COPY backend/MyApp.sln .
COPY backend/MyApp.Api/MyApp.Api.csproj MyApp.Api/
RUN dotnet restore

COPY backend/ .
WORKDIR /src/MyApp.Api
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy .NET app
COPY --from=dotnet-build /app/publish .

# Copy Angular dist to wwwroot (serve as static files)
COPY --from=angular-build /app/frontend/dist ./wwwroot

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
```

---

## 12.3 Azure DevOps Pipeline for Angular + ASP.NET

**azure-pipelines.yml:**

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  nodeVersion: '20.x'
  dotnetVersion: '8.0.x'

stages:
  # ================================================
  # Stage 1: Build Frontend
  # ================================================
  - stage: BuildFrontend
    displayName: Build Angular Frontend
    jobs:
      - job: BuildAngular
        displayName: Build Angular App
        steps:
          - task: NodeTool@0
            inputs:
              versionSpec: $(nodeVersion)

          - task: Npm@1
            displayName: 'Install dependencies'
            inputs:
              command: 'install'
              workingDir: 'frontend'

          - task: Npm@1
            displayName: 'Run unit tests'
            inputs:
              command: 'custom'
              customCommand: 'test -- --watch=false --browsers=ChromeHeadless'
              workingDir: 'frontend'

          - task: Npm@1
            displayName: 'Build production'
            inputs:
              command: 'custom'
              customCommand: 'run build'
              workingDir: 'frontend'

          - task: PublishBuildArtifacts@1
            displayName: 'Publish Angular artifacts'
            inputs:
              pathToPublish: 'frontend/dist'
              artifactName: 'angular-dist'

  # ================================================
  # Stage 2: Build Backend
  # ================================================
  - stage: BuildBackend
    displayName: Build ASP.NET Backend
    dependsOn: BuildFrontend
    condition: succeeded()
    jobs:
      - job: BuildDotNet
        displayName: Build .NET API
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: $(dotnetVersion)

          - task: DotNetCoreCLI@2
            displayName: 'Restore packages'
            inputs:
              command: 'restore'
              projects: 'backend/**/*.csproj'

          - task: DotNetCoreCLI@2
            displayName: 'Build solution'
            inputs:
              command: 'build'
              projects: 'backend/MyApp.sln'
              arguments: '--configuration $(buildConfiguration)'

          - task: DotNetCoreCLI@2
            displayName: 'Run unit tests'
            inputs:
              command: 'test'
              projects: 'backend/**/*Tests.csproj'

          - task: DotNetCoreCLI@2
            displayName: 'Publish API'
            inputs:
              command: 'publish'
              publishWebProjects: false
              projects: 'backend/MyApp.Api/MyApp.Api.csproj'
              arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'

          - task: PublishBuildArtifacts@1
            displayName: 'Publish .NET artifacts'
            inputs:
              pathToPublish: '$(Build.ArtifactStagingDirectory)'
              artifactName: 'api-build'

  # ================================================
  # Stage 3: Build Docker Image
  # ================================================
  - stage: DockerBuild
    displayName: Build Docker Image
    dependsOn: BuildBackend
    condition: succeeded()
    jobs:
      - job: BuildDocker
        displayName: Build & Push Docker Image
        steps:
          - task: DownloadBuildArtifacts@0
            inputs:
              buildType: 'current'
              downloadType: 'all'

          - task: Docker@2
            displayName: 'Build Docker image'
            inputs:
              command: 'build'
              Dockerfile: '$(Build.SourcesDirectory)/Dockerfile'
              tags: |
                myregistry.azurecr.io/myapp:$(Build.BuildId)
                myregistry.azurecr.io/myapp:latest

          - task: Docker@2
            displayName: 'Push to ACR'
            inputs:
              command: 'push'
              containerRegistryType: 'Azure Container Registry'
              containerRegistry: 'myregistry'
              repository: 'myapp'
              tags: |
                $(Build.BuildId)
                latest

  # ================================================
  # Stage 4: Deploy to Development
  # ================================================
  - stage: DeployDev
    displayName: Deploy to Development
    dependsOn: DockerBuild
    condition: succeeded()
    jobs:
      - deployment: DeployDev
        displayName: Deploy to Dev App Service
        environment: 'Development'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebAppContainer@1
                  displayName: 'Deploy to Azure App Service'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    appName: 'myapp-dev'
                    containers: 'myregistry.azurecr.io/myapp:$(Build.BuildId)'

  # ================================================
  # Stage 5: Deploy to Staging
  # ================================================
  - stage: DeployStaging
    displayName: Deploy to Staging
    dependsOn: DeployDev
    condition: succeeded()
    jobs:
      - deployment: DeployStaging
        displayName: Deploy to Staging
        environment: 'Staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebAppContainer@1
                  displayName: 'Deploy to Staging'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    appName: 'myapp-staging'
                    containers: 'myregistry.azurecr.io/myapp:$(Build.BuildId)'

                - task: AzureAppServiceSettings@1
                  displayName: 'Update App Settings'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    appName: 'myapp-staging'
                    appSettings: |
                      [
                        {
                          "name": "ASPNETCORE_ENVIRONMENT",
                          "value": "Staging",
                          "slotSetting": false
                        },
                        {
                          "name": "API_BASE_URL",
                          "value": "https://myapp-staging.azurewebsites.net",
                          "slotSetting": false
                        }
                      ]

  # ================================================
  # Stage 6: Deploy to Production
  # ================================================
  - stage: DeployProd
    displayName: Deploy to Production
    dependsOn: DeployStaging
    condition: succeeded()
    jobs:
      - deployment: ApprovalGate
        displayName: Manual Approval
        environment: 'Production'
        strategy:
          runOnce:
            deploy:
              steps:
                - script: echo "Waiting for approval to deploy to production..."

      - deployment: DeployProd
        displayName: Deploy to Production
        environment: 'Production'
        dependsOn: ApprovalGate
        condition: succeeded()
        strategy:
          runOnce:
            deploy:
              steps:
                - task: AzureWebAppContainer@1
                  displayName: 'Deploy to Production'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    appName: 'myapp-prod'
                    containers: 'myregistry.azurecr.io/myapp:$(Build.BuildId)'

                - task: AzureAppServiceSettings@1
                  displayName: 'Update Production Settings'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    appName: 'myapp-prod'
                    appSettings: |
                      [
                        {
                          "name": "ASPNETCORE_ENVIRONMENT",
                          "value": "Production",
                          "slotSetting": false
                        },
                        {
                          "name": "API_BASE_URL",
                          "value": "https://myapp.azurewebsites.net",
                          "slotSetting": false
                        }
                      ]

                - task: AzureAppServiceManage@0
                  displayName: 'Warm up App Service'
                  inputs:
                    azureSubscription: 'MyAzureSubscription'
                    action: 'Start Azure App Service'
                    appName: 'myapp-prod'
```

---

## 12.4 Configure ASP.NET to Serve Angular

**Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://myapp.azurewebsites.net")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AngularApp");
app.UseHttpsRedirection();
app.UseAuthorization();

// Serve Angular static files
app.UseStaticFiles();

// Route API calls to controllers
app.MapControllers();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");

app.Run();
```

**appsettings.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "ApiSettings": {
    "Version": "1.0",
    "Environment": "Production"
  }
}
```

---

## 12.5 Environment-Specific Configurations

**Development Configuration (config.dev.js):**

```javascript
app.constant('API_CONFIG', {
    baseUrl: 'http://localhost:5000/api',
    timeout: 30000,
    cache: false,
    debug: true
});

app.config(function($httpProvider, API_CONFIG) {
    $httpProvider.defaults.timeout = API_CONFIG.timeout;
    
    if (API_CONFIG.debug) {
        $httpProvider.interceptors.push(function($q, $window) {
            return {
                request: function(config) {
                    console.log('Request:', config);
                    return config;
                },
                response: function(response) {
                    console.log('Response:', response);
                    return response;
                }
            };
        });
    }
});
```

**Production Configuration (config.prod.js):**

```javascript
app.constant('API_CONFIG', {
    baseUrl: 'https://api.myapp.com/api',
    timeout: 30000,
    cache: true,
    debug: false,
    logLevel: 'error'
});

app.config(function($logProvider, API_CONFIG) {
    if (!API_CONFIG.debug) {
        $logProvider.debugEnabled(false);
    }
});
```

**Build Script (package.json):**

```json
{
  "name": "myapp-frontend",
  "scripts": {
    "dev": "ng serve --configuration development",
    "build:dev": "ng build --configuration development",
    "build:prod": "ng build --configuration production",
    "test": "ng test",
    "lint": "eslint app/**/*.js"
  },
  "devDependencies": {
    "grunt": "^1.3.0",
    "grunt-cli": "^1.4.3"
  }
}
```

---

## 12.6 Deployment Checklist

**Pre-Deployment:**
- [ ] Run unit tests (`npm test`)
- [ ] Run linter (`npm run lint`)
- [ ] Build production (`npm run build:prod`)
- [ ] Test with ASP.NET locally (`dotnet run`)
- [ ] Verify API endpoints are accessible
- [ ] Check environment variables/secrets in Key Vault
- [ ] Run security scan (OWASP, SonarQube)

**During Deployment:**
- [ ] Monitor deployment in Azure DevOps
- [ ] Watch Application Insights for errors
- [ ] Verify Angular app loads in browser
- [ ] Test critical user workflows
- [ ] Check API responses in Network tab

**Post-Deployment:**
- [ ] Verify all pages load correctly
- [ ] Check console for JavaScript errors
- [ ] Test form submissions
- [ ] Verify authentication flow
- [ ] Monitor error rates for 24 hours
- [ ] Document any issues in runbook

---

## 12.7 Docker Compose for Local Development

**docker-compose.yml (Full Stack):**

```yaml
version: '3.9'

services:
  # ASP.NET API
  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    container_name: myapp-api
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Server=sqlserver;Database=MyAppDb;User Id=sa;Password=YourPassword123;
    depends_on:
      - sqlserver
    networks:
      - myapp-network

  # Angular App (Nginx)
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
    container_name: myapp-frontend
    ports:
      - "4200:80"
    environment:
      - API_BASE_URL=http://api:80/api
    depends_on:
      - api
    networks:
      - myapp-network

  # SQL Server Database
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: myapp-db
    ports:
      - "1433:1433"
    environment:
      - SA_PASSWORD=YourPassword123
      - ACCEPT_EULA=Y
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - myapp-network

networks:
  myapp-network:
    driver: bridge

volumes:
  sqlserver_data:
```

**Dockerfile.dev for Angular:**

```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

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
    location / {
        root /usr/share/nginx/html;
        try_files $uri $uri/ /index.html;
    }
    location /api/ {
        proxy_pass http://api:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

---

## End of Comprehensive Angular.js Learning Guide

**Complete Coverage:**
✅ Angular.js fundamentals & MVC/MVVM
✅ Two-way data binding
✅ Expressions, interpolation, filters
✅ Built-in & custom directives
✅ Controllers & $scope lifecycle
✅ Services, factories, dependency injection
✅ $http for API calls
✅ Promises with $q
✅ Routing (ngRoute & ui-router)
✅ ASP.NET Web API integration (complete CRUD)
✅ Authentication & authorization
✅ Form validation
✅ Testing with Jasmine
✅ Best practices & performance
✅ Advanced state management
✅ Interceptors & middleware
✅ Building & minification
✅ **Deployment to Azure** (App Service, Container Registry)
✅ **Azure DevOps Pipelines** (multi-stage CI/CD)
✅ **Docker containerization** (Angular + ASP.NET)
✅ **Environment-specific configurations**
✅ **Local development setup** (Docker Compose)

---

### Quick Reference: Angular.js vs Modern Angular vs React

| Feature | Angular.js | Modern Angular | React |
|---------|-----------|-----------------|-------|
| **Type** | Framework | Framework | Library |
| **Version** | 1.x (Legacy) | 2-17+ (Current) | 18+ |
| **Learning Curve** | Steep | Very Steep | Moderate |
| **Data Binding** | Two-way | Two-way (property binding) | One-way |
| **Templating** | HTML directives | TypeScript + Templates | JSX |
| **State Management** | $scope, Services | RxJS, NgRx | useState, Redux, Context |
| **Routing** | ngRoute, ui-router | @angular/router | react-router |
| **Built-in** | Most features | CLI, testing, routing | Minimal |
| **Community** | Mature, declining | Very large, active | Largest, very active |
| **Use Case** | Legacy apps | Enterprise SPAs | Modern apps, mobile |
| **CLI** | Manual setup | Angular CLI (excellent) | Create React App |

---

### Migration Path: Angular.js → Modern Angular → React

**If staying with Angular.js:**
1. Use best practices from this guide
2. Keep code modular and testable
3. Plan migration to modern Angular or React
4. Use TypeScript via AngularJS/TypeScript bindings

**Migrating to Modern Angular:**
- Learn TypeScript first
- Components replace Controllers + Directives
- RxJS Observables replace Promises
- CLI handles bundling/minification automatically
- Same architecture principles apply

**Migrating to React:**
- Learn JSX and functional components
- State management (Redux, Context, Zustand)
- Hooks replace class methods
- Different routing library (react-router)
- More minimal framework (bring your own tools)

---

**This comprehensive guide is production-ready for Angular.js developers transitioning to ASP.NET Web API backends and modern deployment practices.**
