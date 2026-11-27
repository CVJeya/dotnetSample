import { DatePipe } from '@angular/common';
import { appointmentTimezoneOffset } from './appointment-timezone';

describe('appointmentTimezoneOffset', () => {
  it('preserves the appointment offset for display in hospital local time', () => {
    expect(appointmentTimezoneOffset('2026-12-01T09:30:00+05:30')).toBe('+0530');
    expect(appointmentTimezoneOffset('2026-07-12T14:00:00-04:00')).toBe('-0400');
  });

  it('uses UTC for a UTC appointment timestamp', () => {
    expect(appointmentTimezoneOffset('2026-12-01T04:00:00Z')).toBe('UTC');
  });

  it('formats the returned wall-clock time instead of the browser local time', () => {
    const timestamp = '2026-07-12T14:00:00-04:00';
    const pipe = new DatePipe('en-US');

    expect(
      pipe.transform(timestamp, 'h:mm a', appointmentTimezoneOffset(timestamp)),
    ).toBe('2:00 PM');
  });
});
