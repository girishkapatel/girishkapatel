import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MainComponent } from './main.component';

const routes: Routes = [
  {
    path:'',
    component:MainComponent,
    children:[
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },  
      {
        path: 'dashboard',
        loadChildren: () => import('../../pages/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'manageaudits',
        loadChildren: () => import('../../pages/manageaudits/manageaudits.module').then(m => m.ManageauditsModule)
      },
      {
        path: 'myaudits',
        loadChildren: () => import('../../pages/myaudits/myaudits.module').then(m => m.MyauditsModule)
      },
      {
        path: 'scheduling',
        loadChildren: () => import('../../pages/scheduling/scheduling.module').then(m => m.SchedulingModule)
      },
      {
        path: 'workprograms',
        loadChildren: () => import('../../pages/workprograms/workprograms.module').then(m => m.WorkprogramsModule)
      },
      {
        path: 'users',
        loadChildren: () => import('../../pages/users/users.module').then(m => m.UsersModule)
      },
      {
        path: 'roles',
        loadChildren: () => import('../../pages/roles/roles.module').then(m => m.RolesModule)
      },
      {
        path: 'master',
        loadChildren: () => import('../../pages/master/master.module').then(m => m.MasterModule)
      }   
    ]
    
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class MainRoutingModule {}
