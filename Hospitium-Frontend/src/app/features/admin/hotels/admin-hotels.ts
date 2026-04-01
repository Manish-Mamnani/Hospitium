import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-hotels',
  imports: [CommonModule, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './admin-hotels.html',
})
export class AdminHotelsComponent implements OnInit {
  hotels: any[] = [];
  isLoading = false;
  searchTerm = '';

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
    if (!this.searchTerm) return this.hotels;
    const term = this.searchTerm.toLowerCase();
    return this.hotels.filter(h => 
      h.name.toLowerCase().includes(term) || 
      h.city.toLowerCase().includes(term) ||
      h.managerEmail.toLowerCase().includes(term)
    );
  }

  getStatusClass(status: string) {
    switch (status?.toLowerCase()) {
      case 'approved': return 'bg-emerald-100 text-emerald-700';
      case 'pending': return 'bg-amber-100 text-amber-700';
      case 'rejected': return 'bg-rose-100 text-rose-700';
      case 'deleted': return 'bg-gray-100 text-gray-700';
      default: return 'bg-blue-100 text-blue-700';
    }
  }

}
