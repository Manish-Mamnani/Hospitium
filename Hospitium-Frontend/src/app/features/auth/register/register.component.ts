import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterComponent {
  user = { email: '', password: '', name: '', role: 'User' };
  errorMessage = '';
  isLoading = false;
  showPassword = false;

  // Touched state for showing errors only after field interaction
  touched = { name: false, email: false, password: false };

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  // ── Validation getters ──────────────────────────────────────────
  get nameError(): string {
    if (!this.touched.name) return '';
    const v = this.user.name.trim();
    if (!v) return 'Full name is required.';
    if (v.length < 2) return 'Name must be at least 2 characters.';
    if (v.length > 80) return 'Name must be under 80 characters.';
    if (!/^[a-zA-Z\s'-]+$/.test(v)) return 'Name can only contain letters, spaces, hyphens, or apostrophes.';
    return '';
  }

  get emailError(): string {
    if (!this.touched.email) return '';
    const v = this.user.email.trim();
    if (!v) return 'Email is required.';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v)) return 'Enter a valid email address.';
    return '';
  }

  get passwordError(): string {
    if (!this.touched.password) return '';
    const v = this.user.password;
    if (!v) return 'Password is required.';
    if (v.length < 8) return 'Password must be at least 8 characters.';
    if (/\s/.test(v)) return 'Password cannot contain spaces.';
    if (!/[A-Z]/.test(v)) return 'Include at least one uppercase letter.';
    if (!/[0-9]/.test(v)) return 'Include at least one number.';
    return '';
  }

  get passwordStrength(): number {
    const p = this.user.password;
    let score = 0;
    if (p.length >= 8) score++;
    if (p.length >= 12) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    return score; // 0–5
  }

  get strengthLabel(): string {
    const s = this.passwordStrength;
    if (s <= 1) return 'Very weak';
    if (s === 2) return 'Weak';
    if (s === 3) return 'Fair';
    if (s === 4) return 'Strong';
    return 'Very strong';
  }

  get strengthColor(): string {
    const s = this.passwordStrength;
    if (s <= 1) return 'bg-red-500';
    if (s === 2) return 'bg-orange-400';
    if (s === 3) return 'bg-yellow-400';
    if (s === 4) return 'bg-green-400';
    return 'bg-green-600';
  }

  get isFormValid(): boolean {
    return !this.nameError && !this.emailError && !this.passwordError &&
           !!this.user.name && !!this.user.email && !!this.user.password;
  }

  touch(field: 'name' | 'email' | 'password') {
    this.touched[field] = true;
  }

  touchAll() {
    this.touched = { name: true, email: true, password: true };
  }

  onSubmit() {
    this.touchAll();
    if (!this.isFormValid || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.user).subscribe({
      next: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }
}
