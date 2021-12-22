import { Component, OnInit, ViewChild, ElementRef, Input } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ApiService } from "src/app/services/api/api.service";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-testing",
  templateUrl: "./testing.component.html",
  styleUrls: ["./testing.component.css"],
  providers: [],
})
export class TestingComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private api: ApiService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("apForm", { static: false }) apForm: NgForm;
  @ViewChild("tocFiles", { static: false }) tocFiles: ElementRef;
  @ViewChild("fileInput", { static: false }) fileInput;

  TestingOfControlUploads: any = [];

  accessRights: any = {};
  isStackHolder: boolean = false;
  public Editor = ClassicEditor;
  gridData: any = [];

  AuditID: string = "";
  businessCycleOptions: any = [];
  processlevel1Options: any = [];
  processlevel2Options: any = [];

  BusinessCycle: string = "";
  ProcessL1: string = "";
  ProcessL2: string = "";

  isSendForApproval: boolean = false;

  selectedIds: any = [];

  tocMailTotal: number = 0;
  tocMailSent: number = 0;

  /* RACM Details*/

  id: string = "";
  RiskId: string = "";
  RiskTitle: string = "";
  RiskDesc: string = "";
  RiskRating: string = "";
  ControlId: string = "";
  ControlTitle: string = "";
  ControlDescription: string = "";
  ControlNature: string = "";
  ControlFrequency: string = "";
  ControlType: string = "";
  ControlOwner: string = "";

  /* Audit Procedure Details*/
  ProcedureId: string = "";
  ProcedureTitle: string = "";
  ProcedureDesc: string = "";
  ProcedureStartDate: NgbDateStruct;
  ProcedureEndDate: NgbDateStruct;
  Responsibility: string = "";
  Reviewer: string = "";
  RACMAuditProcedureId: string = "";
  Conclusion: string = "";
  ResponsibilityId: string = "";
  ReviewerId: string = "";
  Analytics: string = "";
  TestNumber: string = "";
  Finding: string = "";
  Justification: string = "";
  Status: boolean = false;
  isApprover: boolean = false;
  isCompleted: boolean = false;

  DesignMarks: string = "";
  DesignEffectiveness: string = "";
  OEMarks: string = "";
  OEEffectiveness: string = "";

  summaryNotStarted: number = 0;
  summaryInProgress: number = 0;
  summaryInReview: number = 0;
  summaryCompleted: number = 0;
  summaryTotal: number = 0;

  summaryIneffective: number = 0;
  summaryEffective: number = 0;
  summaryNotSelected: number = 0;
  summaryTRTotal: number = 0;

  //History
  tableFiltersHistory: BehaviorSubject<{ init: boolean }>;

  tableIdHistory: string = "discussnotehistory_table";

  filterOption: string = "all";
  filterTestingResult: string = "all";
  confirmationMessage: string = "";

  tableId: string = "testing_table";
  // tableApiUrl: string = "";
  // tableFilters;
  tableColumns: tableColumn[] = [
    {
      title: "<input type='checkbox' id='chkAllTOC' />",
      data: "id",
      className: "text-center",
      render: (data, type, row, meta) => {
        return (
          "<input type='checkbox' data-id='" +
          data +
          "' class='chkSingleTOC' />"
        );
      },
    },
    {
      title: "Procedure ID",
      data: "procedure.procedureId",
    },
    {
      title: "Procedure Title",
      data: "procedure.procedureTitle",
    },
    {
      title: "Procedure Description",
      data: "procedure.procedureDesc",
    },
    {
      title: "Control ID",
      data: "racmAuditProcedure.control.controlId",
    },
    {
      title: "Control Description",
      data: "racmAuditProcedure.control.description",
      render: (data) => {
        if (data && data.length > 100) {
          return (
            "<span>" +
            data.slice(0, 100) +
            '</span><br><a href="javascript:void(0)" data-title="Control Description" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    {
      title: "Data Period",
      data: "procedureStartDate",
    },
    {
      title: "Audit Check",
      data: "procedure.procedureDesc",
    },
    {
      title: "Performed By",
      data: "responsibility",
    },
    {
      title: "Testing Result",
      data: "conclusion",
    },
    {
      title: "Status",
      data: "status", 
    },
    {
      title: "Justification",
      data: "justification",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editTesting"><i class="fa fa-edit"></i></button>';
        if (!this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-primary emailTOC" title="Send email"><i class="fa fa-send"></i></button>';

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
      data: "testingOfControlDate",
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
      // this.tableFilters.next({});
      this.loadTOCTable();
      this.clearform();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  confirmSaveTOC(e) {
    e.preventDefault();

    this.confirmationMessage =
      "Are you wish to send " +
      this.ControlId +
      " for approval to control owner?";

    window["jQuery"]("#ConfimSendForApprovalModal").modal("show");
  }

  saveSendTOC() {
    this.saveTOC();
    this.updateStatus("inreview");
  }

  saveTOC() {
    let saveTOC = true;

    if (this.isCompleted) {
      saveTOC = confirm(
        "The testing for this audit procedure is completed. Are you sure you want to save the changes?"
      );
    }

    if (saveTOC) {
      this.isSendForApproval = false;
      this.updateRACMDetail();
    }

    window["jQuery"]("#ConfimSendForApprovalModal").modal("hide");
  }

  updateStatus(status: string, params?: {}) {
    let isConfirm = true;
    let statusMsg = {
      inreview:
        "Are you sure you want to sent " + this.ControlId + " for approval?",
      completed: "Are you sure you want to approve " + this.ControlId + "?",
    };

    if (statusMsg[status]) {
      isConfirm = confirm(statusMsg[status]);
    }

    let statusObj = { Id: this.id, Status: status };
    if (params && !this.utils.isEmptyObj(params) && params["justification"]) {
      statusObj["Justification"] = params["justification"];
    }

    if (isConfirm) {
      if (status === "inreview") {
        this.isSendForApproval = true;
        this.updateRACMDetail();
      }

      this.isSendForApproval = false;

      this.api
        .updateData("api/racmauditproceduredetails/updatestatus", statusObj)
        .subscribe((response) => {
          if (status !== "inprogress") {
            this.notifyService.success("Request Process Successfully");
          }
          this.handleFormView.hide();
        });
    }

    window["jQuery"]("#ConfimSendForApprovalModal").modal("hide");
  }

  sendmailtocontrol() {}

  rejectTOC() {
    if (this.Justification && this.Justification !== "") {
      this.updateStatus("inprogress", { justification: this.Justification });
      this.handleRejectModal("hide");
    } else {
      this.notifyService.error("Justification is required");
    }
  }

  handleRejectModal(action) {
    if (action) {
      window["jQuery"]("#rejectModal").modal(action);
    }
  }

  updateRACMDetail() {
    let apObj = this.formatTOCData();

    this.api
      .updateData("api/racmauditproceduredetails/", apObj)
      .subscribe((response) => {
        this.notifyService.success("Audit Procedure Updated Successfully");
        this.handleFormView.hide();
      });
  }

  editTOC(procedureData) {
    let racmData = procedureData.racmAuditProcedure;
    this.isEdit = true;
    this.id = procedureData.id;

    this.BusinessCycle = racmData.businessCycle.name;
    this.ProcessL1 = racmData.processL1 ? racmData.processL1.name : "";
    this.ProcessL2 = racmData.processL2 ? racmData.processL2.name : "";
    this.RiskId = racmData.risk.riskId;
    this.RiskTitle = racmData.risk.title;
    this.RiskDesc = racmData.risk.description;
    this.RiskRating = racmData.risk.rating;
    this.ControlId = racmData.control.controlId;
    this.ControlTitle = racmData.control.title;
    this.ControlDescription = racmData.control.description;
    this.ControlNature = racmData.control.nature;
    this.ControlFrequency = racmData.control.frequency;
    this.ControlType = racmData.control.type;
    this.ControlOwner = `${racmData.control.user.firstName} ${racmData.control.user.lastName}`;
    this.RACMAuditProcedureId = procedureData.racmAuditProcedureId;
    this.ProcedureId = procedureData.procedure.procedureId;
    this.ProcedureTitle = procedureData.procedure.procedureTitle;
    this.ProcedureDesc = procedureData.procedure.procedureDesc;
    this.ProcedureStartDate = this.utils.formatToNgbDate(
      procedureData.procedureStartDate
    );
    this.ProcedureEndDate = this.utils.formatToNgbDate(
      procedureData.procedureEndDate
    );
    this.Responsibility =procedureData.responsibility;
    this.ResponsibilityId = procedureData.responsibilityId;
    this.Reviewer = `${procedureData.reviewer.firstName} ${procedureData.reviewer.lastName}`;
    this.ReviewerId = procedureData.reviewerId;
    this.Conclusion = procedureData.conclusion;
    this.Analytics = procedureData.analytics;
    this.TestNumber = procedureData.testNumber;
    this.Finding = procedureData.finding;
    this.DesignMarks = procedureData.designMarks;
    this.DesignEffectiveness = procedureData.designEffectiveness;
    this.OEMarks = procedureData.oeMarks;
    this.OEEffectiveness = procedureData.oeEffectiveness;
    this.TestingOfControlUploads = procedureData.testingOfControlUploads
      ? procedureData.testingOfControlUploads
      : [];
    this.Status =
      procedureData.status === null ||
      procedureData.status.toLowerCase() === "inprogress"
        ? true
        : false;
    this.isCompleted =
      procedureData.status && procedureData.status.toLowerCase() === "completed"
        ? true
        : false;

    this.fillFileTable(this.TestingOfControlUploads);

    var userId = localStorage.getItem("userId");
    if (userId === procedureData.reviewerId) this.isApprover = true;
    else this.isApprover = false;

    this.handleFormView.show();
  }

  clearform() {
    this.id = "";
    this.RiskId = "";
    this.RiskTitle = "";
    this.RiskDesc = "";
    this.RiskRating = "";
    this.ControlId = "";
    this.ControlTitle = "";
    this.ControlDescription = "";
    this.ControlNature = "";
    this.ControlFrequency = "";
    this.ControlType = "";
    this.ControlOwner = "";
    this.BusinessCycle = "";
    this.ProcessL1 = "";
    this.ProcessL2 = "";
    this.businessCycleOptions = [];
    this.processlevel1Options = [];
    this.processlevel2Options = [];
    this.ProcedureId = "";
    this.ProcedureTitle = "";
    this.ProcedureDesc = "";
    this.ProcedureStartDate = null;
    this.ProcedureEndDate = null;
    this.Responsibility = "";
    this.Reviewer = "";
    this.RACMAuditProcedureId = "";
    this.Conclusion = "";
    this.Analytics = "";
    this.TestNumber = "";
    this.Finding = "";
    this.DesignMarks = "";
    this.DesignEffectiveness = "";
    this.OEMarks = "";
    this.OEEffectiveness = "";
    this.Status = false;
    this.isCompleted = false;
  }

  formatTOCData() {
    let apObj = {
      id: this.id,
      AuditID: this.AuditID,
      ProcedureStartDate: this.utils.formatNgbDateToYMD(
        this.ProcedureStartDate
      ),
      ProcedureEndDate: this.utils.formatNgbDateToYMD(this.ProcedureEndDate),
      ResponsibilityId: this.ResponsibilityId,
      ReviewerId: this.ReviewerId,
      Analytics: this.Analytics,
      TestNumber: this.TestNumber,
      Finding: this.Finding,
      DesignMarks: this.normalizeNumbers(this.DesignMarks),
      DesignEffectiveness: this.normalizeNumbers(this.DesignEffectiveness),
      OEMarks: this.normalizeNumbers(this.OEMarks),
      OEEffectiveness: this.normalizeNumbers(this.OEEffectiveness),
      Conclusion: this.Conclusion,
      Procedure: {
        ProcedureId: this.ProcedureId,
        ProcedureTitle: this.ProcedureTitle,
        ProcedureDesc: this.ProcedureDesc,
      },
      RACMAuditProcedureId: this.RACMAuditProcedureId,
      Status: this.isSendForApproval ? "inreview" : "inprogress",
    };
    return apObj;
  }

  normalizeNumbers(input) {
    return !isNaN(input) ? parseInt(input) : 0;
  }

  showContentModal(title, content) {
    window["jQuery"]("#tocContentModal #toccontent-title").html("").html(title);
    window["jQuery"]("#tocContentModal #toccontent").html("").html(content);
    window["jQuery"]("#tocContentModal").modal("show");
  }

  fillFileTable(fileData) {
    let tableColumns = [
      {
        title: "Name",
        data: "originalFileName",
      },
      {
        title: "Uploaded Date",
        data: "uploadedDatetime",
        render: (data) => {
          return data ? this.utils.formatDbDateToDMY(data) : "";
        },
      },
      {
        title: "Action",
        data: "id",
        orderable: false,
        render: function (data, row, rowData) {
          data = data.replace("\\", "/");
          return `<button type="button" data-name="${rowData.originalFileName}" data-id="${data}" class="btn btn-sm btn-primary downloadTocFile"><i class="fa fa-download"></i></button>`;
        },
      },
    ];

    this.commonApi.initialiseTable("fileTable", fileData, tableColumns);
  }

  handleFileUploadDialog = {
    show: () => {
      window["jQuery"]("#tocFileUploadModal").modal("show");
    },
    hide: () => {
      window["jQuery"]("#tocFileUploadModal").modal("hide");
    },
  };

  clearfiles() {
    let filesElem = this.tocFiles.nativeElement;
    filesElem.value = "";
  }

  uploadfiles() {
    let filesElem = this.tocFiles.nativeElement;

    if (filesElem.files.length) {
      this.spinner.show();
      let fd = new FormData();
      for (let files of filesElem.files) {
        fd.append("files", files);
      }
      this.api
        .insertData(
          "api/testingofcontrol/UploadFileTestingOfControl/" + this.id,
          fd
        )
        .subscribe(
          (result) => {
            this.notifyService.success("Files Uploaded Successfully");
            this.clearfiles();
            this.handleFileUploadDialog.hide();
            this.getTOCFile(this.id);
            this.spinner.hide();
          },
          (error) => {
            this.spinner.hide();
            console.log(error);
          }
        );
      this.spinner.hide();
    } else {
      this.notifyService.error("Please select files");
    }
  }

  getTOCFile(racmDetId) {
    this.spinner.show();
    this.api
      .getData(`api/racmauditproceduredetails/GetByID/${racmDetId}`)
      .subscribe(
        (result) => {
          this.TestingOfControlUploads = result["testingOfControlUploads"];
          this.fillFileTable(this.TestingOfControlUploads);
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  downloadTocFile(fileId, fileName) {
    this.spinner.show();
    this.api
      .downloadFile(
        "api/testingofcontrol/DownloadFileTestingOfControl/" + fileId
      )
      .subscribe(
        (blob) => {
          const blobUrl = URL.createObjectURL(blob);
          const link = document.createElement("a");
          link.href = blobUrl;
          link.download = fileName;
          document.body.appendChild(link);
          link.dispatchEvent(
            new MouseEvent("click", {
              bubbles: true,
              cancelable: true,
              view: window,
            })
          );
          document.body.removeChild(link);
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    this.spinner.hide();
  }

  emailTOC(id) {
    let postData = {
      Id: id,
    };
    this.spinner.show();
    this.api
      .insertData("api/racmauditproceduredetails/sendemail", postData)
      .subscribe(
        (response) => {
          this.spinner.hide();
          let result: any = response;
          if (result.sent)
            this.notifyService.success(
              "Testing of control email sent successfully."
            );
          this.loadTOCTable();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  emailToControlOwner() {
    let postData = {
      Id: this.id,
    };
    this.spinner.show();
    this.api
      .insertData(
        "api/racmauditproceduredetails/SendEmailToControlOwner",
        postData
      )
      .subscribe(
        (response) => {
          this.spinner.hide();
          let result: any = response;
          if (result.sent)
            this.notifyService.success(
              "Email sent to control owner successfully."
            );
          // this.tableFilters.next({});
          this.loadTOCTable();
        },
        (error) => {
          this.spinner.hide();
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
      window["jQuery"]("#sendTOCMultipleMailModal").modal("show");

      this.tocMailTotal = this.selectedIds.length;
      this.tocMailSent = 0;

      let sentMailCounter = 0;
      this.spinner.show();
      this.selectedIds.forEach((element, index) => {
        let postData = {
          Id: element,
        };

        this.api
          .insertData("api/racmauditproceduredetails/sendemail", postData)
          .subscribe(
            (response) => {
              let result: any = response;

              if (result.sent) {
                sentMailCounter++;

                this.tocMailSent = sentMailCounter;
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
      this.loadTOCTable();
    }
  }

  getSummary() {
    this.spinner.show();
    this.api
      .getData("api/racmauditproceduredetails/getsummary/" + this.AuditID)
      .subscribe(
        (response) => {
          let objResult: any = response;

          this.summaryNotStarted = objResult.procNotStarted || 0;
          this.summaryInProgress = objResult.procInProgress || 0;
          this.summaryInReview = objResult.procInReview || 0;
          this.summaryCompleted = objResult.procCompleted || 0;

          this.summaryTotal =
            this.summaryNotStarted +
            this.summaryInProgress +
            this.summaryInReview +
            this.summaryCompleted;

          this.summaryEffective = objResult.procEffective || 0;
          this.summaryIneffective = objResult.procIneffective || 0;
          this.summaryNotSelected = objResult.procNotSelect || 0;

          this.summaryTRTotal =
            this.summaryEffective +
            this.summaryIneffective +
            this.summaryNotSelected;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  ShowFilterData(option) {
    this.filterOption = option;
    this.loadTOCTable();
  }

  filterByTestingResult(option) {
    this.filterTestingResult = option;
    this.loadTOCTable();
  }

  loadTOCTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(
        `api/racmauditproceduredetails/GetByAudit/${this.AuditID}/${this.filterOption}/${this.filterTestingResult}`
      )
      .subscribe(
        (dtData) => {
          this.gridData = dtData;
          this.spinner.hide();
          this.gridData.forEach((element, index) => {
            switch (element.status) {
              case "inreview":
                status = "In Review";
                break;
              case "inprogress":
                status = "In Progress";
                break;
              case "completed":
                status = "Completed";
                break;
              default:
                status = "Not Started";
            }
            dtData[index].status=status.toUpperCase();
            if (element.responsibility != null)
              dtData[
                index
              ].responsibility = `${element.responsibility.firstName} ${element.responsibility.lastName}`;
            if (element.procedureStartDate != null) {
              dtData[
                index
              ].procedureStartDate = `${this.utils.UTCtoLocalDateDDMMMYY(
                element.procedureStartDate
              )} - ${this.utils.UTCtoLocalDateDDMMMYY(
                element.procedureEndDate
              )}`;
            }
          });
          this.commonApi.initialiseTableTestingControl(
            this.tableId,
            dtData,
            this.tableColumns,true
          );
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    this.spinner.hide();
  }

  refreshTable() {
    this.filterOption = "all";
    this.filterTestingResult = "all";
    this.loadTOCTable();
  }

  checkAccess() {
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "auditexecution"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "testingofcontrols"
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
  getHistoryData() {
    this.spinner.show();
    this.api
      .getData(`api/TestingOfControl/GetTestingofControlHistory/${this.id}`)
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
    this.spinner.hide();
  }
  importTestingOfControl() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");
    this.api
      .insertData(
        `api/racmauditproceduredetails/importexcel/${this.AuditID}/${userid}`,
        formData
      )
      .subscribe(
        (response) => {
          this.spinner.hide();
          //Main action plan
          var excptionCount = response["excptionCount"];
          var excptionRowNumber = response["excptionRowNumber"];
          var totalRow = response["totalRow"];
          excptionRowNumber = excptionRowNumber.replace(/,\s*$/, "");
          var successCount = totalRow - excptionCount;

          var msg =
            "Risk Total Rows : " +
            totalRow +
            "<br>Risk Success Count : " +
            successCount +
            " <br>Risk Exception Count : " +
            excptionCount +
            "<br>Risk Exception RowNumber : " +
            excptionRowNumber;
          this.notifyService.success(msg, "", {
            enableHtml: true,
          });
          this.fileInput.nativeElement.value = "";
          this.handleFormView.hide();
        },
        (error) => {
          this.spinner.hide();
          this.notifyService.error(error.error);
        }
      );
  }
  exportTestingOfControl() {
    this.spinner.show();
    this.api
      .downloadFile(
        `api/racmauditproceduredetails/downloadexcel/${this.AuditID}`
      )
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });
          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "TestingOfControl.xlsx");
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
  sampleExportTestingOfControl() {
    this.spinner.show();
    this.api
      .downloadFile(
        `api/racmauditproceduredetails/sampledownloadexcel/${this.AuditID}`
      )
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });
          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "SampleTestingOfControl.xlsx");
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
  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditID = localStorage.getItem("auditId");

    this.loadTOCTable();
    // this.tableApiUrl = `api/racmauditproceduredetails/GetByAudit/${this.AuditID}`;
    // this.isApprover = this.utils.checkIfApprover();
    // this.tableFilters = new BehaviorSubject({ init: false });
    $(document).ready(() => {
      $("#testingComponent").on("click", ".editTesting", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        // let racmData = $("#" + dataId).data();
        let racmData = window["jQuery"]("#" + dataId).data();
        this.editTOC(racmData);
      });

      $("#testingComponent").on("click", ".emailTOC", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        this.emailTOC(dataId);
      });

      $("#testingComponent").on("change", "#chkAllTOC", (e) => {
        $("#testing_table > tbody > tr")
          .find(".chkSingleTOC")
          .prop("checked", $(e.currentTarget).is(":checked"));

        let Ids: any = [];
        $("#testing_table > tbody > tr").each(function () {
          let row = $(this);
          Ids.push(row.attr("id"));
        });

        if ($(e.currentTarget).is(":checked")) this.selectedIds = Ids;
        else this.selectedIds = [];
      });

      $("#testingComponent").on("change", ".chkSingleTOC", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        if ($(e.currentTarget).is(":checked")) this.selectedIds.push(dataId);
        else {
          this.selectedIds.forEach((element, index) => {
            if (element == dataId) delete this.selectedIds[index];
          });
        }

        if (
          $("#testing_table > tbody > tr").find(".chkSingleTOC").length ==
          $("#testing_table > tbody > tr").find(".chkSingleTOC:checked").length
        )
          $("#testing_table > thead > tr")
            .find("#chkAllTOC")
            .prop("checked", true);
        else
          $("#testing_table > thead > tr")
            .find("#chkAllTOC")
            .prop("checked", false);
      });

      $('#auditExecutionTab a[href="#testing"]').on("click", (e) => {
        this.loadTOCTable();
        // if (typeof this.tableFilters !== "undefined") {
        // this.tableFilters.next({ init: true });
        // }
      });

      $("#testingComponent").on("click", ".viewContent", (e) => {
        let title = $(e.currentTarget).attr("data-title");
        let content = $(e.currentTarget).attr("data-content");
        content = decodeURI(content);
        this.showContentModal(title, content);
      });

      $("#testingComponent").on("click", ".downloadTocFile", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let fileName = $(e.currentTarget).attr("data-name");
        this.downloadTocFile(dataId, fileName);
      });
      // $(".wrapper1").scroll(function () {
      //   $(".wrapper2").scrollLeft($(".wrapper1").scrollLeft());
      // });
      // $(".wrapper2").scroll(function () {
      //   $(".wrapper1").scrollLeft($(".wrapper2").scrollLeft());
      // });
      // $(".div1").width($("table").width());
      // $(".div2").width($("table").width());
    });
  }
}
