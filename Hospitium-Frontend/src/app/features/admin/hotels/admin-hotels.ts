import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-admin-hotels',
  imports: [CommonModule, RouterLink, FormsModule, Sidebar],
  templateUrl: './admin-hotels.html',
})
export class AdminHotelsComponent implements OnInit {
  hotels: any[] = [];
  isLoading = false;
  searchTerm = '';
  selectedStatus = '';

  constructor(
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadHotels();
  }

  loadHotels() {
    this.isLoading = true;
    this.apiService.get<any[]>('/hotels/admin/all').subscribe({
      next: (data) => {
        this.hotels = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.error('Failed to load hotels.');
        this.cdr.detectChanges();
      }
    });
  }

  get filteredHotels() {
    let filtered = this.hotels;
    
    if (this.selectedStatus) {
      filtered = filtered.filter(h => h.status?.toLowerCase() === this.selectedStatus.toLowerCase());
    }

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(h => 
        h.name.toLowerCase().includes(term) || 
        h.city.toLowerCase().includes(term) ||
        h.managerEmail.toLowerCase().includes(term)
      );
    }
    
    return filtered;
  }

  getStatusClass(status: string) {
    switch (status?.toLowerCase()) {
      case 'approved': return 'badge-success';
      case 'pending': return 'badge-warning';
      case 'rejected': return 'badge-error';
      case 'deleted': return 'badge-cancelled';
      default: return 'badge-active';
    }
  }

}
