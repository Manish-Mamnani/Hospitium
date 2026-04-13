import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth.service';
import { CommonModule } from '@angular/common';

type Step = 'email' | 'otp' | 'password' | 'done';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPasswordComponent {
  step: Step = 'email';

  // Form fields
  email = '';
  otp = '';
  newPassword = '';
  confirmPassword = '';
  showPassword = false;
  showConfirm = false;

  // UI state
  isLoading = false;
  errorMessage = '';
  touched = { email: false, otp: false, password: false, confirm: false };

  // Resend OTP cooldown
  resendCooldown = 0;
  private resendTimer: any;

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  // ── Validation ─────────────────────────────────────────────────
  get emailError(): string {
    if (!this.touched.email) return '';
    if (!this.email.trim()) return 'Email is required.';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim())) return 'Enter a valid email address.';
    return '';
  }

  get otpError(): string {
    if (!this.touched.otp) return '';
    if (!this.otp.trim()) return 'OTP is required.';
    if (!/^\d{6}$/.test(this.otp.trim())) return 'OTP must be 6 digits.';
    return '';
  }

  get passwordError(): string {
    if (!this.touched.password) return '';
    if (!this.newPassword) return 'Password is required.';
    if (this.newPassword.length < 8) return 'At least 8 characters.';
    if (/\s/.test(this.newPassword)) return 'No spaces allowed.';
    if (!/[A-Z]/.test(this.newPassword)) return 'Include at least one uppercase letter.';
    if (!/[0-9]/.test(this.newPassword)) return 'Include at least one number.';
    return '';
  }

  get confirmError(): string {
    if (!this.touched.confirm) return '';
    if (!this.confirmPassword) return 'Please confirm your password.';
    if (this.newPassword !== this.confirmPassword) return 'Passwords do not match.';
    return '';
  }

  get passwordStrength(): number {
    const p = this.newPassword;
    let score = 0;
    if (p.length >= 8) score++;
    if (p.length >= 12) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    return score;
  }

  get strengthLabel(): string {
    const s = this.passwordStrength;
    if (s <= 1) return 'Very weak'; if (s === 2) return 'Weak';
    if (s === 3) return 'Fair'; if (s === 4) return 'Strong'; return 'Very strong';
  }

  get strengthColor(): string {
    const s = this.passwordStrength;
    if (s <= 1) return 'bg-red-500'; if (s === 2) return 'bg-orange-400';
    if (s === 3) return 'bg-yellow-400'; if (s === 4) return 'bg-green-400'; return 'bg-green-600';
  }

  touch(field: keyof typeof this.touched) { this.touched[field] = true; }

  // ── Step 1 — Request OTP ───────────────────────────────────────
  submitEmail() {
    this.touched.email = true;
    if (this.emailError || this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.forgotPassword(this.email.trim()).subscribe({
      next: () => {
        this.isLoading = false;
        this.step = 'otp';
        this.startResendCooldown();
        this.cdr.detectChanges();
      },
      error: () => {
        // Always show success to prevent email enumeration
        this.isLoading = false;
        this.step = 'otp';
        this.startResendCooldown();
        this.cdr.detectChanges();
      }
    });
  }

  // ── Step 2 — Verify OTP ────────────────────────────────────────
  submitOtp() {
    this.touched.otp = true;
    if (this.otpError || this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.verifyOtp(this.email.trim(), this.otp.trim()).subscribe({
      next: () => {
        this.isLoading = false;
        this.step = 'password';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid or expired OTP. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  resendOtp() {
    if (this.resendCooldown > 0) return;
    this.authService.forgotPassword(this.email.trim()).subscribe({
      next: () => this.startResendCooldown(),
      error: () => this.startResendCooldown()
    });
  }

  private startResendCooldown() {
    this.resendCooldown = 60;
    clearInterval(this.resendTimer);
    this.resendTimer = setInterval(() => {
      this.resendCooldown--;
      if (this.resendCooldown <= 0) clearInterval(this.resendTimer);
    }, 1000);
  }

  // ── Step 3 — Reset Password ────────────────────────────────────
  submitPassword() {
    this.touched.password = true;
    this.touched.confirm = true;
    if (this.passwordError || this.confirmError || this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.resetPassword(this.email.trim(), this.otp.trim(), this.newPassword).subscribe({
      next: () => {
        this.isLoading = false;
        this.step = 'done';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to reset password. Please restart the process.';
        this.cdr.detectChanges();
      }
    });
  }
}
