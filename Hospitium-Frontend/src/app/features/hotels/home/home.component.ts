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
  featuredDestinations = signal([
    { name: 'Mumbai', imageUrl: 'https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80', isAuthentic: false },
    { name: 'Delhi', imageUrl: 'https://images.unsplash.com/photo-1582719508461-905c673771fd?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80', isAuthentic: false },
    { name: 'Goa', imageUrl: 'https://images.unsplash.com/photo-1540541338287-41700207dee6?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80', isAuthentic: false }
  ]);

  isLoading = signal(false);
  dateError = signal<string | null>(null);
  minDate = new Date().toISOString().split('T')[0];

  constructor(
    private apiService: ApiService, 
    private router: Router
  ) {}

  ngOnInit() {
    this.loadFeaturedHotels();
    this.loadFeaturedCityImages();
  }

  loadFeaturedCityImages() {
    const cities = this.featuredDestinations();
    cities.forEach((city, index) => {
      this.apiService.get<any>('/hotels', { city: city.name, sortBy: 'rating_desc', pageSize: 1 }).subscribe({
        next: (response) => {
          const hotels = Array.isArray(response) ? response : (response?.items || []);
          if (hotels.length > 0 && hotels[0].images && hotels[0].images.length > 0) {
            const h = hotels[0];
            const primary = h.images.find((i:any) => i.isPrimary) || h.images[0];
            const updated = [...this.featuredDestinations()];
            updated[index] = { ...city, imageUrl: `http://localhost:5000${primary.imageUrl}`, isAuthentic: true };
            this.featuredDestinations.set(updated);
          }
        }
      });
    });
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
