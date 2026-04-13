import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Sidebar } from '../../../shared/sidebar/sidebar';
import { ConfirmService } from '../../../core/confirm.service';

@Component({
  selector: 'app-global-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, Sidebar],
  templateUrl: './global-bookings.html',
  styleUrl: './global-bookings.css',
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
    private cdr: ChangeDetectorRef,
    private confirmService: ConfirmService
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

  async cancelBooking(bookingId: number) {
    const confirmed = await this.confirmService.confirm({
      title: 'Cancel Booking',
      message: 'As an administrator, you are about to force-cancel this reservation. This action is permanent.',
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

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return 'bg-primary/5 text-primary border-primary/20';
      case 'completed': return 'badge-completed';
      case 'cancelled':
      case 'rejected': return 'bg-error-bg text-error-text border-error-text/10';
      case 'pending': return 'bg-accent/5 text-accent border-accent/20';
      default: return 'bg-neutral-bg text-neutral-secondary border-neutral-border';
    }
  }
}
