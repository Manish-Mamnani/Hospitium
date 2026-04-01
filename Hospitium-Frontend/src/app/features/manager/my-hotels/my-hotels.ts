import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-my-hotels',
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './my-hotels.html',
  styleUrl: './my-hotels.css',
})
export class MyHotelsComponent implements OnInit {
  hotels: any[] = [];
  isLoading = false;

  constructor(private apiService: ApiService, private router: Router, private toastService: ToastService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadHotels();
  }

  loadHotels() {
    this.isLoading = true;
    this.apiService.get<any[]>('/hotels/my').subscribe({
      next: (hotels) => {
        this.hotels = hotels;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
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
