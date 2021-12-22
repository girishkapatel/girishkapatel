import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchedulingComponent } from "./scheduling.component";
import { ScheduleAddComponent } from "./add/schedule-add.component";
import { ScheduleEditComponent } from "./edit/schedule-edit.component";
import { ScheduleViewComponent } from "./view/schedule-view.component";
import { SchedulingRoutingModule } from './scheduling-routing.module';
import { TableModule } from "../../common/table/table.module";

@NgModule({
    imports: [
        SchedulingRoutingModule,
        TableModule,
        CommonModule        
    ],
    declarations: [
        SchedulingComponent,
        ScheduleEditComponent,
        ScheduleAddComponent,
        ScheduleViewComponent
    ]
})

export class SchedulingModule {}