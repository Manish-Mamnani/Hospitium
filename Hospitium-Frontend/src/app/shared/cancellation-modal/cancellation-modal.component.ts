import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CancellationService } from '../../core/cancellation.service';

@Component({
  selector: 'app-cancellation-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cancellation-modal.component.html',
  styleUrl: './cancellation-modal.component.css'
})
export class CancellationModalComponent {
  constructor(public cancellationService: CancellationService) {}
}
