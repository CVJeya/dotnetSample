import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { AppointmentQueueComponent } from './features/admin/appointment-queue.component';
import { DoctorManagementComponent } from './features/admin/doctor-management.component';
import { PatientManagementComponent } from './features/admin/patient-management.component';
import { BookingComponent } from './features/appointments/booking.component';
import { MyAppointmentsComponent } from './features/appointments/my-appointments.component';
import { LoginComponent } from './features/auth/login.component';
import { RegisterComponent } from './features/auth/register.component';
import { DoctorAppointmentsComponent } from './features/doctor/doctor-appointments.component';
import { DoctorScheduleComponent } from './features/doctor/doctor-schedule.component';
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
  {
    path: 'admin/appointments',
    component: AppointmentQueueComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'admin/doctors',
    component: DoctorManagementComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'admin/patients',
    component: PatientManagementComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] },
  },
  {
    path: 'doctor/schedule',
    component: DoctorScheduleComponent,
    canActivate: [authGuard],
    data: { roles: ['Doctor'] },
  },
  {
    path: 'doctor/appointments',
    component: DoctorAppointmentsComponent,
    canActivate: [authGuard],
    data: { roles: ['Doctor'] },
  },
  { path: 'access-denied', component: AccessDeniedComponent },
  { path: '**', redirectTo: 'doctors' },
];
