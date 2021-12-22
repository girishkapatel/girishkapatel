import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { WorkprogramsComponent } from './workprograms.component';
import { WorkprogramsAddComponent } from "./add/workprograms-add.component";
import { WorkprogramsEditComponent } from "./edit/workprograms-edit.component";
import { WorkprogramsViewComponent } from "./view/workprograms-view.component";

const routes: Routes = [
  {
    path: '',
    component: WorkprogramsComponent,
    data: {
      title: 'Workprograms'
    },
    children:[
      {
        path: '',
        component: WorkprogramsViewComponent
      },
      {
        path: 'add',
        component: WorkprogramsAddComponent
      },
      {
        path: 'edit',
        component: WorkprogramsEditComponent
      }  
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class WorkprogramsRoutingModule {}