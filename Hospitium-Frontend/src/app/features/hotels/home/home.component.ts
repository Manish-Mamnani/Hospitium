import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class HomeComponent implements OnInit {
  searchQuery = { city: 'Mumbai', checkIn: '', checkOut: '', guests: 1 };
  featuredHotels = signal<any[]>([]);
  isLoading = signal(false);
  dateError = signal<string | null>(null);
  minDate = new Date().toISOString().split('T')[0];

  constructor(
    private apiService: ApiService, 
    private router: Router
  ) {}

  ngOnInit() {
    this.loadFeaturedHotels();
  }

  loadFeaturedHotels() {
    this.isLoading.set(true);
    this.apiService.get('/hotels').subscribe({
      next: (response: any) => {
        // Handle both paginated responses and flat arrays
        const hotelsArray = Array.isArray(response) ? response : (response?.items || []);
        this.featuredHotels.set(hotelsArray.slice(0, 6)); // Show first 6
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onSearch() {
    this.dateError.set(null);
    
    if (this.searchQuery.checkIn && this.searchQuery.checkIn < this.minDate) {
      this.dateError.set('Check-in date cannot be in the past.');
      return;
    }
    
    if (this.searchQuery.checkOut && this.searchQuery.checkOut < this.minDate) {
      this.dateError.set('Check-out date cannot be in the past.');
      return;
    }
    
    if (this.searchQuery.checkIn && this.searchQuery.checkOut) {
      const inDate = new Date(this.searchQuery.checkIn);
      const outDate = new Date(this.searchQuery.checkOut);
      
      if (outDate < inDate) {
        this.dateError.set('Checkout date cannot be earlier than checkin date.');
        return;
      }
    }
    
    this.router.navigate(['/hotels/search'], { queryParams: this.searchQuery });
  }

  validateDates() {
    this.dateError.set(null);
    
    if (this.searchQuery.checkIn && this.searchQuery.checkIn < this.minDate) {
      this.dateError.set('Check-in date cannot be in the past.');
      return;
    }
    
    if (this.searchQuery.checkOut && this.searchQuery.checkOut < this.minDate) {
      this.dateError.set('Check-out date cannot be in the past.');
      return;
    }

    if (this.searchQuery.checkIn && this.searchQuery.checkOut) {
      const inDate = new Date(this.searchQuery.checkIn);
      const outDate = new Date(this.searchQuery.checkOut);
      if (outDate < inDate) {
        this.dateError.set('Checkout date cannot be earlier than checkin date.');
      }
    }
  }
}
