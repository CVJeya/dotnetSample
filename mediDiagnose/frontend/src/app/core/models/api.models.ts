export interface AuthSession {
  accessToken: string;
  userId: string;
  role: 'Patient' | 'Doctor' | 'Admin';
}

export interface PatientRegistration {
  name: string;
  email: string;
  password: string;
  phoneNumber: string;
}

export interface PatientProfileInput {
  name: string;
  email: string;
  phoneNumber: string;
}

export interface PatientProfile extends PatientProfileInput {
  userId: string;
  isActive: boolean;
}

export interface DoctorProfile {
  id: string;
  name: string;
  specialty: string;
  biography: string;
}

export interface AppointmentSlot {
  startsAt: string;
  endsAt: string;
}

export type AppointmentStatus = 'Pending' | 'Approved' | 'Declined';

export interface Appointment {
  id: string;
  doctorId: string;
  patientId: string;
  startsAt: string;
  endsAt: string;
  shift: 'Morning' | 'Evening';
  status: AppointmentStatus;
}

export interface AppointmentRequest {
  doctorId: string;
  startsAt: string;
}
