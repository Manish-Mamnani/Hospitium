import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { CancellationService } from '../../../core/cancellation.service';
import { Console } from 'node:console';

@Component({
  selector: 'app-my-bookings',
  imports: [CommonModule],
  templateUrl: './my-bookings.html',
  styleUrl: './my-bookings.css',
})
export class MyBookingsComponent implements OnInit {
  activeBookings: any[] = [];
  pastBookings: any[] = [];
  isLoading = false;
  activeTab = 'active';
  cancellingId: number | null = null;
  

  constructor(
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private cancellationService: CancellationService
  ) {}

  ngOnInit() {
    this.loadBookings();
  }
  
  loadBookings() {
    this.isLoading = true;
    // GET /bookings/my returns BookingResponseDto which already has hotelName and roomType
    this.apiService.get<any[]>('/bookings/my').subscribe({
      next: (bookings) => {
        this.activeBookings = bookings.filter(b => b.status === 'Confirmed');
        this.pastBookings = bookings.filter(b => b.status === 'Cancelled' || b.status === 'Completed');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
  
  async cancelBooking(booking: any) {
    if (this.cancellingId) return;
    
    const confirmed = await this.cancellationService.showDialog(booking);
    if (!confirmed) return;

    this.cancellingId = booking.bookingId;
    this.apiService.put(`/bookings/${booking.bookingId}/cancel`, {}).subscribe({
      next: () => {
        this.cancellingId = null;
        this.toastService.success('Booking cancelled successfully.');
        this.loadBookings();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.cancellingId = null;
        const errorMsg = err.error?.message || 'Failed to cancel booking.';
        this.toastService.error(errorMsg);
        this.cdr.detectChanges();
      }
    });
  }

  setTab(tab: string) {
    this.activeTab = tab;
  }
}
