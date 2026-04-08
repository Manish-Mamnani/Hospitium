import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent implements OnInit {
  credentials = { email: '', password: '' };
  errorMessage = '';
  isLoading = false;
  showPassword = false;

  // Touched state — errors only appear after user has interacted with a field
  touched = { email: false, password: false };

  returnUrl: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '';
  }

  // ── Validation getters ──────────────────────────────────────────
  get emailError(): string {
    if (!this.touched.email) return '';
    const v = this.credentials.email.trim();
    if (!v) return 'Email is required.';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v)) return 'Enter a valid email address.';
    return '';
  }

  get passwordError(): string {
    if (!this.touched.password) return '';
    if (!this.credentials.password) return 'Password is required.';
    if (this.credentials.password.length < 8) return 'Password must be at least 8 characters.';
    if (/\s/.test(this.credentials.password)) return 'Password cannot contain spaces.';
    return '';
  }

  get isFormValid(): boolean {
    return !this.emailError && !this.passwordError &&
           !!this.credentials.email && !!this.credentials.password;
  }

  touch(field: 'email' | 'password') {
    this.touched[field] = true;
  }

  touchAll() {
    this.touched = { email: true, password: true };
  }

  onSubmit() {
    this.touchAll();
    if (!this.isFormValid || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.credentials, this.returnUrl).subscribe({
      next: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.message ||
          (err.status === 401 ? 'Invalid email or password.' : 'Login failed. Please try again.');
        this.cdr.detectChanges();
      }
    });
  }
}
