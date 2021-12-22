import { fn } from "@angular/compiler/src/output/output_ast";
import { Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import { UtilsService } from "../../../services/utils/utils.service";
import { AuditPlanService } from "../../master/auditplans/auditplans.service";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { NgForm } from "@angular/forms";
@Component({
  selector: "app-unplannedaudit",
  templateUrl: "unplannedaudit-add.component.html",
  styles: [
    ".myClass {font-weight: bold;}.red-border-class {border: 1px solid red;}",
  ],
  providers: [AuditPlanService],
})
export class UnplannedAuditAddComponent implements OnInit {
  constructor(
    private router: Router,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private auditplan: AuditPlanService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("fileInput", { static: false }) fileInput;
  @ViewChild("resourceForm", { static: false }) resourceForm: NgForm;
  @ViewChild("ApproverForm", { static: false }) ApproverForm: NgForm;
  @ViewChild("AuditeeForm", { static: false }) AuditeeForm: NgForm;
  @ViewChild("auditPlanForm", { static: false }) auditPlanForm: NgForm;
  //Locations variables
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

  locationOptions: any = [];

  //Approvers variables
  approverIndex: number;

  selectedApprover: any = {};

  approvers: any = [];
  userOpts: any = [];

  isEditApprover: boolean = false;

  approver: string;
  appDesignation: string;
  appQualification: string;
  appExperience: string;
  appResponsibility: string;

  //Resources variables
  resourceIndex: number = -1;

  selectedResource: any = {};

  resources: any = [];

  isEditResource: boolean = false;

  resource: string = "";

  uploadedFiles: any = [];
  selectedFiles: any = [];

  isEdit: boolean = false;

  uploadedFileIndex: number;

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
  stackHolderOptions: any = [];

  getResources() {
    if (this.id != null) {
      this.auditplan
        .getAuditPlan("api/scopeandschedule/GetByAudit/" + this.id)
        .subscribe((auditSchedule) => {
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
            this.auditName = auditSchedule[0].audit.auditName;
            this.location = auditSchedule[0].location.profitCenterCode;
            this.locationId = auditSchedule[0].locationId;
            this.approvers =
              auditSchedule[0].auditApprovalMapping &&
              auditSchedule[0].auditApprovalMapping.userData
                ? auditSchedule[0].auditApprovalMapping.userData
                : [];
            this.auditees = auditSchedule[0].auditees;
          }

          if (this.resources !== null && this.resources.length > 0) {
            this.resources.forEach((element) => {
              element.auditStartDate = this.utils.formatToNgbDate(
                element.auditStartDate
              );
              element.auditEndDate = this.utils.formatToNgbDate(
                element.auditEndDate
              );
            });
          }

          this.fillResourceTable();
          this.fillApproverTable();
          this.fillAuditeeTable();

          if (this.isEdit) this.getUploadedFiles();
        });
    }
  }

  getLocations() {
    this.commonApi.getLocations().subscribe((posts) => {
      this.locationOptions = posts;
    });
  }

  fillUserOptions() {
    this.commonApi.getUsers().subscribe((posts) => {
      this.userOpts = posts;
    });
  }

  addNewAprrover() {
    this.approverIndex = -1;
    this.isEditApprover = false;
    this.clearApproverForm();
    this.showApproverForm();
  }

  showApproverForm() {
    window["jQuery"]("#manageApprover").modal("show");
  }
  hideApproverForm() {
    window["jQuery"]("#manageApprover").modal("hide");
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

  addApprover() {
    if (this.ApproverForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appoverSelected = this.selectedApprover;
      appoverSelected.responsibility = this.appResponsibility;
      if (this.isEditApprover) this.addApproverToTable(appoverSelected, false);
      else this.addApproverToTable(appoverSelected, true);
      this.selectedApprover = {};
      this.hideApproverForm();
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

  editApprover(appIndex) {
    let appData = this.approvers[appIndex];
    this.clearApproverForm();
    this.fillApproverEdit(appData);
  }

  fillApproverEdit(approverData) {
    this.selectedApprover = approverData;
    this.approver = approverData.id;
    this.appDesignation = approverData.designation;
    this.appQualification = approverData.qualification;
    this.appExperience = approverData.experiance;
    this.showApproverForm();
  }

  saveApprover() {
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appSelected = this.selectedApprover;
      this.addApproverToTable(appSelected, false);
      this.selectedApprover = {};
    }
  }

  clearApproverForm() {
    this.approver = "";
    this.appDesignation = "";
    this.appQualification = "";
    this.appExperience = "";
    this.appResponsibility = "";
  }

  removeApprover(appIndex) {
    if (this.approvers[appIndex]) {
      this.approvers.splice(appIndex, 1);
    }
    this.fillApproverTable();
  }

  addNewResource() {
    this.resourceIndex = -1;
    this.isEditResource = false;
    this.clearResourceForm();
    this.showResourceForm();
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

  addResource() {
    if (this.resourceForm.invalid) {
      return false;
    }
    if (!this.utils.isEmptyObj(this.selectedResource)) {
      let resourceSelected = this.getAllScheduleDetails(this.selectedResource);
      if (this.isEditResource) this.addResourceToTable(resourceSelected, false);
      else this.addResourceToTable(resourceSelected, true);

      this.selectedResource = {};
      this.hideResourceForm();
    }
  }
  // saveResource() {
  //   if (this.resourceForm.invalid) {
  //     return false;
  //   }
  //   if (!this.utils.isEmptyObj(this.selectedResource)) {
  //     let resourceSelected = this.getAllScheduleDetails(this.selectedResource);

  //     this.selectedResource = {};
  //   }
  // }

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
          <td>${this.utils.formatNgbDateToDMY(resource.auditStartDate)}</td>
          <td>${this.utils.formatNgbDateToDMY(resource.auditEndDate)}</td>
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

  getAllScheduleDetails(resource) {
    let resourceSelected = {
      user: resource.user ? resource.user : resource,
      manDaysRequired: $("#manDaysRequired").val().toString(),
      auditStartDate: this.auditStartDate,
      auditEndDate: this.auditEndDate,
    };

    return resourceSelected;
  }

  editResource(resourceIndex) {
    this.isEditResource = true;
    let resourceData = this.resources[resourceIndex];
    this.clearResourceForm();
    this.fillResourceEdit(resourceData);
  }

  fillResourceEdit(resourceData) {
    this.selectedResource = resourceData;
    this.resource = resourceData.user.id;
    this.designation = resourceData.user.designation;
    this.qualification = resourceData.user.qualification;
    this.experience = resourceData.user.experiance;
    this.manDaysRequired = resourceData.manDaysRequired;
    this.auditStartDate = resourceData.auditStartDate;
    this.auditEndDate = resourceData.auditEndDate;
    this.showResourceForm();
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

  removeResource(resourceIndex) {
    if (this.resources[resourceIndex]) {
      this.resources.splice(resourceIndex, 1);
    }
    this.fillResourceTable();
  }

  showResourceForm() {
    window["jQuery"]("#manageResource").modal("show");
  }

  hideResourceForm() {
    window["jQuery"]("#manageResource").modal("hide");
  }

  getAuditResources() {
    let resourceArray = [];

    for (let resource of this.resources) {
      let resourceObj = {
        UserId: resource.user.id,
        AuditStartDate: this.utils.formatNgbDateToYMD(resource.auditStartDate),
        AuditEndDate: this.utils.formatNgbDateToYMD(resource.auditEndDate),
        ManDaysRequired: resource.manDaysRequired
          ? parseInt(resource.manDaysRequired)
          : null,
      };
      resourceArray.push(resourceObj);
    }
    return resourceArray;
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

  getAuditSchedule() {
    let auditSchedule = {
      AuditId: this.id,
      AuditName: this.auditName,
      LocationId: this.locationId,
      AuditStartDate: this.utils.formatNgbDateToYMD(this.overallAuditStartDate),
      AuditEndDate: this.utils.formatNgbDateToYMD(this.overallAuditEndDate),
      AuditNumber: this.auditNumber,
      Quater: this.quarter,
      AuditResources: this.getAuditResources(),
      AuditApprovalMapping: this.getAuditApprovalMapping(),
      Auditees: this.getAuditees(),
      Status: "unplanned",
    };
    return auditSchedule;
  }

  getAuditees() {
    let apObj = [];
    for (let userObj of this.auditees) {
      apObj.push({
        UserId: userObj.id ? userObj.id : userObj.userId,
        ReportToId: userObj.id ? userObj.id : userObj.reportToId,
      });
    }
    return apObj;
  }

  locationChange(e) {
    this.location = $(e.target).children("option:selected").text().toString();
    this.locationId = $(e.target).children("option:selected").val().toString();
  }

  uploadFile() {
    let filesElem = this.fileInput.nativeElement;

    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        this.selectedFiles.push(file);

        let fileModel = {
          originalFileName: file.name,
          uploadedDatetime: this.utils.getCurrentDate(),
        };

        this.addUploadedFileToTable(fileModel, true);
      }
    }
    this.fileInput.nativeElement.value = "";
  }

  addUploadedFileToTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#uploadedFiles tbody");
    let noOfRecords = uploadedFilesTable.children("tr").length;
    let isInitRender = false;
    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#uploadedFiles tbody .norecords").length) {
        $("#uploadedFiles tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let fileName = `<a href="javascript:void(0);" class="downloadFile" data-index="${noOfRecords}">${uploadFileObj.originalFileName}</a>`;
      if (typeof uploadFileObj.uploadedFileName === "undefined")
        fileName = uploadFileObj.originalFileName;

      let resHtml = `<tr>
          <td>${fileName}</td>
          <td>${this.utils.formatDate(uploadFileObj.uploadedDatetime)}</td>
          <td>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeFile">
              <i class="fa fa-trash"></i></button>
          </td>
        </tr>`;

      uploadedFilesTable.append(resHtml);

      if (!isInitRender) {
        this.uploadedFiles.push(uploadFileObj);
      }
    } else {
      this.uploadedFiles[this.uploadedFileIndex] = uploadFileObj;
      this.fillUploadedFilesTable();
    }
  }

  fillUploadedFilesTable() {
    if (Array.isArray(this.uploadedFiles) && this.uploadedFiles.length) {
      $("#uploadedFiles tbody").html("");

      for (let file of this.uploadedFiles) {
        this.addUploadedFileToTable(file);
      }
    } else {
      $("#uploadedFiles tbody").html(
        '<tr class="norecords"><td colspan="3">No Records Found</td></tr>'
      );
    }
  }

  removeUploadedFile(appIndex) {
    let fileId = this.uploadedFiles[appIndex].id;

    if (this.uploadedFiles[appIndex]) {
      this.uploadedFiles.splice(appIndex, 1);
      this.selectedFiles.splice(appIndex, 1);

      if (typeof fileId === "undefined") {
        this.fillUploadedFilesTable();
      } else {
        this.auditplan
          .removeUploadedFile("api/audit/removefile/" + fileId)
          .subscribe(
            (upload) => {
              if (upload.isDeleted) this.fillUploadedFilesTable();
            },
            (error) => {
              console.log(error);
            }
          );
      }
    }
  }
  saveAuditSchedule(e) {
    e.preventDefault();
    if (this.auditPlanForm.invalid) {
      return false;
    }
    if (
      this.getAuditResources().length <= 0 &&
      this.getAuditApprovalMapping().UserData.length <= 0
    ){
      this.notifyService.error("please Fill Approvers,Resources and Auditees")
      return false;}
    let auditData = {
      Id: this.id,
      LocationId: this.locationId,
      AuditName: this.auditName,
    };
    this.auditplan
      .addAuditPlan("api/audit/saveunplannedaudit", auditData)
      .subscribe((posts) => {
        this.id = posts[0].id;

        let postData = this.getAuditSchedule();

        let saveCall = this.scheduleId
          ? this.auditplan.updateAuditPlan("api/scopeandschedule", postData)
          : this.auditplan.addAuditPlan("api/scopeandschedule", postData);

        if (this.scheduleId) {
          postData["id"] = this.scheduleId;
        }

        saveCall.subscribe(
          (response) => {
            this.saveSelectedFiles();
            this.notifyService.success("Audit Plan Saved Successfully");
            this.backToAuditForm();
          },
          (error) => {
            console.log(error);
          }
        );
      });
  }

  backToAuditForm() {
    localStorage.removeItem("unplannedaudit");
    this.router.navigate(["./pages/manageaudits"]);
  }

  getUploadedFiles() {
    this.uploadedFileIndex = -1;

    this.auditplan.getAuditPlan("api/audit/getallfiles/" + this.id).subscribe(
      (posts) => {
        this.uploadedFiles = posts;
        this.fillUploadedFilesTable();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  saveSelectedFiles() {
    var formData = new FormData();

    for (var file in this.selectedFiles) {
      formData.append("files", this.selectedFiles[file]);
    }

    formData.append("auditId", this.id);
    formData.append("module", "unplannedaudits");

    this.auditplan.uploadFile("api/audit/uploadfile", formData).subscribe(
      (upload) => {
        if (upload.isUploaded) {
          for (let file of upload.files) {
            this.addUploadedFileToTable(file, true);
          }
        }
      },
      (error) => {
        console.log(error);
      }
    );
  }

  downloadFile(appIndex) {
    let fileId = this.uploadedFiles[appIndex].id;

    if (this.uploadedFiles[appIndex]) {
      if (typeof fileId === "undefined")
        this.notifyService.error(
          "Internal server error. Please reload the page and try again."
        );
      else {
        this.auditplan
          .downloadFile("api/audit/downloadfile/" + fileId)
          .subscribe(
            (blob) => {
              let link = document.createElement("a");

              if (link.download !== undefined) {
                let url = URL.createObjectURL(blob);
                link.setAttribute("href", url);
                link.setAttribute(
                  "download",
                  this.uploadedFiles[appIndex].originalFileName
                );
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
              }
            },
            (error) => {
              console.log(error);
            }
          );
      }
    }
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
  }

  auditeeChange(e) {
    let auditeeIndex = $(e.target)
      .children("option:selected")
      .attr("data-index");
    let auditeeData = this.stackHolderOptions[auditeeIndex];
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
      userId: this.auditee,
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
      if (this.isEditAuditee) this.addAuditeeToTable(auditeeSelected, false);
      else this.addAuditeeToTable(auditeeSelected, true);
      this.selectedAuditee = {};
      this.hideAuditeeForm();
    }
  }

  addAuditeeToTable(auditeeObj, isNew?: boolean) {
    let auditeeTable = $("#selectedAuditees tbody");
    let noOfRecords = auditeeTable.children("tr").length;
    let isInitRender = false;
    let auditee = auditeeObj.user ? auditeeObj.user : auditeeObj;
    let reportTo = auditeeObj.reportToUser
      ? auditeeObj.reportToUser
      : this.selectedReportTo;

    // approver.responsibility = auditeeObj.responsibility
    // ? auditeeObj.responsibility
    // : "";
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
    this.isEditAuditee = true;
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

    this.showAuditeeForm();
  }
  saveAuditee() {
    if (!this.utils.isEmptyObj(this.selectedAuditee)) {
      let auditeeSelected = this.getAuditeeInfo(this.selectedAuditee);
      this.addAuditeeToTable(auditeeSelected, false);
      this.selectedAuditee = {};
    }
  }

  removeAuditee(appIndex) {
    if (this.auditees[appIndex]) {
      this.auditees.splice(appIndex, 1);
    }
    this.fillAuditeeTable();
  }

  fillStackHolders() {
    this.commonApi.getStakeholders().subscribe((posts) => {
      this.stackHolderOptions = posts;
    });
  }

  showAuditeeForm() {
    window["jQuery"]("#manageAuditee").modal("show");
  }
  hideAuditeeForm() {
    window["jQuery"]("#manageAuditee").modal("hide");
  }

  ngOnInit() {
    this.id =
      localStorage.getItem("unplannedaudit") === ""
        ? null
        : localStorage.getItem("unplannedaudit");

    this.isEdit = this.id !== null && this.id !== "" ? true : false;

    if (this.isEdit) {
      this.selectedFiles = [];
      this.uploadedFiles = [];
    }

    this.getLocations();
    this.fillUserOptions();
    this.fillStackHolders();
    this.getResources();

    $("#approvers").on("click", ".editApprover", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.approverIndex = dataIndex;
        this.isEditApprover = true;
        this.editApprover(parseInt(dataIndex));
      }
    });

    $("#approvers").on("click", ".removeApprover", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.removeApprover(parseInt(dataIndex));
      }
    });

    $("#resources").on("click", ".editResource", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.resourceIndex = dataIndex;
        this.isEditResource = true;
        this.editResource(parseInt(dataIndex));
      }
    });

    $("#resources").on("click", ".removeResource", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.removeResource(parseInt(dataIndex));
      }
    });

    $("#uploadedFiles").on("click", ".removeFile", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.removeUploadedFile(parseInt(dataIndex));
      }
    });

    $("#uploadedFiles").on("click", ".downloadFile", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.downloadFile(parseInt(dataIndex));
      }
    });

    $("#auditees").on("click", ".editAuditee", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.auditeeIndex = dataIndex;
        this.isEditAuditee = true;
        this.editAuditee(parseInt(dataIndex));
      }
    });

    $("#auditees").on("click", ".removeAuditee", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.removeAuditee(parseInt(dataIndex));
      }
    });
  }
}
