import {
  Component,
  OnInit,
  ViewChild,
  Input,
  ɵNOT_FOUND_CHECK_ONLY_ELEMENT_INJECTOR,
} from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ApiService } from "src/app/services/api/api.service";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
import { IDropdownSettings } from "ng-multiselect-dropdown";

@Component({
  selector: "app-walkthrough",
  templateUrl: "./walkthrough.component.html",
  styleUrls: ["./walkthrough.component.css"],
  providers: [],
})
export class WalkthroughComponent implements OnInit {
  constructor(
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private api: ApiService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("apForm", { static: false }) apForm: NgForm;
  @ViewChild("racmForm", { static: false }) racmForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;

  @Input() userOpts: any = [];
  @Input() shOpts: any = [];
  @Input() approverOpts: any = [];
  OwnerdropdownSettings: IDropdownSettings = {};

  accessRights: any = {};
  isStackHolder: boolean = false;
  model: any = {};
  selectedRACMLib = {};

  public Editor = ClassicEditor;

  filterOption: string = "All";
  filterControlNature: string = "All";
  AuditID: string = "";

  knowledgeLibraryOptions: any = [];
  businessCycleOptions: any = [];
  processlevel1Options: any = [];
  processlevel2Options: any = [];
  lstAuditArea: any = [];

  racmNumberOptions: any[];
  riskIdOptions: any[];
  controlIdOptions: any[];
  proceduereIdOptions: any[];

  SelectedRACMId: string = "";
  BusinessCycleId: string = "";
  ProcessL1Id: string = "";
  ProcessL2Id: string = "";

  /* RACM Details*/
  id: string = "";
  RACMnumber: string = "";
  RiskId: string = "";
  RiskTitle: string = "";
  RiskDesc: string = "";
  RiskRating: string = "";
  ControlId: string = "";
  ControlIdText: string = "";
  ControlTitle: string = "";
  ControlDescription: string = "";
  ControlNature: string = "";
  ControlFrequency: string = "";
  ControlType: string = "";
  ControlOwner: string = "";
  AuditArea: string = "";
  SelectedRACMnumber: string = "";
  SelectedRiskId: string = "";
  SelectedControlId: string = "";
  SelectedProcedureId: string = "";
  TotalProcedure: number = 0;

  // DesignMarks: string = "";
  // DesignEffectiveness: string = "";
  // OEMarks: string = "";
  // OEEffectiveness: string = "";

  /* Audit Procedure Details*/
  pid: string = "";
  ProcedureId: string = "";
  ProcedureTitle: string = "";
  ProcedureDesc: string = "";
  ProcedureStartDate: NgbDateStruct;
  ProcedureEndDate: NgbDateStruct;
  Responsibility: string = "";
  Reviewer: string = "";

  /*Audit Procedure Add/Edit */
  auditProcedures: any = [];
  apIndex: number = -1;
  isEditAP: boolean = false;
  RACMAuditProcedureId: string = "";

  tablelibId: string = "racm_lib_table";
  tableId: string = "walkthrough_table";
  // tableFilters = new BehaviorSubject({});
  // tableApiUrl: string = "";

  RACMCNTLow: number = 0;
  RACMCNTMedium: number = 0;
  RACMCNTHigh: number = 0;
  RACMCNTCritical: number = 0;
  RACMCNTTotal: number = 0;
  RACMCNTAutomated: number = 0;
  RACMCNTManual: number = 0;
  RACMCNTITDependent: number = 0;

  RACMProcedureDetails: any = [];
  RACMProcedurecount: number = 0;
  RACMLibraryDetails: any = [];
  RACMLibrarycount: number = 0;
  selectedImplementationOwnerId = [];
  tableColumns: tableColumn[] = [
    {
      title: "Risk ID",
      data: "risk.riskId",
    },
    {
      title: "Risk Description",
      data: "risk.description",
    },
    {
      title: "Risk Rating",
      data: "risk.rating",
    },
    {
      title: "Control ID",
      data: "control.controlId",
    },
    {
      title: "Control Description",
      data: "control.description",
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
            '" class="btn btn-sm btn-info editRACM"><i class="fa fa-edit"></i></button>';

        return buttons;
      },
    },
  ];

  tableData: tableData[] = [];

  isEdit: boolean = false;

  formVisible: boolean = false;

  tableApiUrlRAMLib: string;
  tableFiltersRAMLib: BehaviorSubject<{ init: boolean }>;

  tableColumnsRAMLib: tableColumn[] = [
    {
      title: "Select",
      data: "id",
      render: (data) => {
        return (
          '<p class="text-center" style="margin:0px"><input type="checkbox" class="selectedRACMLib" data-id="' +
          data +
          '" /></p>'
        );
      },
    },
    {
      title: "Risk ID",
      data: "risk.riskId",
    },
    {
      title: "Risk Description",
      data: "risk.description",
    },
    {
      title: "Risk Rating",
      data: "risk.rating",
    },
    {
      title: "Control ID",
      data: "control.controlId",
    },
    {
      title: "Control Description",
      data: "control.description",
    },
  ];

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.formVisible = false;
      this.isEdit = false;
      // this.tableFilters.next({});
      this.loadRACMTable();
      this.clearform();
      this.clearProcedureForm();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveRACM(e) {
    if (this.racmForm.invalid) {
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateRACM();
      } else {
        this.addNewRACM();
      }
    }
  }

  getRacmObj(formData) {
    let racmObj = {
      AuditId: this.AuditID,
      ProcessL1Id: formData.ProcessL1Id === "" ? null : formData.ProcessL1Id,
      ProcessL2Id: formData.ProcessL2Id === "" ? null : formData.ProcessL2Id,
      BusinessCycleId:
        formData.BusinessCycleId === "" ? null : formData.BusinessCycleId,
      RACMnumber: this.SelectedRACMnumber,
      Risk: {
        Title: formData.RiskTitle,
        Description: formData.RiskDesc,
        RiskId: this.SelectedRiskId,
        Rating: formData.RiskRating,
      },
      Control: {
        Title: formData.ControlTitle,
        ControlId: this.SelectedControlId,
        ControlIdText: formData.controlId,
        Description: formData.ControlDescription,
        Nature: formData.ControlNature,
        Frequency: formData.ControlFrequency,
        Type: formData.ControlType,
        UserId: this.getImplementionOwnerIds(),
      },
      AuditArea: this.AuditArea,
    };

    // DesignMarks: this.normalizeNumbers(formData.DesignMarks),
    //     DesignEffectiveness: this.normalizeNumbers(
    //       formData.DesignEffectiveness
    //     ),
    //     OEMarks: this.normalizeNumbers(formData.OEMarks),
    //     OEEffectiveness: this.normalizeNumbers(formData.OEEffectiveness),

    return racmObj;
  }

  normalizeNumbers(input) {
    return !isNaN(input) ? parseInt(input) : 0;
  }

  getRacmProceduresToPost() {
    let racmApArray = this.auditProcedures.map((apObj) =>
      this.formatApData.post(apObj)
    );
    return racmApArray;
  }

  getRacmProceduresToDisplay(procedureData) {
    let racmApArray = procedureData.map((apObj) =>
      this.formatApData.get(apObj)
    );
    return racmApArray;
  }

  addNewRACM() {
    this.spinner.hide();

    let formData = this.racmForm.form.value;
    let racmObj = this.getRacmObj(formData);

    this.api.insertData("api/racmauditprocedure", racmObj).subscribe(
      (response) => {
        this.RACMAuditProcedureId = response["id"];
        this.addRACMDetails();
      },
      (error) => {
        if (error.status == 208) {
          this.notifyService.error("RiskId and ControlId are already exists.");
        } else {
          this.notifyService.error(error.error);
        }
      }
    );
  }

  addRACMDetails() {
    let racmObj = this.getRacmProceduresToPost();

    this.api
      .insertData("api/racmauditproceduredetails/multiple", racmObj)
      .subscribe((response) => {
        this.notifyService.success("RACM Added Successfully");
        this.handleFormView.hide();
      });
  }

  addRACMDetail(apData) {
    let apObj = this.formatApData.post(apData);

    apObj.RACMAuditProcedureId = this.id;

    this.api
      .insertData("api/racmauditproceduredetails/", apObj)
      .subscribe((response) => {
        this.notifyService.success("Audit Procedure Added Successfully");
        apData.id = response["id"];
        this.auditProcedures.push(apData);
        this.fillAuditProcedureTable();
        this.hideProcedureForm();
        this.clearProcedureForm();
      });
  }

  updateRACM() {
    let formData = this.racmForm.form.value;
    let racmObj = this.getRacmObj(formData);
    racmObj["id"] = this.id;
    this.spinner.show();
    this.api.updateData("api/racmauditprocedure", racmObj).subscribe(
      (response) => {
        this.spinner.hide();
        this.RACMAuditProcedureId = this.id;
        this.updateRACMDetails();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  updateRACMDetails() {
    this.spinner.show();
    let racmObj = this.getRacmProceduresToPost();
    this.api
      .updateData("api/racmauditproceduredetails/multiple", racmObj)
      .subscribe(
        (response) => {
          this.spinner.hide();
          this.notifyService.success("RACM Updated Successfully");
          this.RACMAuditProcedureId = "";
          this.handleFormView.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  addRACM() {
    this.getKnowledgeLibraryOpts();
    this.fillBusinessCycleOpts();
    this.fillAuditProcedureTable();
    this.fillAuditArea();
    this.fillRACMID(false);
    this.handleFormView.show();
  }
  fillRACMID(IsEdit) {
    this.spinner.show();
    this.api
      .getData("api/racmauditprocedure/getracmnumber/" + this.AuditID)
      .subscribe(
        (response) => {
          let objResult: any = response;
          this.racmNumberOptions = objResult.racmNumber;
          this.riskIdOptions = objResult.riskId;
          this.controlIdOptions = objResult.controlId;

          if(!IsEdit){
            let lastracmNumber =
            this.racmNumberOptions[this.racmNumberOptions.length - 1];
          this.SelectedRACMnumber = lastracmNumber;

          let lastRiskId = this.riskIdOptions[this.riskIdOptions.length - 1];
          this.SelectedRiskId = lastRiskId;
        
          let lastcontrolId =
            this.controlIdOptions[this.controlIdOptions.length - 1];
          this.SelectedControlId = lastcontrolId;
        }
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  fillProcedureID() {
    this.spinner.show();
    this.api
      .getData(
        "api/racmauditprocedure/getprocedure/" +
          this.AuditID +
          "/" +
          this.TotalProcedure
      )
      .subscribe(
        (response) => {
          let objResult: any = response;
          this.proceduereIdOptions = objResult.procedureId;
          let lastprocedureId =
            this.proceduereIdOptions[this.proceduereIdOptions.length - 1];
          this.ProcedureId = lastprocedureId;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  editRACM(racmData) {
    this.fillRACMID(true);
    this.isEdit = true;
    this.id = racmData.id;
    this.BusinessCycleId = racmData.businessCycleId;
    this.ProcessL1Id = racmData.processL1Id;
    this.ProcessL2Id = racmData.processL2Id;
    this.RACMnumber = racmData.racMnumber;
    this.SelectedRACMnumber = racmData.racMnumber;
    this.RiskId = racmData.risk.riskId;
    this.SelectedRiskId = racmData.risk.riskId;
    this.RiskTitle = racmData.risk.title;
    this.RiskDesc = racmData.risk.description;
    this.RiskRating = racmData.risk.rating;
    this.SelectedControlId = racmData.control.controlId;
    this.ControlId = racmData.control.controlId;
    this.ControlIdText = racmData.control.controlId;
    this.ControlTitle = racmData.control.title;
    this.ControlDescription = racmData.control.description;
    this.ControlNature = racmData.control.nature;
    this.ControlFrequency = racmData.control.frequency;
    this.ControlType = racmData.control.type;
    this.selectedImplementationOwnerId =
      this.getSelectedImplementationOwnerOpts(racmData.control.userId);
    this.AuditArea = racmData.auditArea;
    this.auditProcedures = this.getRacmProceduresToDisplay(
      racmData.racmAuditProcedureDetails
    );

    this.SelectedRACMId = "0";

    this.getKnowledgeLibraryOpts();
    this.fillBusinessCycleOpts();
    this.getProcessLevel1Opts();
    this.getProcessLevel2Opts();
    this.fillAuditProcedureTable();
    this.fillAuditArea();
    this.handleFormView.show();
  }

  deleteRACM() {}

  clearform() {
    this.id = "";
    this.RACMnumber = "";
    this.RiskId = "";
    this.RiskTitle = "";
    this.RiskDesc = "";
    this.RiskRating = "";
    this.ControlId = "";
    this.ControlIdText = "";
    this.ControlTitle = "";
    this.ControlDescription = "";
    this.ControlNature = "";
    this.ControlFrequency = "";
    this.ControlType = "";
    this.ControlOwner = "";
    this.SelectedRACMId = "";
    this.BusinessCycleId = "";
    this.ProcessL1Id = "";
    this.ProcessL2Id = "";
    this.auditProcedures = [];
    this.knowledgeLibraryOptions = [];
    this.businessCycleOptions = [];
    this.processlevel1Options = [];
    this.processlevel2Options = [];
    this.lstAuditArea = [];
    this.racmNumberOptions = [];
    this.riskIdOptions = [];
    this.controlIdOptions = [];
    this.proceduereIdOptions = [];
    this.TotalProcedure = 0;
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

  getKnowledgeLibraryOpts() {
    this.spinner.show();
    this.commonApi.getAllRACM().subscribe(
      (posts) => {
        this.knowledgeLibraryOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
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

  getProcessLevel1Opts() {
    if (this.BusinessCycleId) {
      this.spinner.show();
      this.commonApi.getProcessLevel1(this.BusinessCycleId).subscribe(
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
      this.ProcessL1Id = "";
      this.processlevel1Options = [];
      this.ProcessL2Id = "";
      this.processlevel2Options = [];
    }
  }

  getProcessLevel2Opts() {
    if (this.ProcessL1Id) {
      this.spinner.show();
      this.commonApi.getProcessLevel2(this.ProcessL1Id).subscribe(
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
      this.ProcessL2Id = "";
      this.processlevel2Options = [];
    }
  }

  getRACMById() {
    if (this.SelectedRACMId != "") {
      this.spinner.show();
      this.commonApi.getRACMById(this.SelectedRACMId).subscribe(
        (posts) => {
          let racmData = posts[0];
          this.BusinessCycleId = racmData.businessCycleId;
          this.ProcessL1Id = racmData.processL1Id;
          this.ProcessL2Id = racmData.processL2Id;
          this.RACMnumber = racmData.racMnumber;
          this.SelectedRACMnumber = racmData.racMnumber;
          this.RiskId = racmData.risk.riskId;
          this.SelectedRiskId =  racmData.risk.riskId;
          this.RiskTitle = racmData.risk.title;
          this.RiskDesc = racmData.risk.description;
          this.RiskRating = racmData.risk.rating;
          this.ControlId = racmData.control.controlId;
          this.SelectedControlId=racmData.control.controlId;
          this.ControlIdText = racmData.control.controlId;
          this.ControlTitle = racmData.control.title;
          this.ControlDescription = racmData.control.description;
          this.ControlNature = racmData.control.nature;
          this.ControlFrequency = racmData.control.frequency;
          this.ControlType = racmData.control.type;
          this.ControlOwner = racmData.control.userId;
          this.spinner.hide();
          this.getProcessLevel1Opts();
          this.getProcessLevel2Opts();
          this.fillRACMID(false);
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
    } else {
      this.id = "";
      this.RACMnumber = "";
      this.SelectedRACMnumber = "";
      this.RiskId = "";
      this.RiskTitle = "";
      this.RiskDesc = "";
      this.RiskRating = "";
      this.ControlId = "";
      this.ControlIdText = "";
      this.ControlTitle = "";
      this.ControlDescription = "";
      this.ControlNature = "";
      this.ControlFrequency = "";
      this.ControlType = "";
      this.ControlOwner = "";
      this.BusinessCycleId = "";
      this.ProcessL1Id = "";
      this.ProcessL2Id = "";

      this.processlevel1Options = [];
      this.processlevel2Options = [];
    }
  }
  getRiskData() {
     this.spinner.show();
      this.commonApi.getRiskData(this.SelectedRiskId).subscribe(
        (posts) => {
          var resp=posts;
          if(resp){
          this.RiskTitle =posts["risk"].title;
          this.RiskDesc =posts["risk"].description;
          this.RiskRating = posts["risk"].rating;}
          else{
            this.RiskTitle ="";
            this.RiskDesc ="";
            this.RiskRating = "";
          }
          this.spinner.hide(); 
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      ); 
  }
  getControlData() {
    this.spinner.show();
     this.commonApi.getControlData(this.SelectedControlId).subscribe(
       (posts) => {
         var resp=posts;
         if(resp){
          this.ControlId = posts["control"].controlId;
          this.SelectedControlId=posts["control"].controlId;
          this.ControlIdText = posts["control"].controlId;
          this.ControlTitle = posts["control"].title;
          this.ControlDescription = posts["control"].description;
          this.ControlNature = posts["control"].nature;
          this.ControlFrequency = posts["control"].frequency;
          this.ControlType = posts["control"].type;
          this.ControlOwner =posts["control"].userId;
         }
         else{
          this.ControlIdText = "";
      this.ControlTitle = "";
      this.ControlDescription = "";
      this.ControlNature = "";
      this.ControlFrequency = "";
      this.ControlType = "";
      this.ControlOwner = "";
         }
         this.spinner.hide(); 
       },
       (error) => {
         this.spinner.hide();
         console.log(error);
       }
     ); 
 }
  addNewAuditProcedure() {
    this.clearProcedureForm();
    this.showProcedureForm();
    this.fillProcedureID();
  }

  showProcedureForm() {
    window["jQuery"]("#manageProcedure").modal("show");
  }

  hideProcedureForm() {
    this.isEditAP = false;

    window["jQuery"]("#manageProcedure").modal("hide");
  }

  addProcedureToTable(procedureData, isNew?: boolean) {
    let resourceTable = $("#auditProceduresTable tbody");
    let noOfRecords = resourceTable.children("tr").length;
    let isInitRender = false;
    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#auditProceduresTable tbody .norecords").length) {
        $("#auditProceduresTable tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let responsibility = this.userOpts.filter(
        (userObj) => userObj.user.id === procedureData.Responsibility
      )[0];
      let reviewer = this.approverOpts.filter(
        (userObj) => userObj.user.id === procedureData.Reviewer
      )[0];

      let responsibleName = responsibility
        ? `${responsibility.user.firstName} ${responsibility.user.lastName}`
        : "";
      let reviewerName = reviewer
        ? `${reviewer.user.firstName} ${reviewer.user.lastName}`
        : "";

      let buttons = "";

      if (this.accessRights.isEdit)
        buttons = `<button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editAuditProcedure"><i class="fa fa-edit"></i></button>`;

      if (this.accessRights.isDelete)
        buttons += `<button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeAuditProcedure"><i class="fa fa-trash"></i></button>`;

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
           
          <td>${buttons}</td>
        </tr>`;

      resourceTable.append(resHtml);
      if (!isInitRender) {
        this.auditProcedures.push(procedureData);
        this.TotalProcedure++;
      }
    } else {
      if (this.auditProcedures[this.apIndex]) {
        procedureData.id = this.auditProcedures[this.apIndex].id;
      }
      this.auditProcedures[this.apIndex] = procedureData;
      this.fillAuditProcedureTable();
    }
  }

  fillAuditProcedureTable() {
    if (Array.isArray(this.auditProcedures) && this.auditProcedures.length) {
      $("#auditProceduresTable tbody").html("");
      for (let procedure of this.auditProcedures) {
        this.addProcedureToTable(procedure);
      }
    } else {
      $("#auditProceduresTable tbody").html(
        '<tr class="norecords"><td colspan="8">No Records Found</td></tr>'
      );
    }
  }

  addAuditProcedure() {
    if (this.apForm.invalid) return false;
    let procedureData = this.apForm.form.value;
    let isNew = !this.isEditAP;

    if (this.id && isNew) this.addRACMDetail(procedureData);
    else this.addProcedureToTable(procedureData, isNew);

    this.hideProcedureForm();
  }

  removeAuditProcedure(apIndex) {
    if (this.auditProcedures[apIndex]) {
      var id = this.auditProcedures[apIndex].id;
      this.auditProcedures.splice(apIndex, 1);
      this.api
        .deleteData("api/racmauditproceduredetails/" + id)
        .subscribe((response) => {
          this.fillAuditProcedureTable();
        });
    }
  }

  editAuditProcedure(apIndex) {
    let resourceData = this.auditProcedures[apIndex];
    this.clearProcedureForm();
    this.fillAuditProcedureEdit(resourceData);
  }

  fillAuditProcedureEdit(apData) {
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

  fillAuditArea() {
    this.spinner.show();
    this.api.getData("api/tor/getAuditArea/" + this.AuditID).subscribe(
      (response) => {
        this.lstAuditArea = response;
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  formatApData = {
    get: (apData) => {
      let apObj = {
        id: apData.id,
        AuditId: this.AuditID,
        ProcedureId: apData.procedure.procedureId,
        ProcedureTitle: apData.procedure.procedureTitle,
        ProcedureDesc: apData.procedure.procedureDesc,
        ProcedureStartDate: this.utils.formatToNgbDate(
          apData.procedureStartDate
        ),
        ProcedureEndDate: this.utils.formatToNgbDate(apData.procedureEndDate),
        Responsibility: apData.responsibilityId,
        Reviewer: apData.reviewerId,
      };
      return apObj;
    },
    post: (apData) => {
      var userid = localStorage.getItem("userId");
      let apObj = {
        AuditId: this.AuditID,
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
        RACMAuditProcedureId: this.RACMAuditProcedureId,
        CreatedBy: userid,
      };

      if (apData.id) {
        apObj["id"] = apData.id;
      }

      return apObj;
    },
  };

  getSummary() {
    this.spinner.show();
    this.api
      .getData("api/racmauditprocedure/getsummary/" + this.AuditID)
      .subscribe(
        (response) => {
          let objResult: any = response;

          this.RACMCNTCritical = objResult.critical || 0;
          this.RACMCNTHigh = objResult.high || 0;
          this.RACMCNTMedium = objResult.medium || 0;
          this.RACMCNTLow = objResult.low || 0;

          this.RACMCNTTotal =
          this.RACMCNTHigh + this.RACMCNTHigh + this.RACMCNTMedium + this.RACMCNTLow;

          this.RACMCNTAutomated = objResult.automated || 0;
          this.RACMCNTManual = objResult.manual || 0;
          this.RACMCNTITDependent = objResult.itDependent || 0;
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  ShowFilterData(option) {
    this.filterOption = option;
    this.loadRACMTable();
    // this.tableApiUrl = `api/racmauditprocedure/GetByAuditAndRiskRating/${this.AuditID}/${this.filterOption}`;
    // this.tableFilters.next({ init: true });
  }

  FilterByControlNature(option) {
    this.filterControlNature = option;
    this.loadRACMTable();
  }

  loadRACMTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(
        `api/racmauditprocedure/GetByAuditAndRiskRating/${this.AuditID}/${this.filterOption}/${this.filterControlNature}`
      )
      .subscribe(
        (dtData) => {
          this.RACMProcedureDetails = dtData;
          this.RACMProcedurecount = this.RACMProcedureDetails.length;
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumns
          );
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }
  loadRACMLibraryTable() {
    this.spinner.show();
    this.api.getData(`api/racm/getRACMbyAudit/${this.AuditID}`).subscribe(
      (dtData) => {
        this.RACMLibraryDetails = dtData;
        this.RACMLibrarycount = this.RACMLibraryDetails.length;
        this.commonApi.initialiseTable(
          this.tablelibId,
          dtData,
          this.tableColumnsRAMLib
        );
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  refreshTable() {
    this.filterOption = "All";
    this.filterControlNature = "All";
    this.loadRACMTable();
  }

  handleLibraryDialog = {
    show: () => {
      window["jQuery"]("#racmLibModal").modal("show");
      this.tableFiltersRAMLib.next({ init: true });
    },
    hide: () => {
      window["jQuery"]("#racmLibModal").modal("hide");
    },
  };

  handleRACMData = {
    checkEmpty: () => {
      return this.utils.isEmptyObj(this.selectedRACMLib);
    },
    add: (dataId, racmData) => {
      if (typeof this.selectedRACMLib[dataId] === "undefined") {
        this.selectedRACMLib[dataId] = {};
      }

      this.selectedRACMLib[dataId] = racmData;
    },
    remove: (dataId) => {
      if (typeof this.selectedRACMLib[dataId] === "object") {
        delete this.selectedRACMLib[dataId];
      }
    },
  };

  saveRACMFromLibrary() {
    if (!this.handleRACMData.checkEmpty()) {
      let racmIDs = Object.keys(this.selectedRACMLib);
      this.spinner.show();
      this.api
        .insertData("api/racmauditprocedure/addFromRACMLibrary", {
          RACMIDs: racmIDs,
          AuditID: this.AuditID,
        })
        .subscribe(
          (response) => {
            this.notifyService.success("Data import from library successful");
            this.handleLibraryDialog.hide();
            this.loadRACMTable();
            this.spinner.hide();
          },
          (error) => {
            this.spinner.hide();
            if (error.status == 208) {
              this.notifyService.error(
                "RiskId and ControlId are already exists."
              );
            } else {
              this.notifyService.error(error.error);
            }
          }
        );
    } else {
      this.notifyService.error("Please select records to save");
    }
  }

  handleSelectedRACMData(dataId, racmData, isChecked) {
    if (isChecked) {
      this.handleRACMData.add(dataId, racmData);
    } else {
      this.handleRACMData.remove(dataId);
    }
  }

  exportRACM() {
    this.spinner.show();
    this.api
      .downloadFile(`api/racmauditprocedure/downloadexcel/${this.AuditID}`)
      .subscribe(
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
            link.setAttribute("download", "AuditRACM.xlsx");
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
  sampleExportRACM() {
    this.spinner.show();
    this.api
      .downloadFile(`api/racmauditprocedure/sampledownloadexcel/${this.AuditID}`)
      .subscribe(
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
            link.setAttribute("download", "SampleAuditRACM.xlsx");
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

  importRACM() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");
    this.api
      .insertData(
        `api/racmauditprocedure/importexcel/${this.AuditID}/${userid}`,
        formData
      )
      .subscribe(
        (response) => {
          this.spinner.hide();
          //Main action plan
          var excptionCount = response["excptionCount"];
          var excptionRowNumber = response["excptionRowNumber"];
          var totalRow = response["totalRow"];
          excptionRowNumber = excptionRowNumber.replace(/,\s*$/, "");
          var successCount = totalRow - excptionCount;

          //sub action plan
          // var subPlanExcptionCount = response["subPlanExcptionCount"];
          // var subPlanExcptionRowNumber = response["subPlanExcptionRowNumber"];
          // var subPlanTotalRow = response["subPlanTotalRow"];
          // subPlanExcptionRowNumber = subPlanExcptionRowNumber.replace(
          //   /,\s*$/,
          //   ""
          // );
          // var subPlanSuccessCount = subPlanTotalRow - subPlanExcptionCount;

          var msg =
            "Risk Total Rows : " +
            totalRow +
            "<br>Risk Success Count : " +
            successCount +
            " <br>Risk Exception Count : " +
            excptionCount +
            "<br>Risk Exception RowNumber : " +
            excptionRowNumber;
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
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "auditexecution"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "racm"
    )[0];
  }
  getImplementionOwnerIds() {
    var implementationOwner = "";
    this.selectedImplementationOwnerId.forEach((element) => {
      implementationOwner = element.id;
    });
    return implementationOwner;
  }
  getSelectedImplementationOwnerOpts(OwnerArray) {
    let rootCauseNamesArray = [];
    if (OwnerArray != null) {
      for (let i of this.shOpts) {
        if (OwnerArray.indexOf(i.id) > -1) {
          rootCauseNamesArray.push({
            id: i.id,
            custom: i.firstName + " " + i.lastName,
          });
        }
      }
    }
    return rootCauseNamesArray;
  }
  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditID = localStorage.getItem("auditId");
    this.loadRACMTable();
    this.loadRACMLibraryTable();
    this.TotalProcedure = 0;
    $(document).ready(() => {
      $("#racmComponent").on("click", ".editRACM", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        // let racmData = $("#" + dataId).data();
        let racmData = window["jQuery"]("#" + dataId).data();
        this.editRACM(racmData);
      });

      $("#racmComponent").on("click", ".removeAuditProcedure", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeAuditProcedure(parseInt(dataIndex));
        }
      });

      $("#racmComponent").on("click", ".editAuditProcedure", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.apIndex = dataIndex;
          this.isEditAP = true;
          this.editAuditProcedure(parseInt(dataIndex));
        }
      });

      $('#auditExecutionTab a[href="#walkthrough"]').on("click", (e) => {
        // if (typeof this.tableFilters !== "undefined") {
        // this.tableFilters.next({ init: true });
        // }
        this.refreshTable();
      });

      $("#racmLibModal").on("change", ".selectedRACMLib", (e) => {
        let isChecked = $(e.target).is(":checked");
        let dataId = $(e.currentTarget).attr("data-id");
        let eybmData = $("#" + dataId).data();
        this.handleSelectedRACMData(dataId, eybmData, isChecked);
      });
    });
    this.initDropdownSettings(
      false,
      "id",
      "custom",
      "Select All",
      "Deselect All",
      3,
      false,
      true
    );
  }
  initDropdownSettings(
    isSingleSelection: boolean,
    idField: string,
    textField: string,
    selectAllText: string,
    unSelectAllText: string,
    itemsShowLimit: number,
    isAllowSearchFilter: boolean,
    closeDropDownOnSelection: boolean
  ) {
    this.OwnerdropdownSettings = {
      singleSelection: true,
      idField: "id",
      textField: "custom",
      allowSearchFilter: true,
      closeDropDownOnSelection: true,
    };
  }
}
