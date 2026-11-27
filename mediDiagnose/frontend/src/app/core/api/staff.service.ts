import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DoctorShift, StaffAppointment } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class StaffService {
  constructor(private readonly http: HttpClient) {}

  getMySchedule(): Observable<DoctorShift[]> {
    return this.http.get<DoctorShift[]>('/api/doctors/me/shifts');
  }

  saveMySchedule(shifts: DoctorShift[]): Observable<void> {
    return this.http.put<void>('/api/doctors/me/shifts', { shifts });
  }

  getMyAppointments(): Observable<StaffAppointment[]> {
    return this.http.get<StaffAppointment[]>('/api/doctors/me/appointments');
  }
}
