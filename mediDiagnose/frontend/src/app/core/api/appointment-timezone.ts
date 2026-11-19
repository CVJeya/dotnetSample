export function appointmentTimezoneOffset(value: string): string | undefined {
  if (value.endsWith('Z')) {
    return 'UTC';
  }

  const offset = /([+-]\d{2}):(\d{2})$/.exec(value);
  return offset ? `${offset[1]}${offset[2]}` : undefined;
}
