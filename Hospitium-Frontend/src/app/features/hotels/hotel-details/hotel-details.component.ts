import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { AuthService } from '../../../core/auth.service';
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
  activeImageUrl: string = '';
  rooms: any[] = [];
  reviews: any[] = [];
  isLoading = false;
  errorMessage = '';
  minDate = new Date().toISOString().split('T')[0];
  bookingForm = {
    roomId: null as number | null,
    checkIn: '',
    checkOut: '',
    guests: 1,
    rooms: 1
  };
  reviewForm = {
    rating: 5,
    comment: ''
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadHotel(+id);
      this.loadReviews(+id);
    }
  }

  loadHotel(id: number) {
    this.isLoading = true;
    this.apiService.get(`/hotels/${id}`).subscribe({
      next: (hotel: any) => {
        this.hotel = hotel;
        if (this.hotel.images && this.hotel.images.length > 0) {
           const primary = this.hotel.images.find((i:any) => i.isPrimary) || this.hotel.images[0];
           this.activeImageUrl = `http://localhost:5000${primary.imageUrl}`;
        } else {
           this.activeImageUrl = 'https://images.unsplash.com/photo-1566073771259-6a8506099945?ixlib=rb-4.0.3&auto=format&fit=crop&w=1200&q=80';
        }
        
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

  loadReviews(id: number) {
    this.apiService.get<any[]>(`/hotels/${id}/reviews`).subscribe({
      next: (data) => {
        this.reviews = data || [];
        this.cdr.detectChanges();
      }
    });
  }

  setActiveImage(imgUrl: string) {
    this.activeImageUrl = `http://localhost:5000${imgUrl}`;
  }

  calculateTotalPrice(): number {
    if (!this.bookingForm.roomId || !this.bookingForm.checkIn || !this.bookingForm.checkOut) return 0;
    
    const room = this.rooms.find(r => r.roomId == this.bookingForm.roomId);
    if (!room) return 0;

    const start = new Date(this.bookingForm.checkIn);
    const end = new Date(this.bookingForm.checkOut);
    const diffTime = end.getTime() - start.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    const days = diffDays > 0 ? diffDays : 1;
    return this.bookingForm.rooms * days * room.price;
  }

  bookRoom() {
    this.errorMessage = '';

    if (!this.authService.isAuthenticated()) {
      this.toastService.info('Please log in to confirm your booking.');
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    if (!this.bookingForm.roomId || !this.bookingForm.checkIn || !this.bookingForm.checkOut) {
      this.errorMessage = 'Please fill all fields';
      return;
    }

    if (this.bookingForm.checkIn < this.minDate || this.bookingForm.checkOut < this.minDate) {
      this.errorMessage = 'Dates cannot be in the past.';
      return;
    }

    const start = new Date(this.bookingForm.checkIn);
    const end = new Date(this.bookingForm.checkOut);
    if (end <= start) {
      this.errorMessage = 'Checkout date must be after checkin date.';
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

  submitReview() {
    if (!this.reviewForm.comment) {
      this.toastService.error('Please add a comment to your review.');
      return;
    }

    const reviewData = {
      hotelId: this.hotel.hotelId,
      rating: this.reviewForm.rating,
      comment: this.reviewForm.comment
    };

    this.apiService.post('/reviews', reviewData).subscribe({
      next: () => {
        this.toastService.success('Review submitted successfully!');
        this.reviewForm = { rating: 5, comment: '' };
        this.loadReviews(this.hotel.hotelId);
      },
      error: (err) => {
        this.toastService.error(err.error?.message || 'Failed to submit review.');
      }
    });
  }
}
