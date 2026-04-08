import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-global-bookings',
  imports: [CommonModule, FormsModule, Sidebar],
  templateUrl: './global-bookings.html',
})
export class GlobalBookingsComponent implements OnInit {
  bookings: any[] = [];
  isLoading = false;
  filterDate = '';
  selectedStatus = '';
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
    const params: any = {};
    if (this.filterDate) params.date = this.filterDate;
    
    this.apiService.get<any[]>('/bookings', params).subscribe({
      next: (bookings) => {
        this.bookings = bookings || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get filteredBookings() {
    if (!this.selectedStatus) return this.bookings;
    return this.bookings.filter(b => b.status?.toLowerCase() === this.selectedStatus.toLowerCase());
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
      case 'confirmed': 
        return 'badge-success';
      case 'completed':
        return 'badge-active';
      case 'cancelled': 
      case 'rejected':
        return 'badge-error';
      case 'pending': 
        return 'badge-warning';
      default: 
        return 'badge-active';
    }
  }
}
