import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  PatientProfile,
  PatientProfileInput,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PatientService {
  constructor(private readonly http: HttpClient) {}

  getMyProfile(): Observable<PatientProfile> {
    return this.http.get<PatientProfile>('/api/patients/me');
  }

  saveMyProfile(profile: PatientProfileInput): Observable<PatientProfile> {
    return this.http.put<PatientProfile>('/api/patients/me', profile);
  }
}
