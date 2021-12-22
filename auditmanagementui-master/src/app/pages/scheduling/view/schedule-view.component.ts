import { Component, OnInit } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { Router } from "@angular/router";
import * as $ from "jquery";

@Component({
  selector: "selector",
  templateUrl: "schedule-view.component.html",
})
export class ScheduleViewComponent implements OnInit {
  constructor(private router: Router) {}

  tableColumnsScheduling: tableColumn[] = [
    {
      title: "Process",
      data: "process",
    },
    {
      title: "Quarter",
      data: "quarter",
    },
    {
      title: "Start Date",
      data: "startdate",
    },
    {
      title: "End Date",
      data: "enddate",
    },
    {
      title: "Total Man Days",
      data: "mandays",
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
          '" class="btn btn-sm btn-info editSchedule"><i class="fa fa-edit"></i></button>'
        );
      },
    },
  ];

  tableData_scheduling: tableData[] = [
    {
      id: 1,
      process: "Procure To Pay",
      quarter: "Q1",
      startdate: "01/04/2020",
      enddate: "31/06/2020",
      mandays: 90,
    },
  ];

  tableFilters = new BehaviorSubject({});
  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".editSchedule", (e) => {
        e.preventDefault();
        let schId = $(e.currentTarget).attr("id");
        this.editSchedule();
      });
    });
  }

  editSchedule() {
    this.router.navigate(["./pages/scheduling/edit"]);
  }

  addSchedule() {
    this.router.navigate(["./pages/scheduling/add"]);
  }
}
