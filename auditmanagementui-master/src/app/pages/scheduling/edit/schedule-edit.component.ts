import { Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";

@Component({
  selector: "selector",
  templateUrl: "schedule-edit.component.html",
})
export class ScheduleEditComponent implements OnInit {
  constructor(private router: Router) {}

  formVisible: boolean = false;

  tableColumns: tableColumn[] = [
    {
      title: "Name",
      data: "name",
    },
    {
      title: "Qualification",
      data: "qualification",
    },
    {
      title: "Designation",
      data: "desgn",
    },
    {
      title: "Total Experience",
      data: "exp",
    },
    {
      title: "Man Days Required",
      data: "mandays",
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
      title: "Action",
      data: "id",
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-danger removeResource"><i class="fa fa-trash"></i></button>'
        );
      },
    },
  ];

  tableData: tableData[] = [
    {
      id: 1,
      name: "Ashwini Marathe",
      qualification: "C.A",
      desgn: "Senior Consultant",
      exp: "5",
      mandays: "45",
      startdate: "1/04/2019",
      enddate: "5/18/2019",
    },
    {
      id: 2,
      name: "Rajashree Basu",
      qualification: "M.Sc",
      desgn: "Senior Consultant",
      exp: "4",
      mandays: "45",
      startdate: "1/04/2019",
      enddate: "5/18/2019",
    },
    {
      id: 3,
      name: "Shonit Jolly",
      qualification: "MBA",
      desgn: "Senior Consultant",
      exp: "3",
      mandays: "45",
      startdate: "1/04/2019",
      enddate: "5/18/2019",
    },
  ];
  ngOnInit() {}

  addResource() {
    this.formVisible = true;
  }

  cancelResourceAddEdit() {
    this.formVisible = false;
  }

  backToScheduleView() {
    this.router.navigate(["./pages/scheduling"]);
  }
}
