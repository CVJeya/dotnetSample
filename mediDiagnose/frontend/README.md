# MediDiagnose patient PWA

The Angular 22 application supports patient registration, doctor browsing, appointment requests, and appointment status tracking. It uses the ASP.NET API gateway at `/api`.

## Run locally

Start the backend stack first by following [`../backend/README.md`](../backend/README.md). The Angular development server proxies `/api/**` to `http://localhost:8080`.

```powershell
npm install
npm start
```

Open `http://localhost:4200`. Create a patient account, then browse a doctor and select an available time. Pending requests reserve shift capacity and must be approved by hospital staff.

## Test and build

```powershell
npm test -- --watch=false
npm run build
```

The production build includes Angular's service worker and installable PWA manifest. The service worker caches static application files only; it does not cache patient or appointment API responses. Deploy the build behind a same-origin `/api` route to the backend gateway and configure the web server to serve `index.html` for Angular routes.

Patient access tokens are kept in `sessionStorage` for the lifetime of the browser session. They are never logged. A Content Security Policy and HTTPS should be enabled by the deployment host.
