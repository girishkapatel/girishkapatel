import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "../../../../common/table/table.model";
import { AuditPlanService } from "../../../master/auditplans/auditplans.service";
import { BehaviorSubject } from "rxjs";
import { UtilsService } from "../../../../services/utils/utils.service";
import { CommonApiService } from "../../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-unaplannedAudit",
  templateUrl: "./unplanned.component.html",

  providers: [AuditPlanService],
})
export class UnplannedAuditComponent implements OnInit {
  approverIndex: number;
  selectedApprover: any = {};
  approvers: any = [];
  isEditApprover: boolean = false;
  approver: string;
  appDesignation: string;
  appQualification: string;
  appExperience: string;
  appResponsibility: string;

  constructor(
    private auditplan: AuditPlanService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService
  ) {}

  userOpts: any = [];
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
  overallAuditStartDate: string = "";
  overallAuditEndDate: string = "";
  designation: string = "";
  qualification: string = "";
  experience: string = "";
  auditStartDate: string = "";
  auditEndDate: string = "";
  manDaysRequired: string = "";
  quarter: string = "";
  auditNumber: string = "";
  status: string = "";

  tableScroll: boolean = false;
  tableId: string = "audit_table";
  tableColumns: tableColumn[] = [
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
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" class="btn btn-sm btn-primary editAuditPlan" title="Schedule Audit"><i class="fa fa-calendar"></i></button>' +
          '<button type="button" data-id="' +
          data +
          '" class="btn btn-sm btn-danger deleteAudit" title="Delete record"><i class="fa fa-trash"></i></button>'
        );
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
        this.notifyService.success("Unplanned audit Saved Successfully");
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
    //this.auditProcess = auditData.overallAssesment.processL1.name;
    this.auditName = auditData.processLocationMapping.auditName;
    this.location = auditData.location.profitCenterCode;
    this.locationId = auditData.location.id;
    this.getResources();
    this.handleFormView.show();
  }

  getResources() {
    this.auditplan
      .getAuditPlan("api/scopeandschedule/GetByAudit/" + this.id)
      .subscribe((auditSchedule) => {
        if (Array.isArray(auditSchedule) && auditSchedule.length) {
          this.resources = auditSchedule[0].auditResources;
          this.overallAuditStartDate = auditSchedule[0].auditStartDate;
          this.overallAuditEndDate = auditSchedule[0].auditEndDate;
          this.auditNumber = auditSchedule[0].auditNumber;
          this.scheduleId = auditSchedule[0].id;
          this.quarter = auditSchedule[0].quater;
          this.approvers =
            auditSchedule[0].auditApprovalMapping &&
            auditSchedule[0].auditApprovalMapping.userData
              ? auditSchedule[0].auditApprovalMapping.userData
              : [];
        }
        this.fillResourceTable();
        this.fillApproverTable();
      });
  }

  getAuditSchedule() {
    let auditSchedule = {
      AuditId: this.id,
      LocationId: this.locationId,
      AuditStartDate: this.overallAuditStartDate,
      AuditEndDate: this.overallAuditEndDate,
      AuditNumber: this.auditNumber,
      Quater: this.quarter,
      AuditResources: this.getAuditResources(),
      AuditApprovalMapping: this.getAuditApprovalMapping(),
      Status: "unplanned",
    };
    return auditSchedule;
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
        AuditStartDate: resource.auditStartDate,
        AuditEndDate: resource.auditEndDate,
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
          <td>${this.utils.formatDate(resource.auditStartDate)}</td>
          <td>${this.utils.formatDate(resource.auditEndDate)}</td>
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
      auditStartDate: $("#auditEndDate").val().toString(),
      auditEndDate: $("#auditEndDate").val().toString(),
    };
    return resourceSelected;
  }

  addResource() {
    if (!this.utils.isEmptyObj(this.selectedResource)) {
      let resourceSelected = this.getAllScheduleDetails(this.selectedResource);
      this.addResourceToTable(resourceSelected, true);
      this.selectedResource = {};
    }
  }

  addApprover() {
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appoverSelected = this.selectedApprover;
      appoverSelected.responsibility = this.appResponsibility;
      this.addApproverToTable(appoverSelected, true);
      this.selectedApprover = {};
    }
  }

  saveResource() {
    if (!this.utils.isEmptyObj(this.selectedResource)) {
      let resourceSelected = this.getAllScheduleDetails(this.selectedResource);
      this.addResourceToTable(resourceSelected, false);
      this.selectedResource = {};
    }
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

  showApproverForm() {
    window["jQuery"]("#manageApprover").modal("show");
  }

  fillUserOptions() {
    this.commonApi.getUsers().subscribe((posts) => {
      this.userOpts = posts;
    });
  }

  fillResourceEdit(resourceData) {
    this.selectedResource = resourceData;
    this.resource = resourceData.user.id;
    this.designation = resourceData.user.designation;
    this.qualification = resourceData.user.qualification;
    this.experience = resourceData.user.experiance;
    this.manDaysRequired = resourceData.manDaysRequired;
    this.auditStartDate = this.utils.formatDate(resourceData.auditStartDate);
    this.auditEndDate = this.utils.formatDate(resourceData.auditEndDate);

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
    this.overallAuditStartDate = "";
    this.overallAuditEndDate = "";
    this.quarter = "";
    this.clearResourceForm();
    this.clearApproverForm();
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
    this.auditStartDate = "";
    this.auditEndDate = "";
  }

  clearApproverForm() {
    this.approver = "";
    this.appDesignation = "";
    this.appQualification = "";
    this.appExperience = "";
  }

  ngOnInit() {
    this.fillUserOptions();
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
    });
  }
}
