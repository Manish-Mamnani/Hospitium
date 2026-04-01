import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manage-rooms',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './manage-rooms.html',
})
export class ManageRoomsComponent implements OnInit {
  hotelId!: number;
  hotel: any = null;
  rooms: any[] = [];
  isLoading = false;
  errorMessage = '';

  // Add room form
  showAddForm = false;
  newRoom = { type: '', price: 0, totalCount: 1 };
  isAddingRoom = false;
  addRoomError = '';
  roomTypes = ['Single', 'Double', 'Deluxe', 'Suite', 'Twin', 'King', 'Family'];

  // Edit room
  editingRoomId: number | null = null;
  editRoom = { price: 0, totalCount: 1 };
  isUpdatingRoom = false;

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private apiService: ApiService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('hotelId');
    if (id) {
      this.hotelId = +id;
      this.loadData();
    }
  }

  loadData() {
    this.isLoading = true;
    // Load hotel details
    this.apiService.get<any>(`/hotels/${this.hotelId}`).subscribe({
      next: (hotel) => {
        this.hotel = hotel;
        this.cdr.detectChanges();
      },
      error: () => { this.cdr.detectChanges(); }
    });

    // Load rooms
    this.apiService.get<any[]>(`/hotels/${this.hotelId}/rooms`).subscribe({
      next: (rooms) => {
        this.rooms = rooms || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.rooms = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // ── Add Room ──────────────────────────────────────────────────
  toggleAddForm() {
    this.showAddForm = !this.showAddForm;
    this.newRoom = { type: '', price: 0, totalCount: 1 };
    this.addRoomError = '';
  }

  submitAddRoom() {
    if (!this.newRoom.type || this.newRoom.price <= 0 || this.newRoom.totalCount < 1) {
      this.addRoomError = 'Please fill all fields correctly.';
      return;
    }
    this.isAddingRoom = true;
    this.addRoomError = '';

    const payload = {
      hotelId: this.hotelId,
      type: this.newRoom.type,
      price: this.newRoom.price,
      totalCount: this.newRoom.totalCount
    };

    this.apiService.post('/hotels/rooms', payload).subscribe({
      next: () => {
        this.isAddingRoom = false;
        this.showAddForm = false;
        this.toastService.success('Room added successfully!');
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isAddingRoom = false;
        this.addRoomError = err.error?.message || 'Failed to add room.';
        this.cdr.detectChanges();
      }
    });
  }

  // ── Edit Room ─────────────────────────────────────────────────
  startEdit(room: any) {
    this.editingRoomId = room.roomId;
    this.editRoom = { price: room.price, totalCount: room.totalCount };
  }

  cancelEdit() {
    this.editingRoomId = null;
  }

  saveEdit(roomId: number) {
    this.isUpdatingRoom = true;
    this.apiService.put(`/hotels/rooms/${roomId}`, this.editRoom).subscribe({
      next: () => {
        this.isUpdatingRoom = false;
        this.editingRoomId = null;
        this.toastService.success('Room updated successfully!');
        this.loadData();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isUpdatingRoom = false;
        this.toastService.error(err.error?.message || 'Failed to update room.');
        this.cdr.detectChanges();
      }
    });
  }
}
