import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/api.service';
import { Sidebar } from '../../../shared/sidebar/sidebar';

@Component({
  selector: 'app-admin-managers',
  standalone: true,
  imports: [CommonModule, Sidebar],
  templateUrl: './admin-managers.html',
  styleUrl: './admin-managers.css',
})
export class AdminManagersComponent implements OnInit {
  managers: any[] = [];
  isLoading = false;

  constructor(private apiService: ApiService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadManagers();
  }

  loadManagers() {
    this.isLoading = true;
    // Assuming backend has an endpoint for this, or we filter users by role
    this.apiService.get<any[]>('/users/managers').subscribe({
      next: (data) => {
        this.managers = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
