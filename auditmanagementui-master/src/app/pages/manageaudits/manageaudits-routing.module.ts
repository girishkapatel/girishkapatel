import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { ManageauditsComponent } from "./manageaudits.component";
import { ManageauditsAddComponent } from "./add/manageaudits-add.component";
import { ManageauditsEditComponent } from "./edit/manageaudits-edit.component";
import { ManageauditsViewComponent } from "./view/manageaudits-view.component";
import { UnplannedAuditComponent } from "./components/unplannedaudit/unplanned.component";
import { UnplannedAuditAddComponent } from "./addunplannedaudit/unplannedaudit-add.component";
import { RoleGaurd } from "../../services/auth/auth.gaurd";

const routes: Routes = [
  {
    path: "",
    component: ManageauditsComponent,
    data: {
      title: "Manage Audits",
    },
    children: [
      {
        path: "",
        component: ManageauditsViewComponent,
        //canActivate:[RoleGaurd]
      },
      {
        path: "add",
        component: ManageauditsAddComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "edit",
        component: ManageauditsEditComponent,
        
      },
      { path: 'edit/:id', component: ManageauditsEditComponent },
      {
        path: "unplannedaudit",
        component: UnplannedAuditComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "addunplannedaudit",
        component: UnplannedAuditAddComponent,
        //canActivate: [RoleGaurd],
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ManageauditsRoutingModule {}
