import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { LocationService } from "./location.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { HttpClient } from "@angular/common/http";
import am4geodata_worldLow from "./../../../common/data/worldLow.data";
import { ApiService } from "src/app/services/api/api.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-location",
  templateUrl: "./location.component.html",
  styleUrls: ["./location.component.css"],
  providers: [LocationService],
})
export class LocationComponent implements OnInit {
  constructor(
    private api: ApiService,
    private locationApi: LocationService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private http: HttpClient,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("locationForm", { static: false }) locationForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "location_table";

  tableColumns: tableColumn[] = [
    {
      title: "Company Name",
      data: "company.name",
    },
    {
      title: "Location",
      data: "locationDescription",
    },
    {
      title: "Country",
      data: "country.name",
    },
    {
      title: "City",
      data: "cityOrTown.name",
      render: function (city) {
        return city ? city : "";
      },
    },
    {
      title: "Division",
      data: "division",
    },
    {
      title: "Risk Index",
      data: "riskIndex",
    },
    // {
    //   title: "Division",
    //   data: "divisionDescription",
    // },
    // // {
    // //   title:'Country ID',
    // //   data:'countryId'
    // // },
    // {
    //   title: "Location ID",
    //   data: "locationID",
    // },

    // {
    //   title: "Profit Center Code",
    //   data: "profitCenterCode",
    // },
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
            '" class="btn btn-sm btn-info editLocation"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteLocation"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  companyOpts: any = [];
  countryOpts: any = [];
  stateOpts: any = [];
  cityOpts: any = [];
  sectorOpts: any = [];

  locationName: string = "";
  locationId: string = "";

  division: string = "";
  divisionDesc: string = "";
  location: string = "";
  locationDesc: string = "";
  riskIndex: string = "";
  pcc: string = "";
  sectorId: string = "";
  sectorName: string = "";

  countryId: string = "";
  country: string = "";
  stateId: string = "";
  state: string = "";
  cityId: string = "";
  city: string = "";
  companyId: string = "";
  companyName: string = "";

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

  getCountryName(country) {
    this.country = country;
  }

  saveLocation() {
    if (this.locationForm.invalid) return false;
    let locationName;
    if (this.isEdit) {
      locationName = `${this.country["name"] || ""}, ${
        this.state["name"] || ""
      }, ${this.city["name"] || ""}`;
    } else {
      locationName = `${this.country || ""}, ${this.state || ""}, ${
        this.city || ""
      }`;
    }

    this.api.getLocationLatLong(locationName).subscribe(
      (res) => {
        let locDet =
          res["data"] && res["data"].length && res["data"][0]
            ? res["data"][0]
            : {};
        let loc = {};
        if (!this.utils.isEmptyObj(locDet)) {
          loc = {
            latitude: locDet.latitude.toString(),
            longitude: locDet.longitude.toString(),
            countrycode: this.getCountryCode(locDet.country),
          };
        }
        this.saveLocationRecord(loc);
      },
      (err) => {
        console.log(err);
        this.saveLocationRecord(null);
      }
    );
  }

  getCountryCode(countryName) {
    let countrycode = "";
    let country = am4geodata_worldLow.features.filter(
      (x) => x.properties.name === countryName
    );
    if (country.length === 1) {
      countrycode = country[0].properties.id;
    }
    return countrycode;
  }

  saveLocationRecord(loc) {
    if (this.isEdit) {
      this.updateLocation(loc);
    } else {
      this.addNewLocation(loc);
    }
  }

  addNewLocation(loc) {
    let postData = this.locationForm.form.value;
    if (loc) {
      postData.Latitude = loc.latitude || "";
      postData.Longitude = loc.longitude || "";
      postData.Countrycode = loc.countrycode || "";
    }
    postData.IsActive = true;
    this.locationApi.addLocation("api/location", postData).subscribe(
      (response) => {
        this.notifyService.success("Location Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateLocation(loc) {
    let postData = this.locationForm.form.value;
    if (loc) {
      postData.Latitude = loc.latitude || "";
      postData.Longitude = loc.longitude || "";
      postData.Countrycode = loc.countrycode || "";
    }
    postData.IsActive = true;
    postData.id = this.locationId;
    this.locationApi.updateLocation("api/location", postData).subscribe(
      (response) => {
        this.notifyService.success("Location Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addLocation() {
    this.fillSectorOptions();
    this.fillCountryOptions();
    this.handleFormView.show();
  }

  editLocation(locationData) {
    this.isEdit = true;
    this.division = locationData.division;
    this.divisionDesc = locationData.divisionDescription;
    this.location = locationData.locationID;
    this.locationDesc = locationData.locationDescription;
    this.riskIndex = locationData.riskIndex;
    this.pcc = locationData.profitCenterCode;
    this.sectorId = locationData.sector;
    this.sectorName = locationData.sectorName;
    this.locationId = locationData.id;
    this.locationName = locationData.name;
    this.countryId = locationData.countryID;
    this.country = locationData.country;
    this.stateId = locationData.stateID;
    this.state = locationData.state;
    this.cityId = locationData.cityOrTown.id;
    this.city = locationData.cityOrTown;
    this.companyId = locationData.companyID;
    this.companyName = locationData.companyName;
    this.fillSectorOptions();
    this.fillCountryOptions();
    this.getStateOptions(this.country);
    this.getCityOptions(this.state);
    this.getCompanyOptions(this.city);
    this.handleFormView.show();
  }

  deleteLocation(locationId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.locationApi.deleteLocation("api/location/" + locationId).subscribe(
        (response) => {
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: ProcessLocationMapping, ScopeAndSchedule, Audit. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  clearform() {
    this.division = "";
    this.divisionDesc = "";
    this.location = "";
    this.locationDesc = "";
    this.riskIndex = "";
    this.pcc = "";
    this.sectorId = "";
    this.sectorName = "";
    this.locationId = "";
    this.locationName = "";
    this.countryId = "";
    this.country = "";
    this.stateId = "";
    this.state = "";
    this.cityId = "";
    this.city = "";
    this.companyId = "";
    this.companyName = "";
    this.companyOpts = [];
    this.countryOpts = [];
    this.stateOpts = [];
    this.cityOpts = [];
    this.sectorOpts = [];
  }

  fillSectorOptions() {
    this.spinner.show();
    this.commonApi.getSector().subscribe(
      (posts) => {
        this.sectorOpts = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  fillCountryOptions() {
    this.spinner.show();
    this.commonApi.getCountry().subscribe(
      (posts) => {
        this.countryOpts = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getCompanyOptions(event) {
    if (!this.isEdit) {
      let cityName =
        event.target.options[event.target.options.selectedIndex].text;
      this.city = cityName;
    }
    this.spinner.show();
    this.commonApi.getCompany().subscribe(
      (posts) => {
        this.companyOpts = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  getStateOptions(event) {
    if (!this.isEdit) {
      let countryName =
        event.target.options[event.target.options.selectedIndex].text;
      this.country = countryName;
    }
    if (this.countryId) {
      this.spinner.show();
      this.commonApi.getState(this.countryId).subscribe(
        (posts) => {
          this.stateOpts = posts;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    } else {
      this.stateId = "";
      this.stateOpts = [];
      this.cityId = "";
      this.cityOpts = [];
    }
  }

  getCityOptions(event) {
    if (!this.isEdit) {
      let stateName =
        event.target.options[event.target.options.selectedIndex].text;
      this.state = stateName;
    }
    if (this.stateId) {
      this.spinner.show();
      this.commonApi.getCity(this.stateId).subscribe(
        (posts) => {
          this.cityOpts = posts;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    } else {
      this.cityId = "";
      this.cityOpts = [];
    }
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "location");
  }
  exportLocation() {
    this.spinner.show();
    this.api.downloadFile(`api/location/downloadexcel`).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "Location.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  sampleExportLocation() {
    this.spinner.show();
    this.api.downloadFile(`api/location/sampledownloadexcel`).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleLocation.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  importExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");

    this.api
      .insertData("api/location/importexcel/" + userid, formData)
      .subscribe(
        (response) => {
          this.spinner.hide();
          var excepitonCount = response["excptionCount"];
          var totalRows = response["totalRow"];
          var exceptionRowNumber = response["excptionRowNumber"];
          exceptionRowNumber = exceptionRowNumber.replace(/,\s*$/, "");
          var successCount = totalRows - excepitonCount;
          var msg =
            "Total Rows : " +
            totalRows +
            "<br> Success Count : " +
            successCount +
            " <br>Exception Count : " +
            excepitonCount +
            "<br>Exception RowNumber : " +
            exceptionRowNumber;
          this.notifyService.success(msg, "", {
            enableHtml: true,
          });
          console.log(msg);
          this.handleFormView.hide();
        },
        (error) => {
          this.spinner.hide();
          this.notifyService.error(error.error);
        }
      );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#locationComponent").on("click", ".editLocation", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let locationData = $("#" + dataId).data();
        this.editLocation(locationData);
      });

      $("#locationComponent").on("click", ".deleteLocation", (e) => {
        let locationId = $(e.currentTarget).attr("data-id");
        this.deleteLocation(locationId);
      });
    });
  }
}
