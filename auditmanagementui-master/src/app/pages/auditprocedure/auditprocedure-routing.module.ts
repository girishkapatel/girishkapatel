import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { AuditProcedureComponent } from "./auditprocedure.component";
import { AuditProcedureAddComponent } from "./add/auditprocedure-add.component";
import { AuditProcedureEditComponent } from "./edit/auditprocedure-edit.component";
import { AuditProcedureViewComponent } from "./view/auditprocedure-view.component";

const routes: Routes = [
  {
    path: "",
    component: AuditProcedureComponent,
    data: {
      title: "Audit Procedure",
    },
    children: [
      {
        path: "",
        component: AuditProcedureViewComponent,
      },
      {
        path: "add",
        component: AuditProcedureAddComponent,
      },
      {
        path: "edit",
        component: AuditProcedureEditComponent,
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AuditProcedureRoutingModule {}
