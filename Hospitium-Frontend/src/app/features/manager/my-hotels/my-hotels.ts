import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';

import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-my-hotels',
  imports: [CommonModule, RouterLink, Sidebar],
  templateUrl: './my-hotels.html',
  styleUrl: './my-hotels.css',
})
export class MyHotelsComponent implements OnInit {
  hotels = signal<any[]>([]);
  isLoading = signal(false);

  constructor(private apiService: ApiService, private router: Router, private toastService: ToastService) {}

  ngOnInit() {
    this.loadHotels();
  }

  loadHotels() {
    this.isLoading.set(true);
    this.apiService.get<any[]>('/hotels/my').subscribe({
      next: (hotels) => {
        const sorted = (hotels || []).sort((a, b) => {
           if (a.status === 'Deleted' && b.status !== 'Deleted') return 1;
           if (a.status !== 'Deleted' && b.status === 'Deleted') return -1;
           return 0;
        });
        this.hotels.set(sorted);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  addHotel() {
    this.router.navigate(['/manager/add-hotel']);
  }

  manageRooms(hotelId: number) {
    this.router.navigate(['/manager/manage-rooms', hotelId]);
  }

  editHotel(hotelId: number) {
    this.router.navigate(['/manager/hotels', hotelId, 'edit']);
  }

  getPrimaryImageUrl(hotel: any): string {
    if (hotel.images && hotel.images.length > 0) {
      const primary = hotel.images.find((i: any) => i.isPrimary) || hotel.images[0];
      return `http://localhost:5000${primary.imageUrl}`;
    }
    return `https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=800&q=80`;
  }
}
