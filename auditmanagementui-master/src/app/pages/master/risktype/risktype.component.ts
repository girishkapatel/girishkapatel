import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { RiskTypeService } from "./risktype.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-risktype",
  templateUrl: "./risktype.component.html",
  styleUrls: ["./risktype.component.css"],
  providers: [RiskTypeService],
})
export class RiskTypeComponent implements OnInit {
  constructor(
    private riskTypeApi: RiskTypeService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  accessRights: any = {};
  isStackHolder: boolean = false;
  riskTypeId: string = "";
  riskTypeName: string = "";

  @ViewChild("riskTypeForm", { static: false }) riskTypeForm: NgForm;

  isEdit: boolean = false;
  submitted: boolean = false;
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
            '" class="btn btn-sm btn-info editRiskType"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteRiskType"><i class="fa fa-trash"></i></button>';

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

  showRiskTypeModal() {
    window["jQuery"]("#manageRiskTypeModal").modal("show");
  }

  hideRiskTypeModal() {
    window["jQuery"]("#manageRiskTypeModal").modal("hide");
  }

  addRiskType() {
    this.submitted = false;
    this.clearValues();
    this.showRiskTypeModal();
  }

  saveRiskType() {
    this.submitted = true;
    if (this.riskTypeForm.invalid) {
      return false;
    }
    let postData = { Id: this.riskTypeId, Name: this.riskTypeName };

    if (this.riskTypeId) {
      this.riskTypeApi.updateRiskType("api/risktype", postData).subscribe(
        (response) => {
          this.notifyService.success("Risk Type Updated Successfully");
          this.handleFormView.refresh();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
    } else {
      this.riskTypeApi.addRiskType("api/risktype", postData).subscribe(
        (response) => {
          this.notifyService.success("Risk Type Inserted Successfully");
          this.handleFormView.refresh();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
    }
    this.hideRiskTypeModal();
  }

  editRiskType(data) {
    this.isEdit = true;
    this.riskTypeId = data.id;
    this.riskTypeName = data.name;

    this.showRiskTypeModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.riskTypeId = "";
    this.riskTypeName = "";
  }

  deleteRiskType(id) {
    this.riskTypeApi.deleteRiskType("api/risktype/" + id).subscribe(
      (response) => {
        this.notifyService.success("Risk Type Deleted Successfully");
        this.handleFormView.refresh();
      },
      (error) => {
        if (error.status == 406) {
          this.notifyService.error(
            "Looks like the selected record reference has been given in following places: ActionPlanning, DiscussionNote, FollowUp. Hence, you cannot delete the selected record"
          );
        } else {
          this.notifyService.error(error.error);
        }
      }
    );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "risktype");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#risktypeComponent").on("click", ".editRiskType", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let riskTypeData = $("#" + dataId).data();

        this.editRiskType(riskTypeData);
      });

      $("#risktypeComponent").on("click", ".deleteRiskType", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deleteRiskType(dataId);
      });
    });
  }
}
