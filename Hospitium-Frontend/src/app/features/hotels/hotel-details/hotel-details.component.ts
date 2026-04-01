import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-hotel-details',
  imports: [CommonModule, FormsModule],
  templateUrl: './hotel-details.html',
  styleUrl: './hotel-details.css',
})
export class HotelDetailsComponent implements OnInit {
  hotel: any = null;
  rooms: any[] = [];
  isLoading = false;
  errorMessage = '';
  bookingForm = {
    roomId: null as number | null,
    checkIn: '',
    checkOut: '',
    guests: 1,
    rooms: 1
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadHotel(+id);
    }
  }

  loadHotel(id: number) {
    this.isLoading = true;
    this.apiService.get(`/hotels/${id}`).subscribe({
      next: (hotel: any) => {
        this.hotel = hotel;
        
        // Fetch rooms dynamically via separate endpoint mapped to API
        this.apiService.get(`/hotels/${id}/rooms`).subscribe({
          next: (roomsData: any) => {
            this.rooms = roomsData || [];
            this.isLoading = false;
            this.cdr.detectChanges();
          },
          error: () => {
             this.rooms = [];
             this.isLoading = false;
             this.cdr.detectChanges();
          }
        });
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Failed to load hotel details';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  bookRoom() {
    if (!this.bookingForm.roomId || !this.bookingForm.checkIn || !this.bookingForm.checkOut) {
      this.errorMessage = 'Please fill all fields';
      return;
    }

    const booking = {
      roomId: this.bookingForm.roomId,
      fromDate: this.bookingForm.checkIn,
      toDate: this.bookingForm.checkOut,
      numberOfRooms: this.bookingForm.rooms
    };

    this.apiService.post('/bookings', booking).subscribe({
      next: () => {
        this.toastService.success('Booking confirmed successfully!');
        this.router.navigate(['/bookings']);
      },
      error: (err) => {
        const errorMsg = err.error?.message || 'Booking failed';
        this.errorMessage = errorMsg;
        this.toastService.error(errorMsg);
      }
    });
  }
}
