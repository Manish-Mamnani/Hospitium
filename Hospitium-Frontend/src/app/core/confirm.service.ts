import { Injectable, signal } from '@angular/core';

export interface ConfirmConfig {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'primary' | 'danger' | 'success';
}

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  private resolveCallback: ((value: boolean) => void) | null = null;
  
  config = signal<ConfirmConfig | null>(null);
  isOpen = signal(false);

  confirm(config: ConfirmConfig): Promise<boolean> {
    return new Promise((resolve) => {
      this.config.set({
        confirmText: 'Confirm',
        cancelText: 'Cancel',
        type: 'primary',
        ...config
      });
      this.resolveCallback = resolve;
      this.isOpen.set(true);
    });
  }

  handleAction(result: boolean) {
    this.isOpen.set(false);
    if (this.resolveCallback) {
      this.resolveCallback(result);
      this.resolveCallback = null;
    }
  }
}
