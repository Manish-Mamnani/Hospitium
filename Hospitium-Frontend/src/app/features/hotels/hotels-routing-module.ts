import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { SearchResultsComponent } from './search-results/search-results.component';
import { HotelDetailsComponent } from './hotel-details/hotel-details.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'search', component: SearchResultsComponent },
  { path: ':id', component: HotelDetailsComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HotelsRoutingModule {}
