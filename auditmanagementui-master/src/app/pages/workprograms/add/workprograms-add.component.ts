import { Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";

@Component({
  selector: "selector",
  templateUrl: "workprograms-add.component.html",
})
export class WorkprogramsAddComponent implements OnInit {
  constructor(private router: Router) {}

  formVisible: boolean = false;

  tableColumns: tableColumn[] = [
    {
      title: "Control ID",
      data: "cid",
    },
    {
      title: "Control Title",
      data: "ctitle",
    },
    {
      title: "Control Description",
      data: "desc",
    },
    {
      title: "Control Frequency",
      data: "freq",
    },
    {
      title: "Control Type",
      data: "type",
    },
    {
      title: "Control Owner",
      data: "owner",
    },
    {
      title: "Control Nature",
      data: "nature",
    },
    {
      title: "Action",
      data: "id",
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-danger removeControl"><i class="fa fa-trash"></i></button>'
        );
      },
    },
  ];

  tableData: tableData[] = [];
  ngOnInit() {}

  addControl() {
    this.formVisible = true;
  }

  cancelControlAddEdit() {
    this.formVisible = false;
  }

  backToWorkprogramsView() {
    this.router.navigate(["./pages/workprograms"]);
  }
}
