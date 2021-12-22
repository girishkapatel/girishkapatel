import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
@Component({
  selector: "selector",
  templateUrl: "auditprocedure-add.component.html",
})
export class AuditProcedureAddComponent implements OnInit {
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
      title: "Control Owner",
      data: "owner",
    },
    {
      title: "Action",
      data: "id",
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-primary viewAuditStep"><i class="fa fa-eye"></i></button>'
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

  backToAuditProcedureView() {
    this.router.navigate(["./pages/auditprocedure"]);
  }
}
