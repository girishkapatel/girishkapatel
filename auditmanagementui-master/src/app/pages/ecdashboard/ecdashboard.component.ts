import {
  Component,
  NgZone,
  OnInit,
  AfterViewInit,
  OnDestroy,
  ViewChild,
  ElementRef,
} from "@angular/core";
import { tableColumn, tableData } from "./../../common/table/table.model";
import * as am4core from "@amcharts/amcharts4/core";
import * as $ from "jquery";
import { Router } from "@angular/router";

@Component({
  selector: "app-ecdashboard",
  templateUrl: "./ecdashboard.component.html",
  styleUrls: ["./ecdashboard.component.css"],
})
export class ECDashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  constructor(private router: Router) {}
  @ViewChild("dataDiv", { static: false }) dataDiv: ElementRef;

  tableColumnsAction: tableColumn[] = [
    {
      title: "Audit Name",
      data: "",
    },
    {
      title: "Year",
      data: "",
    },
    {
      title: "ATR %",
      data: "",
    },
    {
      title: "Completed",
      data: "ec",
    },
    {
      title: "Delayed",
      data: "",
    },
    {
      title: "Action Due (in days)",
      data: "",
    },
    {
      title: "Cancelled",
      data: "",
    },
    {
      title: "Total",
      data: "",
    },
    {
      title: "Rescheduled",
      data: "",
    },
    {
      title: "Pending For Approval",
      data: "",
    },
    {
      title: "Rejected by auditor",
      data: "",
    },
    {
      title: "Action",
      data: "id",
      render: (data, type, row, meta) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" id="' +
          data +
          '" class="btn btn-sm btn-primary editSchedule"><i class="fa fa-eye"></i></button>'
        );
      },
    },
  ];

  tableData_action: tableData[] = [];

  tableColumnsChase: tableColumn[] = [
    {
      title: "Owner / Approver",
      data: "",
    },
    {
      title: "Delayed",
      data: "",
    },
    {
      title: "Due-30 Days",
      data: "",
    },
    {
      title: "Due > 30 Days",
      data: "",
    },
    {
      title: "Total",
      data: "",
    },
    {
      title: "Pending Approvals",
      data: "",
    },
    {
      title: "Action",
      data: "id",
      render: (data, type, row, meta) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" id="' +
          data +
          '" class="btn btn-sm btn-primary"><i class="fa fa-envelope"></i></button>'
        );
      },
    },
  ];

  tableData_chase: tableData[] = [];

  auditChartData: any[] = [
    {
      status: "Completed",
      count: 1,
      color: am4core.color("#ffe600"),
    },
    {
      status: "In Progress",
      count: 1,
      color: am4core.color("#2e2e38"),
    },
    {
      status: "Pending Initiation",
      count: 1,
      color: am4core.color("#c4c4cd"),
    },
  ];

  actionsChartData: any[] = [
    {
      status: "In-Progress",
      count: 1,
      color: am4core.color("#ffe600"),
    },
    {
      status: "In Review",
      count: 1,
      color: am4core.color("#2e2e38"),
    },
    {
      status: "Closed",
      count: 1,
      color: am4core.color("#c4c4cd"),
    },
    {
      status: "Closed",
      count: 1,
      color: am4core.color("#95cb89"),
    },
  ];

  obsChartData: any[] = [
    {
      status: "Low",
      count: 1,
      color: am4core.color("#95cb89"),
    },
    {
      status: "Medium",
      count: 1,
      color: am4core.color("#ffe600"),
    },
    {
      status: "High",
      count: 1,
      color: am4core.color("#f04c3e"),
    },
  ];

  ngAfterViewInit() {}

  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".viewAudit", () => {
        this.viewAudit();
      });
    });
  }
  ngOnDestroy() {}

  viewAudit() {
    this.router.navigate(["./pages/manageaudits/edit"]);
  }
}
