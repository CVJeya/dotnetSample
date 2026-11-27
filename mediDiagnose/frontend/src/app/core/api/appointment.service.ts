import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Appointment,
  AppointmentRequest,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  constructor(private readonly http: HttpClient) {}

  requestAppointment(
    doctorId: string,
    startsAt: string,
  ): Observable<Appointment> {
    const request: AppointmentRequest = { doctorId, startsAt };
    return this.http.post<Appointment>('/api/appointments', request);
  }

  getMyAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>('/api/appointments/mine');
  }
}
