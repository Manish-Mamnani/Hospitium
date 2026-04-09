import { Injectable, signal } from '@angular/core';

export interface CancellationInfo {
  booking: any;
  policy: string;
  deduction: number;
  refundable: number;
}

@Injectable({
  providedIn: 'root'
})
export class CancellationService {
  private resolveCallback: ((value: boolean) => void) | null = null;
  
  config = signal<CancellationInfo | null>(null);
  isOpen = signal(false);

  calculateRefund(booking: any): CancellationInfo {
    const fromDate = new Date(booking.fromDate);
    // Assume 12:00 PM local time for check-in
    const checkInTime = new Date(fromDate.getFullYear(), fromDate.getMonth(), fromDate.getDate(), 12, 0, 0);
    const now = new Date();
    
    const diffMs = checkInTime.getTime() - now.getTime();
    const diffHrs = diffMs / (1000 * 60 * 60);

    let deductionPercent = 0;
    let policy = '';

    if (diffHrs < 0) {
      deductionPercent = 100;
      policy = 'Cancellation after check-in / No-show → 100% deduction';
    } else if (diffHrs < 24) {
      deductionPercent = 50;
      policy = 'Cancellation within 24 hours → 50% deduction';
    } else if (diffHrs < 72) {
      deductionPercent = 25;
      policy = 'Cancellation between 24 and 72 hours → 25% deduction';
    } else {
      deductionPercent = 0;
      policy = 'Cancellation more than 72 hours before check-in → 0% deduction';
    }

    const deduction = (booking.totalPrice * deductionPercent) / 100;
    const refundable = booking.totalPrice - deduction;

    return {
      booking,
      policy,
      deduction,
      refundable
    };
  }

  showDialog(booking: any): Promise<boolean> {
    return new Promise((resolve) => {
      this.config.set(this.calculateRefund(booking));
      this.resolveCallback = resolve;
      this.isOpen.set(true);
    });
  }

  handleAction(result: boolean) {
    this.isOpen.set(false);
    if (this.resolveCallback) {
      this.resolveCallback(result);
      this.resolveCallback = null;
    }
  }
}
