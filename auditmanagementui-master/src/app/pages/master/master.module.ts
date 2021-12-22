import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { MasterComponent } from "./master.component";
import { MasterRoutingModule } from "./master-routing.module";

// import { CompanyComponent } from './company/company.component';
// import { LocationComponent } from './location/location.component';
import { AuditPlanComponent } from "./auditplans/auditplans.component";
import { OverallAssessmentComponent } from "./overallassessment/overallassessment.component";
import { TableModule } from "../../common/table/table.module";

import { KeybusinessinitiativeComponent } from "./keybusinessinitiative/keybusinessinitiative.component";
import { ProcessriskmappingComponent } from "./processriskmapping/processriskmapping.component";
// import { TrialbalanceComponent } from './trialbalance/trialbalance.component';
import { CoverageComponent } from "./coverage/coverage.component";
import { ErmrisksComponent } from "./ermrisks/ermrisks.component";
import { CKEditorModule } from "@ckeditor/ckeditor5-angular";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { BusinessProcessService } from "./process/businessprocess/businessprocess.service";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import{NgMultiSelectDropDownModule}from "ng-multiselect-dropdown"

// import { PlmapComponent } from './processlocationmapping/processlocationmapping.component';
@NgModule({
  imports: [
    CommonModule,
    MasterRoutingModule,
    CKEditorModule,
    FormsModule,
    TableModule,
    NgxSpinnerModule,
    NgbModule,
    NgMultiSelectDropDownModule
  ],
  declarations: [
    MasterComponent,
    // CompanyComponent,
    // LocationComponent,
    AuditPlanComponent,
    OverallAssessmentComponent,
    KeybusinessinitiativeComponent,
    ProcessriskmappingComponent,
    // TrialbalanceComponent,
    CoverageComponent,
    ErmrisksComponent,
    // PlmapComponent
  ],
  providers: [NgxSpinnerService, BusinessProcessService],
})
export class MasterModule {}
