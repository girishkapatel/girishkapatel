import { NgModule } from '@angular/core';
import { ApprovalsComponent } from "./approvals.component";
import { ApprovalsRoutingModule } from './approvals-routing.module';
import { ChartsModule } from "../../common/charts/charts.module";
import { TableModule } from './../../common/table/table.module';

@NgModule({
    imports: [
        ApprovalsRoutingModule,
        ChartsModule,
        TableModule        
    ],
    declarations: [
        ApprovalsComponent
    ]
})

export class ApprovalsModule {}