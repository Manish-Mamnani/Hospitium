import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, Sidebar],
  templateUrl: './my-bookings.html',
})
export class MyBookingsComponent implements OnInit {
  bookings: any[] = [];
  isLoading = false;
  cancellingId: number | null = null;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    this.isLoading = true;
    this.apiService.get<any[]>('/bookings/manager').subscribe({
      next: (bookings) => {
        this.bookings = bookings || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.toastService.error('Failed to load your property bookings.');
        this.cdr.detectChanges();
      }
    });
  }

  cancelBooking(bookingId: number) {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    this.cancellingId = bookingId;
    this.apiService.put(`/bookings/${bookingId}/cancel`, {}).subscribe({
      next: () => {
        this.cancellingId = null;
        this.toastService.success('Booking cancelled successfully.');
        this.loadBookings();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.cancellingId = null;
        this.toastService.error(err.error?.message || 'Failed to cancel booking.');
        this.cdr.detectChanges();
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return 'bg-green-100 text-green-700';
      case 'cancelled': return 'bg-red-100 text-red-700';
      case 'pending': return 'bg-yellow-100 text-yellow-700';
      default: return 'bg-gray-100 text-gray-600';
    }
  }
}
