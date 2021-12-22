import { NgModule } from "@angular/core";
import { RolesComponent } from "./roles.component";
import { RolesRoutingModule } from "./roles-routing.module";
import { TableModule } from "../../common/table/table.module";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { UiSwitchModule } from "ngx-ui-switch";

@NgModule({
  imports: [
    RolesRoutingModule,
    TableModule,
    CommonModule,
    FormsModule,
    UiSwitchModule,
  ],
  declarations: [RolesComponent],
})
export class RolesModule {}
