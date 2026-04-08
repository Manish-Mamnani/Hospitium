import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-edit-hotel',
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-hotel.html',
})
export class EditHotelComponent implements OnInit {
  hotelId!: number;
  hotel: any = {
    name: '',
    city: '',
    description: ''
  };
  isLoading = false;
  isSaving = false;
  isDeleting = false;
  userRole: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.userRole = this.authService.getRole();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.hotelId = +id;
      this.loadHotel();
    }
  }

  loadHotel() {
    this.isLoading = true;
    this.apiService.get<any>(`/hotels/${this.hotelId}`).subscribe({
      next: (data) => {
        this.hotel = {
          name: data.name,
          city: data.city,
          description: data.description
        };
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.error('Failed to load hotel details.');
        this.cdr.detectChanges();
      }
    });
  }

  updateHotel() {
    if (!this.hotel.name || !this.hotel.city) {
      this.toastService.error('Name and City are required.');
      return;
    }

    this.isSaving = true;
    this.apiService.put(`/hotels/${this.hotelId}`, this.hotel).subscribe({
      next: () => {
        this.isSaving = false;
        this.toastService.success('Hotel updated successfully.');
        this.goBack();
      },
      error: (err) => {
        this.isSaving = false;
        this.toastService.error(err.error?.message || 'Failed to update hotel.');
        this.cdr.detectChanges();
      }
    });
  }

  deleteHotel() {
    if (!confirm('Are you sure you want to delete this hotel? This action cannot be undone.')) {
      return;
    }

    this.isDeleting = true;
    this.apiService.delete(`/hotels/${this.hotelId}`).subscribe({
      next: () => {
        this.isDeleting = false;
        this.toastService.success('Hotel deleted successfully.');
        this.goBack();
      },
      error: (err) => {
        this.isDeleting = false;
        this.toastService.error(err.error?.message || 'Failed to delete hotel.');
        this.cdr.detectChanges();
      }
    });
  }

  goBack() {
    if (this.userRole === 'Admin') {
      this.router.navigate(['/admin/hotels']);
    } else {
      this.router.navigate(['/manager']);
    }
  }
}
