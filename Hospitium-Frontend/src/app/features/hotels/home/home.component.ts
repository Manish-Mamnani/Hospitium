import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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
  searchQuery = { city: '', checkIn: '', checkOut: '', guests: 1 };
  featuredHotels: any[] = [];
  isLoading = false;

  constructor(
    private apiService: ApiService, 
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadFeaturedHotels();
  }

  loadFeaturedHotels() {
    this.isLoading = true;
    this.apiService.get('/hotels').subscribe({
      next: (hotels) => {
        this.featuredHotels = (hotels as any[]).slice(0, 6); // Show first 6
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onSearch() {
    this.router.navigate(['/hotels/search'], { queryParams: this.searchQuery });
  }
}
