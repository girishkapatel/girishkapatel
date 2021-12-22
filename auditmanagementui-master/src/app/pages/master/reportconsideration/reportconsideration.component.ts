import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { ReportConsiderationService } from "./reportconsideration.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-reportconsideration",
  templateUrl: "./reportconsideration.component.html",
  styleUrls: ["./reportconsideration.component.css"],
  providers: [ReportConsiderationService],
})
export class ReportConsiderationComponent implements OnInit {
  constructor(
    private reportConsiderationApi: ReportConsiderationService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("reportConsiderationsForm", { static: false })
  reportConsiderationsForm: NgForm;
  submitted: boolean = false;
  accessRights: any = {};
  isStackHolder: boolean = false;
  rcId: string = "";
  rcName: string = "";

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
            '" class="btn btn-sm btn-info editRC"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteRC"><i class="fa fa-trash"></i></button>';

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

  showRCModal() {
    window["jQuery"]("#manageRCModal").modal("show");
  }

  hideRCModal() {
    window["jQuery"]("#manageRCModal").modal("hide");
  }

  addRC() {
    this.submitted = false;
    this.clearValues();
    this.showRCModal();
  }

  saveRC() {
    if (this.reportConsiderationsForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = { Id: this.rcId, Name: this.rcName };

      if (this.rcId) {
        this.reportConsiderationApi
          .updateReportConsideration("api/reportconsideration", postData)
          .subscribe(
            (response) => {
              this.notifyService.success(
                "Report Consideration Updated Successfully"
              );
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      } else {
        this.reportConsiderationApi
          .addReportConsideration("api/reportconsideration", postData)
          .subscribe(
            (response) => {
              this.notifyService.success(
                "Report Consideration Inserted Successfully"
              );
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      }
      this.hideRCModal();
    }
  }

  editRC(data) {
    this.isEdit = true;
    this.rcId = data.id;
    this.rcName = data.name;
    this.showRCModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;
    this.rcId = "";
    this.rcName = "";
  }

  deleteRC(id) {
    this.reportConsiderationApi
      .deleteReportConsideration("api/reportconsideration/" + id)
      .subscribe(
        (response) => {
          this.notifyService.success(
            "Report Consideration Deleted Successfully"
          );
          this.handleFormView.refresh();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: AuditClosure, DraftReport. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "master",
      "reportconsideration"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#reportconsiderationComponent").on("click", ".editRC", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let riskTypeData = $("#" + dataId).data();

        this.editRC(riskTypeData);
      });

      $("#reportconsiderationComponent").on("click", ".deleteRC", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deleteRC(dataId);
      });
    });
  }
}
