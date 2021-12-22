import { Component, OnInit, ViewChild, Input } from "@angular/core";
import { tableColumn } from "./../../../../common/table/table.model";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { ApiService } from "src/app/services/api/api.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { BehaviorSubject } from "rxjs";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
import { IDropdownSettings } from "ng-multiselect-dropdown";
import { dependenciesFromGlobalMetadata } from "@angular/compiler/src/render3/r3_factory";

@Component({
  selector: "app-discussionnote",
  templateUrl: "discussionnote.component.html",
  styleUrls: ["./discussionnote.component.css"],
})
export class DiscussionNoteComponent implements OnInit {
  constructor(
    private api: ApiService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("dnForm", { static: false }) dnForm: NgForm;
  @ViewChild("fileInputDN", { static: false }) fileInputDN;
  @ViewChild("fileInputMainDN", { static: false }) fileInputMainDN;
  @ViewChild("fileInput", { static: false }) fileInput;

  @Input() userOpts: any = [];
  @Input() approverOpts: any = [];

  dropdownSettings: IDropdownSettings = {};

  accessRights: any = {};
  isStackHolder: boolean = false;
  public Editor = ClassicEditor;

  uploadedFiles: any = [];
  selectedFiles: any = [];

  uploadedMainFiles: any = [];
  selectedMainFiles: any = [];

  uploadedFileIndex: number;
  uploadedMainFileIndex: number;

  AuditID: string;
  ObservationHeadingText: any = "";
  DetailedObservationText: any = "";
  RootCauseText: any = "";
  RisksText: any = "";
  FinancialImpactText: any = "";
  RiskTypeIdText: any = "";

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

  /* Dn Table Config*/
  id: string = "";
  RACMNumber: string = "";
  RACMNumberText: string = "";
  DiscussionNumber: string = "";
  RACM_Ids: any = [];
  ObservationHeading: string = "";

  FinancialImpact: string = "";
  FieldBackground: string = "";
  ManagementComments: string = "";
  ReviewerId: string = "";
  PersonResponsibleID: string = "";
  DetailedObservation: string = "";
  RootCause: string = "";
  FlagIssueForReport: string = "";
  Justification: string = "";
  ObservationGrading: string = "";
  Risks: string = "";
  RiskTypeIds: any = [];
  selectedRiskTypeIds = [];
  Status: boolean = true;
  isCompleted: boolean = false;
  isApprover: boolean = false;
  RACMOpts: any = [];
  // isRepeat: boolean = false;
  // isSystemImprovement: boolean = false;
  // isRedFlag: boolean = false;
  // isLeadingPractices: boolean = false;
  PotentialSaving: string = "";
  RealisedSaving: string = "";
  Leakage: string = "";
  Recommendation: string = "";

  impactOptions: any = [];
  selectedImpacts: any = [];
  impactAll: any = [];
  recommendationOptions: any = [];
  recommendationAll: any = [];
  selectedRecommendations: any = [];
  rootCauseOptions: any = [];
  rootCauseAll: any = [];
  selectedRootCauses: any = [];


  riskTypeOptions: any = [];
  selectedIds: any = [];
  dnMailTotal: number = 0;
  dnMailSent: number = 0;

  // tableApiUrl: string = "";
  // tableFilters;
  tableFiltersHistory: BehaviorSubject<{ init: boolean }>;
  tableApiUrlGrading: string = "";
  tableFiltersGraging;

  tableId: string = "dn_table";
  tableIdGrading: string = "dn_tableGrading";
  tableIdHistory: string = "discussnotehistory_table";

  tableColumnsDn: tableColumn[] = [
    {
      title: "<input type='checkbox' id='chkAllDNote' />",
      data: "id",
      className: "text-center",
      render: (data, type, row, meta) => {
        return (
          "<input type='checkbox' data-id='" +
          data +
          "' class='chkSingleDNote' />"
        );
      },
    },
    {
      title: "Discussion No",
      data: "discussionNumber",
    },
    {
      title: "Discussion Heading",
      data: "observationHeading",
    },
    {
      title: "Detailed Observation",
      data: "detailedObservation",
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
      data: "racM_Ids",
      render: function (data) {
        return Array.isArray(data) ? data.join(", ") : "";
      },
    },
    {
      title: "Root Cause",
      data: "rootCause",
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
      data: "riskTypes",
      render: function (data) {
        let names = "";

        data.forEach((element) => {
          names += element.name + ", ";
        });

        return names.trim().slice(0, -1);
      },
    },
    {
      title: "Risks/ Implications",
      data: "risks",
      render: (data) => {
        if (data.length > 100) {
          return (
            "<span>" +
            data.slice(0, 100) +
            '</span><br><a href="javascript:void(0)" data-title="Risks/ Implications" data-content="' +
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
    // data: "financialImpact",
    // },
    {
      title: "Observations Grading",
      data: "",
      render: function (data, row, rowData) {
        let grading = rowData.observationGrading;
        switch (grading) {
          case 0:
            grading = "Low";
            break;
          case 1:
            grading = "Medium";
            break;
          case 2:
            grading = "High";
            break;
          case 3:
            grading = "Critical";
            break;
          default:
            grading = "Repeat";
        }
        return grading;
      },
    },
    {
      title: "Flag Issue",
      data: "flagIssueForReport",
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
      data: "",
      render: function (data, row, rowData) {
        let status = rowData.status;
        switch (status) {
          case "INREVIEW":
            status = "In Review";
            break;
          case "INPROGRESS":
            status = "In Progress";
            break;
          case "COMPLETED":
            status = "Completed";
            break;
          default:
            status = "Not Started";
        }
        return status.toUpperCase();
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        let files: string = "";
        if (row.filesList) {
          row.filesList.forEach((element) => {
            files += `<div id='div-${element.id}'>
            <a href='javascript:void(0);' data-fileId='${element.id}' class='downloadMainFile'>${element.originalFileName}</a> | 
            <a href='javascript:void(0);' data-fileId='${element.id}' class='deleteMainFile'>Remove</a></div>`;
          });
        }

        let buttons = `<button type="button" data-id="${data}" class="btn btn-sm btn-primary UploadFileID">
          <i class="fa fa-upload"></i></button><br />${files}`;

        return buttons;
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
            '" class="btn btn-sm btn-info editDn"><i class="fa fa-edit"></i></button>';
        // if (!this.isStackHolder)
        buttons +=
          '<button type="button" data-id="' +
          data +
          '" class="btn btn-sm btn-primary emailDn" title="Send email for approval"><i class="fa fa-send"></i></button>';

        return buttons;
      },
    },
  ];
  tableColumnsHistory: tableColumn[] = [
    {
      title: "User",
      data: "user",
      render: (data) => {
        return data ? data.firstName + " " + data.lastName : "";
      },
    },
    // {
    //   title: "Discussion Note",
    //   data: "discussionNote.discussionComments",
    // },
    {
      title: "Status",
      data: "status",
    },
    {
      title: "Date",
      data: "discussNoteDate",
      render: (data) => {
        return data ? this.utils.formatDate(data) : null;
      },
    },
  ];

  isEdit: boolean = false;
  formVisible: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.formVisible = false;
      this.isEdit = false;
      this.clearForm();
      // this.tableFilters = new BehaviorSubject({ init: true });
      this.loadDNTable();
    },
  };

  confirmSaveDN(e) {
    e.preventDefault();

    if (this.dnForm.invalid) {
      this.utils.showValidationMsg(this.dnForm.form);

      return false;
    }

    window["jQuery"]("#ConfimSendDNForApprovalModal").modal("show");
  }

  saveSendDn() {
    this.saveDn();
    this.updateStatus("inreview");
  }

  saveDn() {
    if (this.dnForm.invalid) {
      this.utils.showValidationMsg(this.dnForm.form);
      return false;
    }

    if (this.isEdit) {
      let saveDn = true;

      if (this.isCompleted) {
        saveDn = confirm(
          "This discussion note status is marked completed. Are you sure you want to save the changes ?"
        );
      }

      if (saveDn) this.updateDn();
    } else this.addNewDn();

    window["jQuery"]("#ConfimSendDNForApprovalModal").modal("hide");

    this.getSummary();
  }

  addNewDn() {
    this.spinner.show();

    let postData = this.dnForm.form.value;
    postData.AuditId = this.AuditID;
    postData.ObservationGrading = parseInt(postData.ObservationGrading);
    // postData.IsRepeat = this.isRepeat;
    // postData.isSystemImprovement = this.isSystemImprovement;
    // postData.isRedFlag = this.isRedFlag;
    // postData.isLeadingPractices = this.isLeadingPractices;
    // postData.PotentialSaving = this.PotentialSaving.toString();
    // postData.RealisedSaving = this.RealisedSaving.toString();
    // postData.Leakage = this.Leakage.toString();
    postData.Status = "NOTSTARTED";
    //  postData.Impacts = this.getImpactIds();
    // postData.RootCauses = this.getRootCauseIds();
    // postData.Recommendations = this.getRecommendationIds();
    let flagIssueForReportSelectedValue =
      postData.FlagIssueForReport.toLowerCase();

    postData.FlagIssueForReport =
      postData.FlagIssueForReport.toLowerCase() === "yes" ? true : false;
    postData.RACM_Ids = [postData.RACMNumber];
    postData.RiskTypeIds = this.getSelectedRiskTypeIds();
  postData.DiscussionNumber=this.DiscussionNumber.toString();
    delete postData.RACMNumber;

    this.api.insertData("api/discussionnote", postData).subscribe(
      (response) => {
        let result: any = response;
        this.id = result.id;
        this.AuditID = result.auditId;

        if (this.selectedFiles.length > 0) this.saveSelectedFiles();

        this.notifyService.success("Discussion Note Added Successfully");
        this.spinner.hide();
        this.handleFormView.hide();
      },
      (error) => {
        if (error.status == 208) {
          this.notifyService.error("Discussion number already Exists.");
        } else {
          this.notifyService.error(error.error);
        }
        postData.FlagIssueForReport = flagIssueForReportSelectedValue;
        this.spinner.hide();
      }
    );
  }

  updateDn() {
    this.spinner.show();
    let postData = {
      id: this.id,
      AuditId: this.AuditID,
      ObservationGrading: parseInt(this.ObservationGrading),
      FlagIssueForReport:
        this.FlagIssueForReport.toLowerCase() === "yes" ? true : false,
      RACM_Ids: [this.RACMNumber],
      Status: "INPROGRESS",
      FieldBackground: this.FieldBackground,
      DiscussionNumber: this.DiscussionNumber.toString(),
      ObservationHeading: this.ObservationHeading,
      DetailedObservation: this.DetailedObservation,
      RootCause: this.RootCause,
      Risks: this.Risks,
      RiskTypeIds: this.getSelectedRiskTypeIds(),
      PersonResponsibleID: this.PersonResponsibleID,
      ReviewerId: this.ReviewerId,
      ManagementComments: this.ManagementComments,
      // isRepeat: this.isRepeat,
      // isSystemImprovement: this.isSystemImprovement,
      // isRedFlag: this.isRedFlag,
      // isLeadingPractices: this.isLeadingPractices,
      Recommendation: this.Recommendation,
      // PotentialSaving: this.PotentialSaving.toString(),
      // RealisedSaving: this.RealisedSaving.toString(),
      // Leakage: this.Leakage.toString(),
      // Impacts: this.getImpactIds(),
      // RootCauses : this.getRootCauseIds(),
      // Recommendations : this.getRecommendationIds(),
    };

    this.api.updateData("api/discussionnote", postData).subscribe(
      (response) => {
        if (this.selectedFiles.length > 0) this.saveSelectedFiles();

        this.notifyService.success("Discussion Note Updated Successfully");
        this.spinner.hide();
        this.handleFormView.hide();
      },
      (error) => {
        this.spinner.hide();
      }
    );
  }

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  addDn() {
    this.handleFormView.show();
  }

  editDn(dnData) {
    this.isEdit = true;

    this.id = dnData.id;
    this.DiscussionNumber = dnData.discussionNumber;
    this.RACMNumber =
      Array.isArray(dnData.racM_Ids) && dnData.racM_Ids.length === 1
        ? dnData.racM_Ids[0]
        : "";
    this.RACMNumberText =
      Array.isArray(dnData.racM_Ids) && dnData.racM_Ids.length === 1
        ? dnData.racM_Ids[0]
        : "";

    this.RACM_Ids = dnData.racM_Ids;
    this.ObservationHeading = dnData.observationHeading;
    this.ObservationHeadingText = dnData.observationHeading;
    this.DetailedObservation = dnData.detailedObservation;
    this.DetailedObservationText = dnData.detailedObservation;

    this.FinancialImpact = dnData.financialImpact;
    this.FieldBackground = dnData.fieldBackground;
    this.ManagementComments = dnData.managementComments;
    this.ReviewerId = dnData.reviewerId;
    this.PersonResponsibleID = dnData.personResponsibleID;
    this.DetailedObservation = dnData.detailedObservation;
    this.RootCause = dnData.rootCause;
    this.FlagIssueForReport = dnData.flagIssueForReport ? "yes" : "no";
    this.Justification = dnData.justification;
    this.ObservationGrading = dnData.observationGrading;
    this.Risks = dnData.risks;
    this.selectedRiskTypeIds = this.getSelectedRiskTypeOpts(dnData.riskTypeIds);
    this.Status =
      dnData.status === null || dnData.status.toLowerCase() === "inprogress"
        ? true
        : false;
    this.isCompleted =
      dnData.status && dnData.status.toLowerCase() === "completed"
        ? true
        : false;
    // this.isRepeat = dnData.isRepeat;
    // this.isSystemImprovement = dnData.isSystemImprovement;
    // this.isRedFlag = dnData.isRedFlag;
    // this.isLeadingPractices = dnData.isLeadingPractices;
    // this.PotentialSaving = dnData.potentialSaving;
    // this.RealisedSaving = dnData.realisedSaving;
    // this.Leakage = dnData.leakage;

    var userId = localStorage.getItem("userId");
    if (dnData.reviewer != null)
      if (userId === dnData.reviewer.id) this.isApprover = true;
      else this.isApprover = false;

    this.Recommendation = dnData.recommendation;
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
    // this.getImpactOptions();
    // this.selectedImpacts = this.getSelectedImpactOpts(dnData.impacts);
    // this.getRecommendationOptions();
    // this.selectedRecommendations = this.getSelectedRecommendationOpts(dnData.recommendations);
    // this.getRootCauseOptions();
    // this.selectedRootCauses = this.getSelectedRootCauseOpts(dnData.rootCauses);
    this.handleFormView.show();
  }

  clearForm() {
    this.id = "";
    this.RACMNumber = "";
    this.RACMNumberText = "";
    this.DiscussionNumber = "";
    this.RACM_Ids = [];
    this.ObservationHeading = "";
    this.FinancialImpact = "";
    this.FieldBackground = "";
    this.ManagementComments = "";
    this.ReviewerId = "";
    this.PersonResponsibleID = "";
    this.DetailedObservation = "";
    this.RootCause = "";
    this.FlagIssueForReport = "";
    this.Justification = "";
    this.ObservationGrading = "";
    this.Risks = "";
    this.RiskTypeIds = [];
    this.selectedRiskTypeIds = [];
    this.Status = false;
    this.isCompleted = false;
    this.Recommendation = "";
    // this.isRepeat = false;
    // this.isSystemImprovement = false;
    // this.isRedFlag = false;
    // this.isLeadingPractices = false;
    this.PotentialSaving = "";
    this.RealisedSaving = "";
    this.Leakage = "";
  }

  fillRacmOpts() {
    this.spinner.show();
    this.api
      .getData("api/racmauditprocedure/GetIneffectiveRACMs/" + this.AuditID)
      .subscribe(
        (racmOpts) => {
          this.RACMOpts = racmOpts;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  saveToDraftReport() {
    let isYes = confirm(
      `Are you sure you want save ${this.DiscussionNumber} in draft audit report? `
    );
    if (isYes) {
      this.spinner.show();
      this.api
        .insertData("api/draftreport/SaveAsDraft", {
          DiscussionNoteID: this.id,
          AuditId: this.AuditID,
          ObservationGrading: this.ObservationGrading,
        })
        .subscribe(
          (response) => {
            this.spinner.hide();
            this.notifyService.success(
              "Saved to Draft Audit Report successfully"
            );
            this.handleFormView.hide();
          },
          (error) => {
            this.spinner.hide();
            this.notifyService.error(error.error);
            this.handleFormView.hide();
          }
        );
    }
  }

  updateStatus(status: string, params?: {}) {
    let isConfirm = true;
    let statusMsg = {
      inreview:
        "Are you sure you want to sent " +
        this.DiscussionNumber +
        " for approval?",
      completed:
        "Are you sure you want to approve " + this.DiscussionNumber + "?",
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
        .updateData("api/discussionnote/updatediscussionnotestatus", statusObj)
        .subscribe((response) => {
          this.notifyService.success("Request Processed Successfully");
          this.handleFormView.hide();
        });
    }

    window["jQuery"]("#ConfimSendDNForApprovalModal").modal("hide");
  }

  rejectDN() {
    if (this.Justification && this.Justification !== "") {
      this.updateStatus("inprogress", { justification: this.Justification });
      this.handleRejectModal("hide");
    } else {
      this.notifyService.error("Justification is required");
    }
  }

  handleRejectModal(action) {
    if (action) {
      window["jQuery"]("#rejectDNModal").modal(action);
    }
  }

  showContentModal(title, content) {
    window["jQuery"]("#dnContentModal #dncontent-title").html("").html(title);
    window["jQuery"]("#dnContentModal #dncontent").html("").html(content);
    window["jQuery"]("#dnContentModal").modal("show");
  }

  exportToExcel() {
    this.spinner.show();

    this.api
      .downloadFile(`api/discussionnote/downloadexcel/${this.AuditID}`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "DiscussionNotes.xlsx");
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
    this.spinner.hide();
  }
  sampleExportToExcel() {
    this.spinner.show();
    this.api
      .downloadFile(`api/discussionnote/sampledownloadexcel/${this.AuditID}`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });
          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "SampleDiscussionNotes.xlsx");
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
    this.spinner.hide();
  }
  exportToPDF() {
    this.spinner.show();
    this.api
      .downloadFile(`api/discussionnote/downloadpdf/${this.AuditID}`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/pdf",
            // type:
            // "application/vnd.openxmlformats-officedocument.presentationml.presentation",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "DiscussionNotes.pdf");
            // link.setAttribute("download", "DiscussionNotes.pptx");
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
    this.spinner.hide();
  }
  exportToPPT() {
    this.spinner.show();

    try {
      this.api
        .downloadFile(`api/discussionnote/downloadppt/${this.AuditID}`)
        .subscribe(
          (blob) => {
            const objblob: any = new Blob([blob], {
              type: "application/vnd.openxmlformats-officedocument.presentationml.presentation",
              // type:
              // "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            });

            let link = document.createElement("a");

            if (link.download !== undefined) {
              let url = URL.createObjectURL(blob);
              link.setAttribute("href", url);
              link.setAttribute("download", "DiscussionNotes.pptx");
              // link.setAttribute("download", "DiscussionNotes.pptx");
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
    } catch (error) {
      this.spinner.hide();
      console.error("API GetDashboard: ", error);
    }
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

  emailDNote(id) {
    let postData = {
      Id: id,
    };
    this.spinner.show();
    this.api.insertData("api/discussionnote/sendemail", postData).subscribe(
      (response) => {
        let result: any = response;
        this.spinner.hide();
        if (result.sent)
          this.notifyService.success(
            "Discussion note email sent successfully to reviewer."
          );

        this.loadDNTable();
        // this.tableFilters.next({ init: true });
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  uploadTableFile() {
    let selectedMainFiles: any = [];
    let filesElem = this.fileInputMainDN.nativeElement;

    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        selectedMainFiles.push(file);
      }

      this.fileInputMainDN.nativeElement.value = "";
    }

    var formData = new FormData();

    for (var file in selectedMainFiles) {
      formData.append("files", selectedMainFiles[file]);
    }

    formData.append("Id", this.id);
    formData.append("auditId", this.AuditID);
    formData.append("module", "discussionnote");

    this.spinner.show();

    this.api.insertData("api/discussionnote/uploadfile", formData).subscribe(
      (upload) => {
        let result: any = upload;

        if (result.isUploaded) {
          for (let file of result.files) {
            this.addUploadedFileToMainTable(file, true);
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

  addUploadedFileToMainTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#dn_table > tbody")
      .find(`tr#${this.id}`)
      .find("td:eq(11)");

    let files = `<div id='div-${uploadFileObj.id}'>
    <a href='javascript:void(0);' data-fileId='${uploadFileObj.id}' class='downloadMainFile'>${uploadFileObj.originalFileName}</a> | 
    <a href='javascript:void(0);' data-fileId='${uploadFileObj.id}' class='deleteMainFile'>Remove</a></div>`;

    uploadedFilesTable.append(files);
  }

  downloadMainFile(fileId, fileName) {
    this.spinner.show();

    this.api
      .downloadFile("api/discussionnote/downloadfile/" + fileId)
      .subscribe(
        (blob) => {
          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", fileName);
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

  deleteMainFile(element, fileId) {
    this.api.deleteData("api/discussionnote/removefile/" + fileId).subscribe(
      (upload) => {
        let result: any = upload;

        if (result.isDeleted) $(`#div-${fileId}`).remove();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  sendMultipleEmail() {
    if (this.selectedIds.length == 0)
      this.notifyService.error(
        "Please select at least one record to send email."
      );
    else {
      window["jQuery"]("#sendDNMultipleMailModal").modal("show");

      this.dnMailTotal = this.selectedIds.length;
      this.dnMailSent = 0;

      let sentMailCounter = 0;
      this.spinner.show();
      this.selectedIds.forEach((element, index) => {
        let postData = {
          Id: element,
        };

        this.api.insertData("api/discussionnote/sendemail", postData).subscribe(
          (response) => {
            let result: any = response;

            if (result.sent) {
              sentMailCounter++;

              this.dnMailSent = sentMailCounter;
            }
          },
          (error) => {
            console.log(error);
          }
        );
      });
      this.spinner.hide();
      this.selectedIds = [];
      // this.tableFilters.next({ init: true });
      this.loadDNTable();
    }
  }

  getSummary() {
    this.spinner.show();
    this.api.getData("api/discussionnote/getsummary/" + this.AuditID).subscribe(
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
    this.loadDNTable();

    // this.AuditID = localStorage.getItem("auditId");
    // this.tableApiUrl = `api/discussionnote/GetByAuditByGrading/${this.AuditID}/${option}`;
    // this.tableFilters = new BehaviorSubject({ init: true });

    // this.tableApiUrlGrading = `api/discussionnote/GetByAuditByGrading/${this.AuditID}/${option}`;
    // this.tableFiltersGraging = new BehaviorSubject({ init: true });
  }

  FilterByStatus(option) {
    this.filterStatus = option;
    this.loadDNTable();
  }

  loadDNTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(
        `api/discussionnote/GetByAudit/${this.AuditID}/${this.filterGrading}/${this.filterStatus}`
      )
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumnsDn,true
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
    this.loadDNTable();
  }

  uploadDNFile() {
    let filesElem = this.fileInputDN.nativeElement;

    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        this.selectedFiles.push(file);

        let fileModel = {
          originalFileName: file.name,
          uploadedDatetime: this.utils.getCurrentDateddmmyyyy(),
        };

        this.addUploadedFileToTable(fileModel, true);
      }

      this.fileInputDN.nativeElement.value = "";
    }
  }

  addUploadedFileToTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#uploadedDNFiles tbody");
    let noOfRecords = uploadedFilesTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#uploadedDNFiles tbody .norecords").length) {
        $("#uploadedDNFiles tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let fileName = `<a href="javascript:void(0);" class="downloadFile" data-index="${noOfRecords}">${uploadFileObj.originalFileName}</a>`;
      if (typeof uploadFileObj.uploadedFileName === "undefined")
        fileName = uploadFileObj.originalFileName;

      let resHtml = `<tr>
      <td>${fileName}</td>
          <td>${this.utils.formatDbDateToDMY(
            uploadFileObj.uploadedDatetime
          )}</td>
          <td>`;
      if (!this.isStackHolder)
        resHtml += `
            <button type="button" data-index="${noOfRecords}"  class="btn btn-sm btn-danger removeFile">
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
      $("#uploadedDNFiles tbody").html("");

      for (let file of this.uploadedFiles) {
        this.addUploadedFileToTable(file);
      }
    } else {
      $("#uploadedDNFiles tbody").html(
        '<tr class="norecords"><td colspan="3" class="text-center">No Records Found</td></tr>'
      );
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
        this.api
          .deleteData("api/discussionnote/removefile/" + fileId)
          .subscribe(
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
          .downloadFile("api/discussionnote/downloadfile/" + fileId)
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

  getDNFiles() {
    this.spinner.show();
    this.api
      .getData("api/discussionnote/getallfiles/" + this.AuditID + "/" + this.id)
      .subscribe(
        (posts) => {
          this.uploadedFiles = posts;
          this.spinner.hide();
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

    for (var file in this.selectedFiles) {
      formData.append("files", this.selectedFiles[file]);
    }

    formData.append("Id", this.id);
    formData.append("auditId", this.AuditID);
    formData.append("module", "discussionnote");
    this.spinner.show();
    this.api.insertData("api/discussionnote/uploadfile", formData).subscribe(
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
      "auditexecution"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "discussionnote"
    )[0];
  }
  history = {
    show: () => {
      window["jQuery"]("#HistoryModal").modal("show");
      this.getHistoryData();
      // this.tableFiltersHistory.next({ init: true });
    },
    hide: () => {
      window["jQuery"]("#HistoryModal").modal("hide");
    },
  };
  getHistoryData() {
    this.spinner.show();
    this.api
      .getData(`api/DiscussionNote/getDiscussNoteHistory/${this.id}`)
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
  importExcel() {
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");
    this.spinner.show();
    this.api
      .insertData(
        "api/DiscussionNote/importexcel/" + this.AuditID + "/" + userid,
        formData
      )
      .subscribe(
        (response) => {
          this.spinner.hide();
          var excepitonCount = response["excptionCount"];
          var totalRows = response["totalRow"];
          var exceptionRowNumber = response["excptionRowNumber"];
          exceptionRowNumber = exceptionRowNumber.replace(/,\s*$/, "");
          var successCount = totalRows - excepitonCount;
          var msg =
            "Total Rows : " +
            totalRows +
            "<br> Success Count : " +
            successCount +
            " <br>Exception Count : " +
            excepitonCount +
            "<br>Exception RowNumber : " +
            exceptionRowNumber;
          this.notifyService.success(msg, "", {
            enableHtml: true,
          });
          console.log(msg);
          this.handleFormView.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getSelectedRiskTypeIds() {
    let riskTypeIds = [];

    if (this.selectedRiskTypeIds != null) {
      for (let i of this.selectedRiskTypeIds) {
        riskTypeIds.push(i.id);
      }
    }

    return riskTypeIds;
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
      allowSearchFilter: isAllowSearchFilter,
      closeDropDownOnSelection: closeDropDownOnSelection,
    };
  }
  // getImpactOptions() {
  //   this.spinner.show();
  //   this.commonApi.getImpacts().subscribe(
  //     (posts) => {
  //       this.impactOptions = posts;
  //       this.spinner.hide();
  //     },
  //     (error) => {
  //       this.spinner.hide();
  //       console.log(error);
  //     }
  //   );
  // }
  // getSelectedImpactOpts(impactsArray) {
  //   let impactNamesArray = [];
  //   if (impactsArray !== null) {
  //     for (let i of this.impactAll) {
  //       if (impactsArray.indexOf(i.id) > -1) {
  //         impactNamesArray.push({ id: i.id, name: i.name });
  //       }
  //     }
  //   }
  //   return impactNamesArray;
  // }
  // getImpactIds() {
  //   let impactsArray = [];
  //   this.selectedImpacts.forEach((element) => {
  //     impactsArray.push(element.id);
  //   });
  //   return impactsArray;
  // }
  // getAllImpact() {
  //   this.spinner.show();
  //   this.commonApi.getImpacts().subscribe(
  //     (posts) => {
  //       this.impactAll = posts;
  //       this.spinner.hide();
  //     },
  //     (error) => {
  //       this.spinner.hide();
  //       console.log(error);
  //     }
  //   );
  // }
  // getRecommendationOptions() {
  //   this.spinner.show();
  //   this.commonApi.getRecommendation().subscribe(
  //     (posts) => {
  //       this.recommendationOptions = posts;
  //       this.spinner.hide();
  //     },
  //     (error) => {
  //       this.spinner.hide();
  //       console.log(error);
  //     }
  //   );
  // }
  
  // getAllRecommendation() {
  //   this.spinner.show();
  //   this.commonApi.getRecommendation().subscribe(
  //     (posts) => {
  //       this.recommendationAll = posts;
  //       this.spinner.hide();
  //     },
  //     (error) => {
  //       this.spinner.hide();
  //       console.log(error);
  //     }
  //   );
  // }

  // getRecommendationIds() {
  //   let recommendationsArray = [];

  //   this.selectedRecommendations.forEach((element) => {
  //     recommendationsArray.push(element.id);
  //   });

  //   return recommendationsArray;
  // }
  // getSelectedRecommendationOpts(recommendationsArray) {
  //   let recommendationNamesArray = [];

  //   if (recommendationsArray !== null) {
  //     for (let i of this.recommendationAll) {
  //       if (recommendationsArray.indexOf(i.id) > -1) {
  //         recommendationNamesArray.push({
  //           id: i.id,
  //           name: i.name,
  //         });
  //       }
  //     }
  //   }

  //   return recommendationNamesArray;
  // }

  // getSelectedRootCauseOpts(rootCausesArray) {
  //   let rootCauseNamesArray = [];

  //   if (rootCausesArray !== null) {
  //     for (let i of this.rootCauseAll) {
  //       if (rootCausesArray.indexOf(i.id) > -1) {
  //         rootCauseNamesArray.push({
  //           id: i.id,
  //           name: i.name,
  //         });
  //       }
  //     }
  //   }

  //   return rootCauseNamesArray;
  // }
  // getRootCauseOptions() {
  //   this.spinner.show();
  //   this.commonApi.getRootCause().subscribe(
  //     (posts) => {
  //       this.rootCauseOptions = posts;
  //       this.spinner.hide();
  //     },
  //     (error) => {
  //       this.spinner.hide();
  //       console.log(error);
  //     }
  //   );
  // }

  // getAllRootCause() {
  //   this.spinner.show();
  //   this.commonApi.getRootCause().subscribe(
  //     (posts) => {
  //       this.rootCauseAll = posts;
  //       this.spinner.hide();
  //     },
  //     (error) => {
  //       this.spinner.hide();
  //       console.log(error);
  //     }
  //   );
  // }

  // getRootCauseIds() {
  //   let rootCausesArray = [];

  //   this.selectedRootCauses.forEach((element) => {
  //     rootCausesArray.push(element.id);
  //   });

  //   return rootCausesArray;
  // }
  ngOnInit() {
    this.checkAccess();
    this.getRiskTypes();
    // this.getAllImpact();
    // this.getImpactOptions();
    // this.getAllRecommendation();
    // this.getRecommendationOptions();
    // this.getAllRootCause();
    // this.getRootCauseOptions();
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

    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditID = localStorage.getItem("auditId");

    this.loadDNTable();
    this.fillRacmOpts();
    // this.getSummary();
    // this.tableApiUrl = `api/discussionnote/GetByAudit/${this.AuditID}`;
    // this.tableFilters = new BehaviorSubject({ init: true });

    $(document).ready(() => {
      $("#discussionNoteComponent").on("click", ".editDn", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        // let dnData = $("#" + dataId).data();
        let dnData = window["jQuery"]("#" + dataId).data();

        this.editDn(dnData);
        this.getDNFiles();
      });

      $("#discussionNoteComponent").on("click", ".emailDn", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.emailDNote(dataId);
      });

      $("#discussionNoteComponent").on("click", ".UploadFileID", (e) => {
        e.preventDefault();

        let dataId = $(e.currentTarget).attr("data-id");

        this.id = dataId;
        this.fileInputMainDN.nativeElement.click();
      });

      $("#discussionNoteComponent").on("click", ".downloadMainFile", (e) => {
        e.preventDefault();

        let fileId = $(e.currentTarget).attr("data-fileId");
        let fileName = $(e.currentTarget).html();

        this.downloadMainFile(fileId, fileName);
      });

      $("#discussionNoteComponent").on("click", ".deleteMainFile", (e) => {
        let fileId = $(e.currentTarget).attr("data-fileId");

        this.deleteMainFile($(e.currentTarget), fileId);
      });

      $("#discussionNoteComponent").on("change", "#chkAllDNote", (e) => {
        $("#dn_table > tbody > tr")
          .find(".chkSingleDNote")
          .prop("checked", $(e.currentTarget).is(":checked"));

        let Ids: any = [];
        $("#dn_table > tbody > tr").each(function () {
          let row = $(this);
          Ids.push(row.attr("id"));
        });

        if ($(e.currentTarget).is(":checked")) this.selectedIds = Ids;
        else this.selectedIds = [];
      });

      $("#discussionNoteComponent").on("change", ".chkSingleDNote", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        if ($(e.currentTarget).is(":checked")) this.selectedIds.push(dataId);
        else {
          this.selectedIds.forEach((element, index) => {
            if (element == dataId) delete this.selectedIds[index];
          });
        }

        if (
          $("#dn_table > tbody > tr").find(".chkSingleDNote").length ==
          $("#dn_table > tbody > tr").find(".chkSingleDNote:checked").length
        )
          $("#dn_table > thead > tr")
            .find("#chkAllDNote")
            .prop("checked", true);
        else
          $("#dn_table > thead > tr")
            .find("#chkAllDNote")
            .prop("checked", false);
      });

      $('#auditExecutionTab a[href="#discussionnote"]').on("click", (e) => {
        // if (typeof this.tableFilters !== "undefined") {
        this.fillRacmOpts();
        // this.tableFilters.next({ init: true });
        // }

        this.loadDNTable();
      });

      $("#discussionNoteComponent").on("click", ".viewContent", (e) => {
        let title = $(e.currentTarget).attr("data-title");
        let content = $(e.currentTarget).attr("data-content");
        content = decodeURI(content);
        this.showContentModal(title, content);
      });

      $("#uploadedDNFiles").on("click", ".removeFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.removeUploadedFile(parseInt(dataIndex));
        }
      });

      $("#uploadedDNFiles").on("click", ".downloadFile", (e) => {
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
