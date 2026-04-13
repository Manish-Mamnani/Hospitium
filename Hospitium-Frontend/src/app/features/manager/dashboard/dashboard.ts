import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, Sidebar],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class ManagerDashboardComponent implements OnInit {
  isLoading = signal(true);
  stats = signal({
    totalHotels: 0,
    totalBookings: 0,
    totalRevenue: 0,
    activeReservations: 0
  });
  recentBookings = signal<any[]>([]);

  constructor(
    private apiService: ApiService
  ) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.isLoading.set(true);
    
    // 1. Fetch Hotels Count
    this.apiService.get<any[]>('/hotels/my').subscribe({
      next: (hotels) => {
        this.stats.update(s => ({ ...s, totalHotels: hotels?.length || 0 }));
        this.checkLoading();
      },
      error: () => this.checkLoading()
    });

    // 2. Fetch Bookings and Calculate Stats
    this.apiService.get<any[]>('/bookings/manager').subscribe({
      next: (bookings) => {
        const _recentBookings = (bookings || []).slice(0, 5);
        this.recentBookings.set(_recentBookings);
        
        const _totalBookings = bookings?.length || 0;
        const _activeReservations = (bookings || []).filter(b => b.status === 'Completed').length;
        const _totalRevenue = (bookings || [])
          .filter(b => b.status === 'Completed')
          .reduce((sum, b) => sum + (b.totalPrice || 0), 0);
          
        this.stats.update(s => ({ 
          ...s, 
          totalBookings: _totalBookings,
          activeReservations: _activeReservations,
          totalRevenue: _totalRevenue
        }));
          
        this.checkLoading();
      },
      error: () => this.checkLoading()
    });
  }

  private checkLoading() {
    this.isLoading.set(false);
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return 'bg-primary/5 text-primary border-primary/20';
      case 'completed': return 'badge-completed';
      case 'cancelled': return 'bg-error-bg text-error-text border-error-text/10';
      default: return 'bg-accent/5 text-accent border-accent/20';
    }
  }
}
