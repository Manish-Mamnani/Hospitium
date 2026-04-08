import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-approval-queue',
  standalone: true,
  imports: [CommonModule, RouterLink, Sidebar],
  templateUrl: './approval-queue.html',
  styleUrl: './approval-queue.css',
})
export class ApprovalQueueComponent implements OnInit {
  pendingHotels: any[] = [];
  isLoading = false;

  constructor(private apiService: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadPendingHotels();
  }

  loadPendingHotels() {
    this.isLoading = true;
    this.apiService.get<any[]>('/hotels/pending').subscribe({
      next: (hotels) => {
        this.pendingHotels = hotels;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  approveHotel(hotelId: number) {
    this.apiService.put(`/hotels/${hotelId}/approve`, {}).subscribe({
      next: () => {
        this.loadPendingHotels();
      },
      error: (err) => {
        alert('Failed to approve hotel: ' + err.error?.message);
      }
    });
  }

  rejectHotel(hotelId: number) {
    this.apiService.put(`/hotels/${hotelId}/reject`, {}).subscribe({
      next: () => {
        this.loadPendingHotels();
      },
      error: (err) => {
        alert('Failed to reject hotel: ' + err.error?.message);
      }
    });
  }
}
