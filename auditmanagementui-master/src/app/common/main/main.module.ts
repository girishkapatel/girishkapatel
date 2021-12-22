import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DashboardModule } from "../../pages/dashboard/dashboard.module";
import { ManageauditsModule } from "../../pages/manageaudits/manageaudits.module";
import { ChartsComponent } from "../../common/charts/charts.component";
import { MainComponent } from "./main.component";
import { MainRoutingModule } from "./main-routing.module";
import { ToastrModule } from "ngx-toastr";

@NgModule({
  imports: [DashboardModule, ManageauditsModule, MainRoutingModule, ToastrModule.forRoot({
    timeOut: 10000,
    positionClass: 'toast-bottom-right',
    closeButton: true,
  }),],
  declarations: [MainComponent, ChartsComponent],
})
export class MainModule {}
