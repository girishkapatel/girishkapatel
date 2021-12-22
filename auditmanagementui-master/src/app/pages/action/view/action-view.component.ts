import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { Router, NavigationExtras } from "@angular/router";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ApiService } from "src/app/services/api/api.service";
import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { IDropdownSettings } from "ng-multiselect-dropdown";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import * as ClassicEditor from "../../../../assets/ckeditor5/build/ckeditor";
am4core.useTheme(am4themes_animated);

@Component({
  selector: "selector",
  templateUrl: "action-view.component.html",
  styleUrls: ["./action-view.component.css"],
})
export class ActionViewComponent implements OnInit {
  gridData: any = [];
  constructor(
    private router: Router,
    private util: UtilsService,
    private api: ApiService,
    private commonApi: CommonApiService,
    private spinner: NgxSpinnerService,
    private notifyService: ToastrService
  ) { }

  @ViewChild("fileInput", { static: false }) fileInput;
  @ViewChild("mailForm", { static: false }) mailform;

  accessRights: any = {};
  isStackHolder: boolean = false;
  isAuditee: boolean = false;
  SelectedDivisionTitle: string = "Select Location";
  SelectedDivisionId: string = "";
  SelectedAuditTitle: string = "Select Audit";
  SelectedAuditId: string = "";
  SelectedPeriodTitle: string = "Select Period";
  SelectedPeriodId: string = "";
  public Editor = ClassicEditor;
  MailBody: string = "";
  // dropdownSettings: IDropdownSettings = {};
  dropdownSettings: IDropdownSettings = {
    textField: "emailId",
  };
  selectedCategory: string = "";
  selectedObservationGrading: string = "";
  quickActionData: any = [];
  userOpts: any = [];
  selectedToEmail: any = [];
  selectedCCEmail: any = [];
  lowAction: any;
  inprogressAction: any;
  selectedIds: any = [];
  chart;

  showGraph: boolean = false;
  showFilters: boolean = true;
  chartFlag: boolean = false;

  inprogressCount: number = 0;
  pendingCount: number = 0;
  completedCount: number = 0;
  dueTodayCount: number = 0;
  overdueCount: number = 0;
  notdueCount: number = 0;
  revisedTimelineCount: number = 0;

  summaryCountAll: number = 0;

  allRootCauses: any = [];
  quickViewData: any = [];
  locationOptions: any = [];
  auditOptions: any = [];
  periodOptions: any = ["FY 20-21", "FY 21-22", "FY 22-23", "FY 23-24", "FY 24-25", "FY 25-26", "FY 26-27"];

  actionInProgress: boolean = true;

  // tableApiUrlIP: string;
  // tableFiltersIP;

  actionCompleted: boolean = false;

  // tableApiUrlCom: string;
  // tableFiltersCom;

  actionPen: boolean = false;
  actionRevisedTimeline: boolean = false;
  // tableApiUrlPen: string;
  // tableFiltersPen;

  actionOverdue: boolean = false;
  actionNotdue: boolean = false;

  // tableApiUrlOD: string;
  // tableFiltersOD;

  status: string = "inprogress";
  paramStatus: string = "All";
  actionTitle: string = "All Actions";

  tableId: string = "actionPlans_table";
  tableIdActionView: string = "selectedAction_table";
  tableColumns: tableColumn[] = [
    {
      title: "<input type='checkbox' id='chkAllDNote' />",
      data: "id",
      orderable: false,
      className: "text-center",
      render: (data, type, row, meta) => {
        return (
          "<input type='checkbox' data-id='" +
          data +
          "' class='chkSingleDNote' />"
        );
      },
    },
    // {
    //   title: "Audit Number",
    //   data: "",
    //   render: (data, row, rowData) => {
    //     return rowData.audit && rowData.cleaaudit.auditNumber
    //       ? rowData.audit.auditNumber
    //       : rowData.auditNumber;
    //   },
    // },
    {
      title: "Audit",
      data: "auditName",
      render: (data, row, rowData) => {
        return rowData.audit && rowData.audit.audit.processLocationMapping
          ? rowData.audit.audit.processLocationMapping.auditName
          : rowData.processLocationMapping ? rowData.processLocationMapping.auditName : "";
      },
    },
    {
      title: "Location",
      data: "locationName",
      render: (data, row, rowData) => {
        return rowData.audit && rowData.audit.audit.location.locationDescription
          ? rowData.audit.audit.location.locationDescription
          : rowData.location
            ? rowData.location.locationDescription
            : "";
      },
    },
    {
      title: "Observation Heading",
      data: "observationHeading",
      render: (data) => {
        if (data.length > 50) {
          return (
            "<span>" +
            data.slice(0, 50) +
            '</span><br><a href="javascript:void(0)" data-title="Observation Heading" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    {
      title: "Observation<br> Grading",
      data: "observationGrading",
    },
    {
      title: "Agreed <br>Timeline",
      data: "implementationEndDate",
    },
    // {
    //   title: "Responsibility <br> Department",
    //   data: "responsibilityDepartment",
    // },

    // {
    //   title: "Detailed Observation",
    //   data: "detailedObservation",
    //   render: (data) => {
    //     if (data.length > 100) {
    //       return (
    //         "<span>" +
    //         data.slice(0, 100) +
    //         '</span><br><a href="javascript:void(0)" data-title="Detailed Observation" data-content="' +
    //         encodeURI(data) +
    //         '" class="viewContent">...View More</a>'
    //       );
    //     } else {
    //       return data;
    //     }
    //   },
    // },
    // {
    //   title: "Root Cause",
    //   data: "",
    //   render: (data, row, rowData) => {
    //     return this.getRootCauseNames(
    //       rowData.draftReport
    //         ? rowData.draftReport.rootCauses
    //         : rowData.rootCauses
    //     );
    //   },
    // },
    // {
    //   title: "Risk Type",
    //   data: "riskType.name",
    // },
    // {
    //   title:'Risks/ Implications',
    //   data:'implications'
    // },{
    //   title:'Recommendation',
    //   data:'recommendation'
    // },{
    //   title:'Management Action plan',
    //   data:'actionPlan'
    // },
    {
      title: "Implementation Owner",
      data: "implementationOwner",
    },
    {
      title: "Auditor Status",
      data: "status",
    },
    {
      title: "Auditee Status",
      data: "auditeeStatus",
    },
    {
      title: "Revised <br>Date",
      data: "revisedDate",
    },
    {
      title: "Management Implementation  Remarks",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.actionPlansInfo.length > 0) {
          var comments =
            rowData.actionPlansInfo[rowData.actionPlansInfo.length - 1]
              .comments;
          if (comments) {
            if (comments.length > 50) {
              return (
                "<span>" +
                comments.slice(0, 50) +
                '</span><br><a href="javascript:void(0)" data-title="Observation Heading" data-content="' +
                encodeURI(comments) +
                '" class="viewContent">...View More</a>'
              );
            } else {
              return comments;
            }
          } else return "";
        } else return "";
      },
    },
    {
      title: "Updated <br>Date",
      data: "updatedOn",
    },
    {
      title: "Ageing <br>days",
      orderable: true,
      data: "",
      render: (data, row, rowData) => {
        if (rowData.status.toLowerCase() !== "completed") {
          let buttons = "";
          var day = this.util.calculateDiff(
            rowData.implementationEndDate
          );
          if (day) {
            if (Math.sign(day) > 0) {
              buttons =
                '<button type="button" class="btn btn-sm btn-success"> ' + (day) + '</button>';
            }
            else
              buttons =
                '<button type="button" class="btn btn-sm btn-danger"> ' + (day) + '</button>';
            return buttons;
          }
          else return "";
        }
        else
          return "";
      }
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        let buttons = "";
        
        if (this.accessRights.isEdit || this.isAuditee || this.isStackHolder) {
          if (row.status !== "completed") {
            buttons =
              '<button type="button" data-id="' +
              data +
              '" class="btn btn-sm btn-info editAction"><i class="fa fa-edit"></i></button>';
          } else {
            buttons =
              '<button type="button" data-id="' +
              data +
              '" class="btn btn-sm btn-info editAction"><i class="fa fa-eye"></i></button>';
          }
        }
        if (this.accessRights.isDelete)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteAction"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  clearAll() {
    this.actionInProgress = false;
    this.actionCompleted = false;
    this.actionPen = false;
    this.actionOverdue = false;
    this.actionNotdue = false;
  }

  getActions(status) {
    this.clearAll();

    switch (status) {
      case "inprogress":
        this.actionTitle = "Actions In Progress";
        this.loadActionPlansTable("inprogress");
        this.actionInProgress = true;
        this.paramStatus = "inprogress";
        // this.tableFiltersIP.next({ init: true });
        break;
      case "completed":
        this.actionTitle = "Actions Completed";
        this.loadActionPlansTable("completed");
        this.actionCompleted = true;
        this.paramStatus = "completed";
        // this.tableFiltersCom.next({ init: true });
        break;
        case "pending":
          this.actionTitle = "Actions pending to be initiated";
          this.loadActionPlansTable("pending");
          this.actionPen = true;
          this.paramStatus = "pending";
          // this.tableFiltersPen.next({ init: true });
          break;
      // case "pending":
      //   this.actionTitle = "Actions Due Today";
      //   this.loadActionPlansTable("duedate");
      //   this.actionPen = true;
      //   this.paramStatus = "duedate";
      //   // this.tableFiltersPen.next({ init: true });
      //   break;
      // case "overdue":
      //   this.actionTitle = "Actions Overdue";
      //   this.loadActionPlansTable("overdue");
      //   this.actionOverdue = true;
      //   this.paramStatus = "overdue";
      //   // this.tableFiltersOD.next({ init: true });
      //   break;
      // case "notdue":
      //   this.actionTitle = "Actions Notdue";
      //   this.loadActionPlansTable("notdue");
      //   this.actionNotdue = true;
      //   this.paramStatus = "notdue";
      //   // this.tableFiltersOD.next({ init: true });
      //   break;
      // case "revisedtimeline":
      //   this.actionTitle = "Actions Revised Timeline";
      //   this.loadActionPlansTable("revisedtimeline");
      //   this.actionRevisedTimeline = true;
      //   this.paramStatus = "revisedtimeline";
      //   // this.tableFiltersOD.next({ init: true });
      //   break;
      case "all":
        this.actionTitle = "Actions All";
        this.loadActionPlansTable("all");
        this.actionNotdue = true;
        this.paramStatus = "all";
        // this.tableFiltersOD.next({ init: true });
        break;
    }
  }

  changeLayout(event) {
    this.chartFlag = event.target.checked;
    if (this.chartFlag) {
      this.getChartData();
      this.loadActionPlansTable("all");
    }
  }

  graphLayout(chartData) {
    if (this.chart) this.chart.dispose();
    this.chart = am4core.create("chartdiv", am4charts.XYChart);
    this.chart.data = chartData;

    var categoryAxis = this.chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "category";
    categoryAxis.renderer.minGridDistance = 40;
    var valueAxis = this.chart.yAxes.push(new am4charts.ValueAxis());
    var columnSeries = this.chart.series.push(new am4charts.ColumnSeries());
    columnSeries.dataFields.categoryX = "category";
    columnSeries.dataFields.valueY = "value";
    columnSeries.dataFields.openValueY = "open";
    columnSeries.fillOpacity = 0.8;
    columnSeries.sequencedInterpolation = true;
    columnSeries.interpolationDuration = 1500;
    columnSeries.columns.template.tooltipText = "{displayValue.formatNumber('#,## a')}";
    var columnTemplate = columnSeries.columns.template;
    columnTemplate.strokeOpacity = 0;
    columnTemplate.propertyFields.fill = "color";

    var label = columnTemplate.createChild(am4core.Label);
    // label.events.on("click", this.myFunction(), this);
    label.text = "{displayValue.formatNumber('#,## a')}";
    label.align = "center";
    label.valign = "middle";

    // Add events
    columnTemplate.events.on(
      "hit",
      ev => {
        var status = ev.target._dataItem.dataContext["category"].toLowerCase();
        if (status === "total") status = "all";
        else if (status === "not due") status = "notdue";
        else if (status === "implemented") status = "completed";
        else if (status === "not implemented") status = "pending";
        else if (status === "partially implemented") status = "inprogress";
        this.selectedObservationGrading = ev.target._dataItem.dataContext["displayValue"];
        this.loadActionPlansTable(status);
        this.selectedObservationGrading = "";
      },
      this
    );
    //  columnTemplate.events.on("hit", this.highlighColumn);
    //  columnTemplate.cursorOverStyle = am4core.MouseCursorStyle.pointer;
  }
  highlighColumn(ev) {
    //  var data= ev.target["currentText"];
    //  alert(data)
  }

  showContentModal(title, content) {
    window["jQuery"]("#fuContentModal #fucontent-title").html("").html(title);
    window["jQuery"]("#fuContentModal #fucontent").html("").html(content);
    window["jQuery"]("#fuContentModal").modal("show");
  }

  deleteAction(actionId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.api.deleteData("api/followup/" + actionId).subscribe(
        (response) => {
          this.notifyService.error("Action Plan Deleted Successfully");
          if (this.actionInProgress) this.loadActionPlansTable("all");

          if (this.actionCompleted) this.loadActionPlansTable("all");

          if (this.actionPen) this.loadActionPlansTable("duedate");

          if (this.actionOverdue) this.loadActionPlansTable("overdue");
        },
        (error) => {
          console.log(error);
        }
      );
    }
  }

  getChartData() {
    this.spinner.show();
    this.api
      .getData("api/actionplanning/getchartdata/", this.getReportFilters())
      .subscribe(
        (response) => {
          let chartData = response;
          this.graphLayout(chartData);
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getRootCauseNames(rootCauseIds) {
    let rootCauseNames: any = [];
    if (rootCauseIds != null) {
      this.allRootCauses.forEach((element) => {
        if (rootCauseIds.indexOf(element.id) > -1)
          rootCauseNames.push(element.name);
      });
    }
    return rootCauseNames.join(", ");
  }

  getAllRootCauses() {
    this.spinner.show();
    this.commonApi.getRootCause().subscribe(
      (data) => {
        this.allRootCauses = data;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getActionView() {
    this.spinner.show();
    this.api
      .getData(`api/ActionPlanning/GetQuickView/`, this.getReportFilters())
      .subscribe(
        (data) => {
          this.quickActionData = data;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getLocations() {
    this.spinner.show();
    this.commonApi.getLocations().subscribe(
      (posts) => {
        this.locationOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  auditOption() {
    this.spinner.show();
    this.commonApi.getAuditName()
      .subscribe(
        (response) => {
          this.auditOptions = response;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  locationChange(location) {
    this.SelectedDivisionTitle = location.locationDescription;
    this.SelectedDivisionId = location.id;
  }

  auditChange(auditObj) {
    this.SelectedAuditId = auditObj.id;
    this.SelectedAuditTitle = auditObj.auditName;
    // this.SelectedPeriodTitle = "Select Period";
    // this.SelectedPeriodId = "";
    this.spinner.show();
    // this.api
    //   .getData("api/ActionPlanning/getperiodsbyaudit/" + this.SelectedAuditId)
    //   .subscribe(
    //     (response) => {
    //       this.periodOptions = response;
    //       this.spinner.hide();
    //     },
    //     (error) => {
    //       this.spinner.hide();
    //       console.log(error);
    //     }
    //   );
  }

  periodChange(scopeObj) {
    this.SelectedPeriodId = scopeObj;
    this.SelectedPeriodTitle = scopeObj;
  }
  // periodChange(scopeObj) {
  //   this.SelectedPeriodId = scopeObj.quater;
  //   this.SelectedPeriodTitle =
  //     scopeObj.quater + ", FY " + scopeObj.financialYear;
  // }

  filterActionPlans() {
    this.loadActionPlansTable("all");
    this.getChartData();
  }
  editAction(actionData) {
    localStorage.setItem("ActionPlanEditId", actionData.id);
    this.router.navigate(["./pages/action/edit"]);
  }

  addAction() {
    this.router.navigate(["./pages/action/add"]);
  }
  getReportFilters() {
    let filterObj = {
      Divison: this.SelectedDivisionId,
      Audit: this.SelectedAuditId,
      Period: this.SelectedPeriodTitle,
      ObservationGrading: this.selectedObservationGrading,
    };
    return filterObj;
  }
  loadActionPlansTable(status) {
    this.getSummaryCounts();
    this.getActionView();
    this.spinner.show();
    this.api
      .getData(
        `api/actionplanning/GetByStatus/${status}/`,
        this.getReportFilters()
      )
      .subscribe(
        (dtData) => {
          this.gridData = dtData;
          this.spinner.hide();
          this.gridData.forEach((element, index) => {
            let grading = element.observationGrading;
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
            dtData[index].observationGrading = grading;
            let status = element.status.toLowerCase();
            switch (status) {
              case "inprogress":
                status = "In Progress";
                break;
              case "pending":
                status = "Pending to be initiated";
                break;
              case "completed":
                status = "Completed";
                break;
            }
            dtData[index].status = status;

            //Auditee status
            let auditeestatus = element.auditeeStatus.toLowerCase();
            switch (auditeestatus) {
              case "inprogress":
                auditeestatus = "In Progress";
                break;
              case "pending":
                auditeestatus = "Pending to be initiated";
                break;
              case "completed":
                auditeestatus = "Completed";
                break;
            }
            dtData[index].auditeeStatus = auditeestatus;
            if (element.implementationOwner != null)
              dtData[
                index
              ].implementationOwner = `${element.implementationOwner.firstName} ${element.implementationOwner.lastName}`;
            if (element.implementationEndDate != null)
              dtData[index].implementationEndDate = this.util.formatDbDateToDMY(
                element.implementationEndDate
              );
            if (element.revisedDate != null)
              dtData[index].revisedDate = this.util.formatDbDateToDMY(
                element.revisedDate
              );
            if (element.updatedOn != null)
              dtData[index].updatedOn = this.util.formatDbDateToDMY(
                element.updatedOn
              );
          });
          this.commonApi.initialiseTableActionTable(
            this.tableId,
            dtData,
            this.tableColumns
          );
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getSummaryCounts() {
    this.spinner.show();
    this.api
      .getData(`api/actionplanning/getsummarycounts/`, this.getReportFilters())
      .subscribe(
        (response) => {
          let result: any = response;
          this.spinner.hide();
          this.inprogressCount = result.inprogress || 0;
          // this.dueTodayCount = result.duetoday || 0;
          // this.overdueCount = result.overdue || 0;
          this.completedCount = result.completed || 0;
          // this.revisedTimelineCount = result.revisedTimeline || 0;
          // this.notdueCount = result.notDue || 0;
          this.pendingCount = result.pending || 0;
          this.summaryCountAll = result.all || 0;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  exportActionPlans() {
    this.spinner.show();
    this.api.downloadFile(`api/actionplanning/downloadexcel/${this.paramStatus}/`, this.getReportFilters()).subscribe(
      (blob) => {
        //console.log(blob);
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "ActionPlans.xlsx");

          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },
      (error) => {
        this.notifyService.error("Internal Server Error");
        this.spinner.hide();
      }
    );
  }
  sampleActionPlans() {
    this.spinner.show();
    this.api.downloadFile("api/actionplanning/sampledownloadexcel").subscribe(
      (blob) => {
        //console.log(blob);
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleActionPlans.xlsx");

          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },
      (error) => {
        this.notifyService.error("Internal Server Error");
        this.spinner.hide();
      }
    );
  }

  importActionPlans() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");
    this.api
      .insertData("api/actionplanning/importexcel/" + userid, formData)
      .subscribe(
        (response) => {
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
          // this.notifyService.success(msg);
          console.log(msg);
          this.fileInput.nativeElement.value = "";
          this.loadActionPlansTable("all");
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          this.notifyService.error(error.excptionCount);
        }
      );
  }

  checkAccess() {
    this.accessRights = this.util.getSubmoduleAccess("action")[0];
  }
  OpenMultipleEmailModel() {
    if (this.selectedIds.length == 0)
      this.notifyService.error(
        "Please select at least one record to send email."
      );
    else {
      window["jQuery"]("#sendDNMultipleMailModal").modal("show");
      this.fillUserOptions();
    }
  }

  fillUserOptions() {
    this.api.getData("api/user/getalluser").subscribe((posts) => {
      this.userOpts = posts;
    });
  }
  getUserCcEmail() {
    let rootCausesArray = [];

    this.selectedCCEmail.forEach((element) => {
      rootCausesArray.push(element.emailId);
    });

    return rootCausesArray;
  }
  getUserToEmail() {
    let rootCausesArray = [];

    this.selectedToEmail.forEach((element) => {
      rootCausesArray.push(element.emailId);
    });

    return rootCausesArray;
  }
  SendMail(event) {
    if (this.mailform.invalid) {
      return false;
    } else {
      // var formValue=this.mailform.form.value;
      var param = {
        ToEmail: this.getUserToEmail(),
        CcEmail: this.getUserCcEmail(),
        MailBody: this.MailBody,
        IDs: this.selectedIds,
      };
      this.spinner.show();
      this.api.insertData("api/actionplanning/sendemail", param).subscribe(
        (response) => {
          this.spinner.hide();
          this.notifyService.success("Sent Mail Successfully");
          this.clearModel();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
      return true;
    }
  }
  clearModel() {
    window["jQuery"]("#sendDNMultipleMailModal").modal("hide");
    // $("#tblActionPlanSendMail tbody").empty();
    this.selectedToEmail = [];
    this.selectedCCEmail = [];
    this.MailBody = "";
    this.selectedIds = [];
    $("#actionPlans_table > tbody > tr")
      .find(".chkSingleDNote")
      .prop("checked", false);
    $("#chkAllDNote").prop("checked", false);
  }
  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.isAuditee = localStorage.getItem("role") == "Auditee" ? true : false;
    this.getLocations();
    this.auditOption();
    this.getActionView();
    this.getAllRootCauses();
   this.loadActionPlansTable("all");
    $(document).ready(() => {
      $("#action-view-component").on("click", ".editAction", (e) => {
        e.preventDefault();

        let dataId = window["jQuery"](e.currentTarget).attr("data-id");
        let actionData = window["jQuery"]("#" + dataId).data();

        if (!this.util.isEmptyObj(actionData)) this.editAction(actionData);
      });

      $("#action-view-component").on("click", ".deleteAction", (e) => {
        e.preventDefault();

        let dataId = window["jQuery"](e.currentTarget).attr("data-id");

        this.deleteAction(dataId);
      });

      $("#action-view-component").on("click", ".viewContent", (e) => {
        let title = $(e.currentTarget).attr("data-title");

        let content = $(e.currentTarget).attr("data-content");
        content = decodeURI(content);

        this.showContentModal(title, content);
      });

      $("#action-view-component").on("change", "#chkAllDNote", (e) => {
        $("#actionPlans_table > tbody > tr")
          .find(".chkSingleDNote")
          .prop("checked", $(e.currentTarget).is(":checked"));

        let Ids: any = [];
        $("#actionPlans_table > tbody > tr").each(function () {
          let row = $(this);
          let dataId = row.attr("id");
          Ids.push(row.attr("id"));
          // var auditNumber = $(row).children("td:eq(1)").html();
          // var auditName = $(row).children("td:eq(2)").html();
          // var auditLocation = $(row).children("td:eq(3)").html();
          // var observationGrading = $(row).children("td:eq(4)").html();
          // var observationHeading = $(row).children("td:eq(5)").html();
          // var detailedObservation = $(row).children("td:eq(6)").html();
          // var rootCause = $(row).children("td:eq(7)").html();
          // var riskType = $(row).children("td:eq(8)").html();
          // var status = $(row).children("td:eq(9)").html();
          // let resHtml = `<tr id="${dataId}">
          //         <td>${auditNumber}</td>
          //         <td>${auditName}</td>
          //         <td>${auditLocation}</td>
          //         <td>${observationGrading}</td>
          //         <td>${observationHeading}</td>
          //         <td>${detailedObservation}</td>
          //         <td>${rootCause}</td>
          //         <td>${riskType}</td>
          //         <td>${status}</td>
          //       </tr>`;
          // $("#tblActionPlanSendMail tbody").append(resHtml);
        });

        if ($(e.currentTarget).is(":checked")) {
          this.selectedIds = Ids;
        } else {
          // $("#tblActionPlanSendMail tbody").empty();
          this.selectedIds = [];
        }
      });
      $("#action-view-component").on("change", ".chkSingleDNote", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        var row = e.target.closest("tr");
        if ($(e.currentTarget).is(":checked")) {
          this.selectedIds.push(dataId);
          // var auditNumber = $(row).children("td:eq(1)").html();
          // var auditName = $(row).children("td:eq(2)").html();
          // var auditLocation = $(row).children("td:eq(3)").html();
          // var observationGrading = $(row).children("td:eq(4)").html();
          // var observationHeading = $(row).children("td:eq(5)").html();
          // var detailedObservation = $(row).children("td:eq(6)").html();
          // var rootCause = $(row).children("td:eq(7)").html();
          // var riskType = $(row).children("td:eq(8)").html();
          // var status = $(row).children("td:eq(9)").html();
          // let resHtml = `<tr id="${dataId}">
          //         <td>${auditNumber}</td>
          //         <td>${auditName}</td>
          //         <td>${auditLocation}</td>
          //         <td>${observationGrading}</td>
          //         <td>${observationHeading}</td>
          //         <td>${detailedObservation}</td>
          //         <td>${rootCause}</td>
          //         <td>${riskType}</td>
          //         <td>${status}</td>
          //       </tr>`;
          // $("#tblActionPlanSendMail tbody").append(resHtml);
        } else {
          // var dataid = $(
          //   "#tblActionPlanSendMail tbody tr#" + dataId + ""
          // ).remove();
          this.selectedIds.forEach((element, index) => {
            if (element == dataId) this.selectedIds.pop(dataId);
          });
        }
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
  }0                                                                                            

  ngOnDestroy() {
    if (this.chart) this.chart.dispose();
  }
}
