import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

interface Notification {
  id: number;
  message: string;
  type: string;
  date: Date;
  read: boolean;
}

@Component({
  selector: 'app-notification-dropdown',
  imports: [CommonModule],
  templateUrl: './notification-dropdown.html',
  styleUrl: './notification-dropdown.css',
})
export class NotificationDropdown implements OnInit {
  notifications: Notification[] = [];
  isOpen = false;
  unreadCount = 0;

  constructor(private router: Router) {}

  ngOnInit() {
    // Mock notifications - in real app, fetch from API
    this.notifications = [
      {
        id: 1,
        message: 'Your booking for Hotel Paradise has been confirmed.',
        type: 'booking',
        date: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
        read: false
      },
      {
        id: 2,
        message: 'Hotel manager approved your booking request.',
        type: 'approval',
        date: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
        read: false
      },
      {
        id: 3,
        message: 'Welcome to Hospitium! Start exploring hotels.',
        type: 'system',
        date: new Date(Date.now() - 1000 * 60 * 60 * 24), // 1 day ago
        read: true
      }
    ];
    this.updateUnreadCount();
  }

  toggleDropdown() {
    this.isOpen = !this.isOpen;
  }

  markAsRead(id: number) {
    const notification = this.notifications.find(n => n.id === id);
    if (notification) {
      notification.read = true;
      this.updateUnreadCount();
    }
  }

  viewAll() {
    this.isOpen = false;
    this.router.navigate(['/notifications']);
  }

  private updateUnreadCount() {
    this.unreadCount = this.notifications.filter(n => !n.read).length;
  }
}
