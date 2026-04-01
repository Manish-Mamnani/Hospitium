import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MyHotelsComponent } from './my-hotels/my-hotels';
import { AddHotelComponent } from './add-hotel/add-hotel';
import { ManageRoomsComponent } from './manage-rooms/manage-rooms';
import { EditHotelComponent } from '../../shared/edit-hotel/edit-hotel';
import { MyBookingsComponent } from './my-bookings/my-bookings';

const routes: Routes = [
  { path: '', component: MyHotelsComponent },
  { path: 'add-hotel', component: AddHotelComponent },
  { path: 'manage-rooms/:hotelId', component: ManageRoomsComponent },
  { path: 'hotels/:id/edit', component: EditHotelComponent },
  { path: 'bookings', component: MyBookingsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ManagerRoutingModule {}
