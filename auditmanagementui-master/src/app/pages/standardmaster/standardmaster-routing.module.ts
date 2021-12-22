import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { CountryComponent } from "../master/country/country.component";
import { CityComponent } from "../master/city/city.component";
import { StateComponent } from "../master/state/state.component";
import { StandardMasterComponent } from "./standardmaster.component";
import { CompanyComponent } from "../master/company/company.component";
import { LocationComponent } from "../master/location/location.component";
import { PlmapComponent } from "../master/processlocationmapping/processlocationmapping.component";
import { TrialbalanceComponent } from "../master/trialbalance/trialbalance.component";
import { ProcessComponent } from "../master/process/process.component";
import { CountrystatecityComponent } from "../master/countrystatecity/countrystatecity.component";
import { RoleGaurd } from "src/app/services/auth/auth.gaurd";
import { RiskTypeComponent } from "../master/risktype/risktype.component";
import { ReportConsiderationComponent } from "../master/reportconsideration/reportconsideration.component";
import { RecommendationComponent } from "../master/recommendation/recommendation.component";
import { RootCauseComponent } from "../master/rootcause/rootcause.component";
import { ImpactComponent } from "../master/impact/impact.component";
import { ObservationGradingComponent } from "../master/observation-grading/observation-grading.component";
import { EscalationComponent } from "../master/escalation/escalation.component";
import { DesignationComponent } from "../master/designation/designation.component";
import{ControlPerformanceIndicatorComponent} from "../master/control-performance-indicator/control-performance-indicator.component";
import { ActivitylogsComponent } from '../master/activitylogs/activitylogs.component';

const routes: Routes = [
  {
    path: "",
    component: StandardMasterComponent,
    data: {
      title: "Master",
    },
    children: [
      {
        path: "",
        redirectTo: "countrystatecity",
        pathMatch: "full",
      },
      {
        path: "countrystatecity",
        component: CountrystatecityComponent,
        canActivate: [RoleGaurd],
      },
      // {
      //   path: 'city',
      //   component: CityComponent
      // },
      // {
      //   path: 'state',
      //   component: StateComponent
      // },
      {
        path: "company",
        component: CompanyComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "location",
        component: LocationComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "processlocationmapping",
        component: PlmapComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "trialbalance",
        component: TrialbalanceComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "processmaster",
        component: ProcessComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "risktype",
        component: RiskTypeComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "reportconsideration",
        component: ReportConsiderationComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "recommendation",
        component: RecommendationComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "rootcause",
        component: RootCauseComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "impact",
        component: ImpactComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "observationgrading",
        component: ObservationGradingComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "escalation",
        component: EscalationComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "designation",
        component: DesignationComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "controlperformanceindicator",
        component: ControlPerformanceIndicatorComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "activitylogs",
        component: ActivitylogsComponent,
        canActivate: [RoleGaurd],
      },
    ],
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StandardMasterRoutingModule {}
