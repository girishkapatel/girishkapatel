import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SchedulingComponent } from './scheduling.component';
import { ScheduleAddComponent } from "./add/schedule-add.component";
import { ScheduleEditComponent } from "./edit/schedule-edit.component";
import { ScheduleViewComponent } from "./view/schedule-view.component";

const routes: Routes = [
  {
    path: '',
    component: SchedulingComponent,
    data: {
      title: 'Scheduling'
    },
    children:[
      {
        path: '',
        component: ScheduleViewComponent
      },
      {
        path: 'add',
        component: ScheduleAddComponent
      },
      {
        path: 'edit',
        component: ScheduleEditComponent
      }  
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class SchedulingRoutingModule {}