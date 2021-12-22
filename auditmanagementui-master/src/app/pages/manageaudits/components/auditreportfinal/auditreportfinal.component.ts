import { Component, Input, OnInit } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { ApiService } from "src/app/services/api/api.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-auditreportfinal",
  templateUrl: "auditreportfinal.component.html",
  styleUrls: ["./auditreportfinal.component.css"],
})
export class AuditReportFinalComponent implements OnInit {
  constructor(
    private commonApi: CommonApiService,
    private api: ApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) { }

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

  // tableApiUrl: string;
  // tableFilters: BehaviorSubject<{}>;
  AuditID: string;

  /* Report Table Config*/
  tableId: string = "final_report_table";
  tableColumnsReport: tableColumn[] = [
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
      data: "discussionNote.riskType",
      render: function (data) {
        return data ? data.riskType.name : "";
      },
    },
    {
      title: "Risks/ Implications",
      data: "discussionNote.risks",
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
    //   title: "Financial Impact ",
    //   data: "discussionNote.financialImpact",
    // },
    {
      title: "Observations Grading",
      data: "observationGrading",
      render: function (data) {
        let og = data === 0 ? "Low" : data === 1 ? "Medium" : "High";
        return og;
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
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        let button = "";

        if (this.accessRights.isDelete)
          button =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteReport"><i class="fa fa-trash"></i></button>';

        return button;
      },
    },
  ];

  tableData_report: tableData[] = [];

  deleteReport(dataId) {
    let isConfirm = confirm(
      `Are you sure you want to delete record from final report ?`
    );
    if (isConfirm) {
      this.api
        .updateData(`api/draftreport/UpdateAuditReportStatus`, {
          Id: dataId,
          Status: "approved",
        })
        .subscribe((response) => {
          this.deleteFollowup(dataId);

          this.notifyService.success("Delete Successful");
          // this.tableFilters.next({});

          this.loadFinalReportTable();
        });
    }
  }

  deleteFollowup(dataId) {
    this.api
      .deleteData(`api/followup/DeleteBydraftId/${dataId}`)
      .subscribe((response) => { });
  }

  showContentModal(title, content) {
    window["jQuery"]("#arfContentModal #arfcontent-title").html("").html(title);
    window["jQuery"]("#arfContentModal #arfcontent").html("").html(content);
    window["jQuery"]("#arfContentModal").modal("show");
  }

  getSummary() {
    this.spinner.show();
    this.api
      .getData("api/draftreport/getfinalsummary/" + this.AuditID)
      .subscribe(
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
    this.loadFinalReportTable();
  }

  FilterByStatus(option) {
    this.filterStatus = option;
    this.loadFinalReportTable();
  }

  loadFinalReportTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(
        `api/draftreport/GetFinal/${this.AuditID}/${this.filterGrading}/${this.filterStatus}`
      )
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumnsReport, true
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
    this.loadFinalReportTable();
  }

  checkAccess() {
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "reporting"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "finalreport"
    )[0];
  }
  exportToPDF() {
    this.spinner.show();
    this.api
      .downloadFile(`api/draftreport/GetFinaldownloadpdf/${this.AuditID}`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/pdf",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "FinalReport.pdf");
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

  ngOnInit() {
    this.checkAccess();

    this.AuditID = localStorage.getItem("auditId");

    this.loadFinalReportTable();

    // this.tableApiUrl = `api/draftreport/GetFinal/${this.AuditID}`;
    // this.tableFilters = new BehaviorSubject({ init: false });

    $(document).ready(() => {
      $("#auditReportFinalComponent").on("click", ".deleteReport", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        if (dataId) {
          this.deleteReport(dataId);
        }
      });

      $('#reportingTab a[href="#acm"]').on("click", (e) => {
        // if (typeof this.tableFilters !== "undefined") {
        // this.tableFilters.next({ init: true });
        // }
        this.loadFinalReportTable();
      });

      $("#auditReportFinalComponent").on("click", ".viewContent", (e) => {
        let title = $(e.currentTarget).attr("data-title");
        let content = $(e.currentTarget).attr("data-content");
        content = decodeURI(content);
        this.showContentModal(title, content);
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
