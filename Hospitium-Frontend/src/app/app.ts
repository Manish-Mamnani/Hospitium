import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar/navbar.component';
import { ToastComponent } from './shared/toast/toast.component';
import { ConfirmModalComponent } from './shared/confirm-modal/confirm-modal.component';
import { CancellationModalComponent } from './shared/cancellation-modal/cancellation-modal.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, ToastComponent, ConfirmModalComponent, CancellationModalComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Hospitium-Frontend');
}
