import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { StateService } from "./state.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: "app-state",
  templateUrl: "./state.component.html",
  styleUrls: ["./state.component.css"],
  providers: [StateService],
})
export class StateComponent implements OnInit {
  constructor(
    private state: StateService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("stateForm", { static: false }) stateForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "state_table";
  submitted: boolean = false;
  tableGetApi: string = "posts";

  countryOptions: any = [];

  countryId: string = "";
  stateId: string = "";
  stateName: string = "";

  tableColumns: tableColumn[] = [
    {
      title: "State Name",
      data: "name",
    },
    {
      title: "Country",
      data: "country.name",
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
            '" class="btn btn-sm btn-info editState"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteState"><i class="fa fa-trash"></i> </button>';

        return buttons;
      },
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
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
      this.tableFilters.next({});
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveState(e) {
    if (this.stateForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateState();
      } else {
        this.addNewState();
      }
    }
  }

  addNewState() {
    let postData = this.stateForm.form.value;
    this.state.addState("api/state", postData).subscribe(
      (response) => {
        this.notifyService.success("State Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateState() {
    let postData = this.stateForm.form.value;
    postData.id = this.stateId;
    this.state.updateState("api/state", postData).subscribe(
      (response) => {
        this.notifyService.success("State Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addState() {
    this.fillCountryOption();
    this.handleFormView.show();
  }

  editState(stateData) {
    this.isEdit = true;
    this.stateId = stateData.id;
    this.stateName = stateData.name;
    this.countryId = stateData.countryId;
    this.fillCountryOption();
    this.handleFormView.show();
  }

  deleteState(stateId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.state.deleteState("api/state/" + stateId).subscribe(
        (response) => {
          this.notifyService.success("State Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: City, Location, Company. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  fillCountryOption() {
    this.spinner.show();
    this.commonApi.getCountry().subscribe(
      (posts) => {
        this.countryOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  clearform() {
    this.stateId = "";
    this.stateName = "";
    this.countryId = "";
  }

  exportStates() {this.spinner.show();
    this.state.exportToExcel("api/state/downloadexcel").subscribe((blob) => {
      // const blobUrl = URL.createObjectURL(blob);
      // window.open(blobUrl);

      const objblob: any = new Blob([blob], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });

      let link = document.createElement("a");

      if (link.download !== undefined) {
        let url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", "States.xlsx");
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      }
      this.spinner.hide();
    } ,(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }
  sampleExportStates() {this.spinner.show();
    this.state.exportToExcel("api/state/sampledownloadexcel").subscribe((blob) => {
      // const blobUrl = URL.createObjectURL(blob);
      // window.open(blobUrl);

      const objblob: any = new Blob([blob], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });

      let link = document.createElement("a");

      if (link.download !== undefined) {
        let url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", "SampleStates.xlsx");
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      }
      this.spinner.hide();
    } ,(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }
  importExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);

    this.state.importFromExcel("api/state/importexcel", formData).subscribe(
      (response) => {
        this.spinner.hide();
        this.notifyService.success("States Imported Successfully");
        this.handleFormView.hide();

      },
      (error) => {
        this.spinner.hide();
        this.notifyService.error(error.error);
      }
    );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "master",
      "country,state,city"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#stateComponent").on("click", ".editState", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let stateData = $("#" + dataId).data();
        this.editState(stateData);
      });

      $("#stateComponent").on("click", ".deleteState", (e) => {
        let stateId = $(e.currentTarget).attr("id");
        this.deleteState(stateId);
      });

      $('a[href="#state"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });
    });
  }
}
