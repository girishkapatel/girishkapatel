import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
// import { CoverageService } from './coverage.service';
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";

@Component({
  selector: "app-coverage",
  templateUrl: "./coverage.component.html",
  styleUrls: ["./coverage.component.css"],
  providers: [],
})
export class CoverageComponent implements OnInit {
  constructor() {}

  @ViewChild("coverageForm", { static: false }) coverageForm: NgForm;

  tableId: string = "coverage_table";

  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Coverage Name",
      data: "name",
    },
    {
      title: "Division",
      data: "division",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-info editCoverage"><i class="fa fa-edit"></i></button>'
        );
      },
    },
  ];

  tableData: tableData[] = [
    {
      id: 1,
      name: "Tata Chemical Limited",
      division: "Chemical",
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

  saveCoverage(e) {
    e.preventDefault();
    if (this.isEdit) {
      this.updateCoverage();
    } else {
      this.addNewCoverage();
    }
  }

  addNewCoverage() {
    let postData = this.coverageForm.form.value;

    // this.coverage.addCoverage('posts', postData).subscribe(response=>{
    //   this.handleFormView.hide();
    // }, error =>{
    //   console.log(error);
    // });
  }

  updateCoverage() {
    let postData = {
      id: 1,
      title: "foo",
      body: "bar",
      userId: 1,
    };

    // this.coverage.updateCoverage('posts/1', postData).subscribe(response=>{
    //   this.handleFormView.hide();
    // }, error =>{
    //   console.log(error);
    // });
  }

  addCoverage() {
    this.handleFormView.show();
  }

  editCoverage() {
    this.isEdit = true;
    this.handleFormView.show();
  }

  deleteCoverage() {
    // this.coverage.deleteCoverage('post/1').subscribe(response=>{
    //   this.handleFormView.hide();
    // }, error =>{
    //   console.log(error);
    // });
  }

  clearform() {}

  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".editCoverage", (e) => {
        //let dataId = $(e.currentTarget).attr('data-id');
        this.editCoverage();
      });
    });
  }
}
