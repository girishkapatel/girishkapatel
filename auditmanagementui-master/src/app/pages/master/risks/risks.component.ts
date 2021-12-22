import {
  Component,
  ElementRef,
  Input,
  OnInit,
  ViewChild,
  ViewEncapsulation,
} from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { RisksService } from "./risks.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { Observable } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { ApiService } from "src/app/services/api/api.service";
import * as ClassicEditor from "../../../../assets/ckeditor5/build/ckeditor";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import{NgxSpinnerService}from "ngx-spinner";
@Component({
  selector: "app-risks",
  templateUrl: "./risks.component.html",
  styleUrls: ["./risks.component.css"],
  providers: [RisksService],
  encapsulation: ViewEncapsulation.None,
})
export class RisksComponent implements OnInit {
  constructor(
    private risks: RisksService,
    private commonApi: CommonApiService,
    private api: ApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner:NgxSpinnerService
    
  ) {}

  @ViewChild("risksForm", { static: false }) risksForm: NgForm;
  @ViewChild("racmImport", { static: false }) racmImport: ElementRef;
  @ViewChild("apForm", { static: false }) apForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  @Input() userOpts: any = [];
  @Input() shOpts: any = [];
  @Input() approverOpts: any = [];

  accessRights: any = {};
  isStackHolder: boolean = false;
  public Editor = ClassicEditor;

  businessCycleData: any;
  processL1Data: any;
  processL2Data: any;

  isEditAP: boolean = false;
  isEdit: boolean = false;
  formVisible: boolean = false;

  apIndex: number = -1;

  id: string = "";
  BusinessCycleId: string = "";
  ProcessL1Id: string = "";
  ProcessL2Id: string = "";
  RiskId: string = "";
  RiskTitle: string = "";
  RiskDesc: string = "";
  RiskRating: string = "";
  ControlId: string = "";
  ControlTitle: string = "";
  ControlDescription: string = "";
  ControlNature: string = "";
  ControlFrequency: string = "";
  ControlType: string = "";
  ControlOwner: string = "";

  pid: string = "";
  ProcedureId: string = "";
  ProcedureTitle: string = "";
  ProcedureDesc: string = "";

  ProcedureStartDate: NgbDateStruct;
  ProcedureEndDate: NgbDateStruct;

  Responsibility: string = "";
  Reviewer: string = "";
  RACMId: string = "";

  procedures: any = [];
  riskMaster: any = [];

  tableId: string = "risks_table";
  tableGetApi: string = "posts";
  userId: string = "";
  RACMnumber: string = "";
  tableColumns: tableColumn[] = [
    {
      title: "Risk ID",
      data: "risk.riskId",
    },
    // {
    // title: "Risk Title",
    // data: "risk.title",
    // },
    {
      title: "Risk Rating",
      data: "risk.rating",
    },
    {
      title: "Risk Description",
      data: "risk.description",
    },
    {
      title: "Control ID",
      data: "control.controlId",
    },
    {
      title: "Control Type",
      data: "control.type",
    },
    {
      title: "Control Nature",
      data: "control.nature",
    },
    {
      title: "Control Frequency",
      data: "control.frequency",
    },
    {
      title: "Control Description",
      data: "control.description",
    },
    // {
    // title: "Control Title",
    // data: "control.title",
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
            '" class="btn btn-sm btn-info editRisks"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteRisks"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  // userOpts: any = [];
  tableData: tableData[] = [];
  tableFilters = new BehaviorSubject({});

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

  getBusinessCycle() {
    this.spinner.show();
    this.commonApi.getBusinessCycle().subscribe((data) => {
      this.businessCycleData = data;
      this.spinner.hide();
    },(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }

  getProcessL1() {
    this.spinner.show();
    this.commonApi.getProcessLevel1(this.BusinessCycleId).subscribe((data) => {
      this.processL1Data = data;
      this.spinner.hide();
    },(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }

  getProcessL2() {
    this.spinner.show();
    this.commonApi.getProcessLevel2(this.ProcessL1Id).subscribe((data) => {
      this.processL2Data = data;
      this.spinner.hide();
    },(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }

  onChangeBusinessCycle(e) {
    this.getProcessL1();
  }

  onChangeProcessL1(e) {
    this.getProcessL2();
  }

  addNewRACMProcedure() {
    this.clearProcedureForm();
    this.showProcedureForm();
  }

  showProcedureForm() {
    this.fillUserOptions();
    window["jQuery"]("#manageProcedure").modal("show");
  }

  clearProcedureForm() {
    this.pid = "";
    this.ProcedureId = "";
    this.ProcedureTitle = "";
    this.ProcedureDesc = "";
    this.ProcedureStartDate = null;
    this.ProcedureEndDate = null;
    this.Responsibility = "";
    this.Reviewer = "";
  }

  formatApData = {
    get: (apData) => {
      let apObj = {
        id: apData.id,
        ProcedureId: apData.procedure.procedureId,
        ProcedureTitle: apData.procedure.procedureTitle,
        ProcedureDesc: apData.procedure.procedureDesc,
        ProcedureStartDate: this.utils.formatToNgbDate(
          apData.procedureStartDate
        ),
        ProcedureEndDate: this.utils.formatToNgbDate(apData.procedureEndDate),
        Responsibility: apData.responsibilityId,
        Reviewer: apData.reviewerId,
        CreatedBy: this.userId,
      };

      return apObj;
    },
    post: (apData) => {
      let apObj = {
        ProcedureStartDate: this.utils.formatNgbDateToYMD(
          apData.ProcedureStartDate
        ),
        ProcedureEndDate: this.utils.formatNgbDateToYMD(
          apData.ProcedureEndDate
        ),
        ResponsibilityId: apData.Responsibility,
        ReviewerId: apData.Reviewer,
        Procedure: {
          ProcedureId: apData.ProcedureId,
          ProcedureTitle: apData.ProcedureTitle,
          ProcedureDesc: apData.ProcedureDesc,
        },
        CreatedBy: this.userId,
        RACMId: this.RACMId,
      };

      if (apData.id) {
        apObj["id"] = apData.id;
      }

      return apObj;
    },
  };

  addRACMProcedure() {
    if (this.apForm.invalid) return false;

    let procedureData = this.apForm.form.value;
    let isNew = !this.isEditAP;

    if (this.id && isNew) this.addRACMDetail(procedureData);
    else this.addProcedureToTable(procedureData, isNew);

    this.hideProcedureForm();
  }

  hideProcedureForm() {
    this.isEditAP = false;

    window["jQuery"]("#manageProcedure").modal("hide");
  }

  addProcedureToTable(procedureData, isNew?: boolean) {
    let resourceTable = $("#racmProceduresTable tbody");

    let noOfRecords = resourceTable.children("tr").length;

    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#racmProceduresTable tbody .norecords").length) {
        $("#racmProceduresTable tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let responsibility = this.userOpts.filter(
        (userObj) => userObj.id === procedureData.Responsibility
      )[0];

      let reviewer = this.approverOpts.filter(
        (userObj) => userObj.id === procedureData.Reviewer
      )[0];

      let responsibleName = responsibility
        ? `${responsibility.firstName} ${responsibility.lastName}`
        : "";

      let reviewerName = reviewer
        ? `${reviewer.firstName} ${reviewer.lastName}`
        : "";

      let resHtml = `<tr>;
          <td>${procedureData.ProcedureId} </td>
          <td>${procedureData.ProcedureTitle}</td>
          <td>${procedureData.ProcedureDesc}</td>
          <td>${responsibleName}</td>
          <td>${reviewerName}</td>
          <td>${this.utils.formatNgbDateToDMY(
            procedureData.ProcedureStartDate
          )}</td>
          <td>${this.utils.formatNgbDateToDMY(
            procedureData.ProcedureEndDate
          )}</td>
          <td>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editProcedure">
              <i class="fa fa-edit"></i></button>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeProcedure">
              <i class="fa fa-trash"></i></button>
          </td>
        </tr>`;

      resourceTable.append(resHtml);

      if (!isInitRender) this.procedures.push(procedureData);
    } else {
      if (this.procedures[this.apIndex])
        procedureData.id = this.procedures[this.apIndex].id;

      this.procedures[this.apIndex] = procedureData;
      this.fillProcedureTable();
    }
  }

  fillProcedureTable() {
    if (Array.isArray(this.procedures) && this.procedures.length) {
      $("#racmProceduresTable tbody").html("");

      for (let procedure of this.procedures) {
        this.addProcedureToTable(procedure);
      }
    } else {
      $("#racmProceduresTable tbody").html(
        '<tr class="norecords"><td colspan="8" class="text-center">No Records Found</td></tr>'
      );
    }
  }

  getRacmProceduresToPost() {
    let racmApArray = this.procedures.map((apObj) =>
      this.formatApData.post(apObj)
    );

    return racmApArray;
  }

  addRACMDetails() {
    let racmObj = this.getRacmProceduresToPost();

    this.api.insertData("api/racmprocedure/multiple", racmObj).subscribe(
      (response) => {
        this.notifyService.success("RACM Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addRACMDetail(apData) {
    let apObj = this.formatApData.post(apData);
    apObj.RACMId = this.id;

    this.api.insertData("api/racmprocedure/", apObj).subscribe(
      (response) => {
        this.notifyService.success("Procedure Added Successfully");
        apData.id = response["id"];

        this.procedures.push(apData);

        this.fillProcedureTable();
        this.hideProcedureForm();
        this.clearProcedureForm();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  getRacmObj(formData) {
    let racmObj = {
      BusinessCycleId:
        formData.BusinessCycleId === "" ? null : formData.BusinessCycleId,
      ProcessL1Id: formData.ProcessL1Id === "" ? null : formData.ProcessL1Id,
      ProcessL2Id: formData.ProcessL2Id === "" ? null : formData.ProcessL2Id,
      RACMnumber: formData.RACMnumber,
      Risk: {
        //Title: formData.RiskTitle,
        Description: formData.RiskDesc,
        RiskId: formData.RiskId,
        Rating: formData.RiskRating,
      },
      Control: {
        //Title: formData.ControlTitle,
        ControlId: formData.ControlId,
        Description: formData.ControlDescription,
        Nature: formData.ControlNature,
        Frequency: formData.ControlFrequency,
        Type: formData.ControlType,
      },
    };

    return racmObj;
  }

  formatter = (risk: any) => {
    return this.RiskId;
  };

  search = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map((term) =>
        term.length < 0
          ? []
          : this.riskMaster
              .filter(
                (v) => v.riskId.toLowerCase().indexOf(term.toLowerCase()) > -1
              )
              .slice(0, 10)
      )
    );

  selectedItem(selectedOpt) {
    let riskData = selectedOpt.item;
    //this.RiskTitle = riskData.title;
    this.RiskDesc = riskData.description;
  }

  saveRisks(e) {
    if (this.risksForm.invalid) return false;

    if (this.isEdit) this.updateRisks();
    else this.addNewRisks();
  }

  addNewRisks() {
    let formData = this.risksForm.form.value;
    let racmObj = this.getRacmObj(formData);

    this.risks.addRisks("api/racm", racmObj).subscribe(
      (response) => {
        this.RACMId = response["id"];

        this.addRACMDetails();
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  updateRisks() {
    let formData = this.risksForm.form.value;
    let racmObj = this.getRacmObj(formData);

    racmObj["id"] = this.id;

    this.risks.updateRisks("api/racm", racmObj).subscribe(
      (response) => {
        this.notifyService.success("Risk Updated successfully");
        this.RACMId = this.id;
        this.updateRACMDetails();

        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateRACMDetails() {
    let racmObj = this.getRacmProceduresToPost();

    this.api
      .updateData("api/racmprocedure/multiple", racmObj)
      .subscribe((response) => {
        this.notifyService.success("RACM Updated Successfully");
        this.RACMId = "";
        this.handleFormView.hide();
      });
  }

  getAllRisks() {
    this.spinner.show();
    this.risks.getRisks("api/risk").subscribe(
      (allRisks) => {
        this.riskMaster = allRisks;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  addRisks() {
    this.getAllRisks();
    this.procedures = [];
    this.fillProcedureTable();
    this.handleFormView.show();
  }

  editRisks(riskData) {
    this.isEdit = true;

    this.id = riskData.id;
    this.BusinessCycleId = riskData.businessCycleId;
    this.ProcessL1Id = riskData.processL1Id;
    this.ProcessL2Id = riskData.processL2Id;
    this.RACMnumber = riskData.racMnumber;

    this.RiskId = riskData.risk.riskId;
    //this.RiskTitle = riskData.risk.title;
    this.RiskDesc = riskData.risk.description;
    this.RiskRating = riskData.risk.rating;

    this.ControlId = riskData.control.controlId;
    //this.ControlTitle = riskData.control.title;
    this.ControlDescription = riskData.control.description;
    this.ControlNature = riskData.control.nature;
    this.ControlFrequency = riskData.control.frequency;
    this.ControlType = riskData.control.type;
    this.ControlOwner = riskData.control.userId;

    this.procedures = this.getRacmProceduresToDisplay(riskData.racmProcedure);

    this.getBusinessCycle();
    this.getProcessL1();
    this.getProcessL2();
    this.getAllRisks();
    this.fillUserOptions();
    this.fillProcedureTable();

    this.handleFormView.show();
  }

  getRacmProceduresToDisplay(procedureData) {
    let racmApArray = procedureData.map((apObj) =>
      this.formatApData.get(apObj)
    );

    return racmApArray;
  }

  deleteRisks(racmId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.risks.deleteRisks("api/racm/" + racmId).subscribe(
        (response) => {
          this.notifyService.success("Risk Deleted successfully");
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
    }
  }

  fillUserOptions() {
    this.spinner.show();
    this.commonApi.getUsers().subscribe((posts) => {
      this.userOpts = posts;
      this.approverOpts = posts;
      this.spinner.hide();
    },(error) => {
      this.spinner.hide();
       console.log(error);
     });
  }

  handleFileUploadDialog = {
    show: () => {
      window["jQuery"]("#racmFileUploadModal").modal("show");
    },
    hide: () => {
      this.clearfiles();
      window["jQuery"]("#racmFileUploadModal").modal("hide");
    },
  };

  uploadfiles() {
    let filesElem = this.racmImport.nativeElement;
    if (filesElem.files.length) {
      let fd = new FormData();
      fd.append("file", filesElem.files[0]);
      this.spinner.show();
      this.api.insertData("api/ImportRACM/PostRacm", fd).subscribe(
        (result) => {
          this.spinner.hide();
          this.notifyService.success("File Uploaded Successfully");
          this.handleFileUploadDialog.hide();
        },
        (err) => {
          this.spinner.hide();
          this.notifyService.error("Unable to upload file");
        }
      );
    } else {
      this.notifyService.error("Please select file");
    }
  }

  clearfiles() {
    let filesElem = this.racmImport.nativeElement;
    filesElem.value = "";
  }

  clearform() {
    this.id = "";
    this.RACMnumber = "";
    this.BusinessCycleId = "";
    this.ProcessL1Id = "";
    this.ProcessL2Id = "";
    this.RiskId = "";
    this.RiskTitle = "";
    this.RiskDesc = "";
    this.RiskRating = "";
    this.ControlId = "";
    this.ControlTitle = "";
    this.ControlDescription = "";
    this.ControlNature = "";
    this.ControlFrequency = "";
    this.ControlType = "";
    this.ControlOwner = "";
    this.userOpts = [];
  }

  editProcedure(apIndex) {
    let resourceData = this.procedures[apIndex];

    this.clearProcedureForm();
    this.fillProcedureEdit(resourceData);
  }

  fillProcedureEdit(apData) {
    this.pid = apData.id;
    this.ProcedureId = apData.ProcedureId;
    this.ProcedureTitle = apData.ProcedureTitle;
    this.ProcedureDesc = apData.ProcedureDesc;
    this.ProcedureStartDate = apData.ProcedureStartDate;
    this.ProcedureEndDate = apData.ProcedureEndDate;
    this.Responsibility = apData.Responsibility;
    this.Reviewer = apData.Reviewer;

    this.showProcedureForm();
  }

  removeProcedure(apIndex) {
    if (this.procedures[apIndex]) {
      var id = this.procedures[apIndex].id;

      this.procedures.splice(apIndex, 1);

      this.api.deleteData("api/racmprocedure/" + id).subscribe((response) => {
        this.fillProcedureTable();
      });
    }
  }

  exportRACM() {
    this.spinner.show();
    this.api.downloadFile(`api/racm/downloadexcel`).subscribe((blob) => {
      // const blobUrl = URL.createObjectURL(blob);
      // window.open(blobUrl);

      const objblob: any = new Blob([blob], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });

      let link = document.createElement("a");
      if (link.download !== undefined) {
        let url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", "RACM.xlsx");
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
  sampleExportRACM() {
    this.spinner.show();
    this.api.downloadFile(`api/racm/sampledownloadexcel`).subscribe((blob) => {
      // const blobUrl = URL.createObjectURL(blob);
      // window.open(blobUrl);

      const objblob: any = new Blob([blob], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });

      let link = document.createElement("a");
      if (link.download !== undefined) {
        let url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", "SampleRACM.xlsx");
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
  importRACM() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);

    this.api.insertData("api/racm/importexcel", formData).subscribe(
      (response) => {
        this.spinner.hide();
         //Main action plan
         var excptionCount = response["excptionCount"];
         var excptionRowNumber = response["excptionRowNumber"];
         var totalRow = response["totalRow"];
         excptionRowNumber = excptionRowNumber.replace(/,\s*$/,"");
         var successCount = totalRow - excptionCount;
 
         //sub action plan
         var subPlanExcptionCount = response["subPlanExcptionCount"];
         var subPlanExcptionRowNumber = response["subPlanExcptionRowNumber"];
         var subPlanTotalRow = response["subPlanTotalRow"];
         subPlanExcptionRowNumber = subPlanExcptionRowNumber.replace(/,\s*$/, "");
         var subPlanSuccessCount = subPlanTotalRow - subPlanExcptionCount;
 
         var msg =
         "Risk Total Rows : " +
         totalRow +
         "<br>Risk Success Count : " +
         successCount +
         " <br>Risk Exception Count : " +
         excptionCount +
         "<br>Risk Exception RowNumber : " +
          excptionRowNumber +

         "<br><br>Procedure Total Rows : " +
         subPlanTotalRow +
         "<br>Procedure Success Count : " +
         subPlanSuccessCount +
         " <br>Procedure Exception Count : " +
         subPlanExcptionCount +
         "<br>Procedure Exception RowNumber : " +
         subPlanExcptionRowNumber;
         this.notifyService.success(msg, "", {
          enableHtml: true,
        });
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
      "knowledgelibrary",
      "racm"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    this.getBusinessCycle();
    this.fillUserOptions();

    var userid = localStorage.getItem("userId");
    this.userId = userid;

    $(document).ready(() => {
      $("#riskComponent").on("click", ".editRisks", (e) => {
        let dataId = window["jQuery"](e.currentTarget).attr("data-id");
        let riskData = $("#" + dataId).data();
        this.editRisks(riskData);
      });

      $("#riskComponent").on("click", ".deleteRisks", (e) => {
        let dataId = window["jQuery"](e.currentTarget).attr("data-id");
        this.deleteRisks(dataId);
      });

      $("#downloadRacmExcel").on("click", () => {
        $("#risks_table_wrapper .buttons-excel").click();
      });

      $("#riskComponent").on("click", ".editProcedure", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.apIndex = dataIndex;
          this.isEditAP = true;
          this.editProcedure(parseInt(dataIndex));
        }
      });

      $("#riskComponent").on("click", ".removeProcedure", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) this.removeProcedure(parseInt(dataIndex));
      });
    });
  }
}
