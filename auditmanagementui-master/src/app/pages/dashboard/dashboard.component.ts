import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
} from "@angular/core";
import * as am4core from "@amcharts/amcharts4/core";
import * as am4maps from "@amcharts/amcharts4/maps";
import { tableColumn, tableData } from "./../../common/table/table.model";
import am4geodata_worldIndiaLow from "../../common/data/worldindiaLow.data";
import * as moment from "moment";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import * as $ from "jquery";
import { Router } from "@angular/router";
import { ApiService } from "src/app/services/api/api.service";
import { BehaviorSubject } from "rxjs";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgxSpinnerService } from "ngx-spinner";

am4core.useTheme(am4themes_animated);

interface ActionTaken {
  auditRequired: number;
  dueToday: number;
  delayed: number;
  notdue: number;
  completed: number;
  revisedTimeline: number;
}

interface AuditExecution {
  auditInitiated: number;
  auditPending: number;
  auditCompleted: number;
  completedWithDelayed: number;
  overdue: number;
  unPlannedAudit: number;
}
interface DashboardTableData {
  StartDate: string;
  EndDate: string;
  Sector: string;
  Country: string;
  Company: string;
  Rating: string;
}
interface ValueScorecard {
  potentialSaving: number;
  enhancement: number;
  bestPractices: number;
}

interface Assurance {
  redFlags: number;
  statutoryDefault: number;
  highRatedObservations: number;
  repeatObservations: number;
  controlsReport: number;
}

interface AuditHealth {
  onTimeCompilation: number;
  dataChallenge: number;
  totalData: number;
  pendingData: number;
}

interface AuditPieChart {
  initiated: number;
  completed: number;
  inprogress: number;
  overdue: number;
  unplanned: number;
}

class Overview {
  constructor() {
    this.actionTaken;
    this.assurance;
    this.auditExecution;
    this.auditHealth;
    this.valueScorecard;
  }

  auditExecution: AuditExecution = {
    auditInitiated: 0,
    auditPending: 0,
    auditCompleted: 0,
    completedWithDelayed: 0,
    overdue: 0,
    unPlannedAudit: 0,
  };

  actionTaken: ActionTaken = {
    auditRequired: 0,
    dueToday: 0,
    delayed: 0,
    notdue: 0,
    completed: 0,
    revisedTimeline: 0,
  };

  valueScorecard: ValueScorecard = {
    potentialSaving: 0,
    enhancement: 0,
    bestPractices: 0,
  };

  assurance: Assurance = {
    redFlags: 0,
    statutoryDefault: 0,
    highRatedObservations: 0,
    repeatObservations: 0,
    controlsReport: 0,
  };

  auditHealth: AuditHealth = {
    onTimeCompilation: 0,
    dataChallenge: 0,
    totalData: 0,
    pendingData: 0,
  };
}

class TableOverview {
  constructor() {
    this.actionTaken;
    this.assurance;
    this.auditExecution;
    this.auditHealth;
    this.valueScorecard;
  }

  auditExecution: AuditExecution = {
    auditInitiated: 0,
    auditPending: 0,
    auditCompleted: 0,
    completedWithDelayed: 0,
    overdue: 0,
    unPlannedAudit: 0,
  };

  actionTaken: ActionTaken = {
    auditRequired: 0,
    dueToday: 0,
    delayed: 0,
    notdue: 0,
    completed: 0,
    revisedTimeline: 0,
  };

  valueScorecard: ValueScorecard = {
    potentialSaving: 0,
    enhancement: 0,
    bestPractices: 0,
  };

  assurance: Assurance = {
    redFlags: 0,
    statutoryDefault: 0,
    highRatedObservations: 0,
    repeatObservations: 0,
    controlsReport: 0,
  };

  auditHealth: AuditHealth = {
    onTimeCompilation: 0,
    dataChallenge: 0,
    totalData: 0,
    pendingData: 0,
  };
}

class MapObj {
  constructor() {
    this.plottingData = [];
    this.countryRiskMap = [];
    this.overview = new Overview();
  }
  plottingData: Array<Object>;
  countryRiskMap: Array<Object>;
  overview: Overview;
}

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.css"],
})
export class DashboardComponent implements OnInit, OnDestroy {
  constructor(
    private router: Router,
    private api: ApiService,
    private utils: UtilsService,
    private spinner: NgxSpinnerService
  ) { }
  @ViewChild("dataDiv", { static: false }) dataDiv: ElementRef;
  tableTitle: string = "Overall Audits";
  colorSet = new am4core.ColorSet();
  showDetails: boolean = false;
  worldchart: any;
  auditOverview: Overview;
  BusinessUnitData: any = [];
  BusinessUnitTitle: string = "All Sectors";
  BusinessUnit: string = "";

  CompanyData: any = [];
  CompanyTitle: string = "All Companies";
  Company: string = "";

  CountryData: any = [];
  CountryTitle: string = "All Countries";
  Country: string = "";

  CriticalityData: any = ["High", "Medium", "Low"];
  CriticalityTitle: string = "All Criticality";
  Criticality: string = "";

  divisionChartDataCount: { [key: string]: AuditPieChart } = {};
  locationChartDataCount: { [key: string]: AuditPieChart } = {};

  overviewObjCountryWise: { [key: string]: any } = {};
  overallChartObj: { [key: string]: AuditPieChart } = {};
  divisionChartObj: { [key: string]: { [key: string]: AuditPieChart } } = {};
  locationChartObj: { [key: string]: { [key: string]: AuditPieChart } } = {};

  byDivision: any[] = [];
  byProcess: any[] = [];
  byLocation: any[] = [];
  auditDataMap: any[] = [];
  overallDataChange;
  divisionDataChange;
  locationDataChange;
  chartlocationList: { [key: string]: Array<any> } = {};
  chartdivisionList: { [key: string]: Array<any> } = {};

  chartlocationPopulate: any = [];
  chartdivisionPopulate: any = [];

  overallTableData: Overview = new Overview();
  divisionTableData: any = [];
  locationTableData: any = [];
  TableDataForOverAll: any = [];

  chartdivision: string = "";
  chartlocation: string = "";
  countrycode: string = "";

  highRiskColor: string = "#f04c3e";
  mediumRiskColor: string = "#FFC200";
  lowRiskColor: string = "#2c973e";

  fillColor = {
    high: am4core.color(this.highRiskColor),
    medium: am4core.color(this.mediumRiskColor),
    low: am4core.color(this.lowRiskColor),
  };

  fillColorforAudit = {
    complete: am4core.color(this.highRiskColor),
    pending: am4core.color(this.mediumRiskColor),
    overdue: am4core.color(this.lowRiskColor),
  };

  countryRiskMapped: any[];
  dashboardTableData: any = [];
  divisionIDs: any = [];
  locationIDS: any = [];
  //dashboardobj: { [key: string]: any } = {};

  tableColumns: tableColumn[] = [
    {
      title: "Audit Name",
      data: "name",
    },
    {
      title: "Status",
      data: "status",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" id="' +
          row.status +
          data +
          '" class="btn btn-sm btn-primary viewAudit"><i class="fa fa-eye"></i></button>'
        );
      },
    },
  ];

  tableData_ongoing: tableData[] = [
    {
      id: 1,
      name: "Procure To Pay",
      status: "In-Progress",
    },
    {
      id: 2,
      name: "Compliances",
      status: "In-Progress",
    },
    {
      id: 3,
      name: "IT Data privacy",
      status: "Pending Initiation",
    },
    {
      id: 1,
      name: "IT Cyber Security",
      status: "Pending Initiation",
    },
    {
      id: 1,
      name: "Logistics",
      status: "In-Progress",
    },
    {
      id: 1,
      name: "Plant Operations review",
      status: "Completed",
    },
    {
      id: 1,
      name: "Capex",
      status: "Completed",
    },
    {
      id: 1,
      name: "Safety, Health & Environment",
      status: "Completed",
    },
    {
      id: 1,
      name: "Sales and Marketing",
      status: "Completed",
    },
    {
      id: 1,
      name: "Salt packing Centre (SPC)",
      status: "Completed",
    },
  ];

  getChartData(stats) {
    return [
      {
        status: "Initiated",
        count: stats.initiated,
        color: am4core.color("#FFFF00"),
      },
      {
        status: "Completed",
        count: stats.completed,
        color: am4core.color("#00FF00"),
      },
      {
        status: "In Progress",
        count: stats.inprogress,
        color: am4core.color("#2e2e38"),
      },
      {
        status: "Overdue",
        count: stats.overdue,
        color: am4core.color("#FF0000"),
      },

      {
        status: "Unplanned",
        count: stats.unplanned,
        color: am4core.color("#c4c4cd"),
      },
    ];
  }

  scroll(tableTitle: string) {
    this.tableTitle = tableTitle;
    setTimeout(() => {
      this.dataDiv.nativeElement.scrollIntoView();
    }, 300);
  }

  showDetailStats(dataItem: any) {
    this.spinner.show();
    this.showDetails = true;
    this.clearTableData();
    this.countrycode = dataItem.id;
    this.Country = dataItem.name;
    this.populateDetails();
    this.spinner.hide();
  }

  populateDetails() {
    let overallChartDataCount = this.overallChartObj[this.countrycode];
    let divisionChartDataCount = this.divisionChartDataCount[this.countrycode];
    let locationChartDataCount = this.locationChartDataCount[this.countrycode];

    this.chartlocationPopulate = this.chartlocationList[this.countrycode];
    this.chartdivisionPopulate = this.BusinessUnitData.filter(
      (x) =>
        typeof this.divisionChartObj[this.countrycode][x.id] !== "undefined"
    );

    this.overallDataChange.next(this.getChartData(overallChartDataCount));
    this.divisionDataChange.next(this.getChartData(divisionChartDataCount));
    this.locationDataChange.next(this.getChartData(locationChartDataCount));

    let overviewObjCountryWise = this.overviewObjCountryWise[this.countrycode];
    this.overallTableData = overviewObjCountryWise.overall;
    for (let division in overviewObjCountryWise.division) {
      if (this.chartdivisionPopulate.length > 0) {
        let sector = this.chartdivisionPopulate.filter(
          (x) => x.id === division
        )[0].name;
        overviewObjCountryWise.division[division].sector = sector;
        this.divisionTableData.push(overviewObjCountryWise.division[division]);
      }
    }
    for (let location in overviewObjCountryWise.location) {
      let locationName = this.chartlocationPopulate.filter(
        (x) => x.value === location
      )[0].name;
      overviewObjCountryWise.location[location].location = locationName;
      this.locationTableData.push(overviewObjCountryWise.location[location]);
    }
  }

  showMap() {
    this.showDetails = false;
  }

  updateCustomMarkers(imageSeries) {
    // go through all of the images
    if (imageSeries && imageSeries.mapImages) {
      imageSeries.mapImages.each((image) => {
        // check if it has corresponding HTML element
        if (!image.dummyData || !image.dummyData.externalElement) {
          // create onex
          image.dummyData = {
            externalElement: this.createCustomMarker(image),
          };
        }

        // reposition the element accoridng to coordinates
        let xy = this.worldchart.geoPointToSVG({
          longitude: image.longitude,
          latitude: image.latitude,
        });
        image.dummyData.externalElement.style.top = xy.y + "px";
        image.dummyData.externalElement.style.left = xy.x + "px";
      });
    }
  }

  createCustomMarker(image) {
    let ichart = image.dataItem.component.chart;

    // create holder
    let holder = document.createElement("div");
    holder.className = "map-marker";
    holder.title = image.dataItem.dataContext.title;
    holder.style.position = "absolute";

    // maybe add a link to it?
    if (undefined != image.url) {
      holder.onclick = function () {
        window.location.href = image.url;
      };
      holder.className += " map-clickable";
    }

    // create dot
    let dot = document.createElement("div");
    dot.className = "dot";
    holder.appendChild(dot);

    // create pulse
    let pulse = document.createElement("div");
    pulse.className = "pulse";
    holder.appendChild(pulse);

    // append the marker to the map container
    ichart.svgContainer.htmlElement.appendChild(holder);

    return holder;
  }

  fillSectorOpts() {
    this.api.getData("api/sector").subscribe((result) => {
      this.BusinessUnitData = result;
    });
  }

  fillCompanyOpts() {
    this.api.getData("api/company").subscribe((result) => {
      this.CompanyData = result;
    });
  }

  fillCountryOpts() {
    this.api.getData("api/country").subscribe((result) => {
      this.CountryData = result;
    });
  }

  clearChartDetails() {
    this.divisionChartDataCount = {};
    this.locationChartDataCount = {};
    this.divisionChartObj = {};
    this.locationChartObj = {};
    this.overallChartObj = {};
    this.chartlocationPopulate = [];
    this.chartdivisionPopulate = [];
    this.chartlocationList = {};
    this.chartdivisionList = {};
    this.clearTableData();
    this.TableDataForOverAll = [];
    this.overviewObjCountryWise = {};
  }
  clearTableData() {
    this.overallTableData = new Overview();
    this.divisionTableData = [];
    this.locationTableData = [];
  }
  getDashboardData() {
    this.clearChartDetails();
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
      Sector: this.BusinessUnit,
      Country: this.Country,
      Company: this.Company,
      Rating: this.Criticality,
    };

    this.spinner.show();
    try {
      this.api
        .getData("api/scopeandschedule/GetDashboard", filterObj)
        .subscribe((response) => {
          this.populateDashboard(response);
          if (this.showDetails) {
            this.clearTableData();
            this.populateDetails();
          }
          this.spinner.hide();
        }),
        (error) => {
          console.error("error caught in component");
          this.spinner.hide();
        };
    } catch (error) {
      this.spinner.hide();
      console.error("API GetDashboard: ", error);
    }
  }
  initWorldMap(plottingData: any, countryRiskMap: any) {
    // Create map instance
    if (this.worldchart) {
      this.worldchart.dispose();
    }
    this.worldchart = am4core.create("vmap_world", am4maps.MapChart);
    this.worldchart.maxZoomLevel = 1;
    this.worldchart.seriesContainer.draggable = false;
    this.worldchart.seriesContainer.resizable = false;
    // Set map definition
    //this.worldchart.geodata = am4geodata_worldLow;
    this.worldchart.geodata = am4geodata_worldIndiaLow;

    // Set projection
    this.worldchart.projection = new am4maps.projections.Miller();

    // let legend = new am4charts.Legend();
    // legend.parent = this.worldchart.chartContainer;
    // legend.background.fill = am4core.color("#000");
    // legend.background.fillOpacity = 0.05;
    // legend.width = 120;
    // legend.align = "right";
    // legend.valign = "bottom";
    // legend.dy = -50;
    // legend.data = [
    //   {
    //     name: "No of Audit planned",
    //     fill: this.highRiskColor,
    //   },
    //   {
    //     name: "No of Audit Completed",
    //     fill: this.mediumRiskColor,
    //   },
    //   {
    //     name: "Overdue",
    //     fill: this.lowRiskColor,
    //   },
    // ];
    // legend.events.on("hit", (e) => {
    //   // console.log(e.target);
    // });

    // legend.itemContainers.template.clickable = false;
    // legend.itemContainers.template.focusable = false;

    // Create map polygon series
    let polygonSeries = this.worldchart.series.push(
      new am4maps.MapPolygonSeries()
    );

    // Exclude Antartica
    polygonSeries.exclude = ["AQ"];

    // Make map load polygon (like country names) data from GeoJSON
    polygonSeries.useGeodata = true;

    polygonSeries.data = countryRiskMap;

    // Configure series
    let polygonTemplate = polygonSeries.mapPolygons.template;
    polygonTemplate.tooltipText = "{name}  {title}";

    let newColor = new am4core.ColorSet();
    newColor.baseColor = new am4core.Color({ r: 46, g: 46, b: 56 });
    polygonTemplate.fill = newColor.getIndex(0).lighten(0.3);
    // polygonTemplate.fill = am4core.color("#74B266");
    // polygonTemplate.propertyFields.fill = "fill";
    // Create hover state and set alternative fill color
    //let hs = polygonTemplate.states.create("hover");
    //hs.properties.fill = this.worldchart.colors.getIndex(0);
    let hs = polygonTemplate.states.create("hover");
    // hs.properties.fill = am4core.color("#367B25");
    // Add image series
    let imageSeries = this.worldchart.series.push(new am4maps.MapImageSeries());
    imageSeries.mapImages.template.propertyFields.longitude = "longitude";
    imageSeries.mapImages.template.propertyFields.latitude = "latitude";
    imageSeries.data = plottingData;

    // add events to recalculate map position when the map is moved or zoomed
    this.worldchart.events.on("ready", () => {
      this.updateCustomMarkers(imageSeries);
    });
    this.worldchart.events.on("mappositionchanged", () => {
      this.updateCustomMarkers(imageSeries);
    });

    polygonTemplate.events.on("hit", (e) => {
      let dataItem = e.target.dataItem.dataContext;
      if (dataItem["name"] && this.checkCountryStats(dataItem["name"])) {
        this.showDetailStats(dataItem);
      }
    });
  }

  populateDashboard(audits) {
    let mapData: MapObj = this.getMapData(audits);
    this.auditOverview = mapData.overview;
    this.initWorldMap(mapData.plottingData, mapData.countryRiskMap);
  }

  analyseFollowupStatus(followup) {
    let status = followup.status ? followup.status.toLowerCase() : "";
    let overdue = false;
    let revisedDate = false;
    let revisedGreaterDate = false;
    let todayDate = false;
    let greaterDate = false;
    if (followup.implementationEndDate) {
      overdue = this.utils.compareDate(followup.implementationEndDate, "lt");
      todayDate = this.utils.compareDate(followup.implementationEndDate, "eq");
      greaterDate = this.utils.compareDate(
        followup.implementationEndDate,
        "gt"
      );
    }
    if (followup.revisedDate) {
      revisedDate = this.utils.compareDate(followup.revisedDate, "lt");
      revisedGreaterDate = this.utils.compareDate(followup.revisedDate, "gt");
    }

    let currentStatus = "";
    try {
      if (
        (status === "inprogress" || status === "pending") &&
        (overdue || revisedDate)
      )
        currentStatus = "delayed";
      else if ((status === "inprogress" || status === "pending") && todayDate)
        currentStatus = "dueToday";
      else if (
        status === "pending" &&
        followup.implementationEndDate &&
        greaterDate
      )
        currentStatus = "notdue";

    } catch (err) {
      currentStatus = "";
    }
    return currentStatus;
  }

  getFollowupStats(followupArray) {
    let followupStat: ActionTaken = {
      auditRequired: 0,
      dueToday: 0,
      delayed: 0,
      notdue: 0,
      completed: 0,
      revisedTimeline: 0,
    };

    for (let followup of followupArray) {
      var stat = followup.status ? followup.status.toLowerCase() : "";
      if (stat === "inprogress") {
        followupStat.auditRequired++;

      }
      if (stat === "completed") {
        followupStat.completed++;
      }
      if (followup.revisedDate) {
        followupStat.revisedTimeline++;
      }
      let status = this.analyseFollowupStatus(followup);
      if (typeof followupStat[status] !== "undefined") {
        followupStat[status] = followupStat[status] + 1;
      }
    }
    return followupStat;
  }

  getFollowupStatsForOverAll(followupArray) {
    let followupStat: any = {
      auditRequired: 0,
      completed: 0,
      completedWithDelayed: 0,
      delayed: 0,
      total: 0,
    };

    for (let followup of followupArray) {
      var stat = followup.status ? followup.status.toLowerCase() : "";
      if (stat === "inprogress") {
        followupStat.auditRequired++;

      }
      if (stat === "completed") {
        followupStat.completed++;
      }
      if (followup.revisedDate) {
        followupStat.revisedTimeline++;
      }
      let status = this.analyseFollowupStatus(followup);
      if (typeof followupStat[status] !== "undefined") {
        followupStat[status] = followupStat[status] + 1;
      }
    }
    followupStat.total = followupArray.length;
    return followupStat;
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
            ? "completedWithDelayed"
            : status === "inprogress"
              ? "auditPending"
              : status === "completed"
                ? "auditCompleted"
                : "";
    } catch (err) {
      currentStatus = "";
    }
    return currentStatus;
  }

  getAuditStats(audit) {
    let auditstatus = audit.status;
    let _auditCompleted = 0;
    let _auditInitiated = 0;
    let _auditPending = 0;
    let _auditinprogress = 0;
    let _completedWithDelayed = 0;
    let _overdueAudit = 0;
    let _unPlannedAudit = 0;
    let _overdue = this.utils.compareDate(audit.auditEndDate, "lt");

    if (auditstatus === "inprogress" && _overdue)
      _overdueAudit = _overdueAudit + 1;
    else if (auditstatus === null) _auditInitiated = _auditInitiated + 1;
    else if (auditstatus === "inprogress") _auditPending = _auditPending + 1;
    // else if (auditstatus === "completed" && _overdue)
    //   _completedWithDelayed = _completedWithDelayed + 1;
    else if (auditstatus === "completed") _auditCompleted = _auditCompleted + 1;
    else if (auditstatus === "unplanned") _unPlannedAudit = _unPlannedAudit + 1;
    else _auditPending = _auditPending + 1;

    let auditStat: AuditExecution = {
      auditInitiated: _auditInitiated,
      auditCompleted: _auditCompleted,
      auditPending: _auditPending,
      completedWithDelayed: _completedWithDelayed,
      overdue: _overdueAudit,
      unPlannedAudit: _unPlannedAudit,
    };

    // let status = this.analyseAuditStatus(audit);
    // if (typeof auditStat[status] !== "undefined") {
    //   auditStat[status] = auditStat[status] + 1;
    // }

    return auditStat;
  }

  getValueScorecard(auditClosure) {
    let scoreCard: ValueScorecard = {
      potentialSaving: 0,
      enhancement: 0,
      bestPractices: 0,
    };
    if (auditClosure) {
      scoreCard.potentialSaving = auditClosure.savingPotential.potentialsSavings
        ? parseInt(auditClosure.savingPotential.potentialsSavings)
        : 0;
      // scoreCard.potentialSaving += auditClosure.savingPotential.realisedSavings
      //   ? parseInt(auditClosure.savingPotential.realisedSavings)
      //   : 0;
      // scoreCard.potentialSaving += auditClosure.savingPotential.leakage
      //   ? parseInt(auditClosure.savingPotential.leakage)
      //   : 0;
      scoreCard.enhancement = auditClosure.processImprovement.systemImprovement
        ? parseInt(auditClosure.processImprovement.systemImprovement)
        : 0;
      scoreCard.bestPractices = auditClosure.processImprovement.leadingPractices
        ? parseInt(auditClosure.processImprovement.leadingPractices)
        : 0;
    }

    return scoreCard;
  }

  getAssurance(audit) {
    let report: Assurance = {
      redFlags: 0,
      statutoryDefault: 0,
      highRatedObservations: 0,
      repeatObservations: 0,
      controlsReport: 0,
    };

    let auditClosure = audit.auditClosure;
    let controlsReport = audit.racmAuditProcedureDetails.length
      ? audit.racmAuditProcedureDetails.filter(
        (x) => x.status && x.status.toLowerCase() === "effective"
      ).length
      : 0;
    let highRatedObservations = audit.discussionNotes.length
      ? audit.discussionNotes.filter(
        (x) =>
          x.observationGrading &&
          (x.observationGrading === 2 || x.observationGrading === 3)
      ).length
      : 0;

    if (auditClosure) {
      report.redFlags = auditClosure.processImprovement.redFlag
        ? parseInt(auditClosure.processImprovement.redFlag)
        : 0;
      report.statutoryDefault = auditClosure.processImprovement
        .statutoryNonCompliance
        ? parseInt(auditClosure.processImprovement.statutoryNonCompliance)
        : 0;
    }

    report.highRatedObservations = highRatedObservations;
    report.repeatObservations = 0;
    report.controlsReport = controlsReport;
    return report;
  }

  getDataPending(idrData) {
    let auditHealthStat: AuditHealth = {
      totalData: 0,
      pendingData: 0,
      dataChallenge: 0,
      onTimeCompilation: 0,
    };

    auditHealthStat.totalData = idrData.length;
    if (auditHealthStat.totalData) {
      auditHealthStat.pendingData = idrData.filter(
        (x) => x.status && x.status.toLowerCase() === "pending"
      ).length;
    }
    return auditHealthStat;
  }

  onChartDivisionChange(e) {
    this.divisionIDs = [];
    this.divisionTableData = [];
    this.divisionIDs = this.chartdivision;
    let dataCount = this.chartdivision
      ? this.divisionChartObj[this.countrycode][this.chartdivision]
      : this.divisionChartDataCount[this.countrycode];
    this.divisionDataChange.next(this.getChartData(dataCount));

    if (this.chartdivision) {
      let tableData =
        this.overviewObjCountryWise[this.countrycode].division[
        this.chartdivision
        ];
      if (tableData) {
        tableData.sector = this.chartdivisionPopulate.filter(
          (x) => x.id === this.chartdivision
        )[0].name;
        this.divisionTableData.push(tableData);
      }
    } else {
      for (let division in this.overviewObjCountryWise[this.countrycode]
        .division) {
        let sector = this.chartdivisionPopulate.filter(
          (x) => x.id === division
        )[0].name;
        this.overviewObjCountryWise[this.countrycode].division[
          division
        ].sector = sector;
        this.divisionTableData.push(
          this.overviewObjCountryWise[this.countrycode].division[division]
        );
      }
    }
  }

  onChartLocationChange(e) {
    this.locationTableData = [];
    this.locationIDS = [];
    this.locationIDS = this.chartlocation;
    let dataCount = this.chartlocation
      ? this.locationChartObj[this.countrycode][this.chartlocation]
      : this.locationChartDataCount[this.countrycode];
    this.locationDataChange.next(this.getChartData(dataCount));

    if (this.chartlocation) {
      let tableData: any =
        this.overviewObjCountryWise[this.countrycode].location[
        this.chartlocation
        ];
      if (tableData) {
        tableData.location = this.chartlocationPopulate.filter(
          (x) => x.value === this.chartlocation
        )[0].name;
        this.locationTableData.push(tableData);
      }
    } else {
      for (let location in this.overviewObjCountryWise[this.countrycode]
        .location) {
        let locationName = this.chartlocationPopulate.filter(
          (x) => x.value === location
        )[0].name;
        this.overviewObjCountryWise[this.countrycode].location[
          location
        ].location = locationName;
        this.locationTableData.push(
          this.overviewObjCountryWise[this.countrycode].location[location]
        );
      }
    }
  }

  computeChartData(audit, auditChartStat) {
    let countrycode = audit.location.countrycode;
    let sector = audit.location.sector;
    let location = {
      id: audit.location.id,
      name: audit.location.locationDescription,
    };
    this.setOverallChartData(auditChartStat, countrycode);
    this.setDivisionChartData(sector, auditChartStat, countrycode);
    this.setLocationChartData(location, auditChartStat, countrycode);
  }

  setOverallChartData(auditStat, countrycode) {
    this.overallChartObj[countrycode] = this.calculateAuditStat(
      this.overallChartObj[countrycode],
      auditStat
    );
  }

  setDivisionChartData(sector, auditStat: AuditPieChart, countrycode) {
    if (typeof this.divisionChartObj[countrycode] === "undefined") {
      this.divisionChartObj[countrycode] = {};
    }
    let currentStat = this.divisionChartObj[countrycode][sector];
    this.divisionChartObj[countrycode][sector] = this.calculateAuditStat(
      currentStat,
      auditStat
    );

    if (typeof this.divisionChartDataCount[countrycode] === "undefined") {
      this.divisionChartDataCount[countrycode] = {
        initiated: 0,
        completed: 0,
        inprogress: 0,
        overdue: 0,
        unplanned: 0,
      };
    }
    this.divisionChartDataCount[countrycode] = this.calculateCount(
      this.divisionChartDataCount[countrycode],
      auditStat
    );
  }

  setLocationChartData(locationObj, auditStat: AuditPieChart, countrycode) {
    let location = locationObj.id;
    if (typeof this.locationChartObj[countrycode] === "undefined") {
      this.locationChartObj[countrycode] = {};
    }
    let currentStat = this.locationChartObj[countrycode][location];
    this.locationChartObj[countrycode][location] = this.calculateAuditStat(
      currentStat,
      auditStat
    );

    if (typeof this.locationChartDataCount[countrycode] === "undefined") {
      this.locationChartDataCount[countrycode] = {
        initiated: 0,
        completed: 0,
        inprogress: 0,
        overdue: 0,
        unplanned: 0,
      };
    }
    this.locationChartDataCount[countrycode] = this.calculateCount(
      this.locationChartDataCount[countrycode],
      auditStat
    );

    if (typeof currentStat === "undefined") {
      if (typeof this.chartlocationList[countrycode] === "undefined") {
        this.chartlocationList[countrycode] = [];
      }
      this.chartlocationList[countrycode].push({
        name: locationObj.name,
        value: location,
      });
    }
  }

  calculateAuditStat(currentStat, auditStat) {
    if (typeof currentStat === "undefined") {
      currentStat = auditStat;
    } else {
      currentStat = this.calculateCount(currentStat, auditStat);
    }

    return currentStat;
  }

  calculateCount(currentCount, auditStat) {
    let completed = auditStat.completed
      ? currentCount.completed + 1
      : currentCount.completed;
    let inprogress = auditStat.inprogress
      ? currentCount.inprogress + 1
      : currentCount.inprogress;
    let overdue = auditStat.overdue
      ? currentCount.overdue + 1
      : currentCount.overdue;
    let initiated = auditStat.initiated
      ? currentCount.initiated + 1
      : currentCount.initiated;
    let unPlanned = auditStat.unplanned
      ? currentCount.unplanned + 1
      : currentCount.unplanned;

    return {
      completed: completed,
      inprogress: inprogress,
      overdue: overdue,
      initiated: initiated,
      unplanned: unPlanned,
    };
  }

  computeOverview(audit, overviewObj: Overview) {
    let followupStats = this.getFollowupStats(audit.followUp);
    overviewObj.actionTaken.auditRequired += followupStats.auditRequired;
    overviewObj.actionTaken.dueToday += followupStats.dueToday;
    overviewObj.actionTaken.delayed += followupStats.delayed;
    overviewObj.actionTaken.notdue += followupStats.notdue;
    overviewObj.actionTaken.completed += followupStats.completed;
    overviewObj.actionTaken.revisedTimeline += followupStats.revisedTimeline;

    // overviewObj.actionTaken.completedWithDelayed =
    //   followupStats.completedWithDelayed;

    let assuranceStats = this.getAssurance(audit);
    overviewObj.assurance.controlsReport += assuranceStats.controlsReport; //No of Effective controls
    overviewObj.assurance.highRatedObservations +=
      assuranceStats.highRatedObservations; //Observations Critical/High
    overviewObj.assurance.redFlags += assuranceStats.redFlags;
    overviewObj.assurance.repeatObservations +=
      assuranceStats.repeatObservations; //Observations Repeated
    overviewObj.assurance.statutoryDefault += assuranceStats.statutoryDefault;

    let auditStats = this.getAuditStats(audit);

    overviewObj.auditExecution.auditInitiated += auditStats.auditInitiated;
    overviewObj.auditExecution.auditCompleted += auditStats.auditCompleted;
    overviewObj.auditExecution.auditPending += auditStats.auditPending;
    overviewObj.auditExecution.completedWithDelayed +=
      auditStats.completedWithDelayed;
    overviewObj.auditExecution.overdue += auditStats.overdue;
    overviewObj.auditExecution.unPlannedAudit += auditStats.unPlannedAudit;

    let idrData = this.getDataPending(audit.initialDataRequest);
    overviewObj.auditHealth.totalData += idrData.totalData;
    overviewObj.auditHealth.pendingData += idrData.pendingData;
    overviewObj.auditHealth.onTimeCompilation += auditStats.auditCompleted;
    let scorecard = this.getValueScorecard(audit.auditClosure);
    overviewObj.valueScorecard.bestPractices += scorecard.bestPractices;
    overviewObj.valueScorecard.enhancement += scorecard.enhancement;
    if (scorecard.potentialSaving > 0) {
      overviewObj.valueScorecard.potentialSaving = overviewObj.valueScorecard.potentialSaving + this.utils.convertLakhsandCarors(scorecard.potentialSaving, "LAC");
    }
    return {
      overview: overviewObj,
      auditStats: auditStats,
    };
  }

  computeOverviewForAllTable(audit, overviewObj: TableOverview) {
    let followupStats = this.getFollowupStatsForOverAll(audit.followUp);
    overviewObj.actionTaken.auditRequired = followupStats.total;
    overviewObj.actionTaken.completed = followupStats.completed;
    // overviewObj.actionTaken.completedWithDelayed =
    //   followupStats.completedWithDelayed;
    overviewObj.actionTaken.delayed = followupStats.delayed;

    let assuranceStats = this.getAssurance(audit);
    overviewObj.assurance.controlsReport = assuranceStats.controlsReport; //No of Effective controls
    overviewObj.assurance.highRatedObservations =
      assuranceStats.highRatedObservations; //Observations Critical/High
    overviewObj.assurance.redFlags = assuranceStats.redFlags;
    overviewObj.assurance.repeatObservations =
      assuranceStats.repeatObservations; //Observations Repeated
    overviewObj.assurance.statutoryDefault = assuranceStats.statutoryDefault;

    let auditStats = this.getAuditStats(audit);
    overviewObj.auditExecution.auditInitiated += auditStats.auditInitiated;
    overviewObj.auditExecution.auditCompleted = auditStats.auditCompleted;
    overviewObj.auditExecution.auditPending = auditStats.auditPending;
    overviewObj.auditExecution.completedWithDelayed =
      auditStats.completedWithDelayed;
    overviewObj.auditExecution.overdue = auditStats.overdue;
    overviewObj.auditExecution.unPlannedAudit = auditStats.unPlannedAudit;

    let idrData = this.getDataPending(audit.initialDataRequest);
    overviewObj.auditHealth.totalData = idrData.totalData;
    overviewObj.auditHealth.pendingData = idrData.pendingData;
    overviewObj.auditHealth.onTimeCompilation = auditStats.auditCompleted;

    let scorecard = this.getValueScorecard(audit.auditClosure);
    overviewObj.valueScorecard.bestPractices = scorecard.bestPractices;
    overviewObj.valueScorecard.enhancement = scorecard.enhancement;
    overviewObj.valueScorecard.potentialSaving = scorecard.potentialSaving;

    return overviewObj;
  }

  computeOverviewOverall(audit, overview: Overview) {
    let overviewObj = this.computeOverview(audit, overview);

    let stats: AuditPieChart = {
      initiated: overviewObj.auditStats.auditInitiated,
      completed: overviewObj.auditStats.auditCompleted +
        overviewObj.auditStats.completedWithDelayed,
      inprogress: overviewObj.auditStats.auditPending,
      overdue: overviewObj.auditStats.overdue,
      unplanned: overviewObj.auditStats.unPlannedAudit,
    };

    this.computeChartData(audit, stats);
    this.computeOverviewCountryWise(audit);

    return overviewObj.overview;
  }

  computeOverviewCountryWise(audit) {
    let countrycode = audit.location.countrycode;
    let sector = audit.location.sector;
    let location = audit.location.id;
    if (typeof this.overviewObjCountryWise[countrycode] === "undefined") {
      this.overviewObjCountryWise[countrycode] = {
        overall: new Overview(),
        division: {},
        location: {},
      };
    }

    this.overviewObjCountryWise[countrycode].overall = this.computeOverview(
      audit,
      this.overviewObjCountryWise[countrycode].overall
    ).overview;

    if (
      typeof this.overviewObjCountryWise[countrycode].division[sector] ===
      "undefined"
    ) {
      this.overviewObjCountryWise[countrycode].division[sector] =
        new Overview();
    }
    this.overviewObjCountryWise[countrycode].division[sector] =
      this.computeOverview(
        audit,
        this.overviewObjCountryWise[countrycode].division[sector]
      ).overview;

    if (
      typeof this.overviewObjCountryWise[countrycode].location[location] ===
      "undefined"
    ) {
      this.overviewObjCountryWise[countrycode].location[location] =
        new Overview();
    }
    this.overviewObjCountryWise[countrycode].location[location] =
      this.computeOverview(
        audit,
        this.overviewObjCountryWise[countrycode].location[location]
      ).overview;
  }

  getMapData(audits) {
    let countryStats = {};
    let mapObj: MapObj = new MapObj();
    for (let audit of audits) {
      var dataOverAll = this.computeOverviewForAllTable(
        audit,
        new TableOverview()
      );
      var objOverAll = {
        Audit: audit,
        Data: dataOverAll,
      };
      this.TableDataForOverAll.push(objOverAll);
      mapObj.plottingData.push(this.getPlottingDataObj(audit));
      let country = audit.location.country ? audit.location.country.name : "";
      let countrycode = audit.location.countrycode
        ? audit.location.countrycode
        : "";
      let rating =
        audit.audit.overallAssesment === null
          ? ""
          : audit.audit.overallAssesment.processRiskMapping
            .finalProcessrating === null
            ? ""
            : audit.audit.overallAssesment.processRiskMapping.finalProcessrating.toLowerCase();

      mapObj.overview = this.computeOverviewOverall(audit, mapObj.overview);
      //Commented by : Baldev makwana [JIRA] (AUDIT-68) Dashboard Bugs
      // if (typeof countryStats[country] === "undefined") {
      //   countryStats[country] = { high: 0, medium: 0, low: 0, code: "" };
      // }

      // countryStats[country][rating] = countryStats[country][rating] + 1;
      // countryStats[country]["code"] = countrycode;

      if (typeof countryStats[country] === "undefined") {
        countryStats[country] = {
          complete: 0,
          initiated: 0,
          pending: 0,
          overdue: 0,
          unPlannedAudits: 0,
          code: "",
        };
      }
      let auditStats = this.getAuditStats(audit);
      countryStats[country]["initiated"] += auditStats.auditInitiated;
      countryStats[country]["complete"] += auditStats.auditCompleted;
      countryStats[country]["pending"] += auditStats.auditPending;
      countryStats[country]["overdue"] += auditStats.overdue;
      countryStats[country]["unPlannedAudits"] += auditStats.unPlannedAudit;
      countryStats[country]["code"] = countrycode;
    }

    if (mapObj.overview.auditHealth.totalData) {
      mapObj.overview.auditHealth.dataChallenge =
        (mapObj.overview.auditHealth.pendingData /
          mapObj.overview.auditHealth.totalData) *
        100;
    }
    mapObj.countryRiskMap = this.getCountryStatObj(countryStats);

    return mapObj;
  }

  getPlottingDataObj(audit) {
    let plottingDataObj = {};
    let country = audit.location.country ? audit.location.country.name : "";
    let state = audit.location.state ? audit.location.state.name : "";
    let city = audit.location.cityOrTown ? audit.location.cityOrTown.name : "";
    let latitude = audit.location.latitude ? audit.location.latitude : "";
    let longitude = audit.location.longitude ? audit.location.longitude : "";
    let locationName = `${country} ${state} ${city}`;

    plottingDataObj = {
      title: `${locationName}`,
      latitude: `${latitude}`,
      longitude: `${longitude}`,
      color: this.colorSet.next(),
    };

    return plottingDataObj;
  }

  checkCountryStats(countryName) {
    return this.countryRiskMapped.indexOf(countryName.toLowerCase()) > -1;
  }

  getCountryStatObj(countryStats) {
    let countryStatsObj = [];
    this.countryRiskMapped = [];
    for (let country in countryStats) {
      this.countryRiskMapped.push(country.toLowerCase());
      //Commented by : Baldev makwana [JIRA] (AUDIT-68) Dashboard Bugs

      // let high = countryStats[country].high;
      // let medium = countryStats[country].medium;
      // let low = countryStats[country].low;
      let rating = this.getRating(countryStats[country]);
      let initiated = countryStats[country]["initiated"];
      let complete = countryStats[country]["complete"];
      let pending = countryStats[country]["pending"];
      let overdue = countryStats[country]["overdue"];
      let unPlanned = countryStats[country]["unPlannedAudits"];
      let countrycode = countryStats[country]["code"];
      countryStatsObj.push({
        id: countrycode,
        name: `${country}`,
        title: `\nAudit To Be Initiated: ${initiated}\nAudit In Progress : ${pending}\nAudit Completed: ${complete}\nAudit Overdue: ${overdue}\nAudit Unplanned : ${unPlanned}`,
        fill: this.fillColorforAudit[rating],
      });
    }
    return countryStatsObj;
  }

  getRating(countryStats) {
    let rating = "overdue";
    if (
      countryStats.complete === countryStats.pending &&
      countryStats.pending === countryStats.overdue
    ) {
      rating = "overdue";
    } else if (
      countryStats.complete > countryStats.pending &&
      countryStats.complete > countryStats.overdue
    ) {
      rating = "complete";
    } else if (
      countryStats.pending >= countryStats.complete &&
      countryStats.pending > countryStats.overdue
    ) {
      rating = "pending";
    } else if (
      countryStats.overdue >= countryStats.pending &&
      countryStats.overdue > countryStats.complete
    ) {
      rating = "overdue";
    } else {
      rating = "overdue";
    }
    return rating;
  }
  //Commented by : Baldev makwana [JIRA] (AUDIT-68) Dashboard Bugs

  // getRating(countryStats) {
  //   let rating = "low";
  //   if (
  //     countryStats.complete === countryStats.pending &&
  //     countryStats.pending === countryStats.low
  //   ) {
  //     rating = "low";
  //   } else if (
  //     countryStats.high > countryStats.medium &&
  //     countryStats.high > countryStats.low
  //   ) {
  //     rating = "high";
  //   } else if (
  //     countryStats.medium >= countryStats.high &&
  //     countryStats.medium > countryStats.low
  //   ) {
  //     rating = "medium";
  //   } else if (
  //     countryStats.low >= countryStats.medium &&
  //     countryStats.low > countryStats.high
  //   ) {
  //     rating = "low";
  //   } else {
  //     rating = "low";
  //   }
  //   return rating;
  // }
  getCurrentFinancialYear(date1, date2) {
    var fiscalyear = "";
    var fDate = new Date(date1);
    var sDate = new Date(date2);
    if (fDate.getMonth() + 1 <= sDate.getMonth()) {
      fiscalyear =
        "FY " +
        (fDate.getFullYear() - 1).toString().substr(-2) +
        "-" +
        fDate.getFullYear().toString().substr(-2);
    } else {
      fiscalyear =
        "FY " +
        fDate.getFullYear().toString().substr(-2) +
        "-" +
        (fDate.getFullYear() + 1).toString().substr(-2);
    }
    return fiscalyear;
  }
  convertLakhs(rs) {
    if (rs >= 100000)
      return (rs / 100000).toFixed(2);
  }
  ngOnInit() {
    this.overallDataChange = new BehaviorSubject([]);
    this.divisionDataChange = new BehaviorSubject([]);
    this.locationDataChange = new BehaviorSubject([]);
    this.fillSectorOpts();
    this.fillCompanyOpts();
    this.fillCountryOpts();
    this.auditOverview = new Overview();
    $(document).ready(() => {
      $("body").on("click", ".viewAudit", () => {
        this.viewAudit();
      });
      $("body").on("click", ".applyBtn", () => {
        this.getDashboardData();
      });
      $("#quickview .scroller").css({
        "max-height": "630",
        "overflow-y": "auto",
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
      this.getDashboardData();
    });
  }

  ngOnDestroy() {
    if (this.worldchart) {
      this.worldchart.dispose();
    }
  }

  viewAudit() {
    this.router.navigate(["./pages/manageaudits/edit"]);
  }
  commonParam() {
    this.clearChartDetails();
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
      DashboardTableParam: {
        StartDate: startDate,
        EndDate: endDate,
        Sector: this.BusinessUnit,
        Country: this.Country,
        Company: this.Company,
        Rating: this.Criticality,
      },
    };
    this.dashboardTableData.push(filterObj);
    return filterObj;
  }
  exportToPDF() {
    this.spinner.show();
    try {
      this.api
        .postDownloadFile(
          "api/scopeandschedule/OverAllAuditsInformationPDF/",
          this.commonParam()
        )
        .subscribe((blob) => {
          // const objblob: any = new Blob([blob], {
          //   type: "application/pdf",
          // });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "OverAllAuditsInformation.pdf");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
          this.clearLocationandDivison();
          this.getDashboardData();
        });
    } catch (error) {
      this.spinner.hide();
    }
  }
  exportToPPT() {
    this.spinner.show();
    this.commonParam();
    try {
      this.api
        .downloadFile(
          "api/scopeandschedule/OverAllAuditsInformationPPT/",
          this.dashboardTableData
        )
        .subscribe((blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/ppt",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "OverAllAuditsInformation.pptx");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
          this.clearLocationandDivison();
          this.getDashboardData();
        });
    } catch (error) {
      this.spinner.hide();
    }
  }
  exportParam() {
    this.clearChartDetails();
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
      Sector: this.BusinessUnit,
      Country: this.Country,
      Company: this.Company,
      Rating: this.Criticality,
      Division: this.divisionIDs,
      LocationId: this.locationIDS,
    };
    return filterObj;
  }
  clearLocationandDivison() {
    this.chartlocation = "";
    this.chartdivision = "";
  }
  async exportToDivisionPDF(event) {
    event.preventDefault();
    this.spinner.show();
    try {
      const t = await this.api
        .postDownloadFiledata(
          "api/scopeandschedule/DivisionWiseAuditInformationPDF/",
          this.exportParam()
        )
        .then((blob) => {
          // const objblob: any = new Blob([blob], {
          //   type: "application/pdf",
          // });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "DivisionWiseAuditInformation.pdf");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
          this.clearLocationandDivison();
          this.getDashboardData();
        });
    } catch (error) {
      this.spinner.hide();
    }
  }
  exportToDivisionPPT() {
    this.spinner.show();
    try {
      this.api
        .postDownloadFiledata(
          "api/scopeandschedule/DivisionWiseAuditInformationPPT/",
          this.exportParam()
        )
        .then((blob) => {
          // const objblob: any = new Blob([blob], {
          //   type: "application/ppt",
          // });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "DivisionWiseAuditInformation.pptx");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
          this.clearLocationandDivison();
          this.getDashboardData();
        });
    } catch (error) {
      this.spinner.hide();
    }
  }

  exportToLocationPDF() {
    this.spinner.show();
    try {
      this.api
        .postDownloadFiledata(
          "api/scopeandschedule/LocationWiseAuditInformationPDF/",
          this.exportParam()
        )
        .then((blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/pdf",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "LocationWiseAuditInformation.pdf");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
          this.clearLocationandDivison();
          this.getDashboardData();
        });
    } catch (error) {
      this.spinner.hide();
    }
  }
  exportToLocationPPT() {
    this.spinner.show();
    try {
      this.api
        .postDownloadFiledata(
          "api/scopeandschedule/LocationWiseAuditInformationPPT/",
          this.exportParam()
        )
        .then((blob) => {
          // const objblob: any = new Blob([blob], {
          //   type: "application/ppt",
          // });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "LocationWiseAuditInformation.pptx");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
          this.clearLocationandDivison();
          this.getDashboardData();
        });
    } catch (error) {
      this.spinner.hide();
    }
  }
}
