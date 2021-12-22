import { Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";

@Component({
  selector: "selector",
  templateUrl: "schedule-add.component.html",
})
export class ScheduleAddComponent implements OnInit {
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
      qualification: "B.COM",
      desgn: "Accountant",
      exp: "Exp",
      mandays: "30",
      startdate: "1/04/2020",
      enddate: "30/04/2020",
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
