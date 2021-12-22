import { NgModule, OnInit } from '@angular/core';
import { Routes, RouterModule, RouterStateSnapshot, PreloadingStrategy, PreloadAllModules } from '@angular/router';
import { MainComponent } from './common/main/main.component';
import { LoginComponent } from './login/login.component';
import { RoleGaurd } from './services/auth/auth.gaurd';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
   {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'resetpassword',
    component: ResetPasswordComponent,
  },
  {
    path:'pages',
    component:MainComponent,
    children:[
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },  
      {
        path: 'dashboard',
        loadChildren: () => import('./pages/dashboard/dashboard.module').then(m => m.DashboardModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'manageaudits',
        loadChildren: () => import('./pages/manageaudits/manageaudits.module').then(m => m.ManageauditsModule),
        //canActivate:[RoleGaurd]
      },
      {
        path: 'reports',
        loadChildren: () => import('./pages/reports/reports.module').then(m => m.ReportsModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'users',
        loadChildren: () => import('./pages/users/users.module').then(m => m.UsersModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'roles',
        loadChildren: () => import('./pages/roles/roles.module').then(m => m.RolesModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'auditplanningengine',
        loadChildren: () => import('./pages/master/master.module').then(m => m.MasterModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'knowledgelibrary',
        loadChildren: () => import('./pages/library/library.module').then(m => m.LibraryModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'action',
        loadChildren: () => import('./pages/action/action.module').then(m => m.ActionModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'eybenchmarks',
        loadChildren: () => import('./pages/benchmarks/benchmarks.module').then(m => m.BenchmarksModule),
        canActivate:[RoleGaurd]
      },
      {
        path: 'master',
        loadChildren: () => import('./pages/standardmaster/standardmaster.module').then(m => m.StandardMasterModule),
        canActivate:[RoleGaurd]
      }   
      
    ]
    
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes,
    {preloadingStrategy:PreloadAllModules}
    )],
  exports: [RouterModule]
})
export class AppRoutingModule{

}
