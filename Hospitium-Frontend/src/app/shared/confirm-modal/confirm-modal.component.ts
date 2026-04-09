import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from '../../core/confirm.service';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (confirmService.isOpen()) {
      <div class="fixed inset-0 z-[999999] flex items-center justify-center p-4 pointer-events-auto overflow-y-auto overflow-x-hidden">
        <!-- Backdrop -->
        <div (click)="confirmService.handleAction(false)"
             class="absolute inset-0 bg-black/60 backdrop-blur-md animate-fade-in"></div>
      
      <!-- Modal Body -->
      <div class="relative bg-white rounded-3xl shadow-2xl w-full max-w-md overflow-hidden animate-zoom-in border border-white/20">
        <div class="px-8 pt-10 pb-8 text-center">
          <!-- Icon Header -->
          <div [ngClass]="{
            'bg-primary/10 text-primary': confirmService.config()?.type === 'primary',
            'bg-rose-50 text-rose-600': confirmService.config()?.type === 'danger',
            'bg-green-50 text-green-600': confirmService.config()?.type === 'success'
          }" class="w-20 h-20 rounded-3xl flex items-center justify-center mx-auto mb-6 shadow-sm">
            <svg *ngIf="confirmService.config()?.type === 'danger'" class="w-10 h-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
            <svg *ngIf="confirmService.config()?.type === 'success'" class="w-10 h-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
            </svg>
            <svg *ngIf="confirmService.config()?.type === 'primary'" class="w-10 h-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>

          <h3 class="text-2xl font-bold text-neutral-primary mb-3">{{ confirmService.config()?.title }}</h3>
          <p class="text-neutral-secondary font-medium leading-relaxed px-4">
            {{ confirmService.config()?.message }}
          </p>
        </div>

        <!-- Actions -->
        <div class="bg-neutral-bg/30 px-8 py-6 flex gap-3 border-t border-neutral-border">
          <button (click)="confirmService.handleAction(false)"
                  class="flex-1 px-4 py-3.5 bg-white border border-neutral-border text-neutral-secondary rounded-2xl text-sm font-bold hover:bg-neutral-bg transition-all shadow-sm">
            {{ confirmService.config()?.cancelText }}
          </button>
          
          <button (click)="confirmService.handleAction(true)"
                  [ngClass]="{
                    'bg-primary hover:bg-primary-hover shadow-primary/20': confirmService.config()?.type === 'primary',
                    'bg-rose-600 hover:bg-rose-700 shadow-rose-200': confirmService.config()?.type === 'danger',
                    'bg-green-600 hover:bg-green-700 shadow-green-200': confirmService.config()?.type === 'success'
                  }"
                  class="flex-1 px-4 py-3.5 text-white rounded-2xl text-sm font-bold transition-all shadow-lg active:scale-[0.98]">
            {{ confirmService.config()?.confirmText }}
          </button>
        </div>
      </div>
    </div>
    }
  `,
  styles: [`
    .animate-zoom-in {
      animation: zoomIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
    }
    @keyframes zoomIn {
      from { opacity: 0; transform: scale(0.95) translateY(10px); }
      to { opacity: 1; transform: scale(1) translateY(0); }
    }
  `]
})
export class ConfirmModalComponent {
  constructor(public confirmService: ConfirmService) {}
}
