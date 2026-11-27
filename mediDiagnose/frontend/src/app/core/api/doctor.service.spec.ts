import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { DoctorService } from './doctor.service';

describe('DoctorService', () => {
  let service: DoctorService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DoctorService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DoctorService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads public doctor profiles without requiring an authenticated session', async () => {
    const result = firstValueFrom(service.getDoctors());
    const request = http.expectOne('/api/doctors');

    expect(request.request.method).toBe('GET');
    request.flush([
      {
        id: 'doctor-1',
        name: 'Dr. Morgan Lee',
        specialty: 'Cardiology',
        biography: 'Patient-centered care.',
      },
    ]);

    await expect(result).resolves.toEqual([
      {
        id: 'doctor-1',
        name: 'Dr. Morgan Lee',
        specialty: 'Cardiology',
        biography: 'Patient-centered care.',
      },
    ]);
  });
});
