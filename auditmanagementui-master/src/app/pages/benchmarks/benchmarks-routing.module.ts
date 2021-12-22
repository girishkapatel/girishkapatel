import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { BenchmarksComponent } from './benchmarks.component';

const routes: Routes = [
  {
    path: '',
    component: BenchmarksComponent,
    data: {
      title: 'EY Benchmarks'
    }
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})

export class BenchmarksRoutingModule {}
