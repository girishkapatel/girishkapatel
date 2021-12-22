import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { TableModule } from "../../common/table/table.module";
import { CountrystatecityComponent } from "../master/countrystatecity/countrystatecity.component";
import { CountryComponent } from "../master/country/country.component";
import { CityComponent } from "../master/city/city.component";
import { StateComponent } from "../master/state/state.component";
import { CityService } from "../master/city/city.service";
import { CountryService } from "../master/country/country.service";
import { StandardMasterComponent } from "./standardmaster.component";
import { StandardMasterRoutingModule } from "./standardmaster-routing.module";

import { CompanyComponent } from "../master/company/company.component";
import { LocationComponent } from "../master/location/location.component";
import { PlmapComponent } from "../master/processlocationmapping/processlocationmapping.component";
import { TrialbalanceComponent } from "../master/trialbalance/trialbalance.component";
import { RiskTypeComponent } from "../master/risktype/risktype.component";
import { ReportConsiderationComponent } from "../master/reportconsideration/reportconsideration.component";
import { RecommendationComponent } from "../master/recommendation/recommendation.component";
import { RootCauseComponent } from "../master/rootcause/rootcause.component";
import { ImpactComponent } from "../master/impact/impact.component";

import { ProcessComponent } from "../master/process/process.component";
import { SectorComponent } from "../master/process/sector/sector.component";
import { BusinessProcessComponent } from "../master/process/businessprocess/businessprocess.component";
import { ProcessLevel1Component } from "../master/process/processlevel1/processlevel1.component";
import { ProcessLevel2Component } from "../master/process/processlevel2/processlevel2.component";
import { NgMultiSelectDropDownModule } from "ng-multiselect-dropdown";
import { HighchartsChartModule } from "highcharts-angular";
import { ObservationGradingComponent } from "../master/observation-grading/observation-grading.component";
import { EscalationComponent } from "../master/escalation/escalation.component";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { EscalationService } from "../master/escalation/escalation.service";
import { CKEditorModule } from "@ckeditor/ckeditor5-angular";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { DesignationComponent } from "../master/designation/designation.component";
import { ControlPerformanceIndicatorComponent } from "../master/control-performance-indicator/control-performance-indicator.component";
import { ActivitylogsComponent } from "../master/activitylogs/activitylogs.component";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";

@NgModule({
  imports: [
    CommonModule,
    StandardMasterRoutingModule,
    CKEditorModule,
    FormsModule,
    TableModule,
    NgMultiSelectDropDownModule,
    HighchartsChartModule,
    NgxSpinnerModule,
    NgbModule,
  ],
  declarations: [
    CountryComponent,
    CityComponent,
    StateComponent,
    StandardMasterComponent,
    CompanyComponent,
    LocationComponent,
    PlmapComponent,
    TrialbalanceComponent,
    ProcessComponent,
    BusinessProcessComponent,
    ProcessLevel1Component,
    ProcessLevel2Component,
    SectorComponent,
    CountrystatecityComponent,
    RiskTypeComponent,
    ReportConsiderationComponent,
    RecommendationComponent,
    RootCauseComponent,
    ImpactComponent,
    ObservationGradingComponent,
    EscalationComponent,
    DesignationComponent,
    ControlPerformanceIndicatorComponent,
    ActivitylogsComponent,
  ],
  providers: [
    CityService,
    CountryService,
    CommonApiService,
    EscalationService,
    NgxSpinnerService,
  ],
})
export class StandardMasterModule {}
