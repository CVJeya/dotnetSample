import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ApiNoticeService {
  private readonly currentNotice = signal<string | null>(null);
  readonly notice = this.currentNotice.asReadonly();

  show(message: string): void {
    this.currentNotice.set(message);
  }

  clear(): void {
    this.currentNotice.set(null);
  }
}
