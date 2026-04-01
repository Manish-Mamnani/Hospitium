import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-notification-list',
  imports: [CommonModule],
  templateUrl: './notification-list.html',
  styleUrl: './notification-list.css',
})
export class NotificationListComponent implements OnInit {
  notifications: any[] = [];

  ngOnInit() {
    // Mock notifications - in real app, fetch from API
    this.notifications = [
      {
        id: 1,
        message: 'Your booking for Hotel Paradise has been confirmed.',
        type: 'booking',
        date: new Date(),
        read: false
      },
      {
        id: 2,
        message: 'Hotel Royal Palace has been approved.',
        type: 'approval',
        date: new Date(Date.now() - 86400000),
        read: true
      }
    ];
  }

  markAsRead(id: number) {
    const notification = this.notifications.find(n => n.id === id);
    if (notification) {
      notification.read = true;
    }
  }
}
