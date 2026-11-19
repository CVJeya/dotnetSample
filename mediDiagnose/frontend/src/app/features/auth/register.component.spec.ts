import { provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;

  beforeEach(async () => {
    sessionStorage.setItem(
      'medi-diagnose.auth',
      JSON.stringify({
        accessToken: 'patient-token',
        userId: 'patient-1',
        role: 'Patient',
      }),
    );
    sessionStorage.setItem(
      'medi-diagnose.pending-profile',
      JSON.stringify({
        name: 'Avery Patient',
        email: 'avery@example.test',
        phoneNumber: '555-0199',
      }),
    );

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    fixture.detectChanges();
  });

  afterEach(() => sessionStorage.clear());

  it('restores the retry state when a profile completion is pending', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Your account is ready');
    expect(element.querySelector('button')?.textContent).toContain('Retry profile setup');
  });
});
