import { Component, OnInit, AfterViewInit } from "@angular/core";
import * as $ from "jquery";
import * as moment from "moment";
import { BehaviorSubject } from "rxjs";
import * as am4core from "@amcharts/amcharts4/core";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import { tableColumn } from "src/app/common/table/table.model";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ApiService } from "src/app/services/api/api.service";
import { NgxSpinnerService } from "ngx-spinner";
import { CommonApiService } from "src/app/services/utils/commonapi.service";

am4core.useTheme(am4themes_animated);

@Component({
  selector: "app-reports",
  templateUrl: "./reports.component.html",
  styleUrls: ["./reports.component.css"],
})
export class ReportsComponent implements OnInit {
  constructor(
    private utils: UtilsService,
    private api: ApiService,
    private spinner: NgxSpinnerService,
    private commonApi: CommonApiService
  ) {}

  accessRights: any = {};

  reportType = "audit";
  company: string = "";
  audit: string = "";
  status: string = "";
  criticality: string = "";
  quaters: string = "";
  location: string = "";
  startDate: string = "";
  endDate: string = "";
  ImplementationOwnerId: string = "";
  ProcessOwnerId: string = "";

  showGraph: boolean = false;
  showFilters: boolean = true;

  showAudit: boolean = true;
  auditTableFilters;
  auditTableApiUrl: string = "";

  showFollowup: boolean = false;
  followupTableFilters;
  followupTableApiUrl: string = "";

  showDatatracker: boolean = false;
  datatrackerTableFilters;
  datatrackerTableApiUrl: string = "";
  showActivity: boolean = false;
  isInitialTable: boolean = false;

  activityTableFilters;
  activityTableApiUrl: string = "";
  allRootCauses: any = [];

  statusOpts = [];
  GradingOpts = [];
  companyOpt: any = [];
  tempcompanyOpt: any = [];
  auditOpt: any = [];
  locationOpt: any = [];
  shOpts: any = [];

  dataTrackerUserOpts: any = [];
  processWiseChartData: any = [];
  processOpts: any = [];

  auditTableColumns: tableColumn[] = [
    // {
    //   title: "Audit No",
    //   data: "auditNumber",
    // },
    {
      title: "Audit",
      data: "processLocationMapping",
      render: function (data) {
        return data ? data.auditName : "";
      },
    },
    {
      title: "Location",
      data: "location.profitCenterCode",
    },
    {
      title: "Sector",
      data: "location.divisionDescription",
    },
    {
      title: "Country",
      data: "location.country.name",
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
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.UTCtoLocalDateDDMMMYY(rowData.auditStartDate)}`;
      },
    },
    {
      title: "End Date",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.UTCtoLocalDateDDMMMYY(rowData.auditEndDate)}`;
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
      title: "Criticality",
      data: "audit.overallAssesment.processRiskMapping",
      render: function (data) {
        return data && data.finalProcessrating ? data.finalProcessrating : "";
      },
    },
  ];

  followupTableColumns: tableColumn[] = [
    {
      title: "Audit",
      data: "processLocationMapping",
      render: function (data) {
        return data ? data.auditName : "";
      },
    },
    {
      title: "Location",
      data: "",
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
      title: "Root Cause",
      data: "",
      render: (data, row, rowData) => {
        return this.getRootCauseNames(
          rowData.draftReport
            ? rowData.draftReport.rootCauses
            : rowData.rootCauses
        );
      },
    },
    {
      title: "Risk Type",
      data: "",
      render: (data, row, rowData) => {
        return rowData.riskType ? rowData.riskType.name : "";
      },
    },
    // {
    //   title: "Implementation Owner",
    //   data: "implementationOwner",
    //   render: (data) => {
    //     return data ? `${data.firstName} ${data.lastName}` : "";
    //   },
    // },
    {
      title: "End Date",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.UTCtoLocalDateDDMMMYY(
          rowData.implementationEndDate
        )}`;
      },
    },
    {
      title: "Status",
      data: "status",
      render: function (data) {
        if (data) {
        let status = data.toLowerCase();
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
            return status;
          }
          return "";
      },
    },
    // {
    //   title: "Comments",
    //   data: "comments",
    // },
  ];

  datatrackerTableColumns: tableColumn[] = [
    {
      title: "Audit",
      data: "audit",
      render: function (data) {
        return data && data.processLocationMapping
          ? data.processLocationMapping.auditName
          : "";
      },
    },
    {
      title: "Location",
      data: "audit.location",
      render: function (data) {
        return data && data.profitCenterCode ? data.profitCenterCode : "";
      },
    },
    {
      title: "Area",
      data: "area",
    },
    {
      title: "Data Requested",
      data: "dataRequested",
    },
    {
      title: "Process Owner",
      data: "processOwner",
      render: (data) => {
        return data ? `${data.firstName} ${data.lastName}` : "";
      },
    },
    {
      title: "Data Request Date",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.formatDbDateToDMY(rowData.dataRequestDate)}`;
      },
    },
    {
      title: "Data Received Date",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.formatDbDateToDMY(rowData.dataReceivedDate)}`;
      },
    },
    {
      title: "Status",
      data: "status",
      render: (data) => {
        if (data) {
          let d = data.replace(" ", "").toLowerCase();
          let bgColor = this.utils.dtRatingColor[d];
          let textColor = "#ffffff";
          return `<div style="padding: 3px 10px; background:${bgColor}; color:${textColor}"  class="text-capitalize">${
            data ? data.toUpperCase() : ""
          }</div>`;
        } else {
          return "INPROGRESS";
        }
      },
    },
    {
      title: "Overdue (In Days)",
      data: "overdueInDays",
      render: (data, row, rowData) => {
        let rqDate = rowData.dataRequestDate ? rowData.dataRequestDate : "";
        let recDate = rowData.dataReceivedDate ? rowData.dataReceivedDate : "";
        return rqDate ? this.getDateDiff(rqDate, recDate) : 0;
      },
    },
  ];

  activityTableColumns: tableColumn[] = [
    // {
    //   title: "Business Process",
    //   data: "audit.processLocationMapping.businessCycle",
    //   render: function (data) {
    //     return data ? data.name : "";
    //   },
    // },
    // {
    //   title: "Process L1",
    //   data: "audit.processLocationMapping.processL1",
    //   render: function (data) {
    //     return data ? data.name : "";
    //   },
    // },
    // {
    //   title: "Process L2",
    //   data: "audit.processLocationMapping.processL2",
    //   render: function (data) {
    //     return data ? data.name : "";
    //   },
    // },
    {
      title: "Audit",
      data: "audit",
      render: function (data) {
        return data && data.processLocationMapping
          ? data.processLocationMapping.auditName
          : "";
      },
    },
    {
      title: "Location",
      data: "audit.location",
      render: function (data) {
        return data && data.profitCenterCode ? data.profitCenterCode : "";
      },
    },
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
  ];

  handleGraph(status) {
    if (status === "show") {
      this.showGraph = true;
    } else {
      this.showGraph = false;
    }
  }

  handleFilters() {
    this.showFilters = !this.showFilters;
  }

  getDateDiff(d1, d2) {
    let date1: any = d1 ? new Date(d1) : new Date();
    let date2: any = d2 ? new Date(d2) : new Date();
    const diffTime = Math.abs(date2 - date1);
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }

  getReportFilters() {
    let dateRange = window["jQuery"]("#report-range").data("daterangepicker");
    this.startDate =
      dateRange && dateRange.startDate._d
        ? moment(dateRange.startDate._d).format("YYYY-MM-DD")
        : "";
    this.endDate =
      dateRange && dateRange.endDate._d
        ? moment(dateRange.endDate._d).format("YYYY-MM-DD")
        : "";

    return {
      Company: this.company,
      Audit: this.audit,
      AuditId: this.audit,
      Location: this.location,
      CompletionStatus: this.status,
      Quarter: this.quaters,
      Rating: this.criticality,
      StartDate: this.startDate,
      EndDate: this.endDate,
      ImplementationOwnerId: this.ImplementationOwnerId,
      ProcessOwnerId: this.ProcessOwnerId,
      Status: this.status,
    };
  }

  initReportTable() {
    this.spinner.show();
    this.processWiseChartData = [];
    this.showAudit = false;
    this.showFollowup = false;
    this.showDatatracker = false;
    this.showActivity = false;
    this.companyOpt = [];
    this.locationOpt = [];
    this.auditOpt = [];
    switch (this.reportType) {
      case "audit":
        this.showAudit = true;
        this.initAuditReport();
        this.auditplanStatusData();
        this.auditplangrading(); 
        this.fillCompanyOptionsforSchedule();
        this.fillAuditOptionsforSchedule();
        break;
      case "followup":
        this.showFollowup = true;
        this.initFollowupReport();
        this.commongradingData();
        this.followStatusData();
        this.fillAuditOptionsforSchedule();
        break;
      case "datatracker":
        this.showDatatracker = true;
        this.initDatatrackerReport();
        this.dataTrackerStatus();
        this.fillAuditOptionsforSchedule();
        break;
      case "activity":
        this.showActivity = true;
        this.initActivityReport();
        this.commonStatusData();
        this.fillCompanyOptionsforSchedule();
        this.fillAuditOptionsforSchedule();
        break;
    }
  }

  initAuditReport() {
    let filters = this.getReportFilters();
    this.auditTableFilters.next(filters);
  }

  initFollowupReport() {
    let filters = this.getReportFilters();
    this.followupTableFilters.next(filters);
  }

  initDatatrackerReport() {
    let filters = this.getReportFilters();
    this.datatrackerTableFilters.next(filters);
  }

  initActivityReport() {
    let filters = this.getReportFilters();
    this.activityTableFilters.next(filters);
  }

  auditResult(tableData) {
    if (tableData.settings && tableData.settings.aoData) {
      let processWiseStat = {};
      let companyOptIds = [];
      let auditOptIds = [];
      let locationOptIds = [];
      for (let i = 0; i < tableData.settings.aoData.length; i++) {
        let auditData = tableData.settings.aoData[i]._aData;
        // if (companyOptIds.indexOf(auditData.location.company.id) === -1) {
        //   this.companyOpt.push({
        //     id: auditData.location.company.id,
        //     name: auditData.location.company.name,
        //   });
        //   companyOptIds.push(auditData.location.company.id);
        // }

        // if (auditOptIds.indexOf(auditData.id) === -1) {
        //   this.auditOpt.push({
        //     id: auditData.auditId,
        //     name: auditData.auditNumber,
        //   });
        //   auditOptIds.push(auditData.id);
        // }

        // if (locationOptIds.indexOf(auditData.location.id) === -1) {
        //   this.locationOpt.push({
        //     id: auditData.location.id,
        //     name: auditData.location.profitCenterCode,
        //   });
        //   locationOptIds.push(auditData.id);
        // }

        let auditStatus = this.analyseAuditStatus(auditData);
        if (auditData.audit["processLocationMapping"]) {
          let process =
            auditData.audit["processLocationMapping"]["businessCycles"];

          if (typeof processWiseStat[process] === "undefined") {
            processWiseStat[process] = {
              inprogress: 0,
              overdue: 0,
              completed: 0,
            };
          }
          if (auditStatus) {
            processWiseStat[process][auditStatus] =
              processWiseStat[process][auditStatus] + 1;
          }
        }
      }
      for (let processName in processWiseStat) {
        this.processWiseChartData.push(
          this.getChartData(processName, processWiseStat[processName], "status")
        );
      }
    }
  }

  followupResult(tableData) {
    if (tableData.settings && tableData.settings.aoData) {
      let processWiseStat = {};
      let companyOptIds = [];
      let auditOptIds = [];
      let shOptIds = [];

      for (let i = 0; i < tableData.settings.aoData.length; i++) {
        let followupData = tableData.settings.aoData[i]._aData;
        let auditData = followupData.audit;
        if (auditData != null) {
          if (
            followupData.location &&
            companyOptIds.indexOf(followupData.location.company.id) === -1
          ) {
            this.companyOpt.push({
              id: followupData.location.company.id,
              name: followupData.location.company.name,
            });
            companyOptIds.push(followupData.location.company.id);
          }

          // if (auditOptIds.indexOf(auditData.id) === -1) {
          //   this.auditOpt.push({
          //     id: auditData.auditId,
          //     name: auditData.auditNumber,
          //   });
          //   auditOptIds.push(auditData.id);
          // }

          // if (
          //   followupData.actionPlansInfo &&
          //   shOptIds.indexOf(followupData.actionPlansInfo.FollowupId) === -1
          // ) {
          //   this.shOpts.push({
          //     id: followupData.implementationOwner.id,
          //     name: `${followupData.implementationOwner.firstName} ${followupData.implementationOwner.lastName}`,
          //   });
          //   shOptIds.push(followupData.implementationOwner.id);
          // }

          // if(locationOptIds.indexOf(followupData.location.id) === -1){
          //   this.locationOpt.push({
          //     id:followupData.location.id,
          //     name:followupData.location.profitCenterCode
          //   })
          //   locationOptIds.push(followupData.id);
          // }

          let followupStatus = this.analyseFollowupStatus(followupData);
          let process =
            followupData.audit.audit["processLocationMapping"][
              "businessCycles"
            ];

          if (typeof processWiseStat[process] === "undefined") {
            processWiseStat[process] = {
              inprogress: 0,
              overdue: 0,
              completed: 0,
            };
          }
          if (followupStatus) {
            processWiseStat[process][followupStatus] =
              processWiseStat[process][followupStatus] + 1;
          }
        }
        for (let processName in processWiseStat) {
          this.processWiseChartData.push(
            this.getChartData(
              processName,
              processWiseStat[processName],
              "status"
            )
          );
        }
      }
    }
  }
  followStatusData() {
    this.statusOpts = [
      { label: "In Progress", value: "inprogress" },
      { label: "Completed", value: "completed" },  { label: "Pending To be initiated", value: "pending" },
    ];
  }
  auditplanStatusData() {
    this.statusOpts = [
      { label: "In Progress", value: "inprogress" },
      { label: "Completed", value: "completed" },  { label: "To be initiated", value: null },
    ];
  }
  commonStatusData() {
    this.statusOpts = [
      { label: "In Progress", value: "inprogress" },
      { label: "Completed", value: "completed" },  
    ];
  }
  dataTrackerStatus() {
    this.statusOpts = [
      {
        label: "Partially Received",
        value: "Partially Received",
      },
      {
        label: "Received",
        value: "Received",
      },
      {
        label: "Pending",
        value: "Pending",
      },
    ];
  }
  auditplangrading() {
    this.GradingOpts = [
      { label: "High", value: "High" }, 
      { label: "Medium", value: "Medium" },  { label: "Low", value: "Low" },
    ];
  }
  commongradingData() {
    this.GradingOpts = [
      { label: "Critical", value: "Critical" }, { label: "High", value: "High" },
      { label: "Medium", value: "Medium" },  { label: "Low", value: "Low" },
    ];
  }
  datatrackerResult(tableData) {
    if (tableData.settings && tableData.settings.aoData) {
      let processWiseStat = {};
      let auditOptIds = [];
      let shOptIds = [];
      for (let i = 0; i < tableData.settings.aoData.length; i++) {
        let dtData = tableData.settings.aoData[i]._aData;
        let ssData = dtData.scopeAndSchedule;

        // if (auditOptIds.indexOf(ssData.auditId) === -1) {
        //   this.auditOpt.push({
        //     id: ssData.auditId,
        //     name: ssData.auditNumber,
        //   });
        //   auditOptIds.push(ssData.auditId);
        // }

        // if (
        //   dtData.processOwner &&
        //   shOptIds.indexOf(dtData.processOwner.id) === -1
        // ) {
        //   this.dataTrackerUserOpts.push({
        //     id: dtData.processOwner.id,
        //     name: `${dtData.processOwner.firstName} ${dtData.processOwner.lastName}`,
        //   });
        //   shOptIds.push(dtData.processOwner.id);
        // }

        let dtStatus = dtData.status.toLowerCase().replace(" ", "");
        let process = dtData.audit["processLocationMapping"]["businessCycles"];

        if (typeof processWiseStat[process] === "undefined") {
          processWiseStat[process] = {
            pending: 0,
            received: 0,
            partiallyreceived: 0,
          };
        }
        if (dtStatus) {
          processWiseStat[process][dtStatus] =
            processWiseStat[process][dtStatus] + 1;
        }
      }
      for (let processName in processWiseStat) {
        this.processWiseChartData.push(
          this.getChartData(
            processName,
            processWiseStat[processName],
            "dtstatus"
          )
        );
      }
    }
  }

  activityResult(tableData) {
    if (tableData.settings && tableData.settings.aoData) {
      let processWiseStat = {};
      let companyOptIds = [];
      let auditOptIds = [];
      let locationOptIds = [];
      for (let i = 0; i < tableData.settings.aoData.length; i++) {
        let activityData = tableData.settings.aoData[i]._aData;
        let auditData = activityData.audit;
        // if(auditData.location && companyOptIds.indexOf(auditData.location.company.id) === -1){
        //   this.companyOpt.push({
        //     id:auditData.location.company.id,
        //     name:auditData.location.company.name
        //   })
        //   companyOptIds.push(auditData.location.company.id);
        // }

        // if (auditOptIds.indexOf(auditData.id) === -1) {
        //   this.auditOpt.push({
        //     id: auditData.id,
        //     name: auditData.auditNumber,
        //   });
        //   auditOptIds.push(auditData.id);
        // }

        // if (locationOptIds.indexOf(auditData.location.id) === -1) {
        //   this.locationOpt.push({
        //     id: auditData.location.id,
        //     name: auditData.location.profitCenterCode,
        //   });
        //   locationOptIds.push(auditData.id);
        // }

        let activityStatus = this.analyseActivityStatus(activityData);
        let process =
          activityData.audit["processLocationMapping"]["businessCycles"];

        if (typeof processWiseStat[process] === "undefined") {
          processWiseStat[process] = {
            inprogress: 0,
            overdue: 0,
            completed: 0,
          };
        }
        if (activityStatus) {
          processWiseStat[process][activityStatus] =
            processWiseStat[process][activityStatus] + 1;
        }
      }
      for (let processName in processWiseStat) {
        this.processWiseChartData.push(
          this.getChartData(processName, processWiseStat[processName], "status")
        );
      }
    }
  }

  getChartData(chartName, chartStat, chartType) {
    let chartdata = this.processOpts.filter((x) => x.id === chartName);
    if (chartdata != null) {
      return;
    }
    chartName = chartdata[0].name;
    let chartData = {};
    if (
      chartStat.completed === 0 &&
      chartStat.inprogress === 0 &&
      chartStat.overdue === 0
    ) {
      chartData = {
        key: chartName,
        data: [
          {
            status: "No Data",
            disabled: true,
            count: 1,
            color: am4core.color("#dadada"),
            opacity: 0.3,
            strokeDasharray: "4,4",
            tooltip: "",
          },
        ],
      };
    } else {
      chartData = this.chartStat(chartName, chartStat, chartType);
    }
    return chartData;
  }

  chartStat(chartName, chartStat, chartType) {
    let chartData = {};
    switch (chartType) {
      case "status":
        chartData = {
          key: chartName,
          data: [
            {
              status: "Completed",
              count: chartStat.completed,
              color: am4core.color("#ffe600"),
            },
            {
              status: "In Progress",
              count: chartStat.inprogress,
              color: am4core.color("#2e2e38"),
            },
            {
              status: "Overdue",
              count: chartStat.overdue,
              color: am4core.color("#c4c4cd"),
            },
          ],
        };
        break;
      case "dtstatus":
        chartData = {
          key: chartName,
          data: [
            {
              status: "Pending",
              count: chartStat.pending,
              color: am4core.color("#ffe600"),
            },
            {
              status: "Received",
              count: chartStat.received,
              color: am4core.color("#2e2e38"),
            },
            {
              status: "Partially Received",
              count: chartStat.partiallyreceived,
              color: am4core.color("#c4c4cd"),
            },
          ],
        };
        break;
    }

    return chartData;
  }

  analyseAuditStatus(audit) {
    let status = audit.status;
    let overdue = this.utils.compareDate(audit.auditEndDate, "lt");
    let currentStatus = "";
    try {
      currentStatus =
        status === "inprogress" && overdue
          ? "overdue"
          : status === "completed" && overdue
          ? "completed"
          : status === "inprogress"
          ? "inprogress"
          : status === "completed"
          ? "completed"
          : "";
    } catch (err) {
      currentStatus = "";
    }
    return currentStatus;
  }

  analyseFollowupStatus(followup) {
    let status = followup.status;
    let overdue = this.utils.compareDate(followup.implementationEndDate, "lt");
    let currentStatus = "";
    try {
      currentStatus =
        status === "inprogress" && overdue
          ? "overdue"
          : status === "completed" && overdue
          ? "completed"
          : status === "inprogress"
          ? "inprogress"
          : status === "completed"
          ? "completed"
          : "";
    } catch (err) {
      currentStatus = "";
    }
    return currentStatus;
  }

  analyseActivityStatus(activity) {
    let status = activity.activityStatus
      ? activity.activityStatus
      : "inprogress";
    let overdue = this.utils.compareDate(activity.plannedEndDate, "lt");
    let currentStatus = "";
    try {
      currentStatus =
        status === "inprogress" && overdue
          ? "overdue"
          : status === "completed" && overdue
          ? "completed"
          : status === "inprogress"
          ? "inprogress"
          : status === "completed"
          ? "completed"
          : "";
    } catch (err) {
      currentStatus = "";
    }
    return currentStatus;
  }

  fillProcessOpts() {
    this.spinner.show();
    this.api.getData("api/businesscycle").subscribe(
      (res) => {
        this.processOpts = res;
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
      return rootCauseNames.join(", ");
    }
      return ""; 
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
  fillUserOptions() {
    this.spinner.show();
    this.api.getData("api/FollowUp/GetUserbyFollowUp").subscribe(
      (res) => {
        this.shOpts = res;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  fillUserOptionsforInitial() {
    this.spinner.show();
    this.api
      .getData("api/InitialDataRequest/GetUserbyInitialDataRequest")
      .subscribe(
        (res) => {
          this.dataTrackerUserOpts = res;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  fillCompanyOptionsforSchedule() {
    this.spinner.show();
    this.api
      .getData("api/ScopeAndSchedule/GetCompanybyScopeSchedule")
      .subscribe(
        (res) => {
          this.tempcompanyOpt = res;
          this.spinner.hide();
          const uniqueCompany = this.tempcompanyOpt.filter(
            (thing, i, arr) =>
              arr.findIndex((t) => t.companyID === thing.companyID) === i
          );
          // var filteredArray = this.tempcompanyOpt.filter(
          //   (thing, i, arr) => arr.findIndex(t => t.id === thing.id) === i
          // });
          this.companyOpt = uniqueCompany;
          this.locationOpt = res;
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    this.spinner.hide();
  }
  fillAuditOptionsforSchedule() {
    this.spinner.show();
    this.api.getData("api/ScopeAndSchedule/GetAuditbyScopeSchedule").subscribe(
      (res) => {
        this.auditOpt = res;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  checkAccess() {
    this.accessRights = this.utils.getSubmoduleAccess("reports");
  }
  //#region  Export Excel and PDF
  exportAuditExcel() {
    this.spinner.show();
    this.api
      .downloadFile("api/ScopeAndSchedule/downloadexcelforReport/",this.getReportFilters())
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });
          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "ScopeAndSchedule.xlsx");
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
  exportAuditPDF() {
    this.spinner.show();
    this.api.downloadFile(`api/ScopeAndSchedule/downloadpdf`).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/pdf",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "ScopeAndSchedule.pdf");
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
  exportFollowupExcel() {
    this.spinner.show();
    this.api.downloadFile("api/FollowUp/downloadexcelforReport/",this.getReportFilters()).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });
        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "FollowUp.xlsx");
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
  exportFollowupPDF() {
    this.spinner.show();
    this.api.downloadFile(`api/FollowUp/downloadpdf`).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/pdf",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "FollowUp.pdf");
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
  exportToDatatrackerExcel() {
    this.spinner.show;
    this.api
      .downloadFile("api/InitialDataRequest/downloadexcelforReport/",this.getReportFilters())
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });
          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "Datatracker.xlsx");
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
  exportToDatatrackerPDF() {
    this.spinner.show();
    this.api
      .downloadFile(`api/InitialDataRequest/downloadpdfForReport`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/pdf",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "Datatracker.pdf");
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

  exportActivityExcel() {
    this.spinner.show();
    this.api.downloadFile("api/Activity/downloadexcelforReport/",this.getReportFilters()).subscribe(
      (blob) => {
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
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  exportActivityPDF() {
    this.spinner.show();
    this.api.downloadFile(`api/Activity/downloadpdf`).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/pdf",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "Activity.pdf");
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
  //#endregion Export Excel and PDF
  ngOnInit() {
    this.checkAccess();
    this.fillProcessOpts();
    this.getAllRootCauses();
    this.fillUserOptions();
    this.fillUserOptionsforInitial();

    this.auditTableApiUrl = `api/scopeandschedule/GetDashboard`;
    this.auditTableFilters = new BehaviorSubject({ init: false });

    this.followupTableApiUrl = `api/followup/GetFollowup`;
    this.followupTableFilters = new BehaviorSubject({ init: false });

    this.datatrackerTableApiUrl = `api/initialdatarequest/GetInitialDataRequest`;
    this.datatrackerTableFilters = new BehaviorSubject({ init: false });

    this.activityTableApiUrl = `api/activity/GetActivity`;
    this.activityTableFilters = new BehaviorSubject({ init: false });

    $(document).ready(() => {
      $("#downloadReportExcel").on("click", () => {
        if (this.showAudit) {
          $("#audit_table_wrapper .buttons-excel").click();
        }

        if (this.showFollowup) {
          $("#followup_table_wrapper .buttons-excel").click();
        }

        if (this.showActivity) {
          $("#activity_table_wrapper .buttons-excel").click();
        }

        if (this.showDatatracker) {
          $("#datatracker_table_wrapper .buttons-excel").click();
        }
      });

      $("#downloadReportPdf").on("click", () => {
        if (this.showAudit) {
          $("#audit_table_wrapper .buttons-pdf").click();
        }

        if (this.showFollowup) {
          $("#followup_table_wrapper .buttons-pdf").click();
        }

        if (this.showActivity) {
          $("#activity_table_wrapper .buttons-pdf").click();
        }

        if (this.showDatatracker) {
          $("#datatracker_table_wrapper .buttons-pdf").click();
        }
      });

      let financialYear = this.utils.getCurrentFinancialYear();
      window["jQuery"]("#report-range").daterangepicker(
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
            format: "MM/DD/YYYY",
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
        (start, end, label) => {
          if ($("#report-range").attr("data-display-range") != "0") {
            let startDate = start.format("MM/DD/YYYY");
            let endDate = end.format("MM/DD/YYYY");
            $("#report-range span").html(startDate + " - " + endDate);
            this.initReportTable();
          }
        }
      );

      if ($("#report-range").attr("data-display-range") != "0") {
        $("#report-range span").html(
          financialYear.startDate + " - " + financialYear.endDate
        );
      }

      $("#report-range").show();

      this.initReportTable();
    });
  }
}
