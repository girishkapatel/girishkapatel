import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { CountryService } from "./country.service";
import { BehaviorSubject, from } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-country",
  templateUrl: "./country.component.html",
  styleUrls: ["./country.component.css"],
})
export class CountryComponent implements OnInit {
  constructor(
    private country: CountryService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}
  submitted: boolean = false;
  @ViewChild("countryForm", { static: false }) countryForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  isStackHolder: boolean = false;

  tableId: string = "country_table";

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
            '" class="btn btn-sm btn-info editCountry"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteCountry"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  tableData: tableData[] = [];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  countryId: string = "";
  countryName: string = "";
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

  saveCountry(e) {
    this.submitted = true;
    if (this.countryForm.invalid) {
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateCountry();
      } else {
        this.addNewCountry();
      }
    }
  }

  addNewCountry() {
    let postData = this.countryForm.form.value;
    this.country.addCountry("api/country", postData).subscribe(
      (response) => {
        this.notifyService.success("Country Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateCountry() {
    let postData = this.countryForm.form.value;
    postData.id = this.countryId;
    this.country.updateCountry("api/country", postData).subscribe(
      (response) => {
        this.notifyService.success("Country Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addCountry() {
    this.handleFormView.show();
  }

  editCountry(countryData) {
    this.isEdit = true;
    this.countryId = countryData.id;
    this.countryName = countryData.name;
    this.handleFormView.show();
  }

  deleteCountry(countryId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.country.deleteCountry("api/country/" + countryId).subscribe(
        (response) => {
          this.notifyService.success("Country Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: State, City, Location, Company. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  clearform() {
    this.countryId = "";
    this.countryName = "";
  }

  exportCountries() {
    this.spinner.show();
    this.country.exportToExcel("api/country/downloadexcel").subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "Countries.xlsx");
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
  sampleExportCountries() {
    this.spinner.show();
    this.country.exportToExcel("api/country/sampledownloadexcel").subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleCountries.xlsx");
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
    this.country.importFromExcel("api/country/importexcel", formData).subscribe(
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
      $("#countryComponent").on("click", ".editCountry", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let countryData = $("#" + dataId).data();
        this.editCountry(countryData);
      });

      $("#countryComponent").on("click", ".deleteCountry", (e) => {
        let countryId = $(e.currentTarget).attr("id");
        this.deleteCountry(countryId);
      });

      $('a[href="#followupTab"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });
    });
  }
}
