import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import * as $ from "jquery";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { ApiService } from "src/app/services/api/api.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { IDropdownSettings } from "ng-multiselect-dropdown";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { NgForm, NgModel } from "@angular/forms";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: "app-auditreport",
  templateUrl: "auditreport.component.html",
  styleUrls: ["./auditreport.component.css"],
})
export class AuditReportComponent implements OnInit {
  constructor(
    private commonApi: CommonApiService,
    private api: ApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("auditForm", { static: false }) auditForm: NgForm;
  @ViewChild("apForm", { static: false }) apForm: NgForm;
  @ViewChild("fileInputDR", { static: false }) fileInputDR;

  @Input() shOpts: any = [];
  isStackHolder: boolean = false;
  accessRights: any = {};

  filterGrading: string = "all";
  filterStatus: string = "all";

  summaryGradingTotal: number = 0;
  summaryCriticalGrade: number = 0;
  summaryHighGrade: number = 0;
  summaryMediumGrade: number = 0;
  summaryLowGrade: number = 0;
  summaryStatusTotal: number = 0;
  summaryNotStarted: number = 0;
  summaryInProgress: number = 0;
  summaryInReview: number = 0;
  summaryCompleted: number = 0;

  uploadedFiles: any = [];
  selectedFiles: any = [];

  uploadedFileIndex: number;

  model: any = {};
  selectedDR = {};

  drMailTotal: number = 0;
  drMailSent: number = 0;

  dropdownSettings: IDropdownSettings = {};
  OwnerdropdownSettings: IDropdownSettings = {};

  ObservationHeadingText: any = "";
  PotentialSavingText: any = "";
  RealisedSavingText: any = "";
  LeakageText: any = "";

  submitted = false;

  // tableApiUrl: string;
  // tableFilters: BehaviorSubject<{}>;

  AuditID: string;

  impactOptions: any = [];
  impactAll: any = [];
  selectedImpacts: any = [];

  reportConsiderationOptions: any = [];
  reportConsiderationAll: any = [];
  selectedReportConsiderations: any = [];
  recommendationOptions: any = [];
  recommendationAll: any = [];
  selectedRecommendations: any = [];

  rootCauseOptions: any = [];
  rootCauseAll: any = [];
  selectedRootCauses: any = [];

  /*Discussion Note Details */
  DiscussionNoteID: string = "";
  RACMNumber: string = "";
  ObservationHeading: string = "";
  DetailedObservation: string = "";
  RootCause: string = "";
  Risks: string = "";
  FinancialImpact: string = "";
  RiskType: string = "";
  riskTypeOptions: any = [];
  selectedRiskTypeIds = [];
  Status: boolean = false;
  isApprover: boolean = false;
  isCompleted: boolean = false;
  isApproved: boolean = false;
  Justification: string = "";
  isRepeat: boolean = false;

  /*Draft Report Details */
  id: string = "";
  ObservationNumber: string = "";
  ObservationGrading: string = "";
  ActionPlan: string = "";
  ProcessOwnerID: string = "";
  ManagementResponse: string = "";

  ImplementationStartDate: NgbDateStruct;
  ImplementationEndDate: NgbDateStruct;

  ManagementComments: string = "";
  PotentialSaving: string = "";
  RealisedSaving: string = "";
  Leakage: string = "";
  ReportConsideration: string = "";
  Recommendation: string = "";
  RootCauseDR: string = "";
  isSystemImprovement: boolean = false;
  isRedFlag: boolean = false;
  isLeadingPractices: boolean = false;
  FieldBackground: string = "";
  ValueAtRisk: string = "";
  selectedImplementationOwnerId = [];
  public Editor = ClassicEditor;

  actionPlanIndex: number;

  isEditActionPlan: boolean = false;

  actionPlans: any = [];

  //#region  History
  tableFiltersHistory: BehaviorSubject<{ init: boolean }>;
  tableIdHistory: string = "discussnotehistory_table";
  tableColumnsHistory: tableColumn[] = [
    {
      title: "User",
      data: "user",
      render: (data) => {
        return data ? data.firstName + " " + data.lastName : "";
      },
    },
    {
      title: "Status",
      data: "status",
    },
    {
      title: "Date",
      data: "draftReportDate",
      render: (data) => {
        return data ? this.utils.formatDate(data) : "";
      },
    },
  ];
  //#endregion

  //#region  send Mail
  tableIdsendMail: string = "sendMail_table";
  tableColumnsendMail: tableColumn[] = [
    {
      title: "Discussion No",
      data: "discussionNo",
    },
    {
      title: "Email Id",
      data: "emailId",
    },
    {
      title: "Email Sent",
      data: "isSent",
      render: (data) => {
        return data ? "YES" : "NO";
      },
    },
    {
      title: "Remarks",
      data: "message",
    },
  ];
  //#endregion

  /* Report Table Config*/
  tableId: string = "report_table";
  tableColumnsReport: tableColumn[] = [
    {
      title: "Select",
      data: "id",
      render: (data) => {
        return (
          '<p class="text-center" style="margin:0px"><input type="checkbox" class="selectedDR" data-id="' +
          data +
          '" /></p>'
        );
      },
    },
    {
      title: "Discussion No",
      data: "discussionNote.discussionNumber",
    },
    {
      title: "Observation Heading",
      data: "discussionNote.observationHeading",
    },
    {
      title: "Detailed Observation",
      data: "discussionNote.detailedObservation",
      render: (data) => {
        if (data.length > 100) {
          return (
            "<span>" +
            data.slice(0, 100) +
            '</span><br><a href="javascript:void(0)" data-title="Detailed Observation" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    {
      title: "RACM Number",
      data: "discussionNote.racM_Ids",
      render: function (data) {
        return Array.isArray(data) ? data.join(", ") : "";
      },
    },
    {
      title: "Root Cause",
      data: "discussionNote.rootCause",
      render: (data) => {
        if (data.length > 50) {
          return (
            "<span>" +
            data.slice(0, 50) +
            '</span><br><a href="javascript:void(0)" data-title="Root Cause" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    {
      title: "Risk Type",
      data: "discussionNote.riskTypes",
      render: function (data) {
        let names = "";
        data.forEach((element) => {
          names += element.name + ", ";
        });

        return names.trim().slice(0, -1);
      },
    },
    {
      title: "Risks / Business Impact",
      data: "discussionNote.risks",
      render: (data) => {
        if (data.length > 100) {
          return (
            "<span>" +
            data.slice(0, 100) +
            '</span><br><a href="javascript:void(0)" data-title="Risks / Business Impact" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    // {
    // title: "Financial Impact ",
    // data: "discussionNote.financialImpact",
    // },
    {
      title: "Observations Grading",
      data: "observationGrading",
      render: function (data) {
        switch (data) {
          case 1:
            return "Medium";
            break;
          case 2:
            return "High";
            break;
          case 3:
            return "Critical";
            break;
          default:
            return "Low";
            break;
        }
        // let og = data === 0 ? "Low" : data === 1 ? "Medium" : "High";
        // return og;
      },
    },
    {
      title: "Flag Issue",
      data: "discussionNote.flagIssueForReport",
      render: function (data) {
        if (data) {
          return "Yes";
        } else {
          return "No";
        }
      },
    },
    {
      title: "Status",
      data: "status",
      render: (data) => {
        if (data) return data.toUpperCase();
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        let buttons = "";

        if (this.accessRights.isEdit)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editReport"><i class="fa fa-edit"></i></button>';
        if (!this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-primary emailDR" title="Send email for approval"><i class="fa fa-send"></i></button>';

        return buttons;
      },
    },
  ];

  tableData_report: tableData[] = [];

  isEdit: boolean = false;

  formVisible: boolean = false;

  confirmSaveReport(e) {
    e.preventDefault();
    this.submitted = true;
    if (this.auditForm.invalid) {
      this.notifyService.error("All * marked fields are compulsory");
      return false;
    }

    window["jQuery"]("#ConfimSendDRForApprovalModal").modal("show");
  }

  saveSendReport() {
    this.saveReport();
    this.updateStatus("pending");
    window["jQuery"]("#ConfimSendDRForApprovalModal").modal("hide");
  }

  saveReport() {
    this.submitted = true;

    if (this.auditForm.invalid) return false;

    this.updateDraftReport();

    this.submitted = false;

    window["jQuery"]("#ConfimSendDRForApprovalModal").modal("hide");
  }

  updateDraftReport() {
    if (!this.validateDR()) {
      this.notifyService.error("All * marked fields are compulsory");
      return;
    }
    let apObj = this.getDraftObj();

    apObj.ActionPlans.forEach((element) => {
      element.implementationStartDate = this.utils.formatNgbDateToYMD(
        element.implementationStartDate
      );
      element.implementationEndDate = this.utils.formatNgbDateToYMD(
        element.implementationEndDate
      );
    });

    this.api.updateData("api/draftreport/", apObj).subscribe((response) => {
      if (this.selectedFiles.length > 0) this.saveSelectedFiles();
      this.notifyService.success("Draft Updated Successfully");
      this.handleFormView.hide();
    });
  }

  getDraftObj() {
    let draftObj = {
      id: this.id,
      AuditId: this.AuditID,
      DiscussionNoteID: this.DiscussionNoteID,
      ObservationNumber: this.ObservationNumber,
      ObservationGrading: parseInt(this.ObservationGrading),
      // ActionPlan: this.ActionPlan,
      // ProcessOwnerID: this.ProcessOwnerID,
      // ImplementationStartDate: this.ImplementationStartDate,
      // ImplementationEndDate: this.ImplementationEndDate,
      ManagementComments: this.ManagementComments,
      PotentialSaving: this.PotentialSaving.toString(),
      RealisedSaving: this.RealisedSaving.toString(),
      Leakage: this.Leakage.toString(),
      isSystemImprovement: this.isSystemImprovement,
      isRedFlag: this.isRedFlag,
      isLeadingPractices: this.isLeadingPractices,
      ValueAtRisk: this.ValueAtRisk.toString(),
      ActionPlans: this.actionPlans,
      // ReportConsideration: this.ReportConsideration,
      Recommendation: this.Recommendation,
      ManagementResponse: this.ManagementResponse,
      RootCause: this.RootCauseDR,
      ReportConsiderations: this.getReportConsiderationIds(),
      Recommendations: this.getRecommendationIds(),
      RootCauses: this.getRootCauseIds(),
      Impacts: this.getImpactIds(),
      FieldBackground: this.FieldBackground,
    };
    return draftObj;
  }

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.formVisible = false;
      this.isEdit = false;
      this.clearForm();
      // this.tableFilters.next({});
      this.loadDaftReportTable();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  addReport() {
    this.getImpactOptions();
    this.getReportConsiderationOptions();
    this.getRecommendationOptions();
    this.getRootCauseOptions();

    this.handleFormView.show();
  }

  editReport(reportData) { 
    this.isEdit = true;

    /*Discussion Note Details */
    this.DiscussionNoteID = reportData.discussionNote.id;
    this.RACMNumber = Array.isArray(reportData.discussionNote.racM_Ids)
      ? reportData.discussionNote.racM_Ids.join(", ")
      : "";
    this.ObservationHeading = reportData.discussionNote.observationHeading;
    this.FieldBackground = reportData.discussionNote.fieldBackground
      ? reportData.discussionNote.fieldBackground
      : reportData.fieldBackground;
    this.ObservationHeadingText = reportData.discussionNote.observationHeading;
    this.DetailedObservation = reportData.discussionNote.observationHeading;
    this.RootCause = reportData.discussionNote.rootCause;
    this.Risks = reportData.discussionNote.risks;
    this.FinancialImpact = reportData.discussionNote.financialImpact;
   
    this.isRepeat = reportData.discussionNote.isRepeat;

    this.Status =
      reportData.status === null ||
      reportData.status.toLowerCase() === "inprogress"
        ? true
        : false;
    this.isCompleted =
      reportData.status && reportData.status.toLowerCase() === "completed"
        ? true
        : false;

    this.isApproved =
      (reportData.status && reportData.status.toLowerCase() === "approved") ||
      this.isCompleted
        ? true
        : false;

    /*Draft Report Details */
    this.id = reportData.id;
    this.ObservationNumber = reportData.observationNumber;
    this.ObservationGrading = reportData.observationGrading;
    // this.ActionPlan = reportData.actionPlan;
    // this.ProcessOwnerID = reportData.processOwnerID;
    // this.ImplementationStartDate = reportData.implementationStartDate;
    // this.ImplementationEndDate = reportData.implementationEndDate;
    this.ManagementComments = reportData.managementComments;
    this.PotentialSaving = reportData.potentialSaving;
    this.PotentialSavingText = reportData.potentialSaving;
    this.RealisedSaving = reportData.realisedSaving;
    this.RealisedSavingText = reportData.realisedSaving;
    this.Leakage = reportData.leakage;
    this.LeakageText = reportData.leakage;
    this.ReportConsideration = reportData.reportConsideration;
    this.Recommendation = reportData.recommendation;
    this.ManagementResponse = reportData.managementResponse;
    this.RootCauseDR = reportData.rootCause;
    this.isSystemImprovement = reportData.isSystemImprovement;
    this.isRedFlag = reportData.isRedFlag;
    this.isLeadingPractices = reportData.isLeadingPractices;
    this.ValueAtRisk = reportData.valueAtRisk;
    this.actionPlans = reportData.actionPlans ? reportData.actionPlans : [];

    if (this.actionPlans !== null && this.actionPlans.length > 0) {
      this.actionPlans.forEach((element) => {
        element.implementationStartDate = this.utils.formatToNgbDate(
          element.implementationStartDate
        );
        element.implementationEndDate = this.utils.formatToNgbDate(
          element.implementationEndDate
        );
      });
    }

    this.initDropdownSettings(
      false,
      "id",
      "name",
      "Select All",
      "Deselect All",
      3,
      false,
      true
    );

    this.getImpactOptions();
    this.getReportConsiderationOptions();
    this.getRecommendationOptions();
    this.getRootCauseOptions();
    this.fillActionPlanTable();
    this.getRiskTypes();

    this.selectedImpacts = this.getSelectedImpactOpts(reportData.impacts);
    this.selectedReportConsiderations = this.getSelectedReportConsiderationOpts(
      reportData.reportConsiderations
    );
    this.selectedRecommendations = this.getSelectedRecommendationOpts(
      reportData.recommendations
    );
    this.selectedRootCauses = this.getSelectedRootCauseOpts(
      reportData.rootCauses
    );
    if (reportData.discussionNote.riskTypeIds != null)
      this.selectedRiskTypeIds = this.getSelectedRiskTypeOpts(
        reportData.discussionNote.riskTypeIds
      );
    this.handleFormView.show();
  }

  getSelectedImpactOpts(impactsArray) {
    let impactNamesArray = [];

    if (impactsArray !== null) {
      for (let i of this.impactAll) {
        if (impactsArray.indexOf(i.id) > -1) {
          impactNamesArray.push({ id: i.id, name: i.name });
        }
      }
    }

    return impactNamesArray;
  }

  getSelectedReportConsiderationOpts(reportConsiderationsArray) {
    let reportConsiderationNamesArray = [];

    if (reportConsiderationsArray !== null) {
      for (let i of this.reportConsiderationAll) {
        if (reportConsiderationsArray.indexOf(i.id) > -1) {
          reportConsiderationNamesArray.push({
            id: i.id,
            name: i.name,
          });
        }
      }
    }

    return reportConsiderationNamesArray;
  }

  getSelectedRecommendationOpts(recommendationsArray) {
    let recommendationNamesArray = [];

    if (recommendationsArray !== null) {
      for (let i of this.recommendationAll) {
        if (recommendationsArray.indexOf(i.id) > -1) {
          recommendationNamesArray.push({
            id: i.id,
            name: i.name,
          });
        }
      }
    }

    return recommendationNamesArray;
  }

  getSelectedRootCauseOpts(rootCausesArray) {
    let rootCauseNamesArray = [];

    if (rootCausesArray !== null) {
      for (let i of this.rootCauseAll) {
        if (rootCausesArray.indexOf(i.id) > -1) {
          rootCauseNamesArray.push({
            id: i.id,
            name: i.name,
          });
        }
      }
    }

    return rootCauseNamesArray;
  }

  validateDR() {
    return this.ObservationNumber;
    // this.ImplementationStartDate &&
    // this.ImplementationEndDate &&
    // this.ProcessOwnerID
  }

  updateStatus(status: string, params?: {}) {
    if (!this.validateDR()) {
      this.notifyService.error("All * marked fields are compulsory");
      return;
    }
    if (status==="completed" && this.actionPlans.length ==0) {
      this.notifyService.error("Action Plan is Mandatory");
      return;
    }

    let isConfirm = true;
    let statusMsg = {
      pending:
        "Are you sure you want to sent " +
        this.ObservationNumber +
        " for approval?",
      approved: `Are you sure you want save ${this.ObservationNumber} in final audit report? `,
      completed:
        "Are you sure you want to approve " + this.ObservationNumber + "?",
    };

    if (statusMsg[status]) {
      isConfirm = confirm(statusMsg[status]);
    }

    let statusObj = { Id: this.id, Status: status };
    if (params && !this.utils.isEmptyObj(params) && params["justification"]) {
      statusObj["Justification"] = params["justification"];
    }

    if (isConfirm) {
      this.api
        .updateData("api/draftreport/UpdateAuditReportStatus", statusObj)
        .subscribe((response) => {
          if (status === "completed") {
            this.addFollowup();
          }
          this.notifyService.success("Request Process Successfully");
          this.handleFormView.hide();
        });
    }
  }

  addFollowup() {
    this.api
      .insertData("api/followup", {
        DraftReportId: this.id,
        AuditId: this.AuditID,
      })
      .subscribe((response) => {});
  }

  rejectDR() {
    if (this.Justification && this.Justification !== "") {
      this.updateStatus("inprogress", { justification: this.Justification });
      this.handleRejectModal("hide");
    } else {
      this.notifyService.error("Justification is required");
    }
  }

  handleRejectModal(action) {
    if (action) {
      window["jQuery"]("#rejectDRModal").modal(action);
    }
  }
  getImplementionOwnerIds() {
    var implementationOwner = "";
    this.selectedImplementationOwnerId.forEach((element) => {
      implementationOwner = element.id;
    });
    return implementationOwner;
  }
  getSelectedImplementationOwnerOpts(OwnerArray) {
    let rootCauseNamesArray = [];
    if (OwnerArray != null) {
      for (let i of this.shOpts) {
        if (OwnerArray.indexOf(i.id) > -1) {
          rootCauseNamesArray.push({
            id: i.id,
            custom: i.firstName + " " + i.lastName,
          });
        }
      }
    }
    return rootCauseNamesArray;
  }
  clearForm() {
    /*Discussion Note Details */
    this.DiscussionNoteID = "";
    this.RACMNumber = "";
    this.ObservationHeading = "";
    this.DetailedObservation = "";
    this.RootCause = "";
    this.Risks = "";
    this.FinancialImpact = "";
    this.RiskType = "";
    this.isRepeat = false;

    /*Draft Report Details */
    this.id = "";
    this.ObservationNumber = "";
    this.ObservationGrading = "";
    // this.ActionPlan = "";
    // this.ProcessOwnerID = "";
    // this.ImplementationStartDate = "";
    // this.ImplementationEndDate = "";
    this.ManagementComments = "";
    this.PotentialSaving = "";
    this.RealisedSaving = "";
    this.Leakage = "";
    this.ReportConsideration = "";
    this.Recommendation = "";
    this.ManagementResponse = "";
    this.RootCauseDR = "";
    this.isSystemImprovement = false;
    this.isRedFlag = false;
    this.isLeadingPractices = false;
    this.ValueAtRisk = "";

    this.impactOptions = [];
    this.selectedImpacts = [];

    this.reportConsiderationOptions = [];
    this.selectedReportConsiderations = [];

    this.recommendationOptions = [];
    this.selectedRecommendations = [];

    this.rootCauseOptions = [];
    this.selectedRootCauses = [];
    this.selectedImplementationOwnerId = [];
  }

  showContentModal(title, content) {
    window["jQuery"]("#arContentModal #arcontent-title").html("").html(title);
    window["jQuery"]("#arContentModal #arcontent").html("").html(content);
    window["jQuery"]("#arContentModal").modal("show");
  }

  initDropdownSettings(
    isSingleSelection: boolean,
    idField: string,
    textField: string,
    selectAllText: string,
    unSelectAllText: string,
    itemsShowLimit: number,
    isAllowSearchFilter: boolean,
    closeDropDownOnSelection: boolean
  ) {
    this.dropdownSettings = {
      singleSelection: isSingleSelection,
      idField: idField,
      textField: textField,
      selectAllText: selectAllText,
      unSelectAllText: unSelectAllText,
      itemsShowLimit: itemsShowLimit,
      allowSearchFilter: isAllowSearchFilter,
      closeDropDownOnSelection: closeDropDownOnSelection,
    };
    {
      this.OwnerdropdownSettings = {
        singleSelection: true,
        idField: "id",
        textField: "custom",
        allowSearchFilter: true,
        closeDropDownOnSelection: true,
      };
    }
  }

  getImpactOptions() {
    this.spinner.show();
    this.commonApi.getImpacts().subscribe(
      (posts) => {
        this.impactOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getAllImpact() {
    this.spinner.show();
    this.commonApi.getImpacts().subscribe(
      (posts) => {
        this.impactAll = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getImpactIds() {
    let impactsArray = [];
    this.selectedImpacts.forEach((element) => {
      impactsArray.push(element.id);
    });
    return impactsArray;
  }

  getReportConsiderationOptions() {
    this.spinner.show();
    this.commonApi.getReportConsiderations().subscribe(
      (posts) => {
        this.reportConsiderationOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getAllReportConsideration() {
    this.spinner.show();
    this.commonApi.getReportConsiderations().subscribe(
      (posts) => {
        this.reportConsiderationAll = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getReportConsiderationIds() {
    let reportConsiderationsArray = [];

    this.selectedReportConsiderations.forEach((element) => {
      reportConsiderationsArray.push(element.id);
    });

    return reportConsiderationsArray;
  }

  getRecommendationOptions() {
    this.spinner.show();
    this.commonApi.getRecommendation().subscribe(
      (posts) => {
        this.recommendationOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getAllRecommendation() {
    this.spinner.show();
    this.commonApi.getRecommendation().subscribe(
      (posts) => {
        this.recommendationAll = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getRecommendationIds() {
    let recommendationsArray = [];

    this.selectedRecommendations.forEach((element) => {
      recommendationsArray.push(element.id);
    });

    return recommendationsArray;
  }

  getRootCauseOptions() {
    this.spinner.show();
    this.commonApi.getRootCause().subscribe(
      (posts) => {
        this.rootCauseOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getAllRootCause() {
    this.spinner.show();
    this.commonApi.getRootCause().subscribe(
      (posts) => {
        this.rootCauseAll = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getRootCauseIds() {
    let rootCausesArray = [];

    this.selectedRootCauses.forEach((element) => {
      rootCausesArray.push(element.id);
    });

    return rootCausesArray;
  }

  exportToPDF() {
    this.spinner.show();
    this.api
      .downloadFile(`api/draftreport/downloadpdf/${this.AuditID}`)
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/pdf",
          // type:
          // "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        });
        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "DraftReport.pdf");
          // link.setAttribute("download", "DiscussionNotes.pptx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      });
    this.spinner.hide();
  }
  exportToExcel() {
    this.spinner.show();
    this.api
      .downloadFile(`api/draftreport/downloadexcel/${this.AuditID}`)
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "DraftReport.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      });
    this.spinner.hide();
  }
  addNewActionPlan() {
    this.actionPlanIndex = -1;
    this.isEditActionPlan = false;
    this.clearActionPlanForm();
    this.showActionPlanForm();
  }

  clearActionPlanForm() {
    this.ActionPlan = "";
    this.ProcessOwnerID = "";
    this.ImplementationStartDate = null;
    this.ImplementationEndDate = null;
    this.selectedImplementationOwnerId = [];
  }

  showActionPlanForm() {
    window["jQuery"]("#manageActionPlan").modal("show");
  }
  hideActionPlanForm() {
    window["jQuery"]("#manageActionPlan").modal("hide");
  }

  addActionPlan() {
    if (this.apForm.invalid) return false;

    let actionPlanModel: any = {};
    actionPlanModel.actionPlan = this.ActionPlan;
    actionPlanModel.processOwnerID = this.getImplementionOwnerIds();
    actionPlanModel.implementationStartDate = this.ImplementationStartDate;
    actionPlanModel.implementationEndDate = this.ImplementationEndDate;

    // this.actionPlans.push(actionPlanModel);

    this.addActionPlanToTable(actionPlanModel, true);
    this.hideActionPlanForm();
  }

  addActionPlanToTable(actionPlanObj, isNew?: boolean) {
    let actionPlanTable = $("#selectedActionPlans tbody");
    let noOfRecords = actionPlanTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#selectedActionPlans tbody .norecords").length) {
        $("#selectedActionPlans tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let resHtml = `<tr>;
          <td>${actionPlanObj.actionPlan}</td>
          <td>${this.getUserName(actionPlanObj.processOwnerID)}</td>
          <td>${this.utils.formatNgbDateToDMY(
            actionPlanObj.implementationStartDate
          )}</td>
          <td>${this.utils.formatNgbDateToDMY(
            actionPlanObj.implementationEndDate
          )}</td>
          <td>`;
      if (!this.isStackHolder)
        resHtml += `
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editActionPlan">
          <i class="fa fa-edit"></i></button>&nbsp;
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeActionPlan">
          <i class="fa fa-trash"></i></button></td>
        </tr>`;

      actionPlanTable.append(resHtml);

      if (!isInitRender) {
        this.actionPlans.push(actionPlanObj);
      }
    } else {
      this.actionPlans[this.actionPlanIndex] = actionPlanObj;
      this.fillActionPlanTable();
    }
  }

  fillActionPlanTable() {
    if (Array.isArray(this.actionPlans) && this.actionPlans.length) {
      $("#selectedActionPlans tbody").html("");
      for (let actionPlan of this.actionPlans) {
        this.addActionPlanToTable(actionPlan);
      }
    } else {
      $("#selectedActionPlans tbody").html(
        '<tr class="norecords text-center"><td colspan="5">No Records Found</td></tr>'
      );
    }
  }

  editActionPlan(appIndex) {
    let appData = this.actionPlans[appIndex];

    this.clearActionPlanForm();
    this.fillActionPlanEdit(appData);
  }

  fillActionPlanEdit(actionPlanData) {
    // this.selectedApprover = approverData;
    this.ActionPlan = actionPlanData.actionPlan;
    this.selectedImplementationOwnerId =
      this.getSelectedImplementationOwnerOpts(actionPlanData.processOwnerID);
    this.ImplementationStartDate = actionPlanData.implementationStartDate;
    this.ImplementationEndDate = actionPlanData.implementationEndDate;
    this.showActionPlanForm();
  }

  saveActionPlan() {
    if (this.apForm.invalid) return false;

    let actionPlanModel: any = {};

    actionPlanModel.actionPlan = this.ActionPlan;
    actionPlanModel.processOwnerID = this.getImplementionOwnerIds();
    actionPlanModel.implementationStartDate = this.ImplementationStartDate;
    actionPlanModel.implementationEndDate = this.ImplementationEndDate;

    this.addActionPlanToTable(actionPlanModel, false);
    this.hideActionPlanForm();
  }

  removeActionPlan(appIndex) {
    if (this.actionPlans[appIndex]) {
      this.actionPlans.splice(appIndex, 1);
    }
    this.fillActionPlanTable();
  }

  getUserName(userId) {
    let userName: string = "";

    this.shOpts.forEach((element) => {
      if (element.id === userId)
        userName = element.firstName + " " + element.lastName;
    });

    return userName;
  }

  getSummary() {
    this.spinner.show();
    this.api.getData("api/draftreport/getsummary/" + this.AuditID).subscribe(
      (response) => {
        let objResult: any = response;

        this.summaryCriticalGrade = objResult.critical || 0;
        this.summaryHighGrade = objResult.high || 0;
        this.summaryMediumGrade = objResult.medium || 0;
        this.summaryLowGrade = objResult.low || 0;

        this.summaryGradingTotal =
          this.summaryCriticalGrade +
          this.summaryHighGrade +
          this.summaryMediumGrade +
          this.summaryLowGrade;

        this.summaryNotStarted = objResult.notStarted || 0;
        this.summaryInProgress = objResult.inProgress || 0;
        this.summaryInReview = objResult.inReview || 0;
        this.summaryCompleted = objResult.completed || 0;

        this.summaryStatusTotal =
          this.summaryNotStarted +
          this.summaryInProgress +
          this.summaryInReview +
          this.summaryCompleted;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  FilterByGrading(option) {
    this.filterGrading = option;
    this.loadDaftReportTable();
  }

  FilterByStatus(option) {
    this.filterStatus = option;
    this.loadDaftReportTable();
  }

  loadDaftReportTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(
        `api/draftreport/GetByAudit/${this.AuditID}/${this.filterGrading}/${this.filterStatus}`
      )
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumnsReport,true
          );
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  refreshTable() {
    this.filterGrading = "all";
    this.filterStatus = "all";
    this.loadDaftReportTable();
  }

  handleSelectedDRData(dataId, drData, isChecked) {
    if (isChecked) {
      this.handleDRData.add(dataId, drData);
    } else {
      this.handleDRData.remove(dataId);
    }
  }

  handleDRData = {
    checkEmpty: () => {
      return this.utils.isEmptyObj(this.selectedDR);
    },
    add: (dataId, drData) => {
      if (typeof this.selectedDR[dataId] === "undefined") {
        this.selectedDR[dataId] = {};
      }

      this.selectedDR[dataId] = drData;
    },
    remove: (dataId) => {
      if (typeof this.selectedDR[dataId] === "object") {
        delete this.selectedDR[dataId];
      }
    },
  };

  sendMultipleEmail() {
    if (this.handleDRData.checkEmpty())
      this.notifyService.error(
        "Please select at least one record to send email."
      );
    else {
      let drIDs = Object.keys(this.selectedDR);

      // window["jQuery"]("#sendDRMultipleMailModal").modal("show");

      this.drMailTotal = drIDs.length;
      this.drMailSent = 0;

      let sentMailCounter = 0;
      //Remove Send mail history table by userid

      var userid = localStorage.getItem("userId");
      this.api
        .getData("api/draftreport/RemoveSendEmailHistory/" + userid)
        .subscribe(
          (posts) => {},
          (error) => {}
        );
      drIDs.forEach((element, index) => {
        let postData = {
          Id: element,
        };

        this.api.insertData("api/draftreport/sendemail", postData).subscribe(
          (response) => {
            let result: any = response;
            // if (result.sent) {
            //   sentMailCounter++;
            //   this.drMailSent = sentMailCounter;
            //   window["jQuery"]("#sendDRMultipleMailModal").modal("hide");
            //   this.notifyService.success(
            //     "Draft email(s) sent successfully for approval."
            //   );
            // }
            this.sendMail.show();
          },
          (error) => {
            console.log(error);
          }
        );
      });
      this.loadDaftReportTable();
      this.selectedDR = {};
    }
  }

  emailDR(id) {
    let postData = {
      Id: id,
    };
    this.spinner.show();
    this.api.insertData("api/draftreport/sendemail", postData).subscribe(
      (response) => {
        let result: any = response;
        this.spinner.hide();
        if (result.sent)
          this.notifyService.success(
            "Draft report email sent successfully to reviewer."
          );
        this.loadDaftReportTable();
      },
      (error) => {
        this.spinner.hide();
        this.notifyService.error(error.error);
      }
    );
  }

  uploadDRFile() {
    let filesElem = this.fileInputDR.nativeElement;
    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        this.selectedFiles.push(file);
        let fileModel = {
          originalFileName: file.name,
          uploadedDatetime: this.utils.getCurrentDate(),
        };
        this.addUploadedFileToTable(fileModel, true);
      }
      this.fileInputDR.nativeElement.value = "";
    }
  }

  addUploadedFileToTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#uploadedDRFiles tbody");
    let noOfRecords = uploadedFilesTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#uploadedDRFiles tbody .norecords").length) {
        $("#uploadedDRFiles tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let fileName = `<button type="button" class="downloadFile" data-index="${noOfRecords}">${uploadFileObj.originalFileName}</button>`;
      if (typeof uploadFileObj.uploadedFileName === "undefined")
        fileName = uploadFileObj.originalFileName;

      let resHtml = `<tr>
      <td>${fileName}</td>
          <td>${this.utils.formatDateToStr(uploadFileObj.uploadedDatetime)}</td>
          <td>`;
      if (!this.isStackHolder)
        resHtml += `
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeFile">
          <i class="fa fa-trash"></i></button>`;
      resHtml += `</td></tr>`;

      uploadedFilesTable.append(resHtml);

      if (!isInitRender) {
        this.uploadedFiles.push(uploadFileObj);
      }
    } else {
      this.uploadedFiles[this.uploadedFileIndex] = uploadFileObj;
      this.fillUploadedFilesTable();
    }
  }

  fillUploadedFilesTable() {
    if (Array.isArray(this.uploadedFiles) && this.uploadedFiles.length) {
      $("#uploadedDRFiles tbody").html("");

      for (let file of this.uploadedFiles) {
        this.addUploadedFileToTable(file);
      }
    } else {
      $("#uploadedDRFiles tbody").html(
        '<tr class="norecords"><td colspan="3" class="text-center">No Records Found</td></tr>'
      );
    }
  }

  downloadFile(appIndex) {
    let fileId = this.uploadedFiles[appIndex].id;

    if (this.uploadedFiles[appIndex]) {
      if (typeof fileId === "undefined")
        this.notifyService.error(
          "Internal server error. Please reload the page and try again."
        );
      else {
        this.spinner.show();
        this.api
          .downloadFile("api/draftreport/downloadfile/" + fileId)
          .subscribe(
            (blob) => {
              let link = document.createElement("a");
              if (link.download !== undefined) {
                let url = URL.createObjectURL(blob);
                link.setAttribute("href", url);
                link.setAttribute(
                  "download",
                  this.uploadedFiles[appIndex].originalFileName
                );
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
              }
              this.spinner.hide();
            },
            (error) => {
              this.spinner.hide();
              console.log(error);
            }
          );
      }
    }
  }

  removeUploadedFile(appIndex) {
    let fileId = this.uploadedFiles[appIndex].id;

    if (this.uploadedFiles[appIndex]) {
      this.uploadedFiles.splice(appIndex, 1);
      this.selectedFiles.splice(appIndex, 1);

      if (typeof fileId === "undefined") {
        this.fillUploadedFilesTable();
      } else {
        this.api.deleteData("api/draftreport/removefile/" + fileId).subscribe(
          (upload) => {
            let result: any = upload;
            if (result.isDeleted) this.fillUploadedFilesTable();
          },
          (error) => {
            console.log(error);
          }
        );
      }
    }
  }

  getDRFiles() {
    this.spinner.show();
    this.api
      .getData("api/draftreport/getallfiles/" + this.AuditID + "/" + this.id)
      .subscribe(
        (posts) => {
          this.spinner.hide();
          this.uploadedFiles = posts;
          this.fillUploadedFilesTable();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  saveSelectedFiles() {
    var formData = new FormData();
    this.spinner.show();
    for (var file in this.selectedFiles) {
      formData.append("files", this.selectedFiles[file]);
    }

    formData.append("Id", this.id);
    formData.append("auditId", this.AuditID);
    formData.append("module", "draftreport");

    this.api.insertData("api/draftreport/uploadfile", formData).subscribe(
      (upload) => {
        let result: any = upload;

        if (result.isUploaded) {
          for (let file of result.files) {
            this.addUploadedFileToTable(file, true);
          }
        }
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  checkAccess() {
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "reporting"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "draftreport"
    )[0];
  }

  history = {
    show: () => {
      window["jQuery"]("#HistoryModal").modal("show");
      this.getHistoryData();
    },
    hide: () => {
      window["jQuery"]("#HistoryModal").modal("hide");
    },
  };
  sendMail = {
    show: () => {
      window["jQuery"]("#SendMailModal").modal("show");
      this.getSendMailHistoryData();
    },
    hide: () => {
      window["jQuery"]("#SendMailModal").modal("hide");
    },
  };
  getHistoryData() {
    this.spinner.show();
    this.api
      .getData(`api/DraftReport/getDraftReportHistory/${this.id}`)
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableIdHistory,
            dtData,
            this.tableColumnsHistory
          );
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  getSendMailHistoryData() {
    this.spinner.show();
    this.api
      .getData(
        `api/DraftReport/GetSendEmailHistory/${localStorage.getItem("userId")}`
      )
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableIdsendMail,
            dtData,
            this.tableColumnsendMail
          );
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  getSelectedRiskTypeOpts(_riskTypeIds) {
    let riskTypeNamesArray = [];
    if (_riskTypeIds != null) {
      for (let i of this.riskTypeOptions) {
        if (_riskTypeIds.indexOf(i.id) > -1) {
          riskTypeNamesArray.push({
            id: i.id,
            name: i.name,
          });
        }
      }
    }
    return riskTypeNamesArray;
  }
  getRiskTypes() {
    this.spinner.show();
    this.commonApi.getAllRiskTypes().subscribe(
      (posts) => {
        this.riskTypeOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
      }
    );
  }
  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditID = localStorage.getItem("auditId");

    this.isApprover = this.utils.checkIfApprover();

    this.loadDaftReportTable();

    // this.tableApiUrl = `api/draftreport/GetByAudit/${this.AuditID}`;
    // this.tableFilters = new BehaviorSubject({ init: true });
    // this.tableApiUrlHistory = `api/DraftReport/getDraftReportHistory`;
    // this.tableFiltersHistory = new BehaviorSubject({ init: false });

    this.getAllImpact();
    this.getAllReportConsideration();
    this.getAllRecommendation();
    this.getAllRootCause();
    this.getRiskTypes();
    this.getImpactOptions();
    this.getReportConsiderationOptions();
    this.getRecommendationOptions();
    this.getRootCauseOptions();

    this.initDropdownSettings(
      false,
      "id",
      "name",
      "Select All",
      "Deselect All",
      3,
      false,
      true
    );

    $(document).ready(() => {
      $("#auditReportComponent").on("click", ".editReport", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        if (dataId) {
          // let reportData = $("#" + dataId).data();
          let reportData = window["jQuery"]("#" + dataId).data();
          this.editReport(reportData);
          this.getDRFiles();
        }
      });

      $("#auditReportComponent").on("change", ".selectedDR", (e) => {
        let isChecked = $(e.target).is(":checked");
        let dataId = $(e.currentTarget).attr("data-id");
        let reportData = window["jQuery"]("#" + dataId).data();

        this.handleSelectedDRData(dataId, reportData, isChecked);
      });

      $("#auditReportComponent").on("click", ".emailDR", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        if (dataId) {
          let dataId = $(e.currentTarget).attr("data-id");
          this.emailDR(dataId);
        }
      });

      $('a[href="#reportingTab"], #reportingTab a[href="#auditreport"]').on(
        "click",
        (e) => {
          // if (typeof this.tableFilters !== "undefined") {
          // this.tableFilters.next({ init: true });
          // }

          this.loadDaftReportTable();
        }
      );

      $("#auditReportComponent").on("click", ".viewContent", (e) => {
        let title = $(e.currentTarget).attr("data-title");
        let content = $(e.currentTarget).attr("data-content");
        content = decodeURI(content);
        this.showContentModal(title, content);
      });

      $("#selectedActionPlans").on("click", ".editActionPlan", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.actionPlanIndex = dataIndex;
          this.isEditActionPlan = true;
          this.editActionPlan(parseInt(dataIndex));
        }
      });

      $("#selectedActionPlans").on("click", ".removeActionPlan", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeActionPlan(parseInt(dataIndex));
        }
      });

      $("#uploadedDRFiles").on("click", ".removeFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeUploadedFile(parseInt(dataIndex));
        }
      });

      $("#uploadedDRFiles").on("click", ".downloadFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.downloadFile(parseInt(dataIndex));
        }
      });
      // $(".wrapper1").scroll(function () {
      //   $(".wrapper2").scrollLeft($(".wrapper1").scrollLeft());
      // });

      // $(".wrapper2").scroll(function () {
      //   $(".wrapper1").scrollLeft($(".wrapper2").scrollLeft());
      // });

      // $(".div1").width($("#dn_table").width());
      // $(".div2").width($("#dn_table").width());
    });
  }
}
