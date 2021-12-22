import { NgModule } from '@angular/core';
import { ReportsComponent } from "./reports.component";
import { ReportsRoutingModule } from './reports-routing.module';
import { ChartsModule } from "../../common/charts/charts.module";
import { TableModule } from './../../common/table/table.module';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";


@NgModule({
    imports: [
        ReportsRoutingModule,
        ChartsModule,
        TableModule,
        FormsModule,
        CommonModule,
        NgxSpinnerModule
    ],
    declarations: [
        ReportsComponent
    ],
  providers: [NgxSpinnerService],

})

export class ReportsModule {}