import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { DatarequestService } from "./datarequest.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";

@Component({
  selector: "app-datarequest",
  templateUrl: "./datarequest.component.html",
  styleUrls: ["./datarequest.component.css"],
  providers: [DatarequestService],
})
export class DatarequestComponent implements OnInit {
  constructor(private datarequest: DatarequestService) {}

  @ViewChild("datarequestForm", { static: false }) datarequestForm: NgForm;

  tableId: string = "datarequest_table";

  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Process",
      data: "process",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-infp editDatarequest"><i class="fa fa-edit"></i></button>'
        );
      },
    },
  ];

  tableData: tableData[] = [
    {
      id: 1,
      process: "Procure to pay",
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.tableFilters.next({});
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveDatarequest(e) {
    e.preventDefault();
    if (this.isEdit) {
      this.updateDatarequest();
    } else {
      this.addNewDatarequest();
    }
  }

  addNewDatarequest() {
    let postData = this.datarequestForm.form.value;

    this.datarequest.addDatarequest("posts", postData).subscribe(
      (response) => {
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  updateDatarequest() {
    let postData = {
      id: 1,
      title: "foo",
      body: "bar",
      userId: 1,
    };

    this.datarequest.updateDatarequest("posts/1", postData).subscribe(
      (response) => {
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  addDatarequest() {
    this.handleFormView.show();
  }

  editDatarequest() {
    this.isEdit = true;
    this.handleFormView.show();
  }

  deleteDatarequest() {
    this.datarequest.deleteDatarequest("post/1").subscribe(
      (response) => {
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  clearform() {}

  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".editDatarequest", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        this.editDatarequest();
      });
    });
  }
}
