import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn } from "./../../../common/table/table.model";
import { ErmrisksService } from "./ermrisks.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-ermrisks",
  templateUrl: "./ermrisks.component.html",
  styleUrls: ["./ermrisks.component.css"],
  providers: [ErmrisksService],
})
export class ErmrisksComponent implements OnInit {
  constructor(
    private utils: UtilsService,
    private ermrisks: ErmrisksService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("ermrisksForm", { static: false }) ermrisksForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  processlevel2Options: Object = [];
  isStackHolder: boolean = false;
  processlevel2Id: string = "";
  // tableApiUrl: string;

  tableFilters = new BehaviorSubject({});

  tableAllApiUrl: string;
  tableId: string = "ermrisks_table";
  ProcessLocationMappingId: string = "";

  auditNameOptions: any = [];
  businessCycleOptions: any = [];
  processlevel1Options: any = [];

  businessCycleId: string = "";
  processlevel1Id: string = "";
  ermId: string = "";
  riskId: string = "";
  riskTitle: string = "";
  riskDesc: string = "";
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
      title: "ERM ID",
      data: "ermid",
    },
    {
      title: "Risk Title",
      data: "riskTitle",
    },
    {
      title: "Risk Desciption",
      data: "riskDescription",
    },
    // {
    // title: "Business Cycle",
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
            '" class="btn btn-sm btn-info editErmrisks"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteErmrisks"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

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

  isEdit: boolean = false;

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

  saveErmrisks(e) {
    e.preventDefault();

    if (this.isEdit) this.updateErmrisks();
    else this.addNewErmrisks();
  }

  addNewErmrisks() {
    let postData = this.ermrisksForm.form.value;
    postData.ProcessLocationMappingID = this.ProcessLocationMappingId;

    this.ermrisks.addErmrisks("api/ermrisks", postData).subscribe(
      (response) => {
        this.notifyService.success("ERM Risk Added Successfully");
        this.updateOverallAssessmentErmRiskFlag();
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateErmrisks() {
    let postData = this.ermrisksForm.form.value;
    postData.id = this.riskId;
    postData.processLocationMappingID = this.ProcessLocationMappingId;

    this.ermrisks.updateErmrisks("api/ermrisks", postData).subscribe(
      (response) => {
        this.notifyService.success("ERM Risk Updated Successfully");
        this.updateOverallAssessmentErmRiskFlag();
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
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

  addErmrisks() {
    //this.fillBusinessCycleOpts();
    this.fillAuditNameOpts();
    this.handleFormView.show();
  }

  editErmrisks(ermrisksData) {
    this.isEdit = true;
    this.riskId = ermrisksData.id;
    this.ermId = ermrisksData.ermid;
    this.riskTitle = ermrisksData.riskTitle;
    this.riskRating = ermrisksData.riskRating;
    this.riskDesc = ermrisksData.riskDescription;
    this.ProcessLocationMappingId = ermrisksData.processLocationMappingID;
    // this.businessCycleId = ermrisksData.businessCycleID;
    // this.processlevel1Id = ermrisksData.processL1ID;
    // this.processlevel2Id = ermrisksData.processL2Id;
    // this.fillBusinessCycleOpts();
    // this.getProcessL1Opts();
    // this.getProcessLevel2Opts();
    this.fillAuditNameOpts();
    this.handleFormView.show();
  }

  deleteErmrisks(ermId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);

    if (isConfirm) {
      this.ermrisks.deleteErmrisks("api/ermrisks/" + ermId).subscribe(
        (response) => {
          this.notifyService.success("ERM Risk Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
    }
  }

  clearform() {
    this.ermId = "";
    this.riskTitle = "";
    this.processlevel1Id = "";
    this.processlevel2Id = "";
    this.businessCycleId = "";
    this.riskId = "";
    this.riskDesc = "";
    this.riskRating = "";
    this.processlevel2Options = [];
    this.businessCycleOptions = [];
    this.processlevel1Options = [];
  }

  exportErmrisks() {
    this.spinner.show();
    this.ermrisks.exportToExcel("api/ermrisks/downloadexcel").subscribe(
      (blob) => {
        //console.log(blob);
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });
        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "KeyERMRisks.xlsx");
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
  sampleExportErmrisks() {
    this.spinner.show();
    this.ermrisks.exportToExcel("api/ermrisks/sampledownloadexcel").subscribe(
      (blob) => {
        //console.log(blob);
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });
        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleKeyERMRisks.xlsx");
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

    this.ermrisks
      .importFromExcel("api/ermrisks/importexcel/" + userid, formData)
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
          this.fileInput.nativeElement.value = "";
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
      isERMRisks: true,
    };

    this.ermrisks
      .addErmrisks("api/overallassesment/updateermflag", postData)
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
      "ermrisk"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.fillAuditNameOpts();

    $(document).ready(() => {
      $("#ermComponent").on("click", ".editErmrisks", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        let ermrisksData = $("#" + dataId).data();
        //this.fillBusinessCycleOpts();
        this.editErmrisks(ermrisksData);
      });

      $("#ermComponent").on("click", ".deleteErmrisks", (e) => {
        let ermId = $(e.currentTarget).attr("data-id");
        this.deleteErmrisks(ermId);
      });
    });
  }
}
