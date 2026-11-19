import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { BookingComponent } from './features/appointments/booking.component';
import { MyAppointmentsComponent } from './features/appointments/my-appointments.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { AccessDeniedComponent } from './features/shared/access-denied.component';
import { DoctorAvailabilityComponent } from './features/doctors/doctor-availability.component';
import { DoctorListComponent } from './features/doctors/doctor-list.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'doctors' },
  { path: 'doctors', component: DoctorListComponent },
  { path: 'doctors/:doctorId', component: DoctorAvailabilityComponent },
  {
    path: 'doctors/:doctorId/book',
    component: BookingComponent,
    canActivate: [authGuard],
    data: { roles: ['Patient'] },
  },
  { path: 'register', component: RegisterComponent },
  { path: 'login', component: LoginComponent },
  {
    path: 'appointments',
    component: MyAppointmentsComponent,
    canActivate: [authGuard],
    data: { roles: ['Patient'] },
  },
  { path: 'access-denied', component: AccessDeniedComponent },
  { path: '**', redirectTo: 'doctors' },
];
