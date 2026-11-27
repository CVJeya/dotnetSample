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

export interface AdminDoctorProfile extends DoctorProfile {
  userId: string;
  isActive: boolean;
}

export interface DoctorAccount {
  userId: string;
  name: string;
  email: string;
  role: 'Doctor';
}

export interface DoctorShift {
  dayOfWeek: number;
  shift: 'Morning' | 'Evening';
  startTime: string;
  endTime: string;
  appointmentDurationMinutes: number;
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

export interface StaffAppointment extends Appointment {
  patientName: string;
  patientEmail: string;
  patientPhoneNumber: string;
}

export interface AdminPatientProfile extends PatientProfile {
  isActive: boolean;
}

export interface AppointmentRequest {
  doctorId: string;
  startsAt: string;
}
