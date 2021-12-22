import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { CpiService } from "./cpi.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-control-performance-indicator",
  templateUrl: "./control-performance-indicator.component.html",
  styleUrls: ["./control-performance-indicator.component.css"],
})
export class ControlPerformanceIndicatorComponent implements OnInit {
  constructor(
    private cpiApi: CpiService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}
  @ViewChild("cpiForm", { static: false })
  cpiForm: NgForm;
  submitted: boolean = false;
  accessRights: any = {};
  isStackHolder: boolean = false;
  cpiId: string = "";
  controlRating: string = "";
  weightage: string = "";

  isEdit: boolean = false;

  tableColumns: tableColumn[] = [
    {
      title: "ControlRating",
      data: "controlRating",
    },
    {
      title: "Weightage",
      data: "weightage",
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
            '" class="btn btn-sm btn-info editCPI"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteCPI"><i class="fa fa-trash"></i></button>';

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

  showCPIModal() {
    window["jQuery"]("#manageCPIModal").modal("show");
  }

  hideCPIModalal() {
    window["jQuery"]("#manageCPIModal").modal("hide");
  }

  addcpi() {
    this.submitted = false;
    this.clearValues();
    this.showCPIModal();
  }

  saveCPI() {
    if (this.cpiForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = {
        Id: this.cpiId,
        ControlRating: this.controlRating,
        Weightage: this.weightage,
      };

      if (this.cpiId) {
        this.cpiApi
          .updateCPI("api/controlperformanceindicator", postData)
          .subscribe(
            (response) => {
              this.notifyService.success(
                "Controlperformanceindicator Updated Successfully"
              );
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      } else {
        this.cpiApi
          .addCPI("api/controlperformanceindicator", postData)
          .subscribe(
            (response) => {
              this.notifyService.success(
                "Controlperformanceindicator Inserted Successfully"
              );
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      }
      this.hideCPIModalal();
      this.handleFormView.refresh();
    }
  }

  editCPI(data) {
    this.isEdit = true;
    this.cpiId = data.id;
    this.controlRating = data.controlRating;
    this.weightage = data.weightage;

    this.showCPIModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.cpiId = "";
    this.controlRating = "";
    this.weightage = "";
  }

  deleteCPI(id) {
    this.cpiApi.deleteCPI("api/controlperformanceindicator/" + id).subscribe(
      (response) => {
        this.notifyService.success(
          "Controlperformanceindicator Deleted Successfully"
        );
        this.handleFormView.refresh();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "master",
      "controlperformanceindicator"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#CPIComponent").on("click", ".editCPI", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let CPIData = $("#" + dataId).data();

        this.editCPI(CPIData);
      });

      $("#CPIComponent").on("click", ".deleteCPI", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deleteCPI(dataId);
      });
    });
  }
}
