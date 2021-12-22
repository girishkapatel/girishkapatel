import { Component, OnInit } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { Router } from "@angular/router";
import * as $ from "jquery";
import * as ClassicEditor from "@ckeditor/ckeditor5-angular";
import { NgxSpinnerService } from "ngx-spinner";
import { UtilsService } from "src/app/services/utils/utils.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { ApiService } from "src/app/services/api/api.service";
import { CheckboxControlValueAccessor } from "@angular/forms";
import * as moment from "moment";
@Component({
  selector: "app-auditadd",
  templateUrl: "manageaudits-view.component.html",
})
export class ManageauditsViewComponent implements OnInit {
  constructor(
    private router: Router,
    private spinner: NgxSpinnerService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private api: ApiService
  ) {}

  public Editor = ClassicEditor;
  isStackHolder: boolean = false;
  accessRights: any = {};
  unplannedAuditAccess: any = {};

  summaryCountAll: number = 0;
  summaryCountPlanned: number = 0;
  summaryCountInProgress: number = 0;
  summaryCountCompleted: number = 0;
  summaryCountUnPlanned: number = 0;
  summaryCountOverdue: number = 0;

  tableId: string = "manageaudites_table";
  // tableAllApiUrl: string;
  // tableAllFiltersAudit: BehaviorSubject<{ init: boolean }>;

  // tableApiUrl: string;
  // tableFiltersAudit: BehaviorSubject<{ init: boolean }>;

  // tableOngoingApiUrl: string;
  // tableFiltersAuditOngoing: BehaviorSubject<{ init: boolean }>;

  // tableCompletedApiUrl: string;
  // tableFiltersAuditCompleted: BehaviorSubject<{ init: boolean }>;

  // tableUnplannedApiUrl: string;
  // tableFiltersAuditUnplanned: BehaviorSubject<{ init: boolean }>;

  // tableData_ongoing: tableData[] = [];
  // tableData_completed: tableData[] = [];
  // tableData_planned: tableData[] = [];
  // tableData_unplanned: tableData[] = [];

  dataTitle: string = "All Audits";

  showAll: boolean = true;
  showGraph: boolean = false;
  showOngoing: boolean = false;
  showCompleted: boolean = false;
  showPlanned: boolean = false;
  showUnplanned: boolean = false;
  showAddAuditForm: boolean = false;
  showunplannedbtn: boolean = false;

  allAuditCount: number = 0;
  onGoingAuditCount: number = 0;
  completedAuditCount: number = 0;
  toBeInitiatedAuditCount: number = 0;
  unplannedAuditCount: number = 0;

  paramStatus: string = "";
  sectors: any = [];

  tableFilters;
  tableColumns: tableColumn[] = [
    {
      title: "Audit No",
      data: "auditNumber",
    },
    {
      title: "Audit Name",
      data: "audit.auditName",
    },
    // {
    // title:'Audit',
    // data:'audit.overallAssesment.processL1',
    // render:function(data){
    // return data && data.name ? data.name : '';
    // }
    // },
    {
      title: "Location",
      data: "location.locationDescription",
    },
    {
      title: "Sector",
      data: "",
      render: (data, row, rowData) => {
        return this.getSectorNames(rowData.location.sector);
      },
    },
    {
      title: "Country",
      data: "location.country.name",
    },
    {
      title: "FY",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.auditStartDate && rowData.auditEndDate)
          return this.utils.getCurrentFinancialYearByDate(
            this.utils.UTCtoLocalDateDDMMMYY(rowData.auditStartDate),
            this.utils.UTCtoLocalDateDDMMMYY(rowData.auditEndDate)
          );
        else return "";
      },
    },
    {
      title: "Quarter",
      data: "quater",
      render: function (data) {
        return data;
      },
    },
    {
      title: "Start Date",
      data: "auditStartDate",
      render: (data) => {
        {
          if (data) {
            return this.utils.UTCtoLocalDateDDMMMYY(data);
          } else {
            return "";
          }
        }
      },
    },
    {
      title: "End Date",
      data: "auditEndDate",
      render: (data) => {
        {
          if (data) {
            return this.utils.UTCtoLocalDateDDMMMYY(data);
          } else {
            return "";
          }
        }
      },
    },
    {
      title: "Status",
      data: "status",
      render: function (data) {
        switch (data) {
          case "inprogress":
            data = "In Progress";
            break;
          case "completed":
            data = "Completed";
            break;
          default:
            data = "To be initiated ";
        }
        return data;
      },
    },
    {
      title: "Action",
      data: "audit.id",
      orderable: false,
      render: (data, type, row, meta) => {
        var buttons = "";

        if (this.unplannedAuditAccess.isEdit && row.status == "unplanned")
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editUnplannedAudits"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.access) {
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-primary editAudits"><i class="fa fa-eye"></i></button>';
        }

        return buttons;
      },
    },
  ];

  showAllAuditInit(tableData: any) {
    if (tableData.settings) {
      this.allAuditCount = tableData.settings.aoData.length;
    }
  }

  showPlannedInit(tableData: any) {
    if (tableData.settings) {
      this.toBeInitiatedAuditCount = tableData.settings.aoData.length;
    }
  }

  showOngoingInit(tableData: any) {
    if (tableData.settings) {
      this.onGoingAuditCount = tableData.settings.aoData.length;
    }
  }

  showCompletedInit(tableData: any) {
    if (tableData.settings) {
      this.completedAuditCount = tableData.settings.aoData.length;
    }
  }

  showUnplannedInit(tableData: any) {
    if (tableData.settings) {
      this.unplannedAuditCount = tableData.settings.aoData.length;
    }
  }

  hideAll() {
    this.showAll = false;
    this.showGraph = false;
    this.showOngoing = false;
    this.showCompleted = false;
    this.showPlanned = false;
    this.showUnplanned = false;
    this.showunplannedbtn = false;
  }

  showDataView(type: string) {
    this.showGraph = type === "graph" ? true : false;
  }

  showData(title: string) {
    // this.hideAll();

    this.dataTitle = title;

    switch (title) {
      case "All Audits":
        this.loadManageAuditesTable("all");
        // this.showAll = true;
        break;
      case "In-Progress Audits":
        // this.showOngoing = true;
        this.loadManageAuditesTable("inprogress");
        break;
      case "Audits Completed":
        // this.showCompleted = true;
        this.loadManageAuditesTable("completed");
        break;
      case "Audits to be Initiated":
        // this.showPlanned = true;
        this.loadManageAuditesTable("planned");
        break;
      case "Audits Overdue":
        // this.showPlanned = true;
        this.loadManageAuditesTable("overdue");
        break;
      case "Unplanned Audits":
        // this.showUnplanned = true;
        this.loadManageAuditesTable("unplanned");
        this.showunplannedbtn = true;
        break;
    }
  }

  addAuditForm() {
    this.router.navigate(["./pages/manageaudits/add"]);
  }

  editAuditForm(dataId) {
    localStorage.setItem("auditId", dataId);
    this.router.navigate(["./pages/manageaudits/edit"]);
  }

  addUnplannedAudit() {
    localStorage.removeItem("unplannedaudit");
    this.router.navigate(["./pages/manageaudits/addunplannedaudit"]);
  }

  getAllSectors() {
    this.commonApi.getSector().subscribe((sectorData) => {
      this.sectors = sectorData;
    });
  }

  getSectorNames(sectorArray) {
    let sectorNames = [];

    for (let sector of this.sectors) {
      if (sectorArray.indexOf(sector.id) > -1) {
        sectorNames.push(sector.name);
      }
    }

    return sectorNames.join(",");
  }

  getSummaryCounts() {
    this.spinner.show();
    this.api
      .getData(
        "api/scopeandschedule/getsummarycounts/",this.commonParam()
      )
      .subscribe(
        (response) => {
          let result: any = response;

          this.summaryCountPlanned = result.planned || 0;
          this.summaryCountInProgress = result.inprogress || 0;
          this.summaryCountCompleted = result.completed || 0;
          this.summaryCountUnPlanned = result.unplanned || 0;
          this.summaryCountOverdue = result.overdue || 0;

          this.summaryCountAll =
            this.summaryCountPlanned +
            this.summaryCountInProgress +
            this.summaryCountCompleted +
            this.summaryCountUnPlanned +
            this.summaryCountOverdue;
          this.spinner.hide();
        },
        (error) => {
          // console.log(error);
          this.spinner.hide();
        }
      );
  }
  commonParam() {
    let dateRange = window["jQuery"]("#dashboard-report-range").data(
      "daterangepicker"
    );
    let startDate =
      dateRange && dateRange.startDate._d
        ? moment(dateRange.startDate._d).format("YYYY-MM-DD")
        : "";
    let endDate =
      dateRange && dateRange.endDate._d
        ? moment(dateRange.endDate._d).format("YYYY-MM-DD")
        : "";
    let filterObj = {
      StartDate: startDate,
      EndDate: endDate,
      UserId: localStorage.getItem("userId"),
      Status: this.paramStatus,
    };
    return filterObj;
  }
  loadManageAuditesTable(status) { 
    this.spinner.show();
    this.getSummaryCounts();
    this.paramStatus = status;
    this.api
      .getData("api/scopeandschedule/GetAuditsByUser/", this.commonParam())
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumns
          );
          this.spinner.hide();
        },
        (error) => {
          // console.log(error);
          this.spinner.hide();
        }
      );
  }

  checkAccess() {
    this.accessRights = this.utils.getSubmoduleAccess("manageaudits")[0];

    this.unplannedAuditAccess = this.utils.getAccessOnLevel1(
      "manageaudits",
      "unplannedaudit"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    // this.tableAllApiUrl = `api/scopeandschedule/GetAuditsByUser/${localStorage.getItem(
    // "userId"
    // )}/all`;
    // this.tableAllFiltersAudit = new BehaviorSubject({ init: true });

    // this.tableApiUrl = `api/scopeandschedule/GetAuditsByUser/${localStorage.getItem(
    // "userId"
    // )}/planned`;
    // this.tableFiltersAudit = new BehaviorSubject({ init: true });

    // this.tableOngoingApiUrl = `api/scopeandschedule/GetAuditsByUser/${localStorage.getItem(
    // "userId"
    // )}/inprogress`;
    // this.tableFiltersAuditOngoing = new BehaviorSubject({ init: true });

    // this.tableCompletedApiUrl = `api/scopeandschedule/GetAuditsByUser/${localStorage.getItem(
    // "userId"
    // )}/completed`;
    // this.tableFiltersAuditCompleted = new BehaviorSubject({ init: true });

    // this.tableUnplannedApiUrl = `api/scopeandschedule/GetAuditsByUser/${localStorage.getItem(
    // "userId"
    // )}/unplanned`;
    // this.tableFiltersAuditUnplanned = new BehaviorSubject({ init: true });

    this.getAllSectors();

    $(document).ready(() => {
      $("body").on("click", ".editAudits", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        if (dataId) {
          this.editAuditForm(dataId);
        }
      });

      $("body").on("click", ".editUnplannedAudits", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        if (dataId) {
          localStorage.setItem("unplannedaudit", dataId);
          this.router.navigate(["./pages/manageaudits/addunplannedaudit"]);
        }
      });
      $("body").on("click", ".applyBtn", () => {
        this.loadManageAuditesTable("all");
      });
      let financialYear = this.utils.getCurrentFinancialYear();

      window["jQuery"]("#dashboard-report-range").daterangepicker(
        {
          ranges: {
            Today: [moment(), moment()],
            Yesterday: [
              moment().subtract("days", 1),
              moment().subtract("days", 1),
            ],
            "Last 7 Days": [moment().subtract("days", 6), moment()],
            "Last 30 Days": [moment().subtract("days", 29), moment()],
            "This Month": [moment().startOf("month"), moment().endOf("month")],
            "Last Month": [
              moment().subtract("month", 1).startOf("month"),
              moment().subtract("month", 1).endOf("month"),
            ],
          },
          locale: {
            format: "DD/MM/YYYY",
            separator: " - ",
            applyLabel: "Apply",
            cancelLabel: "Cancel",
            fromLabel: "From",
            toLabel: "To",
            customRangeLabel: "Custom",
            daysOfWeek: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"],
            monthNames: [
              "January",
              "February",
              "March",
              "April",
              "May",
              "June",
              "July",
              "August",
              "September",
              "October",
              "November",
              "December",
            ],
            firstDay: 1,
          },
          startDate: financialYear.startDate,
          endDate: financialYear.endDate,
          opens: false ? "right" : "left",
        },
        function (start, end, label) {
          if ($("#dashboard-report-range").attr("data-display-range") != "0") {
            let startDate = start.format("DD/MM/YYYY");
            let endDate = end.format("DD/MM/YYYY");
            $("#dashboard-report-range span").html(startDate + " - " + endDate);
          }
        }
      );
      if ($("#dashboard-report-range").attr("data-display-range") != "0") {
        $("#dashboard-report-rgane span").html(
          financialYear.startDate + " - " + financialYear.endDate
        );
      }
      $("#dashboard-report-range").show();
      // $(".input-mini").prop("disabled", true);
      this.loadManageAuditesTable("all");
    });
    
  }
}
