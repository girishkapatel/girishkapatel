import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { LibraryComponent } from "./library.component";
import { ControlsComponent } from "../master/controls/controls.component";
import { RisksComponent } from "../master/risks/risks.component";
import { DatarequestComponent } from "../master/datarequest/datarequest.component";
import { ProcessUniverseComponent } from "../master/processuniverse/processuniverse.component";
import { RoleGaurd } from "src/app/services/auth/auth.gaurd";
// import { BenchmarksComponent } from '../master/benchmarks/benchmarks.component';

const routes: Routes = [
  {
    path: "",
    component: LibraryComponent,
    data: {
      title: "Knowledge Library",
    },
    children: [
      {
        path: "",
        redirectTo: "racm",
        pathMatch: "full",
      },
      {
        path: "racm",
        component: RisksComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "controls",
        component: ControlsComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "datarequest",
        component: DatarequestComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "processuniverse",
        component: ProcessUniverseComponent,
        canActivate: [RoleGaurd],
      },

      // ,
      // {
      //   path:'benchmarks',
      //   component:BenchmarksComponent
      // }
    ],
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LibraryRoutingModule {}
