import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { CityService } from "./city.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: "app-city",
  templateUrl: "./city.component.html",
  styleUrls: ["./city.component.css"],
  providers: [CityService],
})
export class CityComponent implements OnInit {
  constructor(
    private city: CityService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("cityForm", { static: false }) cityForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  submitted: boolean = false;
  tableId: string = "city_table";
  isStackHolder: boolean = false;
  countryOptions: any = [];
  stateOptions: any = [];
  countryId: string = "";
  stateId: string = "";

  cityId: string = "";
  cityName: string = "";

  tableColumns: tableColumn[] = [
    {
      title: "City Name",
      data: "name",
    },
    {
      title: "State",
      data: "state.name",
    },
    {
      title: "Country",
      data: "country.name",
      render: function (country) {
        return country ? country : "";
      },
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
            '" class="btn btn-sm btn-info editCity"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteCity"><i class="fa fa-trash"></i></button>';

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
      this.tableFilters.next({});
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveCity(e) {
    this.submitted = true;
    if (this.cityForm.invalid) {
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateCity();
      } else {
        this.addNewCity();
      }
    }
  }

  addNewCity() {
    let postData = this.cityForm.form.value;
    this.city.addCity("api/cityortown", postData).subscribe(
      (response) => {
        this.notifyService.success("City Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  updateCity() {
    let postData = this.cityForm.form.value;
    postData.id = this.cityId;
    this.city.updateCity("api/cityortown", postData).subscribe(
      (response) => {
        this.notifyService.success("City Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  addCity() {
    this.fillCountryOpts();
    this.handleFormView.show();
  }

  editCity(cityData) {
    this.isEdit = true;
    this.cityId = cityData.id;
    this.cityName = cityData.name;
    this.countryId = cityData.country.id;
    this.stateId = cityData.stateId;
    this.fillCountryOpts();
    this.getStateOption();
    this.handleFormView.show();
  }

  deleteCity(cityId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.city.deleteCity("api/cityortown/" + cityId).subscribe(
        (response) => {
          this.notifyService.success("City Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: Location, Company. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  clearform() {
    this.cityId = "";
    this.stateId = "";
    this.countryId = "";
    this.stateOptions = [];
    this.cityName = "";
  }

  getStateOption() {
    this.spinner.show();
    this.commonApi.getState(this.countryId).subscribe((posts) => {
      this.stateOptions = posts;
      this.spinner.hide();
    } ,(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }

  fillCountryOpts() {
    this.spinner.show();
    this.commonApi.getCountry().subscribe((posts) => {
      this.countryOptions = posts;
      this.spinner.hide();
    },(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }

  exportCities() {
    this.spinner.show();
    this.city
      .exportToExcel("api/cityortown/downloadexcel")
      .subscribe((blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "Cities.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },(error) => {
						this.spinner.hide();
					   console.log(error);
					 });
  }
  sampleExportCities() {
    this.spinner.show();
    this.city
      .exportToExcel("api/cityortown/sampledownloadexcel")
      .subscribe((blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleCities.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },(error) => {
						this.spinner.hide();
					   console.log(error);
					 });
  }

  importExcel() {this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);

    this.city.importFromExcel("api/cityortown/importexcel", formData).subscribe(
      (response) => {
        this.notifyService.success("Cities Imported Successfully");
        this.handleFormView.hide();
        this.spinner.hide();
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
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#cityComponent").on("click", ".editCity", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let cityData = $("#" + dataId).data();
        this.editCity(cityData);
      });

      $("#cityComponent").on("click", ".deleteCity", (e) => {
        let cityId = $(e.currentTarget).attr("data-id");
        this.deleteCity(cityId);
      });

      $('a[href="#city"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });
    });
  }
}
