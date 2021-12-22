import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BusinessProcessService } from "./businessprocess.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-businessprocess",
  templateUrl: "./businessprocess.component.html",
  styleUrls: ["./businessprocess.component.css"],
  providers: [BusinessProcessService],
})
export class BusinessProcessComponent implements OnInit {
  constructor(
    private businessprocess: BusinessProcessService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("businessprocessForm", { static: false })
  businessprocessForm: NgForm;
  submitted: boolean = false;
  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "businessprocess_table";

  tableGetApi: string = "posts";

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
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editBusinessProcess"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteBusinessProcess"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  businessprocessId: string = "";
  businessprocessName: string = "";
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

  saveBusinessProcess(e) {
    this.submitted = true;
    if (this.businessprocessForm.invalid) {
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateBusinessProcess();
      } else {
        this.addNewBusinessProcess();
      }
    }
  }

  addNewBusinessProcess() {
    let postData = this.businessprocessForm.form.value;
    this.businessprocess
      .addBusinessProcess("api/businesscycle", postData)
      .subscribe(
        (response) => {
          this.notifyService.success("BusinessProcess Added Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 208)
            this.notifyService.error("Business Cycle already exists.");
          else this.notifyService.error(error.error);
        }
      );
  }

  updateBusinessProcess() {
    let postData = this.businessprocessForm.form.value;
    postData.id = this.businessprocessId;
    this.businessprocess
      .updateBusinessProcess("api/businesscycle", postData)
      .subscribe(
        (response) => {
          this.notifyService.success("BusinessProcess Updated Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }

  addBusinessProcess() {
    this.handleFormView.show();
  }

  editBusinessProcess(businessprocessData) {
    this.isEdit = true;
    this.businessprocessId = businessprocessData.id;
    this.businessprocessName = businessprocessData.name;
    this.handleFormView.show();
  }

  deleteBusinessProcess(businessprocessId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.businessprocess
        .deleteBusinessProcess("api/businesscycle/" + businessprocessId)
        .subscribe(
          (response) => {
            this.notifyService.success("BusinessProcess Deleted Successfully");
            this.handleFormView.hide();
          },
          (error) => {
            if (error.status == 406) {
              this.notifyService.error(
                "Looks like the selected record reference has been given in following places: ERMRisks, EYBenchmark, EYBenchmarkAuditwise, FollowUp, KeyBusinessInitiative, OverallAssesment, ProcessL1, ProcessL2. Hence, you cannot delete the selected record"
              );
            } else {
              this.notifyService.error(error.error);
            }
          }
        );
    }
  }

  clearform() {
    this.businessprocessId = "";
    this.businessprocessName = "";
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "processmaster");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#bcComponent").on("click", ".editBusinessProcess", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let businessprocessData = $("#" + dataId).data();
        this.editBusinessProcess(businessprocessData);
      });

      $("#bcComponent").on("click", ".deleteBusinessProcess", (e) => {
        let businessprocessId = $(e.currentTarget).attr("id");
        this.deleteBusinessProcess(businessprocessId);
      });
    });
  }
}
