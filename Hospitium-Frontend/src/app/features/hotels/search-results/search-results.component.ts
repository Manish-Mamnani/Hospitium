import { Component, OnInit } from '@angular/core';
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
  hotels: any[] = [];
  isLoading = false;
  errorMessage = '';
  filters = {
    city: '',
    minPrice: null as number | null,
    maxPrice: null as number | null,
    sortBy: 'price'
  };

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
    this.isLoading = true;
    this.errorMessage = '';
    const params = {
      city: this.filters.city,
      minPrice: this.filters.minPrice,
      maxPrice: this.filters.maxPrice,
      sortBy: this.filters.sortBy
    };
    this.apiService.get('/hotels', params).subscribe({
      next: (response: any) => {
        this.hotels = response.data || response;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load hotels';
        this.isLoading = false;
      }
    });
  }

  applyFilters() {
    this.searchHotels();
  }
}
