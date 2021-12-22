import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MyauditsComponent } from './myaudits.component';

const routes: Routes = [
  {
    path: '',
    component: MyauditsComponent,
    data: {
      title: 'My Audits'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class MyauditsRoutingModule {}