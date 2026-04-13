import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-results',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './search-results.html',
  styleUrl: './search-results.css',
})
export class SearchResultsComponent implements OnInit {
  hotels = signal<any[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  filters = {
    city: 'Mumbai',
    minPrice: null as number | null,
    maxPrice: null as number | null,
    minRating: 0,
    availableOnly: false,
    sortBy: 'price_asc',
    page: 1,
    pageSize: 10
  };
  totalCount = signal(0);
  totalPages = signal(0);

  resetFilters() {
    this.filters = {
      city: 'Mumbai',
      minPrice: null,
      maxPrice: null,
      minRating: 0,
      availableOnly: false,
      sortBy: 'price_asc',
      page: 1,
      pageSize: 10
    };
    this.applyFilters();
  }

  changePage(newPage: number) {
    if (newPage < 1 || newPage > this.totalPages()) return;
    this.filters.page = newPage;
    this.searchHotels();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filters.city = params['city'] || '';
      this.searchHotels();
    });
  }

  searchHotels() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    // The backend expects separate 'sortBy' (name/price/rating) and 'order' (asc/desc)
    // but the UI uses combined values like 'price_asc', 'rating_desc'
    const [sortField, sortOrder] = this.filters.sortBy.split('_');

    const params: any = {
      city: this.filters.city,
      minPrice: this.filters.minPrice,
      maxPrice: this.filters.maxPrice,
      minRating: this.filters.minRating,
      availableOnly: this.filters.availableOnly,
      sortBy: sortField,
      order: sortOrder,
      page: this.filters.page,
      pageSize: this.filters.pageSize
    };
    
    this.apiService.get<any>('/hotels', params).subscribe({
      next: (response) => {
        // Handle paginated responses (.items or .data) or fallback
        const hotelsData = response.items || response.data || [];
        this.hotels.set(hotelsData);
        this.totalCount.set(response.totalCount || 0);
        this.totalPages.set(Math.ceil(this.totalCount() / this.filters.pageSize));
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load hotels');
        this.isLoading.set(false);
      }
    });
  }

  applyFilters() {
    this.searchHotels();
  }

  getPrimaryImageUrl(hotel: any): string {
    if (hotel.images && hotel.images.length > 0) {
      const primary = hotel.images.find((img: any) => img.isPrimary);
      const path = primary ? primary.imageUrl : hotel.images[0].imageUrl;
      return `http://localhost:5000${path}`;
    }
    return 'https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=1200&q=80';
  }
}
