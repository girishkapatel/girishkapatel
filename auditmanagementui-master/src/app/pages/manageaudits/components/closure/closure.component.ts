import { number, string } from "@amcharts/amcharts4/core";
import { Component, Input, OnInit } from "@angular/core";
import { ApiService } from "src/app/services/api/api.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-closure",
  templateUrl: "closure.component.html",
  styleUrls: ["./closure.component.css"],
})
export class ClosureComponent implements OnInit {
  constructor(
    private api: ApiService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) { }

  accessRights: any = {};
  TableDataForOverAlls: any = [];
  public Editor = ClassicEditor;
  isStackHolder: boolean = false;
  id: any;

  AuditId: string = "";

  //Saving Potential
  PotentialsSavings: string = "";
  RealisedSavings: string = "";
  Leakage: string = "";

  _PotentialsSavings: string = "";
  _RealisedSavings: string = "";
  _Leakage: string = "";

  SavingPotential: Object = {
    PotentialsSavings: string,
    RealisedSavings: string,
    Leakage: string,
  };

  //Process Improvement
  SystemImprovement: string = "";
  RedFlag: string = "";
  LeadingPractices: string = "";
  // Quality: string = "";
  // Velocity: string = "";
  // TrustedAdvisor: string = "";

  ProcessImprovement: Object = {
    SystemImprovement: string,
    RedFlag: string,
    LeadingPractices: string,
    // Quality: string,
    // Velocity: string,
    // TrustedAdvisor: string,
  };

  //ReportConsideration
  NonCompliance: string = "";
  ICFR: string = "";
  NA: string = "";

  ReportConsideration: { [index: string]: number };

  ReportConsiderations: any = [];

  //People
  // Learning: string = "";
  // LeaderDevelopment: string = "";

  // People: Object = {
  // Learning: string,
  // LeaderDevelopment: string,
  // };

  //Control Health Scorecard
  ControlsIdentifiedAndTested: string = "";
  DesignControlRating: string = "";
  ControlsWithObservation: string = "";
  OEControlRating: string = "";
  ControlsWithOperatingObservation: string = "";
  ScopeLimitation: string = "";

  ControlHealthScoreCard: Object = {
    ControlsIdentifiedAndTested: string,
    DesignControlRating: string,
    ControlsWithObservation: string,
    OEControlRating: string,
  };

  //NumberOfObservation
  Critical: string = "";
  High: string = "";
  Medium: string = "";
  Low: string = "";
  Repeat: string = "";

  NumberOfObservation: { [index: string]: number };

  //Recommendation
  PeopleRec: string = "";
  Process: string = "";
  Technology: string = "";

  Recommendation: { [index: string]: number };

  Recommendations: any = [];

  //RootCause
  ControlDesign: string = "";
  ControlOperatingEffectiveness: string = "";
  SystemLimitation: string = "";
  ProcessCompliance: string = "";

  RootCause: { [index: string]: number };

  RootCauses: any = [];

  //Impact
  ReportingImpact: string = "";
  OperationalImpact: string = "";
  StatutoryNonCompliance: string = "";
  ReputationalLoss: string = "";

  Impact: Object = {
    ReportingImpact: string,
    OperationalImpact: string,
    StatutoryNonCompliance: string,
    ReputationalLoss: string,
  };

  ImpactMaster: any = [];

  Impacts: { [index: string]: number };

  //Grand Total
  TotalControlsIdentifiedTested: number = 0;
  TotalDesignControlRating: number = 0;
  TotalControlswithObservation: number = 0;
  TotalControlwithnoException: number = 0;
  TotalOEControlRating: number = 0;

  fillClosureData(closureData) {
    //Saving Potential
    this.id = closureData.id;
    this.PotentialsSavings = closureData.savingPotential.potentialsSavings || 0;
    this.RealisedSavings = closureData.savingPotential.realisedSavings || 0;
    this.Leakage = closureData.savingPotential.leakage || 0;
    this._PotentialsSavings = this.PotentialsSavings;
    this._RealisedSavings = this.RealisedSavings;
    this._Leakage = this.Leakage;
    //Process Improvement
    this.SystemImprovement =
      closureData.processImprovement.systemImprovement || 0;
    this.RedFlag = closureData.processImprovement.redFlag || 0;
    this.LeadingPractices =
      closureData.processImprovement.leadingPractices || 0;
    // this.Quality = closureData.processImprovement.quality || 0;
    // this.Velocity = closureData.processImprovement.velocity || 0;
    // this.TrustedAdvisor = closureData.processImprovement.trustedAdvisor || 0;

    //ReportConsideration
    // this.NonCompliance =
    //   closureData.reportConsideration[
    //     "noncompliancewithstatutorylaws&regulation"
    //   ] || 0;
    // this.ICFR = closureData.reportConsideration.impactonicfr || 0;
    // this.NA = closureData.reportConsideration.na
    //   ? closureData.reportConsideration.na
    //   : 0;

    // this.NonCompliance = closureData.reportConsideration.NonCompliance || 0;
    // this.ICFR = closureData.reportConsideration.ICFR || 0;
    // this.NA = closureData.reportConsideration.NA || 0;

    //People
    // this.Learning = closureData.people.learning || 0;
    // this.LeaderDevelopment = closureData.people.leaderDevelopment || 0;

    //Control Health Scorecard
    this.ControlsIdentifiedAndTested =
      closureData.controlHealthScoreCard.controlsIdentifiedAndTested || "0";

    this.DesignControlRating =
      closureData.controlHealthScoreCard.designControlRating || "0";
    this.DesignControlRating =
      this.DesignControlRating.length > 5
        ? this.DesignControlRating.substr(0, 5)
        : this.DesignControlRating;

    this.ControlsWithObservation =
      closureData.controlHealthScoreCard.controlsWithObservation || "0";

    this.OEControlRating =
      closureData.controlHealthScoreCard.oeControlRating || "0";
    this.OEControlRating =
      this.OEControlRating.length > 5
        ? this.OEControlRating.substr(0, 5)
        : this.OEControlRating;

    this.ScopeLimitation = closureData.scopeLimitation || "";

    //NumberOfObservation
    this.Critical = closureData.numberOfObservation.Critical || 0;
    this.High = closureData.numberOfObservation.High || 0;
    this.Medium = closureData.numberOfObservation.Medium || 0;
    this.Low = closureData.numberOfObservation.Low || 0;
    this.Repeat = closureData.numberOfObservation.Repeat || 0;

    //Recommendation
    this.PeopleRec = closureData.recommendation.People || 0;
    this.Process = closureData.recommendation.Process || 0;
    this.Technology = closureData.recommendation.Technology || 0;

    //RootCause
    this.ControlDesign = closureData.rootCause.ControlDesign || 0;
    this.ControlOperatingEffectiveness =
      closureData.rootCause.ControlOperatingEffectiveness || 0;
    this.SystemLimitation = closureData.rootCause.SystemLimitation || 0;
    this.ProcessCompliance = closureData.rootCause.ProcessCompliance || 0;

    //Impact
    this.ReportingImpact = closureData.impact.reportingImpact || 0;
    this.OperationalImpact = closureData.impact.operationalImpact || 0;
    this.StatutoryNonCompliance =
      closureData.impact.statutoryNonCompliance || 0;
    this.ReputationalLoss = closureData.impact.reputationalLoss || 0;
  }

  saveAuditClosure() {
    let closureObj = this.getClosureObj();

    this.api.updateData("api/auditclosure", closureObj).subscribe(
      (closureData) => {
        this.notifyService.success("Audit Closure Updated Successfully");
      },
      (error) => {
        console.log(error);
      }
    );
  }

  getClosureObj() {
    this.SavingPotential = this.SavingPotential = {
      PotentialsSavings: this.PotentialsSavings || "0",
      RealisedSavings: this.RealisedSavings || "0",
      Leakage: this.Leakage || "0",
    };

    this.ProcessImprovement = this.ProcessImprovement = {
      SystemImprovement: this.SystemImprovement || "0",
      RedFlag: this.RedFlag || "0",
      LeadingPractices: this.LeadingPractices || "0",
      // Quality: this.Quality || 0,
      // Velocity: this.Velocity || 0,
      // TrustedAdvisor: this.TrustedAdvisor || 0,
    };

    // this.ReportConsideration = {
    // NonCompliance: parseInt(this.NonCompliance) || 0,
    // ICFR: parseInt(this.ICFR) || 0,
    // NA: parseInt(this.NA) || 0,
    // };

    // this.People = this.People = {
    // Learning: this.Learning || 0,
    // LeaderDevelopment: this.LeaderDevelopment || 0,
    // };

    this.ControlHealthScoreCard = this.ControlHealthScoreCard = {
      ControlsIdentifiedAndTested: this.ControlsIdentifiedAndTested || "0",
      DesignControlRating:
        this.DesignControlRating.length > 5
          ? this.DesignControlRating.substr(0, 5)
          : this.DesignControlRating,
      ControlsWithObservation: this.ControlsWithObservation || "0",
      OEControlRating:
        this.OEControlRating.length > 5
          ? this.OEControlRating.substr(0, 5)
          : this.OEControlRating,
    };

    this.NumberOfObservation = {
      Critical: parseInt(this.Critical) || 0,
      High: parseInt(this.High) || 0,
      Medium: parseInt(this.Medium) || 0,
      Low: parseInt(this.Low) || 0,
      Repeat: parseInt(this.Repeat) || 0,
    };

    this.Recommendation = {
      People: parseInt(this.PeopleRec) || 0,
      Process: parseInt(this.Process) || 0,
      Technology: parseInt(this.Technology) || 0,
    };

    this.RootCause = {
      ControlDesign: parseInt(this.ControlDesign) || 0,
      ControlOperatingEffectiveness:
        parseInt(this.ControlOperatingEffectiveness) || 0,
      SystemLimitation: parseInt(this.SystemLimitation) || 0,
      ProcessCompliance: parseInt(this.ProcessCompliance) || 0,
    };

    this.Impact = this.Impact = {
      ReportingImpact: this.ReportingImpact || 0,
      OperationalImpact: this.OperationalImpact || 0,
      StatutoryNonCompliance: this.StatutoryNonCompliance || 0,
      ReputationalLoss: this.ReputationalLoss || 0,
    };

    let closureObj = {
      id: this.id,
      AuditId: this.AuditId,
      ScopeLimitation: this.ScopeLimitation,
      NumberOfObservation: this.NumberOfObservation,
      Recommendation: this.Recommendations,
      RootCause: this.RootCauses,
      ReportConsideration: this.ReportConsiderations,
      // People: this.People,
      Impact: this.ImpactMaster,
      ControlHealthScoreCard: this.ControlHealthScoreCard,
      SavingPotential: this.SavingPotential,
      ProcessImprovement: this.ProcessImprovement,
    };

    return closureObj;
  }

  getAuditClosure(auditId) {
    this.spinner.show();
    this.api.getData("api/auditclosure/GetByAudit/" + auditId).subscribe(
      (closureData) => {
        if (Array.isArray(closureData) && closureData.length) {
          this.fillClosureData(closureData[0]);
        }
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
    this.spinner.hide();
  }

  getReportConsiderations() {
    this.spinner.show();
    this.api
      .getData(
        "api/auditclosure/getauditclousrereportconsideration/" + this.AuditId
      )
      .subscribe(
        (data) => {
          this.ReportConsiderations = data;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getRecommendations() {
    this.spinner.show();
    this.api
      .getData("api/auditclosure/getauditclousrerecommendation/" + this.AuditId)
      .subscribe(
        (data) => {
          this.Recommendations = data;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getRoutCauses() {
    this.spinner.show();
    this.api
      .getData("api/auditclosure/getauditclousrerootcauses/" + this.AuditId)
      .subscribe(
        (data) => {
          this.RootCauses = data;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getImpactMaster() {
    this.spinner.show();
    this.api
      .getData("api/auditclosure/getauditclousreimpactmaster/" + this.AuditId)
      .subscribe(
        (data) => {
          this.ImpactMaster = data;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "manageaudits",
      "auditclosure"
    );
  }
  fillOverallPerfromance() {
    this.spinner.show();
    this.api
      .getData("api/auditclosure/GetOverallPerformance/" + this.AuditId)
      .subscribe(
        (data) => {
          this.TableDataForOverAlls = data;

          this.TableDataForOverAlls.forEach((a) => {
            (this.TotalControlsIdentifiedTested += a.controlsIdentifiedTested),
              (this.TotalDesignControlRating += a.designControlRating),
              (this.TotalControlswithObservation += a.controlswithObservation),
              (this.TotalControlwithnoException += a.controlwithnoException),
              (this.TotalOEControlRating += a.oeControlRating);
          });

          this.TotalDesignControlRating /=
            this.TableDataForOverAlls.length > 0
              ? this.TableDataForOverAlls.length
              : 1;
          this.TotalOEControlRating /=
            this.TableDataForOverAlls.length > 0
              ? this.TableDataForOverAlls.length
              : 1;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    this.spinner.hide();
  }
  convertRupees(ev) {
    let type = ev.target.value;
    this.PotentialsSavings = this.utils.convertLakhsandCarors(this._PotentialsSavings, type)
    this.RealisedSavings = this.utils.convertLakhsandCarors(this._RealisedSavings, type)
    this.Leakage = this.utils.convertLakhsandCarors(this._Leakage, type)
  }
  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditId = localStorage.getItem("auditId");

    this.getAuditClosure(this.AuditId);

    this.getReportConsiderations();
    this.getRecommendations();
    this.getRoutCauses();
    this.getImpactMaster();
    this.fillOverallPerfromance();
  }
}
