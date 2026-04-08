import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { CommonModule } from '@angular/common';

import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink, Sidebar],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent implements OnInit {
  stats = {
    totalHotels: 0,
    pendingApprovals: 0,
    totalBookings: 0
  };
  isLoading = false;

  constructor(private apiService: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.isLoading = true;
    this.apiService.get<any[]>('/hotels/pending').subscribe({
      next: (pending) => {
        this.stats.pendingApprovals = pending.length;
        this.apiService.get<any[]>('/hotels/approved').subscribe({
          next: (approved) => {
            this.stats.totalHotels = approved.length;
            this.apiService.get<any[]>('/bookings').subscribe({
              next: (bookings) => {
                this.stats.totalBookings = bookings.length;
                this.isLoading = false;
                this.cdr.detectChanges();
              },
              error: () => {
                this.isLoading = false;
                this.cdr.detectChanges();
              }
            });
          },
          error: () => {
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        });
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
