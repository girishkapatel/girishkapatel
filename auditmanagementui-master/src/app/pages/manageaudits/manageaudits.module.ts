import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { ManageauditsComponent } from "./manageaudits.component";
import { ManageauditsAddComponent } from "./add/manageaudits-add.component";
import { ManageauditsEditComponent } from "./edit/manageaudits-edit.component";
import { ManageauditsViewComponent } from "./view/manageaudits-view.component";
import { ManageauditsRoutingModule } from "./manageaudits-routing.module";
import { TorComponent } from "./components/tor/tor.component";
import { ActivityComponent } from "./components/activity/activity.component";
import { AuditMemoComponent } from "./components/auditmemo/auditmemo.component";
import { AuditReportComponent } from "./components/auditreport/auditreport.component";
import { AuditReportFinalComponent } from "./components/auditreportfinal/auditreportfinal.component";
import { DataTrackerComponent } from "./components/datatracker/datatracker.component";
import { WalkthroughComponent } from "./components/walkthrough/walkthrough.component";
import { DiscussionNoteComponent } from "./components/discussionnote/discussionnote.component";
import { FollowupComponent } from "./components/followup/followup.component";
import { TestingComponent } from "./components/testing/testing.component";
import { ClosureComponent } from "./components/closure/closure.component";
import { EyBenchmarkComponent } from "./components/eybenchmark/eybenchmark.component";
import { TableModule } from "./../../common/table/table.module";
import { ChartsModule } from "../../common/charts/charts.module";
import { UnplannedAuditComponent } from "./components/unplannedaudit/unplanned.component";
import { UnplannedAuditAddComponent } from "./addunplannedaudit/unplannedaudit-add.component";
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgMultiSelectDropDownModule } from "ng-multiselect-dropdown";
import { CKEditorModule } from "@ckeditor/ckeditor5-angular";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";

@NgModule({
  imports: [
    ManageauditsRoutingModule,
    CommonModule,
    ChartsModule,
    TableModule,
    CKEditorModule,
    FormsModule,
    NgbModule,
    NgMultiSelectDropDownModule,
    NgxSpinnerModule,
  ],
  declarations: [
    ManageauditsComponent,
    ManageauditsAddComponent,
    ManageauditsEditComponent,
    ManageauditsViewComponent,
    TorComponent,
    ActivityComponent,
    AuditMemoComponent,
    AuditReportComponent,
    AuditReportFinalComponent,
    DataTrackerComponent,
    WalkthroughComponent,
    TestingComponent,
    DiscussionNoteComponent,
    FollowupComponent,
    ClosureComponent,
    EyBenchmarkComponent,
    UnplannedAuditComponent,
    UnplannedAuditAddComponent,
  ],
  providers: [NgxSpinnerService],
})
export class ManageauditsModule {}
