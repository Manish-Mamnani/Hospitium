import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../core/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-24 right-5 z-[9999] flex flex-col gap-3 pointer-events-none" style="min-width:320px; max-width:420px;">
      <div *ngFor="let toast of toasts; let i = index"
           class="flex items-start gap-3 px-4 py-3 bg-white rounded-xl shadow-2xl pointer-events-auto animate-fade-in border-l-4"
           [ngClass]="getToastClass(toast.type)">
        <!-- Icon -->
        <div class="flex-shrink-0 mt-0.5">
          <svg *ngIf="toast.type === 'success'" class="h-5 w-5 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <svg *ngIf="toast.type === 'error'" class="h-5 w-5 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <svg *ngIf="toast.type === 'info'" class="h-5 w-5 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <!-- Message -->
        <p class="flex-1 text-sm font-medium text-gray-800 leading-snug">{{ toast.message }}</p>
        <!-- Close button -->
        <button (click)="remove(i)" class="flex-shrink-0 text-gray-400 hover:text-gray-600 transition-colors mt-0.5">
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
    </div>
  `
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
