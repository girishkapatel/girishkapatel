import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { AuditProcedureComponent } from "./auditprocedure.component";
import { AuditProcedureAddComponent } from "./add/auditprocedure-add.component";
import { AuditProcedureEditComponent } from "./edit/auditprocedure-edit.component";
import { AuditProcedureViewComponent } from "./view/auditprocedure-view.component";
import { AuditProcedureRoutingModule } from "./auditprocedure-routing.module";
import { TableModule } from "../../common/table/table.module";

@NgModule({
  imports: [AuditProcedureRoutingModule, TableModule, CommonModule],
  declarations: [
    AuditProcedureComponent,
    AuditProcedureEditComponent,
    AuditProcedureAddComponent,
    AuditProcedureViewComponent,
  ],
})
export class AuditProcedureModule {}
