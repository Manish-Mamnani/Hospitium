import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { ConfirmService } from '../../core/confirm.service';

@Component({
  selector: 'app-edit-hotel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-hotel.html',
  styleUrl: './edit-hotel.css',
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
  selectedFiles: File[] = [];

  get descriptionError(): string {
    if (this.hotel.description && this.hotel.description.length > 500) return 'Description cannot exceed 500 characters.';
    return '';
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private confirmService: ConfirmService
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
          description: data.description,
          images: data.images || []
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
    if (!this.hotel.name || !this.hotel.city || this.descriptionError) {
      this.toastService.error('Please fix validation errors before updating.');
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

  async deleteHotel() {
    const confirmed = await this.confirmService.confirm({
      title: 'Delete Property',
      message: 'Are you sure you want to delete this hotel? This action is permanent and will remove all associated rooms and records.',
      confirmText: 'Delete Forever',
      type: 'danger'
    });

    if (!confirmed) return;

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

  onFileSelected(event: any) {
    if (event.target.files) {
      const files = Array.from(event.target.files) as File[];
      for (const file of files) {
        if ((this.hotel.images?.length || 0) + this.selectedFiles.length >= 10) {
           this.toastService.error('Maximum limit of 10 images reached');
           break;
        }
        if (file.size <= 5 * 1024 * 1024) {
          this.selectedFiles.push(file);
        } else {
          this.toastService.error(`${file.name} exceeds 5MB limit`);
        }
      }
      event.target.value = '';
    }
  }

  removeSelectedFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  uploadNewImages() {
    if (this.selectedFiles.length === 0) return;
    this.isSaving = true;
    const formData = new FormData();
    this.selectedFiles.forEach(file => formData.append('images', file));

    this.apiService.post(`/hotels/${this.hotelId}/images`, formData).subscribe({
      next: (res: any) => {
        this.hotel.images = [...(this.hotel.images || []), ...res];
        this.selectedFiles = [];
        this.isSaving = false;
        this.toastService.success('Images uploaded successfully.');
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isSaving = false;
        this.toastService.error('Failed to upload images.');
        this.cdr.detectChanges();
      }
    });
  }

  async deleteImage(imageId: number) {
    const confirmed = await this.confirmService.confirm({
      title: 'Remove Image',
      message: 'Are you sure you want to remove this image from your property gallery?',
      confirmText: 'Remove',
      type: 'danger'
    });

    if (!confirmed) return;

    this.apiService.delete(`/hotels/${this.hotelId}/images/${imageId}`).subscribe({
      next: () => {
        this.hotel.images = this.hotel.images.filter((img: any) => img.imageId !== imageId);
        this.toastService.success('Image removed.');
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.toastService.error('Failed to remove image.');
      }
    });
  }

  getPrimaryImageUrl(imageUrl: string): string {
    return `http://localhost:5000${imageUrl}`;
  }
}
