import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { AdminService } from '../../core/api/admin.service';
import { PatientManagementComponent } from './patient-management.component';

describe('PatientManagementComponent', () => {
  let fixture: ComponentFixture<PatientManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientManagementComponent],
      providers: [
        provideRouter([]),
        {
          provide: AdminService,
          useValue: {
            getPatients: () =>
              of([
                {
                  userId: 'patient-1',
                  name: 'Avery Patient',
                  email: 'avery@example.test',
                  phoneNumber: '555-0199',
                  isActive: true,
                },
              ]),
            updatePatient: vi.fn(),
            deactivatePatient: vi.fn(),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientManagementComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('lists patient profiles and exposes profile management actions', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Manage patients');
    expect(element.textContent).toContain('Avery Patient');
    expect(element.textContent).toContain('avery@example.test');
    expect(element.querySelector('button[aria-label="Edit patient"]')).toBeTruthy();
    expect(element.querySelector('button[aria-label="Deactivate patient"]')).toBeTruthy();
  });
});
