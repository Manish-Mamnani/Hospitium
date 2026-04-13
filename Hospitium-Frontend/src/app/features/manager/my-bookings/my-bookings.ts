import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmService } from '../../../core/confirm.service';

import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, Sidebar],
  templateUrl: './my-bookings.html',
  styleUrl: './my-bookings.css',
})
export class MyBookingsComponent implements OnInit {
  bookings: any[] = [];
  isLoading = false;
  cancellingId: number | null = null;
  completingId: number | null = null;
  selectedStatus = '';
  filterDate = '';

  get filteredBookings(): any[] {
    return this.bookings.filter(b => {
      const statusMatch = !this.selectedStatus || b.status?.toLowerCase() === this.selectedStatus.toLowerCase();
      const dateMatch = !this.filterDate || b.fromDate?.startsWith(this.filterDate) || b.toDate?.startsWith(this.filterDate);
      return statusMatch && dateMatch;
    });
  }

  resetFilters() {
    this.selectedStatus = '';
    this.filterDate = '';
  }

  constructor(
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private confirmService: ConfirmService
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

  async cancelBooking(bookingId: number) {
    const confirmed = await this.confirmService.confirm({
      title: 'Cancel Reservation',
      message: 'Are you sure you want to cancel this booking? This action might be subject to the property policy.',
      confirmText: 'Yes, Cancel',
      type: 'danger'
    });

    if (!confirmed) return;
    
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

  async completeBooking(bookingId: number) {
    const confirmed = await this.confirmService.confirm({
      title: 'Complete Stay',
      message: 'Mark this guest stay as finished? This will officially close the reservation records.',
      confirmText: 'Complete',
      type: 'success'
    });

    if (!confirmed) return;

    this.completingId = bookingId;
    this.apiService.put(`/bookings/${bookingId}/complete`, {}).subscribe({
      next: () => {
        this.completingId = null;
        this.toastService.success('Booking marked as completed.');
        this.loadBookings();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.completingId = null;
        this.toastService.error(err.error?.message || 'Failed to complete booking.');
        this.cdr.detectChanges();
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return 'bg-primary/5 text-primary border-primary/20';
      case 'completed': return 'badge-completed';
      case 'cancelled': return 'bg-error-bg text-error-text border-error-text/10';
      case 'pending': return 'bg-accent/5 text-accent border-accent/20';
      default: return 'bg-gray-100 text-gray-600 border-gray-200';
    }
  }
}
