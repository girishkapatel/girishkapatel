import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { MasterComponent } from "./master.component";
import { AuditPlanComponent } from "./auditplans/auditplans.component";
import { OverallAssessmentComponent } from "./overallassessment/overallassessment.component";
// import { CompanyComponent } from './company/company.component';
// import { LocationComponent } from './location/location.component';
import { KeybusinessinitiativeComponent } from "./keybusinessinitiative/keybusinessinitiative.component";
import { ProcessriskmappingComponent } from "./processriskmapping/processriskmapping.component";
// import { TrialbalanceComponent } from './trialbalance/trialbalance.component';
import { CoverageComponent } from "./coverage/coverage.component";
import { ErmrisksComponent } from "./ermrisks/ermrisks.component";
import { RoleGaurd } from "src/app/services/auth/auth.gaurd";

const routes: Routes = [
  {
    path: "",
    component: MasterComponent,
    data: {
      title: "Master",
    },
    children: [
      {
        path: "",
        redirectTo: "processriskmapping",
        pathMatch: "full",
      },
      // {
      //   path: 'company',
      //   component: CompanyComponent
      // },
      // {
      //   path: 'location',
      //   component: LocationComponent
      // },
      {
        path: "overallassessment",
        component: OverallAssessmentComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "auditplan",
        component: AuditPlanComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "keybusinessinitiative",
        component: KeybusinessinitiativeComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "processriskmapping",
        component: ProcessriskmappingComponent,
        canActivate: [RoleGaurd],
      },
      // {
      //   path:'trialbalance',
      //   component:TrialbalanceComponent
      // },
      {
        path: "coverage",
        component: CoverageComponent,
        canActivate: [RoleGaurd],
      },
      {
        path: "ermrisk",
        component: ErmrisksComponent,
        canActivate: [RoleGaurd],
      },

      // ,
      // {
      //   path:'processlocationmapping',
      //   component:PlmapComponent
      // }
    ],
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MasterRoutingModule {}
