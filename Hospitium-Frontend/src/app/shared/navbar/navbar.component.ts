import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class NavbarComponent implements OnInit, OnDestroy {
  isAuthenticated = false;
  role: string | null = null;
  fullName: string = '';
  isProfileOpen = false;
  private authSubscription!: Subscription;

  constructor(
    private authService: AuthService, 
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.authSubscription = this.authService.authState$.subscribe((userInfo) => {
      this.isAuthenticated = userInfo.isAuthenticated;
      this.role = userInfo.role;
      this.fullName = userInfo.fullName || 'Guest';
      setTimeout(() => this.cdr.markForCheck());
    });
  }

  ngOnDestroy() {
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  updateAuthStatus() {
    this.isAuthenticated = this.authService.isAuthenticated();
    this.role = this.authService.getRole();
    this.fullName = this.authService.getFullName() || 'Guest';
    this.cdr.detectChanges();
  }

  toggleProfile() {
    this.isProfileOpen = !this.isProfileOpen;
  }

  logout() {
    this.authService.logout();
    this.updateAuthStatus();
    this.router.navigate(['/']);
  }
}
