import { Component, OnInit, ViewChild, Input } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import * as $ from "jquery";
import { ApiService } from "../../../../services/api/api.service";
import { UtilsService } from "../../../../services/utils/utils.service";

import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import { NgForm } from "@angular/forms";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { NgbDate, NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { speed } from "jquery";

am4core.useTheme(am4themes_animated);

@Component({
  selector: "app-activity",
  templateUrl: "activity.component.html",
  styleUrls: ["./activity.component.css"],
})
export class ActivityComponent implements OnInit {
  constructor(
    private api: ApiService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private spinner: NgxSpinnerService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("activityForm", { static: false }) activityForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  @Input() userOpts: any;

  accessRights: any = {};
  isStackHolder: boolean = false;
  summaryDue: number = 0;
  summaryCompleted: number = 0;
  summaryInProgress: number = 0;
  summaryDelayed: number = 0;

  filterOption: string = "all";
  buttonTitle: string = "Show Timeline";
  timelineview: boolean = false;

  id: string = "";
  AuditID: string = "";
  ActivityName: string = "";
  Status: string = "";

  PlannedStartDate: NgbDateStruct;
  PlannedEndDate: NgbDateStruct;
  ActualStartDate: NgbDateStruct;
  ActualEndDate: NgbDateStruct;

  PersonResponsibleID: string = "";
  /* Activity Table Config*/
  tableId: string = "activity_table";
  tableFilters;
  tableApiUrl: string = "";
  tableColumnsActivity: tableColumn[] = [
    {
      title: "Activity Name",
      data: "activityName",
    },
    {
      title: "Planned Timeline",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.formatDbDateToDMY(
          rowData.plannedStartDate
        )} - ${this.utils.formatDbDateToDMY(rowData.plannedEndDate)}`;
      },
    },
    {
      title: "Actual Timeline",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.formatDbDateToDMY(
          rowData.actualStartDate
        )} - ${this.utils.formatDbDateToDMY(rowData.actualEndDate)}`;
      },
    },
    {
      title: "Status",
      data: "activityStatus",
      render: function (data) {
        if (data) {
          return data.toUpperCase();
        } else {
          return "INPROGRESS";
        }
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editActivity"><i class="fa fa-edit"></i></button>';
        if (!this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-primary emailActivity" title="Send email"><i class="fa fa-send"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteActivity"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];
  tableData_activity: tableData[] = [];
  isEdit: boolean = false;
  formVisible: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.formVisible = false;
      this.isEdit = false;

      this.clearform();
      this.loadActivityTable();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveActivity(e) {
    if (this.activityForm.invalid) {
      return false;
    }
    e.preventDefault();
    if (this.isEdit) {
      this.updateActivity();
    } else {
      this.addNewActivity();
    }
  }

  switchActivityView() {
    if (this.timelineview) {
      this.buttonTitle = "Show Timeline";
      this.timelineview = false;
    } else {
      this.buttonTitle = "Show Table";
      this.timelineview = true;
    }
  }

  addNewActivity() {
    let postData = this.activityForm.form.value;
    postData.AuditID = this.AuditID;
    postData.PlannedStartDate = this.utils.formatNgbDateToYMD(
      postData.PlannedStartDate
    );
    postData.PlannedEndDate = this.utils.formatNgbDateToYMD(
      postData.PlannedEndDate
    );
    postData.ActualStartDate = this.utils.formatNgbDateToYMD(
      postData.ActualStartDate
    );
    postData.ActualEndDate = this.utils.formatNgbDateToYMD(
      postData.ActualEndDate
    );
    postData.ActivityStatus = postData.ActivityStatus
      ? postData.ActivityStatus.ActivityStatus
      : "inprogress";

    this.api.insertData("api/activity", postData).subscribe(
      (response) => {
        this.notifyService.success("Activity Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateActivity() {
    let postData = this.activityForm.form.value;
    postData.id = this.id;
    postData.AuditID = this.AuditID;
    postData.PlannedStartDate = this.utils.formatNgbDateToYMD(
      postData.PlannedStartDate
    );
    postData.PlannedEndDate = this.utils.formatNgbDateToYMD(
      postData.PlannedEndDate
    );
    postData.ActualStartDate = this.utils.formatNgbDateToYMD(
      postData.ActualStartDate
    );
    postData.ActualEndDate = this.utils.formatNgbDateToYMD(
      postData.ActualEndDate
    );

    this.api.updateData("api/activity", postData).subscribe(
      (response) => {
        this.notifyService.success("Activity Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addActivity() {
    this.clearform();
    this.handleFormView.show();
  }

  editActivity(activityData) {
    this.isEdit = true;
    this.id = activityData.id;
    this.ActivityName = activityData.activityName;
    this.Status = activityData.activityStatus;

    //Mayur Start
    this.PlannedStartDate = this.utils.formatToNgbDate(
      activityData.plannedStartDate
    );
    //Mayur End

    this.PlannedEndDate = this.utils.formatToNgbDate(
      activityData.plannedEndDate
    );
    this.ActualStartDate = this.utils.formatToNgbDate(
      activityData.actualStartDate
    );
    this.ActualEndDate = this.utils.formatToNgbDate(activityData.actualEndDate);

    this.PersonResponsibleID = activityData.personResponsibleID;
    this.handleFormView.show();
  }

  deleteActivity(activityId) {
    this.spinner.show();
    let isConfirm = confirm(`Are you sure you want to delete this activity?`);
    if (isConfirm) {
      this.api.deleteData("api/activity/" + activityId).subscribe(
        (response) => {
          this.spinner.hide();
          this.notifyService.success("Activity Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
          this.spinner.hide();
        }
      );
    }
    this.spinner.hide();
  }

  emailActivity(id) {
    let postData = {
      Id: id,
    };
this.spinner.show();
    this.api.insertData("api/Activity/sendemail", postData).subscribe(
      (response) => {
        this.spinner.hide();
        let result: any = response;
        if (result.sent)
          this.notifyService.success("Activity Data mail sent successfully.");
        this.loadActivityTable();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
    this.spinner.hide();
  }

  clearform() {
    this.id = "";
    this.ActivityName = "";
    this.Status = "";
    this.PlannedStartDate = null;
    this.PlannedEndDate = null;
    this.ActualStartDate = null;
    this.ActualEndDate = null;
    this.PersonResponsibleID = "";
  }

  initTimeline() {
    let chart = am4core.create("timelinediv", am4charts.XYChart);
    chart.hiddenState.properties.opacity = 0; // this creates initial fade-in

    chart.marginLeft = 70;
    chart.dateFormatter.inputDateFormat = "yyyy-MM-dd HH:mm";

    let colorSet = new am4core.ColorSet();
    colorSet.saturation = 0.4;

    chart.data = [
      {
        category: "Audit Announcement",
        start: "2019-04-01",
        end: "2019-04-02",
        color: am4core.color("#2c973e").brighten(0),
        task: "TOR",
      },
      {
        category: "Audit Announcement",
        start: "2019-04-02",
        end: "2019-04-03",
        color: am4core.color("#2c973e").brighten(0.4),
        task: "Information Request",
      },
      {
        category: "Audit Announcement",
        start: "2019-04-02",
        end: "2019-04-03",
        color: am4core.color("#2c973e").brighten(0.8),
        task: "Team Finalisation",
      },
      {
        category: "Audit Announcement",
        start: "2019-04-02",
        end: "2019-04-03",
        color: am4core.color("#2c973e").brighten(1.2),
        task: "Annoucement for kick off Meeting",
      },
      {
        category: "Audit Announcement",
        start: "2019-04-10",
        end: "2019-05-11",
        color: am4core.color("#2c973e").brighten(1.2),
        task: "Actual kick off meeting",
      },
      {
        category: "Audit Execution Planning",
        start: "2019-04-05",
        end: "2019-04-07",
        color: am4core.color("#91278f").brighten(0),
        task: "Audit Planning Memo",
      },
      {
        category: "Audit Execution Planning",
        start: "2019-04-10",
        end: "2019-04-29",
        color: am4core.color("#91278f").brighten(0.4),
        task: "Work Programme",
      },
      {
        category: "Audit Execution Planning",
        start: "2019-04-10",
        end: "2019-04-29",
        color: am4core.color("#91278f").brighten(0.8),
        task: "Field Work Start Date",
      },
      {
        category: "Audit Execution Planning",
        start: "2019-04-10",
        end: "2019-04-29",
        color: am4core.color("#91278f").brighten(1.2),
        task: "Field Work End Date",
      },
      {
        category: "Audit Execution",
        start: "2019-04-30",
        end: "2019-05-02",
        color: am4core.color("#747480").brighten(0),
        task: "Walkthrough",
      },
      {
        category: "Audit Execution",
        start: "2019-05-10",
        end: "2019-05-12",
        color: am4core.color("#747480").brighten(0.4),
        task: "Detailed Data Analytics",
      },
      {
        category: "Audit Execution",
        start: "2019-05-10",
        end: "2019-05-12",
        color: am4core.color("#747480").brighten(0.8),
        task: "Sampling",
      },
      {
        category: "Audit Execution",
        start: "2019-05-10",
        end: "2019-05-02",
        color: am4core.color("#747480").brighten(1.2),
        task: "Testing of controls",
      },
      {
        category: "Audit Execution",
        start: "2019-05-10",
        end: "2019-05-12",
        color: am4core.color("#747480").brighten(1.2),
        task: "Discussion Note",
      },
      {
        category: "Reporting & Communication",
        start: "2019-05-18",
        end: "2019-05-18",
        color: am4core.color("#f04c3e").brighten(0),
        task: "Draft report",
      },
      {
        category: "Reporting & Communication",
        start: "2019-05-18",
        end: "2019-05-18",
        color: am4core.color("#f04c3e").brighten(0.4),
        task: "Management comments",
      },
      {
        category: "Reporting & Communication",
        start: "2019-05-18",
        end: "2019-05-18",
        color: am4core.color("#f04c3e").brighten(0.8),
        task: "Final report",
      },
      {
        category: "ACM",
        start: "2019-05-22",
        end: "2019-05-22",
        color: am4core.color("#336699").brighten(0),
        task: "Preparation of AC Deck",
      },
      {
        category: "ACM",
        start: "2019-05-22",
        end: "2019-05-22",
        color: am4core.color("#336699").brighten(0.4),
        task: "Update to the MD on the audit reports and AC Deck",
      },
      {
        category: "ACM",
        start: "2019-05-22",
        end: "2019-05-22",
        color: am4core.color("#336699").brighten(0.8),
        task: "Finalize AC Deck",
      },
    ];

    chart.dateFormatter.dateFormat = "yyyy-MM-dd";
    chart.dateFormatter.inputDateFormat = "yyyy-MM-dd";

    let categoryAxis = chart.yAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "category";
    categoryAxis.renderer.grid.template.location = 0;
    categoryAxis.renderer.inversed = true;

    let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
    dateAxis.renderer.minGridDistance = 70;
    dateAxis.baseInterval = { count: 1, timeUnit: "day" };
    // dateAxis.max = new Date(2018, 0, 1, 24, 0, 0, 0).getTime();
    //dateAxis.strictMinMax = true;
    dateAxis.renderer.tooltipLocation = 0;

    let series1 = chart.series.push(new am4charts.ColumnSeries());
    series1.columns.template.height = am4core.percent(70);
    series1.columns.template.tooltipText =
      "{task}: [bold]{openDateX}[/] - [bold]{dateX}[/]";

    series1.dataFields.openDateX = "start";
    series1.dataFields.dateX = "end";
    series1.dataFields.categoryY = "category";
    series1.columns.template.propertyFields.fill = "color"; // get color from data
    series1.columns.template.propertyFields.stroke = "color";
    series1.columns.template.strokeOpacity = 1;

    chart.scrollbarX = new am4core.Scrollbar();
  }

  exportActivity() {
    this.spinner.show();
    this.api
      .downloadFile(`api/activity/downloadexcel/${this.AuditID}`)
      .subscribe((blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "Activity.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },(error) => {
        this.spinner.hide();
        console.log(error);
      });
      this.spinner.hide();
  }
  exportSampleActivity() {
    this.spinner.show();
    this.api
      .downloadFile(`api/activity/sampledownloadexcel/${this.AuditID}`)
      .subscribe((blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleActivity.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },(error) => {
        this.spinner.hide();
        console.log(error);
      });
      this.spinner.hide();
  }
  importExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);

    this.api
      .insertData("api/activity/importexcel/" + this.AuditID, formData)
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
      this.spinner.hide();
  }

  getSummary() {
    this.spinner.show();
    this.api.getData("api/activity/getsummary/" + this.AuditID).subscribe(
      (response) => {
        let objResult: any = response;

        this.summaryDue = objResult.due || 0;
        this.summaryCompleted = objResult.completed || 0;
        this.summaryDelayed = objResult.delayed || 0;
        this.summaryInProgress = objResult.inProgress || 0;
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
    this.spinner.hide();
  }

  filterData(option) {
    this.filterOption = option;
    this.loadActivityTable();
  }

  loadActivityTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(`api/activity/GetByAudit/${this.AuditID}/${this.filterOption}`)
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumnsActivity
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
    this.filterOption = "all";
    this.loadActivityTable();
  }

  checkAccess() {
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "planning"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "activity"
    )[0];
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditID = localStorage.getItem("auditId");

    this.loadActivityTable();
    this.initTimeline();

    $(document).ready(() => {
      $("#activityComponent").on("click", ".editActivity", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        // let activityData = $("#" + dataId).data();
        let activityData = window["jQuery"]("#" + dataId).data();
        this.editActivity(activityData);
      });

      $("#activityComponent").on("click", ".deleteActivity", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.deleteActivity(dataId);
      });

      $("#activityComponent").on("click", ".emailActivity", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.emailActivity(dataId);
      });

      $('#planningTab a[href="#activity"]').on("click", (e) => {
        this.loadActivityTable();
      });
    });
  }
}
