import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-hotel',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-hotel.html',
})
export class AddHotelComponent {
  hotel = {
    name: '',
    city: '',
    description: ''
  };
  isLoading = false;
  errorMessage = '';
  touched = { name: false, city: false, description: false };

  constructor(
    private apiService: ApiService,
    public router: Router,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  get nameError(): string {
    if (!this.touched.name) return '';
    if (!this.hotel.name.trim()) return 'Hotel name is required.';
    if (this.hotel.name.trim().length < 3) return 'Name must be at least 3 characters.';
    return '';
  }

  get cityError(): string {
    if (!this.touched.city) return '';
    if (!this.hotel.city.trim()) return 'City is required.';
    return '';
  }

  get isFormValid(): boolean {
    return !this.nameError && !this.cityError && !!this.hotel.name && !!this.hotel.city;
  }

  touch(field: 'name' | 'city' | 'description') { this.touched[field] = true; }

  onSubmit() {
    this.touched = { name: true, city: true, description: true };
    if (!this.isFormValid || this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = '';

    const payload = {
      name: this.hotel.name.trim(),
      city: this.hotel.city.trim(),
      description: this.hotel.description.trim()
    };

    this.apiService.post('/hotels', payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.success('Hotel submitted for approval!');
        this.router.navigate(['/manager']);
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to add hotel. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }
}
