import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { ActionComponent } from "./action.component";
import { ActionAddComponent } from "./add/action-add.component";
import { ActionEditComponent } from "./edit/action-edit.component";
import { ActionViewComponent } from "./view/action-view.component";
import { ActionRoutingModule } from "./action-routing.module";
import { TableModule } from "../../common/table/table.module";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { CKEditorModule } from "@ckeditor/ckeditor5-angular";
import { HighchartsChartModule } from "highcharts-angular";
import { ChartsModule } from "../../common/charts/charts.module";
import { NgMultiSelectDropDownModule } from "ng-multiselect-dropdown";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";

@NgModule({
  imports: [
    ActionRoutingModule,
    TableModule,
    CommonModule,
    CKEditorModule,
    FormsModule,
    NgbModule,
    HighchartsChartModule,
    ChartsModule,
    NgMultiSelectDropDownModule,
    NgxSpinnerModule,
  ],
  declarations: [
    ActionComponent,
    ActionEditComponent,
    ActionAddComponent,
    ActionViewComponent,
  ],
  providers: [NgxSpinnerService],
})
export class ActionModule {}
