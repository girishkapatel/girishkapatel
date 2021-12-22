import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { ImpactService } from "./impact.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-impact",
  templateUrl: "./impact.component.html",
  styleUrls: ["./impact.component.css"],
  providers: [ImpactService],
})
export class ImpactComponent implements OnInit {
  constructor(
    private impactApi: ImpactService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  accessRights: any = {};
  submitted: boolean = false;
  isStackHolder: boolean = false;
  impactId: string = "";
  impactName: string = "";

  isEdit: boolean = false;

  // tableId: string = "impact_table";
  // tableApiUrl: string = "api/impactmaster";

  @ViewChild("impactForm", { static: false })
  impactForm: NgForm;

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
            '" class="btn btn-sm btn-info editImpact"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteImpact"><i class="fa fa-trash"></i></button>';

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

  showImpactModal() {
    window["jQuery"]("#manageImpactModal").modal("show");
  }

  hideImpactModal() {
    window["jQuery"]("#manageImpactModal").modal("hide");
  }

  addImpact() {
    this.submitted = false;
    this.clearValues();
    this.showImpactModal();
  }

  saveImpact() {
    if (this.impactForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = { Id: this.impactId, Name: this.impactName };

      if (this.impactId) {
        this.impactApi.updateImpact("api/impactmaster", postData).subscribe(
          (response) => {
            this.notifyService.success("Impact Updated Successfully");
            this.handleFormView.refresh();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
      } else {
        this.impactApi.addImpact("api/impactmaster", postData).subscribe(
          (response) => {
            this.notifyService.success("Impact Inserted Successfully");
            this.handleFormView.refresh();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
      }
      this.handleFormView.refresh();
      this.hideImpactModal();
    }
  }

  editImpact(data) {
    this.isEdit = true;
    this.impactId = data.id;
    this.impactName = data.name;

    this.showImpactModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.impactId = "";
    this.impactName = "";
  }

  deleteImpact(id) {
    this.impactApi.deleteImpact("api/impactmaster/" + id).subscribe(
      (response) => {
        this.notifyService.success("Impact Deleted Successfully");
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
    this.accessRights = this.utils.getAccessOnLevel1("master", "impact");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#impactComponent").on("click", ".editImpact", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let impactData = $("#" + dataId).data();

        this.editImpact(impactData);
      });

      $("#impactComponent").on("click", ".deleteImpact", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deleteImpact(dataId);
      });
    });
  }
}
