import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { DesignationService } from "./designation.service";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-designation",
  templateUrl: "./designation.component.html",
  styleUrls: ["./designation.component.css"],
  providers: [DesignationService],
})
export class DesignationComponent implements OnInit {
  constructor(
    private designationApi: DesignationService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("DesignationForm", { static: false })
  DesignationForm: NgForm;
  submitted: boolean = false;
  accessRights: any = {};
  isStackHolder: boolean = false;
  designationId: string = "";
  designationName: string = "";

  isEdit: boolean = false;

  tableColumns: tableColumn[] = [
    {
      title: "Name",
      data: "name",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editdesignation"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deletedesignation"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  tableFilters = new BehaviorSubject({});

  handleFormView = {
    refresh: () => {
      this.tableFilters.next({ init: true });

      this.clearValues();
    },
  };

  showDesignationModal() {
    window["jQuery"]("#manageDesignationModal").modal("show");
  }

  hideDesignationModal() {
    window["jQuery"]("#manageDesignationModal").modal("hide");
  }

  addDesignation() {
    this.submitted = false;
    this.clearValues();
    this.showDesignationModal();
  }

  saveDesignation() {
    if (this.DesignationForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = { Id: this.designationId, Name: this.designationName };

      if (this.designationId) {
        this.designationApi
          .updateDesignation("api/designation", postData)
          .subscribe(
            (response) => {
              this.notifyService.success("Designation Updated Successfully");
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      } else {
        this.designationApi
          .addDesignation("api/designation", postData)
          .subscribe(
            (response) => {
              this.notifyService.success("Designation Inserted Successfully");
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      }
      this.hideDesignationModal();
    }
  }

  editdesignation(data) {
    this.isEdit = true;
    this.designationId = data.id;
    this.designationName = data.name;

    this.showDesignationModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.designationId = "";
    this.designationName = "";
  }

  deletedesignation(id) {
    if (confirm("Are you sure to Delete ?")) {
      this.designationApi.deleteDesignation("api/designation/" + id).subscribe(
        (response) => {
          this.notifyService.success("Designation Deleted Successfully");
          this.handleFormView.refresh();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: User. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "designation");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#designationComponent").on("click", ".editdesignation", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let designationData = $("#" + dataId).data();

        this.editdesignation(designationData);
      });

      $("#designationComponent").on("click", ".deletedesignation", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deletedesignation(dataId);
      });
    });
  }
}
