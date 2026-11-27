import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AdminDoctorProfile,
  AdminPatientProfile,
  Appointment,
  AppointmentStatus,
  DoctorAccount,
  DoctorProfile,
  PatientProfileInput,
  StaffAppointment,
} from '../models/api.models';

export interface CreateDoctorAccountInput {
  name: string;
  email: string;
  password: string;
}

export interface CreateDoctorProfileInput {
  userId: string;
  name: string;
  specialty: string;
  biography: string;
}

export interface UpdateDoctorProfileInput {
  name: string;
  specialty: string;
  biography: string;
  isActive: boolean;
}

export interface UpdatePatientProfileInput extends PatientProfileInput {
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  getAppointments(status: AppointmentStatus): Observable<StaffAppointment[]> {
    const params = new HttpParams().set('status', status);
    return this.http.get<StaffAppointment[]>('/api/admin/appointments', { params });
  }

  approveAppointment(appointmentId: string): Observable<Appointment> {
    return this.http.post<Appointment>(
      `/api/admin/appointments/${appointmentId}/approve`,
      null,
    );
  }

  declineAppointment(appointmentId: string): Observable<Appointment> {
    return this.http.post<Appointment>(
      `/api/admin/appointments/${appointmentId}/decline`,
      null,
    );
  }

  getDoctorProfiles(): Observable<DoctorProfile[]> {
    return this.http.get<DoctorProfile[]>('/api/doctors');
  }

  createDoctorAccount(input: CreateDoctorAccountInput): Observable<DoctorAccount> {
    return this.http.post<DoctorAccount>('/api/admin/doctor-accounts', input);
  }

  createDoctorProfile(input: CreateDoctorProfileInput): Observable<AdminDoctorProfile> {
    return this.http.post<AdminDoctorProfile>('/api/admin/doctors', input);
  }

  updateDoctorProfile(
    doctorId: string,
    input: UpdateDoctorProfileInput,
  ): Observable<AdminDoctorProfile> {
    return this.http.put<AdminDoctorProfile>(`/api/admin/doctors/${doctorId}`, input);
  }

  deactivateDoctor(doctorId: string): Observable<void> {
    return this.http.delete<void>(`/api/admin/doctors/${doctorId}`);
  }

  getPatients(): Observable<AdminPatientProfile[]> {
    return this.http.get<AdminPatientProfile[]>('/api/admin/patients');
  }

  updatePatient(
    userId: string,
    input: UpdatePatientProfileInput,
  ): Observable<AdminPatientProfile> {
    return this.http.put<AdminPatientProfile>(`/api/admin/patients/${userId}`, input);
  }

  deactivatePatient(userId: string): Observable<void> {
    return this.http.delete<void>(`/api/admin/patients/${userId}`);
  }
}
