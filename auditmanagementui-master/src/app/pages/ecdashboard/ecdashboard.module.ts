import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ECDashboardComponent } from "./ecdashboard.component";
import { ChartsModule } from "../../common/charts/charts.module";
import { TableModule } from "../../common/table/table.module";
import { ECDashboardRoutingModule } from './ecdashboard-routing.module';

@NgModule({
    imports: [
        ECDashboardRoutingModule,
        CommonModule,
        ChartsModule,
        TableModule        
    ],
    declarations: [
        ECDashboardComponent
    ]
})

export class ECDashboardModule {}