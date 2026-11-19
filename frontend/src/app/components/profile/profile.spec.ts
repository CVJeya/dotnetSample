import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ProfileComponent } from './profile';
import { environment } from '../../../environments/environment';

describe('ProfileComponent', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProfileComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requestsCurrentUserProfile', () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();

    const request = http.expectOne(`${environment.apiUrl}/users/me`);
    expect(request.request.method).toBe('GET');
    request.flush({
      id: 1042,
      name: 'Avery Morgan',
      email: 'avery@example.com',
      createdAt: '2026-11-18T00:00:00Z',
    });
    fixture.detectChanges();

    const nativeElement = fixture.nativeElement as HTMLElement;
    const rows = Array.from(nativeElement.querySelectorAll('.detail-row'))
      .map((row) => Array.from(row.querySelectorAll('span'))
        .map((span) => span.textContent?.trim()).join(' '));
    expect(rows).toEqual([
      'Name Avery Morgan',
      'Email avery@example.com',
      'Account ID 1042',
      'Member since Nov 18, 2026',
    ]);
  });

  it('showsLoadingWhileProfileRequestIsPending', () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.loading')?.textContent).toContain('Loading profile');
    http.expectOne(`${environment.apiUrl}/users/me`).flush({
      id: 1042,
      name: 'Avery Morgan',
      email: 'avery@example.com',
      createdAt: '2026-11-18T00:00:00Z',
    });
  });

  it('showsErrorWhenProfileRequestFails', () => {
    const fixture = TestBed.createComponent(ProfileComponent);
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/users/me`).flush(
      { message: 'Test failure' },
      { status: 500, statusText: 'Server Error' },
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.error')?.textContent).toContain('Failed to fetch user profile');
  });
});
