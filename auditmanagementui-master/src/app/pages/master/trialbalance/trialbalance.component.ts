import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { TrialbalanceService } from "./trialbalance.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { ErmrisksService } from "../ermrisks/ermrisks.service";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-trialbalance",
  templateUrl: "./trialbalance.component.html",
  styleUrls: ["./trialbalance.component.css"],
  providers: [TrialbalanceService],
})
export class TrialbalanceComponent implements OnInit {
  constructor(
    private trialbalance: TrialbalanceService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("trialbalanceForm", { static: false }) trialbalanceForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "trialbalance_table";
  tableGetApi: string = "posts";

  ProcessLocationMappingId: string = "";

  locMapped: any[];
  auditNameOptions: any = [];
  locationTrialBalance: any = [];

  totalbalance: number = 0;

  id = "";
  glcode: string = "";
  glclass: string = "";
  gldescription: string = "";
  mainbalance: string = "";

  tableColumns: tableColumn[] = [
    {
      title: "GL Code",
      data: "glCode",
    },
    {
      title: "GL Class",
      data: "glClass",
    },
    {
      title: "GL Description",
      data: "glDescription",
    },
    {
      title: "Audit Name",
      data: "processLocationMapping",
      render: function (audit) {
        return audit.auditName ? audit.auditName : "";
      },
    },

    {
      title: "Profit Center",
      data: "location",
      render: function (loc) {
        return loc.profitCenterCode ? loc.profitCenterCode : "";
      },
    },
    {
      title: "Divison", 
      data: "location",
      render: function (loc) {
        return loc ? loc.divisionDescription : "";
      },
    },
    {
      title: "Locations",
      data: "location",
      render: (data, row, rowData) => {

        if (rowData.location.cityOrTown != null){
          if(rowData.location.cityOrTown)
          return rowData.location.cityOrTown.name;
          // return this.getLocationNames(rowData.processLocationMapping.locations);
        }
      },
    },
    {
      title: "Balance (INR)",
      className: "text-right",
      data: "trialBalances",
      render: (data) => {
        return this.utils.todecimalRound(data);
      },
    },
    {
      title: "Material Account",
      className: "text-center",
      data: "materialAccount",
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
            '" class="btn btn-sm btn-info editTB"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteTB"><i class="fa fa-trash"></i></button>';

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
  getLocationNames(locArray) {
    let locationNameArray = [];

    for (let location of this.locMapped) {
      if (locArray.indexOf(location.id) > -1) {
        locationNameArray.push(
          `<a  href="javascript:void(0);" class="btn btn-sm blue mb-10"> ${location.profitCenterCode} </a>`
        );
      }
    }

    return locationNameArray.join(" ");
  }
  saveTrialbalance(e) {
    if (this.trialbalanceForm.invalid) {
      return false;
    }

    e.preventDefault();

    if (this.isEdit) {
      this.updateTrialbalance();
    } else {
      this.addNewTrialbalance();
    }
  }

  importExcel() {
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    this.trialbalance
      .importFromExcel("api/trialbalance/importexcel", formData)
      .subscribe(
        (response) => {
          this.notifyService.success("Trial Balance Imported Successfully");
          this.fileInput.nativeElement.value = "";
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }

  addNewTrialbalance() {
    let postData = this.getLocationTB();

    this.trialbalance
      .addTrialbalance("api/TrialBalance/savetrialbalance", postData)
      .subscribe(
        (response) => {
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
        }
      );
  }

  updateTrialbalance() {
    let postData = this.trialbalanceForm.form.value;
    postData.id = this.id;
    postData.LocationTrialBalance = this.getLocationTB();
    postData.ProcessLocationMappingId = this.ProcessLocationMappingId;
    this.trialbalance
      .updateTrialbalance("api/TrialBalance", postData)
      .subscribe(
        (response) => {
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
        }
      );
  }

  addTrialbalance() {
    this.fillAuditNameOpts();
    this.handleFormView.show();
  }

  editTrialbalance(prmData) {
    this.isEdit = true;

    this.id = prmData.id;
    this.glcode = prmData.glCode;
    this.glclass = prmData.glClass;
    this.gldescription = prmData.glDescription;
    this.ProcessLocationMappingId = prmData.processLocationMappingId;
    this.totalbalance = prmData.mainBalance;

    this.fillAuditNameOpts();
    this.getLocationOpts();
    this.fillTbData();

    this.handleFormView.show();
  }

  ExportTrialBalance() {
    this.trialbalance
      .exportToExcel("api/TrialBalance/downloadexcel")
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "TrialBalance.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
      });
  }
  sampleExportTrialBalance() {
    this.trialbalance
      .exportToExcel("api/TrialBalance/sampledownloadexcel")
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleTrialBalance.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
      });
  }
  clearform() {
    this.glcode = "";
    this.glclass = "";
    this.gldescription = "";
    this.ProcessLocationMappingId = "";
    this.totalbalance = 0;
  }

  getLocationOpts() {
    if (this.ProcessLocationMappingId) {
      this.commonApi
        .getLocationByAuditName(this.ProcessLocationMappingId)
        .subscribe((posts) => {
          this.locMapped = posts[0].locationDetails;
          this.locMapped.forEach((res) => {
            this.totalbalance = this.totalbalance + res.trialBalance;
          });
          if (this.isEdit) {
            this.fillTbData();
          }
        });
    } else {
      this.locMapped = [];
    }
  }

  fillTbData() {
    setTimeout(() => {
      if (this.locationTrialBalance.length) {
        for (let locTb of this.locationTrialBalance) {
          let tbInputElem = window["jQuery"]("#" + locTb.locationId);
          if (tbInputElem.length) {
            tbInputElem.val(locTb.trialBalance);
          }
        }
      }
    }, 100);
  }

  fillAuditNameOpts() {
    this.commonApi.getAuditName().subscribe((posts) => {
      this.auditNameOptions = posts;
    });
  }

  getLocationTB() {
    let locations = [];
    let selectedLoc = window["jQuery"](
      '#tableLoc tbody tr td:nth-last-child(1) input[type="text"]'
    );

    for (let loc of selectedLoc) {
      let locId = window["jQuery"](loc).attr("id");
      let tb = window["jQuery"](loc).val() ? window["jQuery"](loc).val() : 0;

      locations.push({
        GLCode: this.glcode,
        GLClass: this.glclass,
        GLDescription: this.gldescription,
        ProcessLocationMappingId: this.ProcessLocationMappingId,
        MainBalance: this.totalbalance,
        LocationId: locId == "" ? null : locId,
        TrialBalances: parseFloat(tb),
      });
    }

    return locations;
  }

  calculateBalance(value) {
    let amount = value;
    if (amount !== "")
      this.totalbalance = this.totalbalance + parseFloat(amount);
    else this.totalbalance = 0;
  }

  deleteTrialBalance(prmId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.trialbalance
        .deleteTrialbalance("api/TrialBalance/" + prmId)
        .subscribe(
          (response) => {
            this.notifyService.success("Trail Balance Deleted Successfully");
            this.handleFormView.hide();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
    }
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "trialbalance");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.fillAuditNameOpts();

    $(document).ready(() => {
      $("#trialbalance_table").on("click", ".editTB", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let prmData = $("#" + dataId).data();

        this.editTrialbalance(prmData);
      });

      $("#trialbalance_table").on("click", ".deleteTB", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.deleteTrialBalance(dataId);
      });
    });
  }
}
