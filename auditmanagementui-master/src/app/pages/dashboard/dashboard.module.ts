import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { DashboardComponent } from "./dashboard.component";
import { ChartsModule } from "../../common/charts/charts.module";
import { TableModule } from "../../common/table/table.module";
import { DashboardRoutingModule } from "./dashboard-routing.module";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
@NgModule({
  imports: [
    DashboardRoutingModule,
    CommonModule,
    ChartsModule,
    TableModule,
    FormsModule,
    NgxSpinnerModule,
  ],
  declarations: [DashboardComponent],
  providers: [NgxSpinnerService],
})
export class DashboardModule {}
