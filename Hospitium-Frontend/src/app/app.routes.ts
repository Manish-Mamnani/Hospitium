import { Routes } from '@angular/router';
import { AuthGuard } from './core/auth.guard';
import { RoleGuard } from './core/role.guard';
import { ManagerGuard } from './core/manager.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { HomeComponent } from './features/hotels/home/home.component';
import { SearchResultsComponent } from './features/hotels/search-results/search-results.component';
import { HotelDetailsComponent } from './features/hotels/hotel-details/hotel-details.component';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  { path: 'auth/forgot-password', component: ForgotPasswordComponent },
  { path: 'home', component: HomeComponent },
  { path: 'hotels/search', component: SearchResultsComponent },
  { path: 'hotels/:id', component: HotelDetailsComponent },
  {
    path: 'bookings',
    loadChildren: () => import('./features/bookings/bookings-module').then(m => m.BookingsModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'manager',
    loadChildren: () => import('./features/manager/manager-module').then(m => m.ManagerModule),
    canActivate: [ManagerGuard]
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin-module').then(m => m.AdminModule),
    canActivate: [RoleGuard]
  }
];
