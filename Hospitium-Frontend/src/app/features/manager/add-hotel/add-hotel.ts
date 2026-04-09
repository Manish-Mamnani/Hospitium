import { Component, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-add-hotel',
  standalone: true,
  imports: [CommonModule, FormsModule, Sidebar],
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
  selectedFiles: File[] = [];

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
    return !this.nameError && !this.cityError && !this.descriptionError && !!this.hotel.name && !!this.hotel.city;
  }

  get descriptionError(): string {
    if (!this.touched.description) return '';
    if (this.hotel.description && this.hotel.description.length > 500) return 'Description cannot exceed 500 characters.';
    return '';
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

    this.apiService.post<any>('/hotels', payload).subscribe({
      next: (res) => {
        if (this.selectedFiles.length > 0) {
          this.uploadImages(res.hotelId);
        } else {
          this.finalizeSubmit();
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to add hotel. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  onFileSelected(event: any) {
    if (event.target.files) {
      const files = Array.from(event.target.files) as File[];
      for (const file of files) {
        if (this.selectedFiles.length < 10) {
          if (file.size <= 5 * 1024 * 1024) { // 5MB
            this.selectedFiles.push(file);
          } else {
             this.toastService.error(`${file.name} exceeds 5MB limit`);
          }
        } else {
           this.toastService.error('Maximum limit of 10 images reached');
           break;
        }
      }
      // Reset input value so same files can be re-selected if removed
      event.target.value = '';
    }
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  private uploadImages(hotelId: number) {
    const formData = new FormData();
    this.selectedFiles.forEach(file => {
      formData.append('images', file);
    });

    this.apiService.post(`/hotels/${hotelId}/images`, formData).subscribe({
      next: () => {
        this.finalizeSubmit();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Hotel created, but failed to upload images: ' + (err.error?.message || 'Unknown error');
        this.cdr.detectChanges();
      }
    });
  }

  private finalizeSubmit() {
    this.isLoading = false;
    this.toastService.success('Hotel submitted for approval!');
    this.router.navigate(['/manager']);
    this.cdr.detectChanges();
  }
}
