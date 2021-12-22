import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { ProcessriskmappingService } from "./processriskmapping.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ApiService } from "src/app/services/api/api.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-processriskmapping",
  templateUrl: "./processriskmapping.component.html",
  styleUrls: ["./processriskmapping.component.css"],
  providers: [ProcessriskmappingService],
})
export class ProcessriskmappingComponent implements OnInit {
  constructor(
    private processriskmapping: ProcessriskmappingService,
    private commonApi: CommonApiService,
    private api: ApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("processriskmappingForm", { static: false })
  processriskmappingForm: NgForm;

  @ViewChild("fileInput", { static: false }) fileInput;

  accessRights: any = {};
  isStackHolder: boolean = false;
  processlevel2Options: Object = [];

  tableId: string = "processriskmapping_table";

  id = "";
  auditNameOptions: any = [];
  businessCycleOptions: any = [];
  processlevel1Options: any = [];
  locMapped: any = [];

  ProcessLocationMappingId: string = "";
  businessCycleId: string = "";
  processlevel1Id: string = "";
  processlevel2Id: string = "";
  quan: string = "";
  qual: string = "";
  fpr: string = "";
  locationTrialBalance: any = [];

  tableColumns: tableColumn[] = [
    // {
    // title:'Business Cycle',
    // data:'businessCycle.name'
    // },
    // {
    // title:'Process L1',
    // data:'processL1.name'
    // },
    {
      title: "Audit Name",
      data: "processLocationMapping.auditName",
      // render: (data, row, rowData) => {
      // return this.getAuditNames(rowData.processLocationMappingID);
      // },
    },
    {
      title: "Quantitative Assesment",
      data: "quantativeAssessment",
    },
    {
      title: "Qualitative Assesment",
      data: "qualitativeAssessment",
    },
    {
      title: "Final Process Rating",
      data: "finalProcessrating",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editProcessriskmapping "><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteProcessriskmapping"><i class="fa fa-trash"></i></button>';

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

  saveProcessriskmapping(e) {
    e.preventDefault();
    if (this.isEdit) {
      this.updateProcessriskmapping();
    } else {
      this.addNewProcessriskmapping();
    }
  }

  getLocationTB() {
    let locations = [];
    let selectedLoc = window["jQuery"](
      '#tableLoc tbody tr td:nth-last-child(1) input[type="text"]'
    );
    for (let loc of selectedLoc) {
      let locId = window["jQuery"](loc).attr("id");
      let tb = window["jQuery"](loc).val() ? window["jQuery"](loc).val() : 0;
      locations.push({ LocationId: locId, TrialBalance: parseFloat(tb) });
    }
    return locations;
  }

  addNewProcessriskmapping() {
    let postData = this.processriskmappingForm.form.value;
    postData.LocationTrialBalance = this.getLocationTB();
    postData.ProcessLocationMappingId = this.ProcessLocationMappingId;
    this.processriskmapping
      .addProcessriskmapping("api/processriskmapping", postData)
      .subscribe(
        (response) => {
          this.handleFormView.hide();
          this.createOverallAssessment(postData);
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }

  updateProcessriskmapping() {
    let postData = this.processriskmappingForm.form.value;
    postData.id = this.id;
    postData.LocationTrialBalance = this.getLocationTB();
    postData.ProcessLocationMappingId = this.ProcessLocationMappingId;
    this.processriskmapping
      .updateProcessriskmapping("api/processriskmapping", postData)
      .subscribe(
        (response) => {
          this.handleFormView.hide();
          this.createOverallAssessment(postData);
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }

  addProcessriskmapping() {
    //this.fillBusinessCycleOpts();
    this.fillAuditNameOpts();
    this.handleFormView.show();
  }

  editProcessriskmapping(prmData) {
    this.isEdit = true;
    this.id = prmData.id;
    this.ProcessLocationMappingId = prmData.processLocationMappingID;
    // this.businessCycleId = prmData.businessCycle.id;
    // this.processlevel1Id = prmData.processL1.id;
    // this.processlevel2Id = prmData.processL2 ? prmData.processL2.id : "";
    this.quan = prmData.quantativeAssessment;
    this.qual = prmData.qualitativeAssessment;
    this.fpr = prmData.finalProcessrating;
    this.locationTrialBalance = prmData.locationTrialBalance;
    this.fillAuditNameOpts();
    // this.fillBusinessCycleOpts();
    // this.getProcessLevel1Opts();
    // this.getProcessLevel2Opts();
    this.getLocationOpts();
    this.fillTbData();
    this.handleFormView.show();
  }

  deleteProcessriskmapping(prmId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.processriskmapping
        .deleteProcessriskmapping("api/processriskmapping/" + prmId)
        .subscribe(
          (response) => {
            this.notifyService.success("Mapping Deleted Successfully");
            this.handleFormView.hide();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
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

  clearform() {
    this.id = "";
    this.ProcessLocationMappingId = "";
    this.businessCycleId = "";
    this.processlevel1Id = "";
    this.processlevel2Id = "";
    this.quan = "";
    this.qual = "";
    this.fpr = "";
    this.locationTrialBalance = [];
    //this.auditNameOptions = [];
    this.businessCycleOptions = [];
    this.processlevel1Options = [];
    this.processlevel2Options = [];
    this.locMapped = [];
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

  getLocationOpts() {
    if (this.ProcessLocationMappingId) {
      //this.getProcessLevel2Opts();
      this.spinner.show();
      this.commonApi
        .getLocationByAuditName(this.ProcessLocationMappingId)
        .subscribe(
          (posts) => {
            this.spinner.hide();
            this.locMapped = posts[0].locationDetails;
            if (this.isEdit) {
              this.fillTbData();
            }
          },
          (error) => {
            this.spinner.hide();
            console.log(error);
          }
        );
    } else {
      this.locMapped = [];
      this.processlevel2Id = "";
      this.processlevel2Options = [];
    }
  }

  getProcessLevel1Opts() {
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
      this.locMapped = [];
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

  createOverallAssessment(prMap) {
    let postData = {
      ProcessLocationMappingId: prMap.ProcessLocationMappingId,
      // BusinessCycleId: prMap.BusinessCycleId,
      // ProcessL1Id: prMap.ProcessL1Id,
      // ProcessL2Id: prMap.ProcessL2Id,
    };
    this.spinner.show();
    this.processriskmapping
      .addProcessriskmapping("api/overallassesment", postData)
      .subscribe(
        (response) => {
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  exportPRM() {
    this.spinner.show();
    this.api.downloadFile(`api/processriskmapping/downloadexcel`).subscribe(
      (blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "ProcessRiskMapping.xlsx");
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
  sampleExportPRM() {
    this.spinner.show();
    this.api.downloadFile(`api/processriskmapping/sampledownloadexcel`).subscribe(
      (blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");
        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleProcessRiskMapping.xlsx");
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
  importPRM() {
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");
    this.spinner.show();
    this.api
      .insertData("api/processriskmapping/importexcel/" + userid, formData)
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

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "auditplanningengine",
      "processriskmapping"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.fillAuditNameOpts();

    $(document).ready(() => {
      $("#prmComponent").on("click", ".editProcessriskmapping", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let prmData = $("#" + dataId).data();
        this.editProcessriskmapping(prmData);
      });

      $("#prmComponent").on("click", ".deleteProcessriskmapping", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.deleteProcessriskmapping(dataId);
      });
    });
  }
}
