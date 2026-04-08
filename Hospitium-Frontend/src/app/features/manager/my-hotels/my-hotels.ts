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
        this.hotels.set(hotels || []);
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
}
