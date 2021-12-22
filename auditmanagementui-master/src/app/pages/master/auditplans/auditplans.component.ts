import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { AuditPlanService } from "./auditplans.service";
import { BehaviorSubject } from "rxjs";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { NgForm } from "@angular/forms";
import { NgxSpinnerService } from "ngx-spinner";
import { DatePipe } from "@angular/common";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { IDropdownSettings } from "ng-multiselect-dropdown";
@Component({
  selector: "app-auditplan",
  templateUrl: "./auditplans.component.html",
  styleUrls: ["./auditplans.component.css"],
  providers: [AuditPlanService, NgxSpinnerService],
})
export class AuditPlanComponent implements OnInit {
  constructor(
    private auditplan: AuditPlanService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private spinner: NgxSpinnerService,
    private datePipe: DatePipe,
    private notifyService: ToastrService
  ) {}

  @ViewChild("auditPlanForm", { static: false }) auditPlanForm: NgForm;
  @ViewChild("ApproverForm", { static: false }) ApproverForm: NgForm;
  @ViewChild("ResourceForm", { static: false }) ResourceForm: NgForm;
  @ViewChild("AuditeeForm", { static: false }) AuditeeForm: NgForm;
  @ViewChild("apForm", { static: false }) apForm: NgForm;
  OwnerdropdownSettings: IDropdownSettings = {};

  accessRights: any = {};
  isStackHolder: boolean = false;
  approverIndex: number;

  selectedApprover: any = {};
  approvers: any = [];

  isEditApprover: boolean = false;

  approver: string;
  appDesignation: string;
  appQualification: string;
  appExperience: string;
  appResponsibility: string;

  model: any = {};

  selectedAuditee: any = {};
  auditees: any = [];
  auditeeIndex: number;
  isEditAuditee: boolean = false;

  auditee: string;
  auditeeDesignation: string;
  auditeeQualification: string;
  auditeeExperience: string;
  auditeeEmailId: string;
  auditeeReportTo: string;

  selectedReportTo: any = {};

  userOpts: any = [];
  stackHolderOptions: any = [];
  resources: any = [];
  resource: string = "";
  resourceIndex: number = -1;

  selectedResource: any = {};
  isEditResource: boolean = false;

  id: string = "";
  scheduleId: string = "";
  auditProcess: string = "";
  auditName: string = "";
  location: string = "";
  locationId: string = "";

  overallAuditStartDate: NgbDateStruct;
  overallAuditEndDate: NgbDateStruct;

  designation: string = "";
  qualification: string = "";
  experience: string = "";

  auditStartDate: NgbDateStruct;
  auditEndDate: NgbDateStruct;

  manDaysRequired: string = "";
  quarter: string = "";
  auditNumber: string = "";
  processLocationMappingId: string = "";

  selectedIds: any = [];
  email_id: string = "";
  Email: string = "";
  singleMail: boolean = false;
  selectedImplementationOwnerId = [];

  tableScroll: boolean = false;
  tableId: string = "audit_table";
  tableColumns: tableColumn[] = [
    {
      title: "<input type='checkbox' id='chkAllDataTracker' />",
      data: "id",
      orderable: false,
      className: "text-center",
      render: (data, type, row, meta) => {
        return (
          "<input type='checkbox' data-id='" +
          data +
          "' class='chkSingleDataTracker' />"
        );
      },
    },
    {
      title: "Year",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.scopeAndSchedule) {
          if (rowData.scopeAndSchedule.auditStartDate && rowData.scopeAndSchedule.auditEndDate )
            return this.utils.getCurrentFinancialYearByDate(
              this.utils.UTCtoLocalDateDDMMMYY(rowData.scopeAndSchedule.auditStartDate),this.utils.UTCtoLocalDateDDMMMYY(rowData.scopeAndSchedule.auditEndDate)
            );
          else return "";
        } else {
          return "";
        }
      },
    },
    {
      title: "Audit Name",
      data: "",
      render: function (data, row, rowData) {
        if (rowData) {
          return rowData.processLocationMapping.auditName;
        } else {
          return "";
        }
      },
    },
    // {
    // title:'Audit Process',
    // data:'overallAssesment.processL1.name',
    // render:function(data){
    // if(data){
    // return data;
    // }else{
    // return '';
    // }
    // }
    // },
    {
      title: "Location",
      data: "location.profitCenterCode",
      render: function (data) {
        if (data) {
          return data;
        } else {
          return "";
        }
      },
    },
    {
      title: "Sector",
      data: "location.divisionDescription",
    },
    {
      title: "Country",
      data: "location.country.name",
    },
    {
      title: "Audit Start Date",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.scopeAndSchedule) {
          if (rowData.scopeAndSchedule.auditStartDate)
            return this.utils.formatDbDateToDMY(
              rowData.scopeAndSchedule.auditStartDate
            );
          else return "";
        } else {
          return "";
        }
      },
    },
    {
      title: "Audit End Date",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.scopeAndSchedule) {
          if (rowData.scopeAndSchedule.auditEndDate)
            return this.utils.formatDbDateToDMY(
              rowData.scopeAndSchedule.auditEndDate
            );
          else return "";
        } else {
          return "";
        }
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editAuditPlan"><i class="fa fa-calendar"></i></button>';
        if (!this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-primary emailAuditPlan" title="Send email"><i class="fa fa-send"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteAudit"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;
  formVisible: boolean = false;
  tableFilters = new BehaviorSubject({});

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.tableFilters.next({ init: true });
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
    },
  };

  cancelAuditEdit() {
    this.handleFormView.hide();
  }

  saveAuditSchedule(e) {
    if (this.auditPlanForm.invalid) {
      return false;
    }

    e.preventDefault();

    let postData = this.getAuditSchedule();

    let saveCall = this.scheduleId
      ? this.auditplan.updateAuditPlan("api/scopeandschedule", postData)
      : this.auditplan.addAuditPlan("api/scopeandschedule", postData);

    if (this.scheduleId) {
      postData["id"] = this.scheduleId;
    }

    saveCall.subscribe(
      (response) => {
        this.notifyService.success("Audit Plan Saved Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  deleteAudit(auditId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.auditplan.deleteAuditPlan("api/audit/" + auditId).subscribe(
        (response) => {
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
        }
      );
    }
  }

  addAuditPlan() {
    this.handleFormView.show();
  }

  editAuditPlan(auditData) {
    this.id = auditData.id;
    this.processLocationMappingId = auditData.processLocationMapping.id;
    //this.auditProcess = auditData.overallAssesment.processL1.name;
    this.auditName = auditData.processLocationMapping.auditName;
    this.location = auditData.location.profitCenterCode;
    this.locationId = auditData.location.id;

    this.getResources();

    this.handleFormView.show();
  }

  getResources() {
    this.spinner.show();
    this.auditplan
      .getAuditPlan("api/scopeandschedule/GetByAudit/" + this.id)
      .subscribe(
        (auditSchedule) => {
          if (Array.isArray(auditSchedule) && auditSchedule.length) {
            this.resources = auditSchedule[0].auditResources;

            this.overallAuditStartDate = this.utils.formatToNgbDate(
              auditSchedule[0].auditStartDate
            );
            this.overallAuditEndDate = this.utils.formatToNgbDate(
              auditSchedule[0].auditEndDate
            );
            this.auditNumber = auditSchedule[0].auditNumber;
            this.scheduleId = auditSchedule[0].id;
            this.quarter = auditSchedule[0].quater;
            this.approvers =
              auditSchedule[0].auditApprovalMapping &&
              auditSchedule[0].auditApprovalMapping.userData
                ? auditSchedule[0].auditApprovalMapping.userData
                : [];
            this.auditees = auditSchedule[0].auditees;
          }
          this.spinner.hide();
          this.fillResourceTable();
          this.fillApproverTable();
          this.fillAuditeeTable();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getAuditSchedule() {
    let auditSchedule = {
      AuditId: this.id,
      LocationId: this.locationId,
      ProcessLocationMappingId: this.processLocationMappingId,
      AuditStartDate: this.utils.formatNgbDateToYMD(this.overallAuditStartDate),
      AuditEndDate: this.utils.formatNgbDateToYMD(this.overallAuditEndDate),
      AuditNumber: this.auditNumber,
      Quater: this.quarter,
      AuditResources: this.getAuditResources(),
      AuditApprovalMapping: this.getAuditApprovalMapping(),
      Auditees: this.getAuditees(),
    };
    return auditSchedule;
  }

  getAuditees() {
    let apObj = [];
    for (let userObj of this.auditees) {
      apObj.push({
        UserId: userObj.id ?this.getImplementionOwnerIds() : userObj.userId,
        ReportToId: userObj.id ? userObj.id : userObj.reportToId,
      });
    }
    return apObj;
  }

  getAuditApprovalMapping() {
    let apObj = {
      AuditId: this.id,
      UserData: [],
    };
    for (let userObj of this.approvers) {
      apObj.UserData.push({
        UserId: userObj.id ? userObj.id : userObj.userId,
        Responsibility: userObj.responsibility,
      });
    }
    return apObj;
  }

  getAuditResources() {
    let resourceArray = [];
    for (let resource of this.resources) {
      let resourceObj = {
        UserId: resource.user.id,
        AuditStartDate: this.utils.formatNgbDateToYMD(
          this.utils.formatToNgbDate(resource.auditStartDate)
        ),
        AuditEndDate: this.utils.formatNgbDateToYMD(
          this.utils.formatToNgbDate(resource.auditEndDate)
        ),
        ManDaysRequired: resource.manDaysRequired
          ? parseInt(resource.manDaysRequired)
          : null,
      };
      resourceArray.push(resourceObj);
    }
    return resourceArray;
  }

  fillResourceTable() {
    if (Array.isArray(this.resources) && this.resources.length) {
      $("#selectedResources tbody").html("");
      for (let resource of this.resources) {
        this.addResourceToTable(resource);
      }
    } else {
      $("#selectedResources tbody").html(
        '<tr class="norecords"><td colspan="9">No Records Found</td></tr>'
      );
    }
  }

  fillApproverTable() {
    if (Array.isArray(this.approvers) && this.approvers.length) {
      $("#selectedApprovers tbody").html("");
      for (let approver of this.approvers) {
        this.addApproverToTable(approver);
      }
    } else {
      $("#selectedApprovers tbody").html(
        '<tr class="norecords"><td colspan="6">No Records Found</td></tr>'
      );
    }
  }

  addResourceToTable(resource, isNew?: boolean) {
    let resourceTable = $("#selectedResources tbody");
    let noOfRecords = resourceTable.children("tr").length;
    let isInitRender = false;

    this.auditStartDate = resource.auditStartDate;
    this.auditEndDate = resource.auditEndDate;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#selectedResources tbody .norecords").length) {
        $("#selectedResources tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let resHtml = `<tr>;
          <td>${resource.user.firstName} ${resource.user.lastName}</td>
          <td>${resource.user.designation}</td>
          <td>${resource.user.experiance}</td>
          <td>${
            resource.user.qualification ? resource.user.qualification : ""
          }</td>
          <td>${resource.manDaysRequired}</td>
          <td>${this.utils.formatDbDateToDMY(this.auditStartDate)}</td>
          <td>${this.utils.formatDbDateToDMY(this.auditEndDate)}</td>
          <td>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editResource">
              <i class="fa fa-edit"></i></button>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeResource"> 
              <i class="fa fa-trash"></i></button>
          </td>
        </tr>`;

      resourceTable.append(resHtml);

      if (!isInitRender) {
        this.resources.push(resource);
      }
    } else {
      this.resources[this.resourceIndex] = resource;
      this.fillResourceTable();
    }
  }

  addApproverToTable(approverObj, isNew?: boolean) {
    let approverTable = $("#selectedApprovers tbody");
    let noOfRecords = approverTable.children("tr").length;
    let isInitRender = false;
    let approver = approverObj.user ? approverObj.user : approverObj;
    approver.responsibility = approverObj.responsibility
      ? approverObj.responsibility
      : "";
    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#selectedApprovers tbody .norecords").length) {
        $("#selectedApprovers tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let resHtml = `<tr>;
          <td>${approver.firstName} ${approver.lastName}</td>
          <td>${approver.responsibility}</td>
          <td>${approver.designation}</td>
          <td>${approver.experiance}</td>
          <td>${approver.qualification ? approver.qualification : ""}</td>
          <td>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editApprover hide">
              <i class="fa fa-edit"></i></button>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeApprover">
              <i class="fa fa-trash"></i></button>
          </td>
        </tr>`;

      approverTable.append(resHtml);

      if (!isInitRender) {
        this.approvers.push(approver);
      }
    } else {
      this.approvers[this.approverIndex] = approver;
      this.fillApproverTable();
    }
  }

  removeResource(resourceIndex) {
    if (this.resources[resourceIndex]) {
      this.resources.splice(resourceIndex, 1);
    }
    this.fillResourceTable();
  }

  removeApprover(appIndex) {
    if (this.approvers[appIndex]) {
      this.approvers.splice(appIndex, 1);
    }
    this.fillApproverTable();
  }

  editResource(resourceIndex) {
    let resourceData = this.resources[resourceIndex];
    this.clearResourceForm();
    this.fillResourceEdit(resourceData);
  }

  editApprover(appIndex) {
    let appData = this.approvers[appIndex];
    this.clearApproverForm();
    this.fillApproverEdit(appData);
  }

  addNewResource() {
    this.resourceIndex = -1;
    this.isEditResource = false;
    this.clearResourceForm();
    this.showResourceForm();
  }

  addNewAprrover() {
    this.approverIndex = -1;
    this.isEditApprover = false;
    this.clearApproverForm();
    this.showApproverForm();
  }

  getAllScheduleDetails(resource) {
    let resourceSelected = {
      user: resource.user ? resource.user : resource,
      manDaysRequired: $("#manDaysRequired").val().toString(),
      auditStartDate: $("#auditStartDate").val().toString(),
      auditEndDate: $("#auditEndDate").val().toString(),
    };
    return resourceSelected;
  }

  addResource() {
    if (this.ResourceForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedResource)) {
      let resourceSelected = this.getAllScheduleDetails(this.selectedResource);
      this.addResourceToTable(resourceSelected, true);
      this.selectedResource = {};
    }
    this.hideResourceForm();
  }

  addApprover() {
    if (this.ApproverForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appoverSelected = this.selectedApprover;
      appoverSelected.responsibility = this.appResponsibility;
      this.addApproverToTable(appoverSelected, true);
      this.selectedApprover = {};
    }
    this.hideApproverForm();
  }

  saveResource() {
    if (this.ResourceForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedResource)) {
      let resourceSelected = this.getAllScheduleDetails(this.selectedResource);
      this.addResourceToTable(resourceSelected, false);
      this.selectedResource = {};
    }
    this.hideResourceForm();
  }

  saveApprover() {
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appSelected = this.selectedApprover;
      this.addApproverToTable(appSelected, false);
      this.selectedApprover = {};
    }
  }

  showResourceForm() {
    window["jQuery"]("#manageResource").modal("show");
  }
  hideResourceForm() {
    window["jQuery"]("#manageResource").modal("hide");
  }
  showApproverForm() {
    window["jQuery"]("#manageApprover").modal("show");
  }

  hideApproverForm() {
    window["jQuery"]("#manageApprover").modal("hide");
  }
  showAuditeeForm() {
    window["jQuery"]("#manageAuditee").modal("show");
  }
  hideAuditeeForm() {
    window["jQuery"]("#manageAuditee").modal("hide");
  }

  fillUserOptions() {
    this.spinner.show();
    this.commonApi.getUsers().subscribe(
      (posts) => {
        this.userOpts = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  fillStackHolders() {
    this.spinner.show();
    this.commonApi.getStakeholders().subscribe(
      (posts) => {
        this.stackHolderOptions = posts;
        this.stackHolderOptions.forEach((user) => {
          user["custom"] = user.firstName + " " + user.lastName;
        });
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  fillResourceEdit(resourceData) {
    this.selectedResource = resourceData;
    this.resource = resourceData.user.id;
    this.designation = resourceData.user.designation;
    this.qualification = resourceData.user.qualification;
    this.experience = resourceData.user.experiance;
    this.manDaysRequired = resourceData.manDaysRequired;

    this.auditStartDate = this.utils.formatToNgbDate(
      resourceData.auditStartDate
    );
    this.auditEndDate = this.utils.formatToNgbDate(resourceData.auditEndDate);
    this.showResourceForm();
  }

  fillApproverEdit(approverData) {
    this.selectedApprover = approverData;
    this.approver = approverData.id;
    this.appDesignation = approverData.designation;
    this.appQualification = approverData.qualification;
    this.appExperience = approverData.experiance;
    this.showApproverForm();
  }

  resourceChange(e) {
    let resourceIndex = $(e.target)
      .children("option:selected")
      .attr("data-index");
    let resourceData = this.userOpts[resourceIndex];
    if (resourceData) {
      this.selectedResource = resourceData;
      this.designation = resourceData.designation;
      this.qualification = resourceData.qualification;
      this.experience = resourceData.experiance;
    } else {
      this.designation = "";
      this.qualification = "";
      this.experience = "";
    }
  }

  approverChange(e) {
    let approverIndex = $(e.target)
      .children("option:selected")
      .attr("data-index");
    let approverData = this.userOpts[approverIndex];
    if (approverData) {
      this.selectedApprover = approverData;
      this.appDesignation = approverData.designation;
      this.appQualification = approverData.qualification;
      this.appExperience = approverData.experiance;
    } else {
      this.appDesignation = "";
      this.appQualification = "";
      this.appExperience = "";
      this.appResponsibility = "";
    }
  }

  clearform() {
    this.id = "";
    this.auditProcess = "";
    this.auditNumber = "";
    this.location = "";
    this.locationId = "";
    this.overallAuditStartDate = null;
    this.overallAuditEndDate = null;
    this.quarter = "";
    this.clearResourceForm();
    this.clearApproverForm();
    this.clearAuditeeForm();
    this.approvers = [];
    this.resources = [];
    this.resourceIndex = -1;
    this.approverIndex = -1;
  }

  clearResourceForm() {
    this.designation = "";
    this.qualification = "";
    this.experience = "";
    this.resource = "";
    this.manDaysRequired = "";
    this.auditStartDate = null;
    this.auditEndDate = null;
  }

  clearApproverForm() {
    this.approver = "";
    this.appDesignation = "";
    this.appQualification = "";
    this.appExperience = "";
    this.appResponsibility = "";
  }

  exportAuditPlans() {
    this.spinner.show();
    this.auditplan
      .exportToExcel("api/ScopeAndSchedule/DownloadExcel")
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "Audit Plan.xlsx");
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

  addNewAuditee() {
    this.auditeeIndex = -1;
    this.isEditAuditee = false;
    this.clearAuditeeForm();
    this.showAuditeeForm();
  }

  clearAuditeeForm() {
    this.auditee = "";
    this.auditeeDesignation = "";
    this.auditeeQualification = "";
    this.auditeeExperience = "";
    this.auditeeEmailId = "";
    this.auditeeReportTo = "";
    this.selectedImplementationOwnerId = [];
  }

  auditeeChange(e) {
    let auditeeIndex = e.id;

    if (auditeeIndex != null) {
      for (let auditeeData of this.stackHolderOptions) {
        if (auditeeIndex.indexOf(auditeeData.id) > -1) {
          if (auditeeData) {
            this.selectedAuditee = auditeeData;
            this.auditeeDesignation = auditeeData.designation;
            this.auditeeQualification = auditeeData.qualification;
            this.auditeeExperience = auditeeData.experiance;
            this.auditeeEmailId = auditeeData.emailId;
          } else {
            this.auditeeDesignation = "";
            this.auditeeQualification = "";
            this.auditeeExperience = "";
            this.auditeeEmailId = "";
          }
        }
      }
    }
  }

  reportToChange(e) {
    let auditeeIndex = $(e.target)
      .children("option:selected")
      .attr("data-index");
    let auditeeData = this.userOpts[auditeeIndex];

    if (auditeeData) {
      this.selectedReportTo = auditeeData;
      this.selectedAuditee.reportToUser = auditeeData;
      this.selectedAuditee.reportToId = auditeeData.id;
    }
  }

  getAuditeeInfo(auditee) {
    let auditeeSelected = {
      user: auditee.user ? auditee.user : auditee,
      userId: this.getImplementionOwnerIds(),
      reportToUser: auditee.reportToUser
        ? auditee.reportToUser
        : this.selectedReportTo,
      reportToId: this.auditeeReportTo,
    };

    return auditeeSelected;
  }

  addAuditee() {
    if (this.AuditeeForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedAuditee)) {
      let auditeeSelected = this.getAuditeeInfo(this.selectedAuditee);
      // let auditeeSelected = this.selectedAuditee;
      this.addAuditeeToTable(auditeeSelected, true);
      this.selectedAuditee = {};
    }
    this.hideAuditeeForm();
  }

  addAuditeeToTable(auditeeObj, isNew?: boolean) {
    let auditeeTable = $("#selectedAuditees tbody");
    let noOfRecords = auditeeTable.children("tr").length;
    let isInitRender = false;

    let auditee = auditeeObj.user ? auditeeObj.user : auditeeObj;
    let reportTo = auditeeObj.reportToUser
      ? auditeeObj.reportToUser
      : this.selectedReportTo;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }
    if (isNew) {
      if ($("#selectedAuditees tbody .norecords").length) {
        $("#selectedAuditees tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let resHtml = `<tr>;
        <td>${auditee.firstName} ${auditee.lastName}</td>
        <td>${auditee.emailId}</td>
        <td>${auditee.experiance}</td>
        <td>${auditee.qualification ? auditee.qualification : ""}</td>
        <td>${auditee.designation}</td>
        <td>${reportTo.firstName} ${reportTo.lastName}</td>
        <td>
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editAuditee">
            <i class="fa fa-edit"></i></button>
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeAuditee">
            <i class="fa fa-trash"></i></button>
        </td>
      </tr>`;

      auditeeTable.append(resHtml);

      if (!isInitRender) {
        this.auditees.push(auditeeObj);
      }
    } else {
      this.auditees[this.auditeeIndex] = auditeeObj;
      this.fillAuditeeTable();
    }
  }

  fillAuditeeTable() {
    if (Array.isArray(this.auditees) && this.auditees.length) {
      $("#selectedAuditees tbody").html("");

      for (let auditee of this.auditees) {
        this.addAuditeeToTable(auditee);
      }
    } else {
      $("#selectedAuditees tbody").html(
        '<tr class="norecords"><td colspan="7">No Records Found</td></tr>'
      );
    }
  }

  editAuditee(appIndex) {
    let appData = this.auditees[appIndex];

    this.clearAuditeeForm();
    this.fillAuditeeEdit(appData);
  }

  fillAuditeeEdit(auditeeData) {
    this.selectedAuditee = auditeeData;

    this.auditee = auditeeData.user.id;
    this.auditeeDesignation = auditeeData.user.designation;
    this.auditeeQualification = auditeeData.user.qualification;
    this.auditeeExperience = auditeeData.user.experiance;
    this.auditeeEmailId = auditeeData.user.emailId;
    this.auditeeReportTo = auditeeData.reportToId;
    this.selectedImplementationOwnerId = this.getSelectedImplementationOwnerOpts(auditeeData.userId);
     this.selectedReportTo = auditeeData.reportToUser;
    this.showAuditeeForm();
  }

  saveAuditee() {
    if (this.AuditeeForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedAuditee)) {
      let auditeeSelected = this.getAuditeeInfo(this.selectedAuditee);
      this.addAuditeeToTable(auditeeSelected, false);
      this.selectedAuditee = {};
    }
    this.hideAuditeeForm();
  }

  removeAuditee(appIndex) {
    if (this.auditees[appIndex]) {
      this.auditees.splice(appIndex, 1);
    }
    this.fillAuditeeTable();
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "auditplanningengine",
      "auditplan"
    );
  }
  exportToPDF() {
    this.spinner.show();
    this.auditplan
      .exportToExcel("api/scopeandschedule/downloadpdfscopeandschedule")
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/pdf",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "Audit Plan.pdf");
            // link.setAttribute("download", "DiscussionNotes.pptx");
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
  exportToPPT() {
    this.spinner.show();
    try {
      this.auditplan
        .exportToExcel("api/scopeandschedule/downloadppt")
        .subscribe((blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/ppt",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "Audit Plan.pptx");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
          }
          this.spinner.hide();
        });
    } catch (error) {
      this.spinner.hide();
      console.error("API GetDashboard: ", error);
    }
  }
  showMailForm() {
    window["jQuery"]("#sendmailForm").modal("show");
  }

  hideMailForm() {
    window["jQuery"]("#sendmailForm").modal("hide");
  }
  openSendForm() {
    if (this.selectedIds.length == 0) {
      this.notifyService.error(
        "Please select at least one record to send email."
      );
      return false;
    }
    this.showMailForm();
  }
  sendMail(e) {
    if (this.apForm.invalid) {
      return false;
    }
    e.preventDefault();
    if (this.singleMail) {
      this.singleSendMail();
    } else {
      this.sendMultipleEmail();
    }
  }
  singleSendMail() {
    let apform = this.apForm.form.value;
    var lst: any = [];
    lst.push(this.email_id);
    let postData = {
      Id: lst,
      Email: apform.Email,
    };
    this.spinner.show();
    this.auditplan.addAuditPlan("api/audit/sendemail", postData).subscribe(
      (response) => {
        let result: any = response;
        this.spinner.hide();
        if (result.sent)
          this.notifyService.success("AuditPlan Data mail sent successfully.");
        this.hideMailForm();
        this.Email = "";
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  sendMultipleEmail() {
    let apform = this.apForm.form.value;
    var lst: any = [];
    this.selectedIds.forEach((element, index) => {
      lst.push(element);
    });
    let postData = {
      Id: lst,
      Email: apform.Email,
    };
    this.spinner.show();
    this.auditplan.addAuditPlan("api/audit/sendemail", postData).subscribe(
      (response) => {
        this.spinner.hide();
        this.notifyService.success("AuditPlan Data mail sent successfully.");
        this.hideMailForm();
        this.Email = "";
      },
      (error) => {
        this.spinner.hide();
        this.notifyService.error(error);
      }
    );
    this.tableFilters.next({ init: true });
    this.selectedIds = [];
  }
  getImplementionOwnerIds() {
    var implementationOwner = "";
    this.selectedImplementationOwnerId.forEach((element) => {
      implementationOwner = element.id;
    });
    return implementationOwner;
  }
  getSelectedImplementationOwnerOpts(OwnerArray) {
    let ImplementationOwnerArray = [];
    if (OwnerArray != null) {
      for (let i of this.stackHolderOptions) {
        if (OwnerArray.indexOf(i.id) > -1) {
          ImplementationOwnerArray.push({
            id: i.id,
            custom: i.firstName + " " + i.lastName,
          });
        }
      }
    }
    return ImplementationOwnerArray;
  }
  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.fillUserOptions();
    this.fillStackHolders();

    $(document).ready(() => {
      $("#apComponent").on("click", ".editAuditPlan", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let auditData = $("#" + dataId).data();

        this.editAuditPlan(auditData);
      });

      $("#apComponent").on("click", ".deleteAudit", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.deleteAudit(dataId);
      });

      $("#apComponent").on("click", ".removeResource", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.removeResource(parseInt(dataIndex));
        }
      });

      $("#apComponent").on("click", ".editResource", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.resourceIndex = dataIndex;
          this.isEditResource = true;
          this.editResource(parseInt(dataIndex));
        }
      });

      $("#apComponent").on("click", ".removeApprover", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeApprover(parseInt(dataIndex));
        }
      });

      $("#apComponent").on("click", ".editApprover", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.approverIndex = dataIndex;
          this.isEditApprover = true;
          this.editApprover(parseInt(dataIndex));
        }
      });

      $("#apComponent").on("click", ".editAuditee", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.auditeeIndex = dataIndex;
          this.isEditAuditee = true;

          this.editAuditee(parseInt(dataIndex));
        }
      });

      $("#apComponent").on("click", ".removeAuditee", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeAuditee(parseInt(dataIndex));
        }
      });
      $("#apComponent").on("click", ".emailAuditPlan", (e) => {
        this.showMailForm();
        let dataId = $(e.currentTarget).attr("data-id");
        this.email_id = dataId;
        this.singleMail = true;
      });
      $("#apComponent").on("change", "#chkAllDataTracker", (e) => {
        $("#audit_table > tbody > tr")
          .find(".chkSingleDataTracker")
          .prop("checked", $(e.currentTarget).is(":checked"));

        let Ids: any = [];
        $("#audit_table > tbody > tr").each(function () {
          let row = $(this);
          Ids.push(row.attr("id"));
        });

        if ($(e.currentTarget).is(":checked")) this.selectedIds = Ids;
        else this.selectedIds = [];
      });

      $("#apComponent").on("change", ".chkSingleDataTracker", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        if ($(e.currentTarget).is(":checked")) this.selectedIds.push(dataId);
        else {
          this.selectedIds.forEach((element, index) => {
            if (element == dataId) delete this.selectedIds[index];
          });
        }

        if (
          $("#audit_table > tbody > tr").find(".chkSingleDataTracker").length ==
          $("#audit_table > tbody > tr").find(".chkSingleDataTracker:checked")
            .length
        )
          $("#audit_table > thead > tr")
            .find("#chkAllDataTracker")
            .prop("checked", true);
        else
          $("#audit_table > thead > tr")
            .find("#chkAllDataTracker")
            .prop("checked", false);
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
