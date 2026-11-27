import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AppointmentSlot,
  DoctorProfile,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class DoctorService {
  constructor(private readonly http: HttpClient) {}

  getDoctors(): Observable<DoctorProfile[]> {
    return this.http.get<DoctorProfile[]>('/api/doctors');
  }

  getDoctor(doctorId: string): Observable<DoctorProfile> {
    return this.http.get<DoctorProfile>(`/api/doctors/${doctorId}`);
  }

  getAvailability(
    doctorId: string,
    date: string,
    shift: 'Morning' | 'Evening',
  ): Observable<AppointmentSlot[]> {
    const params = new HttpParams().set('date', date).set('shift', shift);
    return this.http.get<AppointmentSlot[]>(
      `/api/availability/doctors/${doctorId}`,
      { params },
    );
  }
}
