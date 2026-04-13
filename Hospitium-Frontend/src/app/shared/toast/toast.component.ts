import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../core/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css'
})
export class ToastComponent implements OnInit, OnDestroy {
  toasts: ToastMessage[] = [];
  private subscription!: Subscription;

  constructor(private toastService: ToastService) {}

  ngOnInit() {
    this.subscription = this.toastService.toastState$.subscribe(toast => {
      this.toasts = [...this.toasts, toast];
      setTimeout(() => this.removeToast(toast), 5000);
    });
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  remove(index: number) {
    this.toasts = this.toasts.filter((_, i) => i !== index);
  }

  private removeToast(toast: ToastMessage) {
    this.toasts = this.toasts.filter(t => t !== toast);
  }

  getToastClass(type: string): string {
    if (type === 'success') return 'border-green-500';
    if (type === 'error') return 'border-red-500';
    return 'border-blue-500';
  }
}
