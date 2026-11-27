import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AdminService } from '../../core/api/admin.service';
import { DoctorManagementComponent } from './doctor-management.component';

describe('DoctorManagementComponent', () => {
  let fixture: ComponentFixture<DoctorManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorManagementComponent],
      providers: [
        provideRouter([]),
        {
          provide: AdminService,
          useValue: {
            getDoctorProfiles: () =>
              of([
                {
                  id: 'doctor-1',
                  name: 'Dr. Morgan Lee',
                  specialty: 'Cardiology',
                  biography: 'Patient-centered care.',
                },
              ]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorManagementComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('shows doctor directory management and the doctor account form', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Manage doctors');
    expect(element.textContent).toContain('Dr. Morgan Lee');
    expect(element.querySelector('form')).toBeTruthy();
    expect(element.textContent).toContain('Create doctor account');
  });
});
