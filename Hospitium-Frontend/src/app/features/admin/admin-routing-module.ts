import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard';
import { ApprovalQueueComponent } from './approval-queue/approval-queue';
import { GlobalBookingsComponent } from './global-bookings/global-bookings';
import { AdminHotelsComponent } from './hotels/admin-hotels';
import { AdminManagersComponent } from './managers/admin-managers';
import { EditHotelComponent } from '../../shared/edit-hotel/edit-hotel';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'approvals', component: ApprovalQueueComponent },
  { path: 'global-bookings', component: GlobalBookingsComponent },
  { path: 'hotels', component: AdminHotelsComponent },
  { path: 'managers', component: AdminManagersComponent },
  { path: 'hotels/:id/edit', component: EditHotelComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}
