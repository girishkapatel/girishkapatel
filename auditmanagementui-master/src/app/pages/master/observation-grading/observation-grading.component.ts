import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { ObservationGradingService } from "./observation-grading.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-observation-grading",
  templateUrl: "./observation-grading.component.html",
  styleUrls: ["./observation-grading.component.css"],
  providers: [ObservationGradingService],
})
export class ObservationGradingComponent implements OnInit {
  constructor(
    private obsApi: ObservationGradingService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("obGradingForm", { static: false })
  obGradingForm: NgForm;
  submitted: boolean = false;
  isStackHolder: boolean = false;
  accessRights: any = {};

  obsGradingId: string = "";
  obsGradingName: string = "";

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
            '" class="btn btn-sm btn-info editObservationGrading"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteObservationGrading"><i class="fa fa-trash"></i></button>';

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

  showObservationGradingModal() {
    window["jQuery"]("#manageObservationGradingModal").modal("show");
  }

  hideObservationGradingModal() {
    window["jQuery"]("#manageObservationGradingModal").modal("hide");
  }

  addObservationGrading() {
    this.submitted = false;
    this.clearValues();
    this.showObservationGradingModal();
  }

  saveObservationGrading() {
    if (this.obGradingForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = { Id: this.obsGradingId, Name: this.obsGradingName };

      if (this.obsGradingId) {
        this.obsApi
          .updateObservationGrading("api/observationgrading", postData)
          .subscribe((response) => {
            this.notifyService.success("Observationgrading Updated Successfully");
            this.handleFormView.refresh();
          },
          (error) => {
            this.notifyService.error(error.error);
          });
      } else {
        this.obsApi
          .addObservationGrading("api/observationgrading", postData)
          .subscribe((response) => {
            this.notifyService.success("Observationgrading Inserted Successfully");
            this.handleFormView.refresh();
          },
          (error) => {
            this.notifyService.error(error.error);
          });
      }

      this.hideObservationGradingModal();
      this.handleFormView.refresh();
    }
  }

  editObservationGrading(data) {
    this.isEdit = true;
    this.obsGradingId = data.id;
    this.obsGradingName = data.name;

    this.showObservationGradingModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.obsGradingId = "";
    this.obsGradingName = "";
  }

  deleteObservationGrading(id) {
    this.obsApi
      .deleteObservationGrading("api/observationgrading/" + id)
      .subscribe((response) => {
        this.notifyService.success("Observationgrading Deleted Successfully");
        this.handleFormView.refresh();
      },
      (error) => {
        this.notifyService.error(error.error);
      });
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "master",
      "observationgrading"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#observationGradingComponent").on(
        "click",
        ".editObservationGrading",
        (e) => {
          let dataId = $(e.currentTarget).attr("data-id");
          let observationGradingData = $("#" + dataId).data();

          this.editObservationGrading(observationGradingData);
        }
      );

      $("#observationGradingComponent").on(
        "click",
        ".deleteObservationGrading",
        (e) => {
          let dataId = $(e.currentTarget).attr("data-id");

          this.deleteObservationGrading(dataId);
        }
      );
    });
  }
}
