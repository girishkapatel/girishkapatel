import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkprogramsComponent } from "./workprograms.component";
import { WorkprogramsAddComponent } from "./add/workprograms-add.component";
import { WorkprogramsEditComponent } from "./edit/workprograms-edit.component";
import { WorkprogramsViewComponent } from "./view/workprograms-view.component";
import { WorkprogramsRoutingModule } from './workprograms-routing.module';
import { TableModule } from "../../common/table/table.module";

@NgModule({
    imports: [
        WorkprogramsRoutingModule,
        TableModule,
        CommonModule        
    ],
    declarations: [
        WorkprogramsComponent,
        WorkprogramsEditComponent,
        WorkprogramsAddComponent,
        WorkprogramsViewComponent
    ]
})

export class WorkprogramsModule {}