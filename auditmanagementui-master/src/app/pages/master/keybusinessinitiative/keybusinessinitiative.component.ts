import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn } from "./../../../common/table/table.model";
import { KeybusinessinitiativeService } from "./keybusinessinitiative.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { downloadFile } from "file-saver";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-keybusinessinitiative",
  templateUrl: "./keybusinessinitiative.component.html",
  styleUrls: ["./keybusinessinitiative.component.css"],
  providers: [KeybusinessinitiativeService],
})
export class KeybusinessinitiativeComponent implements OnInit {
  constructor(
    private keybusinessinitiative: KeybusinessinitiativeService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("keybusinessinitiativeForm", { static: false })
  keybusinessinitiativeForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  processlevel2Options: Object = [];
  isStackHolder: boolean = false;
  processlevel2Id: string = "";
  tableId: string = "keybusinessinitiative_table";

  auditNameOptions: any = [];
  businessCycleOptions: any = [];
  processlevel1Options: any = [];

  ProcessLocationMappingId: string = "";
  businessCycleId: string = "";
  processlevel1Id: string = "";
  bI_Id: string = "";
  Id: string = "";
  biDesc: string = "";
  riskRating: string = "";

  tableColumns: tableColumn[] = [
    {
      title: "Audit Name",
      data: "processLocationMapping.auditName",
      // render: (data, row, rowData) => {
      // return this.getAuditNames(rowData.processLocationMappingID);
      // },
    },
    {
      title: "BI ID",
      data: "businessInitiativeID",
    },
    {
      title: "Description",
      data: "businessIntiativeDescription",
    },
    // {
    // title: "Business Cycle ",
    // data: "businessCycle.name",
    // },
    // {
    // title: "Process L1",
    // data: "processL1.name",
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
            '" class="btn btn-sm btn-info editKeybusinessinitiative"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteKeybusinessinitiative"><i class="fa fa-trash"></i></button>';

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

  saveKeybusinessinitiative(e) {
    e.preventDefault();
    if (this.keybusinessinitiativeForm.invalid) {
      return false;
    }
    if (this.isEdit) this.updateKeybusinessinitiative();
    else this.addNewKeybusinessinitiative();
  }

  addNewKeybusinessinitiative() {
    let postData = this.keybusinessinitiativeForm.form.value;
    postData.ProcessLocationMappingId = this.ProcessLocationMappingId;

    this.keybusinessinitiative
      .addKeybusinessinitiative("api/keybusinessinitiative", postData)
      .subscribe(
        (response) => {
          this.notifyService.success(
            "Key business initiative added successfully"
          );
          this.updateOverallAssessmentErmRiskFlag();
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }

  updateKeybusinessinitiative() {
    let postData = this.keybusinessinitiativeForm.form.value;
    postData.id = this.Id;
    postData.ProcessLocationMappingId = this.ProcessLocationMappingId;

    this.keybusinessinitiative
      .updateKeybusinessinitiative("api/keybusinessinitiative", postData)
      .subscribe(
        (response) => {
          this.notifyService.success(
            "Key business initiative updated successfully."
          );
          this.updateOverallAssessmentErmRiskFlag();
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }
  fillAuditNameOpts() {
    this.spinner.show();
    this.commonApi.getAuditName().subscribe(
      (posts) => {
        this.auditNameOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getAuditNames(ProcessLocMapId) {
    let AuditNames = "";
    for (let bc of this.auditNameOptions) {
      if (bc.id == ProcessLocMapId) {
        AuditNames = bc.auditName;
      }
    }
    return AuditNames;
  }

  fillBusinessCycleOpts() {
    this.spinner.show();
    this.commonApi.getBusinessCycle().subscribe(
      (posts) => {
        this.businessCycleOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getProcessL1Opts() {
    if (this.businessCycleId) {
      this.spinner.show();
      this.commonApi.getProcessLevel1(this.businessCycleId).subscribe(
        (posts) => {
          this.processlevel1Options = posts;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    } else {
      this.processlevel1Id = "";
      this.processlevel1Options = [];
      this.processlevel2Id = "";
      this.processlevel2Options = [];
    }
  }

  getProcessLevel2Opts() {
    if (this.processlevel1Id) {
      this.spinner.show();
      this.commonApi.getProcessLevel2(this.processlevel1Id).subscribe(
        (posts) => {
          this.processlevel2Options = posts;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    } else {
      this.processlevel2Id = "";
      this.processlevel2Options = [];
    }
  }

  addKeybusinessinitiative() {
    this.fillBusinessCycleOpts();
    this.handleFormView.show();
  }

  editKeybusinessinitiative(keybusinessinitiativeData) {
    this.isEdit = true;
    this.Id = keybusinessinitiativeData.id;
    this.bI_Id = keybusinessinitiativeData.businessInitiativeID;
    this.riskRating = keybusinessinitiativeData.riskRating;
    this.biDesc = keybusinessinitiativeData.businessIntiativeDescription;
    this.ProcessLocationMappingId =
      keybusinessinitiativeData.processLocationMappingID;
    // this.businessCycleId = keybusinessinitiativeData.businessCycleID;
    // this.processlevel1Id = keybusinessinitiativeData.processL1ID;
    // this.processlevel2Id = keybusinessinitiativeData.processL2Id;
    // this.fillBusinessCycleOpts();
    // this.getProcessL1Opts();
    // this.getProcessLevel2Opts();
    this.fillAuditNameOpts();
    this.handleFormView.show();
  }

  deleteKeybusinessinitiative(bI_Id) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.keybusinessinitiative
        .deleteKeybusinessinitiative("api/keybusinessinitiative/" + bI_Id)
        .subscribe(
          (response) => {
            this.notifyService.success("BI Deleted Successfully");
            this.handleFormView.hide();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
    }
  }

  clearform() {
    this.bI_Id = "";
    this.processlevel1Id = "";
    this.businessCycleId = "";
    this.Id = "";
    this.biDesc = "";
    this.riskRating = "";
    this.processlevel2Id = "";
    this.processlevel2Options = [];
    this.businessCycleOptions = [];
    this.processlevel1Options = [];
  }

  exportBI() {
    this.spinner.show();
    this.keybusinessinitiative
      .exportToExcel("api/KeyBusinessInitiative/downloadexcel")
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "Keybusinessinitiative.xlsx");
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

  sampleExportBI() {
    this.spinner.show();
    this.keybusinessinitiative
      .exportToExcel("api/KeyBusinessInitiative/sampledownloadexcel")
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "SampleKeybusinessinitiative.xlsx");
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

    this.keybusinessinitiative
      .importFromExcel(
        "api/KeyBusinessInitiative/importexcel/" + userid,
        formData
      )
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

  updateOverallAssessmentErmRiskFlag() {
    let postData = {
      ProcessLocationMappingID: this.ProcessLocationMappingId,
      isKeyBusiness: true,
    };

    this.keybusinessinitiative
      .addKeybusinessinitiative(
        "api/overallassesment/updatekeybusinessflag",
        postData
      )
      .subscribe(
        (response) => {},
        (error) => {
          console.log(error);
        }
      );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "auditplanningengine",
      "keybusinessinitiative"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.fillAuditNameOpts();

    $(document).ready(() => {
      $("#kbiComponent").on("click", ".editKeybusinessinitiative", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        let keybusinessinitiativeData = $("#" + dataId).data();
        //this.fillBusinessCycleOpts();
        this.editKeybusinessinitiative(keybusinessinitiativeData);
      });

      $("#kbiComponent").on("click", ".deleteKeybusinessinitiative", (e) => {
        let bI_Id = $(e.currentTarget).attr("data-id");
        this.deleteKeybusinessinitiative(bI_Id);
      });
    });
  }
}
