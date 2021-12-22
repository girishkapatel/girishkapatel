import { NgModule } from "@angular/core";
import { UsersComponent } from "./users.component";
import { UsersRoutingModule } from "./users-routing.module";
import { TableModule } from "../../common/table/table.module";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";

@NgModule({
  imports: [
    UsersRoutingModule,
    TableModule,
    CommonModule,
    NgxSpinnerModule,
    FormsModule,
  ],
  declarations: [UsersComponent],
  providers: [NgxSpinnerService],
})
export class UsersModule {}
