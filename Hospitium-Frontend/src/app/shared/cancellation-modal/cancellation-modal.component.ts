import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CancellationService } from '../../core/cancellation.service';

@Component({
  selector: 'app-cancellation-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="cancellationService.isOpen()" 
         class="fixed inset-0 z-[10000] flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div (click)="cancellationService.handleAction(false)"
           class="absolute inset-0 bg-black/40 backdrop-blur-sm animate-fade-in"></div>
      
      <!-- Modal Body -->
      <div class="relative bg-white rounded-[2.5rem] shadow-2xl w-full max-w-lg overflow-hidden animate-zoom-in border border-white/20">
        <div class="px-10 pt-12 pb-10">
          <div class="flex items-start gap-6 mb-8">
             <div class="w-16 h-16 rounded-3xl bg-error-bg text-error-text flex items-center justify-center shrink-0 shadow-sm">
                <svg class="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
             </div>
             <div>
                <h2 class="text-2xl font-black text-neutral-primary tracking-tight">Confirm Cancellation</h2>
                <p class="text-neutral-secondary font-medium mt-1 leading-relaxed">
                  You are cancelling your booking for <span class="text-neutral-primary font-bold">{{ cancellationService.config()?.booking?.hotelName }}</span>.
                </p>
             </div>
          </div>

          <!-- Policy Details Card -->
          <div class="bg-neutral-bg/40 border border-neutral-border rounded-3xl p-6 mb-8">
             <div class="flex items-center gap-3 text-sm font-bold text-neutral-primary mb-6 uppercase tracking-widest opacity-80">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
                Cancellation Policy Applied
             </div>
             
             <p class="text-neutral-primary font-semibold text-base flex items-center gap-3">
                <span class="w-2 h-2 rounded-full bg-error-text"></span>
                {{ cancellationService.config()?.policy }}
             </p>

             <div class="grid grid-cols-2 gap-4 mt-8 pt-6 border-t border-neutral-border/50">
                <div>
                   <p class="text-[10px] font-black text-neutral-secondary uppercase tracking-widest mb-1">Refundable Amount</p>
                   <p class="text-2xl font-black text-primary">₹{{ cancellationService.config()?.refundable | number:'1.0-0' }}</p>
                </div>
                <div>
                   <p class="text-[10px] font-black text-neutral-secondary uppercase tracking-widest mb-1">Deduction</p>
                   <p class="text-2xl font-black text-error-text">₹{{ cancellationService.config()?.deduction | number:'1.0-0' }}</p>
                </div>
             </div>
          </div>

          <!-- Warning -->
          <p class="text-xs text-neutral-secondary italic text-center px-4 mb-4">
             Note: Refunds usually take 5-7 business days to reflect in your original payment method.
          </p>
        </div>

        <!-- Sticky Footer Actions -->
        <div class="bg-neutral-bg/30 px-10 py-8 flex gap-4 border-t border-neutral-border">
          <button (click)="cancellationService.handleAction(false)"
                  class="flex-1 px-6 py-4 bg-white border border-neutral-border text-neutral-secondary rounded-2xl text-sm font-bold hover:bg-neutral-bg transition-all shadow-sm">
            Keep Booking
          </button>
          
          <button (click)="cancellationService.handleAction(true)"
                  class="flex-1 px-6 py-4 bg-error-text text-white rounded-2xl text-sm font-bold transition-all shadow-lg shadow-error-text/20 hover:scale-[1.02] active:scale-[0.98]">
            Confirm Cancellation
          </button>
        </div>
      </div>
    </div>
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
export class CancellationModalComponent {
  constructor(public cancellationService: CancellationService) {}
}
