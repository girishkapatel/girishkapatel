import { Component, OnInit, ViewEncapsulation, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { NgForm } from "@angular/forms";
import { Observable } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import { ApiService } from "src/app/services/api/api.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import * as ClassicEditor from "../../../../assets/ckeditor5/build/ckeditor";
import { IDropdownSettings } from "ng-multiselect-dropdown";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "selector",
  templateUrl: "action-add.component.html",
  styleUrls: ["action-add.component.css"],
  encapsulation: ViewEncapsulation.None,
})
export class ActionAddComponent implements OnInit {
  constructor(
    private router: Router,
    private api: ApiService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {
    const current = new Date();
      this.CurrentDate = {
        year: current.getFullYear(),
        month: current.getMonth() + 1,
        day: current.getDate()
      };
  }

  @ViewChild("actionForm", { static: false }) actionForm: NgForm;
  @ViewChild("apForm", { static: false }) apForm: NgForm;
  @ViewChild("fileInputAP", { static: false }) fileInputAP;

  dropdownSettings: IDropdownSettings = {};
  OwnerdropdownSettings: IDropdownSettings = {};
  selectedImplementationOwnerId = [];
  public Editor = ClassicEditor;
  CurrentDate:  {year: number, month: number, day: number};
  AuditExist: boolean = false;
  locationOpts: any = [];
  rootCauseOptions: any = [];
  selectedRootCauses: any = [];
  riskTypeOptions: any = [];

  businessCycleId: string = "";
  processlevel1Id: string = "";
  processlevel2Id: string = "";

  businessCycleOptions: any = [];
  processlevel1Options: any = [];
  processlevel2Options: any = [];

  shOpts: any = [];
  auditMaster: any = [];
  Id: string = "";
  AuditId: string = "";
  ObservationGrading: string = "";
  ObservationHeading: string = "";
  DetailedObservation: string = "";
  ImplementationEndDate: NgbDateStruct;
  Status: string = "";
  AuditeeStatus: string = "";
  RiskTypeId: string = "";
  ResponsibilityId: string = "";
  RootCause: string = "";

  processLocationMappingOptions: any = [];
  ProcessLocationMappingId: string = "";
  AuditNumber: string = "";
  AuditName: string = "";
  LocationID: string = "";
  ReviewQtr: string = "";
  ObsNumber: string = "";
  ResponsibilityDepartment: string = "";
  ImplementationRemarks: string = "";
  ManagementResponse: string = "";
  AgreedActionPlan: string = "";

  //Action plan fields
  apId: string = "";
  isEditAP: boolean = false;
  Comments: string = "";
  RevisedDate: NgbDateStruct;
  RevisedDates: NgbDateStruct;
  ActionPlan: string = "";
  ImplementationOwnerId: string = "";
  ImplementationOwner: string = "";
  ActionPlans: any = [];
  apIndex: number = -1;
  RevisionCount: number = 0;
  uploadedFiles: any = [];
  selectedFiles: any = [];
  uploadedFileIndex: number;

  formatter = (schedule: any) => {
    return schedule.auditNumber;
  };

  search = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map((term) =>
        term.length < 0
          ? []
          : this.auditMaster
              .filter(
                (v) =>
                  v.auditNumber.toLowerCase().indexOf(term.toLowerCase()) > -1
              )
              .slice(0, 10)
      )
    );

  selectedItem(selectedOpt) {
    let auditData = selectedOpt.item;
    // this.Company = auditData.location.profitCenterCode;
    // this.AuditStartDate = auditData.auditStartDate;
    // this.AuditEndDate = auditData.auditEndDate;
    //this.Quarter = auditData.quarter;
    this.AuditId = auditData.audit.id;
  }

  getAllAudit() {
    this.spinner.show();
    this.api.getData("api/scopeandschedule").subscribe(
      (schedule) => {
        this.auditMaster = schedule;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getRootCauseIds() {
    let rootCausesArray = [];
    this.selectedRootCauses.forEach((element) => {
      rootCausesArray.push(element.id);
    });
    return rootCausesArray;
  }
  saveAction() {
    if (this.actionForm.invalid) return false;

    let actionData = this.actionForm.form.value;
    actionData.AuditExist = false;
    actionData.AuditNumber = this.AuditNumber;
    // actionData.AuditName = this.AuditName;
    actionData.ProcessLocationMappingId = this.ProcessLocationMappingId;
    actionData.LocationID = this.LocationID;
    actionData.ReviewQtr = this.ReviewQtr;
    actionData.ObsNumber = this.ObsNumber;
    actionData.ResponsibilityDepartment = this.ResponsibilityDepartment;
    actionData.ImplementationRemarks = this.ImplementationRemarks;
    actionData.ManagementResponse = this.ManagementResponse;
    actionData.AgreedActionPlan = this.AgreedActionPlan;
    actionData.ImplementationOwnerId = this.getImplementionOwnerIds();
    // actionData.BusinessCycleID = this.businessCycleId;
    // actionData.ProcessL1ID = this.processlevel1Id;
    // actionData.ProcessL2ID = this.processlevel2Id;
    actionData.RootCauses = this.getRootCauseIds();
    actionData.ObservationGrading = parseInt(this.ObservationGrading);
    actionData.ObservationHeading = this.ObservationHeading;
    actionData.DetailedObservation = this.DetailedObservation;
    actionData.RiskTypeId = this.RiskTypeId;
    actionData.ImplementationEndDate = this.utils.formatNgbDateToYMD(
      actionData.ImplementationEndDate
    );
    actionData.RevisedDate = this.utils.formatNgbDateToYMD(
      actionData.RevisedDates
    );
    actionData.Status = this.Status;
    actionData.AuditeeStatus=this.AuditeeStatus;
    this.api
      .insertData("api/actionplanning/AddFollowup", actionData)
      .subscribe((response) => {
        this.Id = response["id"];
        this.addAPDetails();
      });
  }
  clearAll() {
    this.AuditId = "";
    this.AuditNumber = "";
    this.Comments = "";
    this.ObservationGrading = "";
    this.ObservationHeading = "";
    this.DetailedObservation = "";
    this.selectedImplementationOwnerId = [];
    // this.ImplementationEndDate = "";
    this.RevisedDate = null;
    this.RevisedDates= null;
    this.Status = "";
    this.AuditeeStatus = "";
    this.RiskTypeId = "";
    this.ImplementationOwnerId = "";
    this.ManagementResponse = "";
    this.AgreedActionPlan = "";
    this.ResponsibilityId = "";
    this.RootCause = "";
    this.ActionPlan = "";
    this.AuditExist = false;
    this.businessCycleId = "";
    this.processlevel1Id = "";
    this.processlevel2Id = "";
    this.LocationID = "";
    this.processlevel1Options = [];
    this.processlevel2Options = [];
    this.ReviewQtr = "";
    this.ObsNumber = "";
    this.ResponsibilityDepartment = "";
    this.ImplementationRemarks = "";
    this.AuditName = "";
    // this.Company = "";
    // this.AuditStartDate = "";
    // this.AuditEnd    Date = "";
    // this.ImplementationStartDate = "";
    // this.Implications = "";
    // this.Recommendation = "";
    // this.Quarter = "";
  }

  fillStakeHolderOptions() {
    this.spinner.show();
    this.commonApi.getStakeholders().subscribe(
      (posts) => {
        this.shOpts = posts;
        this.shOpts.forEach((user) => {
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

  backToActionView() {
    this.router.navigate(["./pages/action"]);
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
    if (this.businessCycleId) {
      this.spinner.show();
      this.commonApi.getProcessLevel1(this.businessCycleId).subscribe(
        (posts) => {
          this.processlevel1Options = posts;
          this.processlevel1Id = "";
          this.processlevel2Id = "";
          this.processlevel2Options = [];
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
          this.processlevel2Id = "";
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

  getLocations() {
    this.commonApi.getLocations().subscribe((locationData) => {
      this.locationOpts = locationData;
    });
  }

  getProcessLocationMappingOptions() {
    this.commonApi.getAuditName().subscribe((response) => {
      this.processLocationMappingOptions = response;
    });
  }

  getRootCauseOptions() {
    this.spinner.show();
    this.commonApi.getRootCause().subscribe(
      (posts) => {
        this.rootCauseOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getRiskTypeOptions() {
    this.spinner.show();
    this.commonApi.getAllRiskTypes().subscribe(
      (posts) => {
        this.riskTypeOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  addNewAP() {
    this.clearAPForm();
    this.showAPForm();
  }

  clearAPForm() {
    this.apId = "";
    this.Comments = "";
    this.ActionPlan = "";
    this.RevisedDate = null;
    // this.ImplementationOwnerId = "";
    this.uploadedFileIndex = -1;
    this.uploadedFiles = [];
    this.selectedFiles = [];
    this.fillUploadedFilesTable();
  }

  showAPForm() {
    window["jQuery"]("#manageFollowUpActionPlan").modal("show");
  }

  addAP() {
    if (this.apForm.invalid) return false;
    let apData = this.apForm.form.value;
    apData.FilesInfo = this.uploadedFiles;
    apData.RevisedDate = apData.RevisedDate;
    let isNew = !this.isEditAP;
    if (this.apId && isNew) this.addAPDetail(apData);
    else this.addAPToTable(apData, isNew);
    this.hideAPForm();
  }

  addAPDetail(apData) {
    let apObj = this.formatApData.post(apData);
    apObj.FollowupId = this.Id;
    this.spinner.show();
    this.api
      .insertData("api/actionplanning/savefollowupactionplan/", apObj)
      .subscribe(
        (response) => {
          apData.id = response["id"];
          this.ActionPlans.push(apData);
          this.fillAPTable();
          this.hideAPForm();
          this.clearAPForm();
          this.spinner.hide();
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
        Comments: apData.comments,
        ActionPlan: apData.actionPlan,
        // ImplementationOwnerId: apData.implementationOwnerId,
        RevisedDate: this.utils.formatToNgbDate(apData.revisedDate),
      };
      return apObj;
    },
    post: (apData) => {
      var userid = localStorage.getItem("userId");
      let apObj = {
        Comments: apData.Comments,
        ActionPlan: apData.ActionPlan,
        // ImplementationOwnerId: apData.ImplementationOwnerId,
        RevisedDate: this.utils.formatNgbDateToYMD(apData.RevisedDate),
        FollowupId: this.Id,
        Files: this.getFileNamesArrayToPost(apData.FilesInfo),
        CreatedBy: userid,
      };

      if (apData.id) {
        apObj["id"] = apData.id;
      }
      return apObj;
    },
  };

  getFileNamesArrayToPost(files) {
    let fileArray = [];

    files.forEach((element) => {
      fileArray.push(element.id);
    });

    return fileArray;
  }

  fillAPTable() {
    if (Array.isArray(this.ActionPlans) && this.ActionPlans.length) {
      $("#FAPTable tbody").html("");

      for (let ap of this.ActionPlans) {
        this.addAPToTable(ap);
      }
    } else {
      $("#FAPTable tbody").html(
        '<tr class="norecords"><td colspan="6" class="text-center">No Records Found</td></tr>'
      );
    }
  }

  addAPToTable(apData, isNew?: boolean) {
    let resourceTable = $("#FAPTable tbody");

    let noOfRecords = resourceTable.children("tr").length;
    this.RevisionCount = this.RevisionCount + 1;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#FAPTable tbody .norecords").length) {
        $("#FAPTable tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      // let owner = this.shOpts.filter(
      //   (userObj) => userObj.id === apData.ImplementationOwnerId
      // )[0];

      // let ownerName = owner ? `${owner.firstName} ${owner.lastName}` : "";
      let fileNames = this.getUploadedFileNames(apData.FilesInfo).join(", ");
      // <td>${apData.ActionPlan}</td>
      // <td>${ownerName}</td>
      this.RevisedDates = apData.RevisedDate;
      let resHtml = `<tr>;
          <td>${apData.Comments} </td>
        
          <td>${this.utils.formatNgbDateToDMY(apData.RevisedDate)}</td>
          <td>${fileNames}</td>
          <td>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editAP"><i class="fa fa-edit"></i> </button>
            <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeAP"><i class="fa fa-trash"></i> </button></td>
        </tr>`;

      resourceTable.append(resHtml);

      if (!isInitRender) this.ActionPlans.push(apData);
    } else {
      if (this.ActionPlans[this.apIndex])
        apData.id = this.ActionPlans[this.apIndex].id;

      this.ActionPlans[this.apIndex] = apData;
      this.fillAPTable();
    }
  }

  getUploadedFileNames(files) {
    let uploadedFileNames = [];

    if (files.length > 0) {
      let index = 0;

      files.forEach((element) => {
        let fileName = `<a href="javascript:void(0);" class="downloadFile" data-index="${index}">${element.originalFileName}</a>`;

        if (typeof element.uploadedFileName === "undefined")
          fileName = element.originalFileName;

        uploadedFileNames.push(fileName);

        index++;
      });
    }

    return uploadedFileNames;
  }

  hideAPForm() {
    window["jQuery"]("#manageFollowUpActionPlan").modal("hide");
  }

  addAPDetails() {
    this.spinner.show();
    let racmObj = this.getAPToPost();
    this.api.insertData("api/actionplanning/multiple", racmObj).subscribe(
      (response) => {
        this.spinner.hide();
        this.notifyService.success("Action Plan saved successfully");
        this.clearAll();
        this.backToActionView();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  updateAPDetails() {
    this.spinner.show();
    let racmObj = this.getAPToPost();
    this.api.updateData("api/actionplanning/multiple", racmObj).subscribe(
      (response) => {
        this.spinner.hide();
        this.notifyService.success("Action Plan saved successfully");
        this.clearAll();
        this.backToActionView();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  getAPToPost() {
    let apArray = this.ActionPlans.map((apObj) =>
      this.formatApData.post(apObj)
    );
    return apArray;
  }

  editAP(apIndex) {
    let apData = this.ActionPlans[apIndex];
    this.clearAPForm();
    this.fillAPEdit(apData);
  }

  fillAPEdit(apData) {
    this.apId = apData.id;
    this.Comments = apData.Comments;
    this.ActionPlan = apData.ActionPlan;
    // this.ImplementationOwnerId = apData.ImplementationOwnerId;
    this.RevisedDate = apData.RevisedDate;
    this.uploadedFiles = apData.FilesInfo;
    this.fillUploadedFilesTable();
    this.showAPForm();
  }

  removeAP(apIndex) {
    if (this.ActionPlans[apIndex]) {
      this.RevisionCount=0;
      var id = this.ActionPlans[apIndex].id;
      if (id) {
        this.api
          .deleteData("api/actionplanning/deletefollowupactionplan/" + id)
          .subscribe((response) => {});
      } else {
        let filesInfo = this.ActionPlans[apIndex].FilesInfo;

        if (filesInfo) {
          let ids = [];

          filesInfo.forEach((element) => {
            ids.push(element.id);
          });

          this.api
            .deleteData(
              "api/actionplanning/removemultiplefiles/" + ids.join(",")
            )
            .subscribe((response) => {});
        }
      }
      this.ActionPlans.splice(apIndex, 1);
      this.fillAPTable();
    }
  }

  uploadAPFile(event) {
    let filesElem = this.fileInputAP.nativeElement;
    let formData = new FormData();

    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        formData.append("files", file);
      }
      this.spinner.show();
      this.api
        .insertData("api/actionplanning/multiplefiles", formData)
        .subscribe(
          (upload) => {
            let result: any = upload;
            this.spinner.hide();
            if (result.isUploaded) {
              for (let file of result.files) {
                this.addUploadedFileToTable(file, true);
              }
            }
          },
          (error) => {
            this.spinner.hide();
            console.log(error);
          }
        );
      this.spinner.hide();
      this.fileInputAP.nativeElement.value = "";
    }
  }

  addUploadedFileToTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#uploadedAPFiles tbody");
    let noOfRecords = uploadedFilesTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#uploadedAPFiles tbody .norecords").length) {
        $("#uploadedAPFiles tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let fileName = `<a href="javascript:void(0);" class="downloadFile" data-index="${noOfRecords}">${uploadFileObj.originalFileName}</a>`;
      if (typeof uploadFileObj.uploadedFileName === "undefined")
        fileName = uploadFileObj.originalFileName;

      let resHtml = `<tr>
          <td>${fileName}</td>
          <td>${this.utils.formatDateToStr(uploadFileObj.uploadedDatetime)}</td>
          <td><button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeFile"><i class="fa fa-trash"></i></button></td>
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
      $("#uploadedAPFiles tbody").html("");

      for (let file of this.uploadedFiles) {
        this.addUploadedFileToTable(file);
      }
    } else {
      $("#uploadedAPFiles tbody").html(
        '<tr class="norecords"><td colspan="3" class="text-center">No Records Found</td></tr>'
      );
    }
  }

  downloadFile(appIndex) {
    let fileId = this.uploadedFiles[appIndex].id;

    if (this.uploadedFiles[appIndex]) {
      if (typeof fileId === "undefined")
        this.notifyService.error(
          "Internal server error. Please reload the page and try again."
        );
      else {
        this.spinner.show();
        this.api
          .downloadFile("api/actionplanning/downloadfile/" + fileId)
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
              this.spinner.hide();
            },
            (error) => {
              this.spinner.hide();
              console.log(error);
            }
          );
      }
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
        this.api
          .deleteData("api/actionplanning/removefile/" + fileId)
          .subscribe(
            (upload) => {
              let result: any = upload;
              if (result.isDeleted) this.fillUploadedFilesTable();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      }
    }
  }
  getImplementionOwnerIds() {
    var implementationOwner = "";
    this.selectedImplementationOwnerId.forEach((element) => {
      implementationOwner = element.id;
    });
    return implementationOwner;
  }
  ngOnInit() {
    this.getProcessLocationMappingOptions();
    // // this.fillBusinessCycleOpts();
    this.getLocations();
    // this.getAllAudit();
    this.fillStakeHolderOptions();
    this.getRootCauseOptions();
    this.getRiskTypeOptions();

    this.initDropdownSettings(
      false,
      "id",
      "name",
      "Select All",
      "Deselect All",
      3,
      false,
      true
    );

    $(document).ready(() => {
      $("#FAPTable").on("click", ".editAP", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.apIndex = dataIndex;
          this.isEditAP = true;
          this.editAP(parseInt(dataIndex));
        }
      });

      $("#FAPTable").on("click", ".removeAP", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) this.removeAP(parseInt(dataIndex));
      });

      $("#uploadedAPFiles").on("click", ".downloadFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.downloadFile(parseInt(dataIndex));
        }
      });

      $("#uploadedAPFiles").on("click", ".removeFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.removeUploadedFile(parseInt(dataIndex));
        }
      });
    });
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
      selectAllText: "Select All",
      unSelectAllText: "UnSelect All",
      itemsShowLimit: 3,
      allowSearchFilter: true,
      closeDropDownOnSelection: true,
    };
    this.dropdownSettings = {
      singleSelection: isSingleSelection,
      idField: idField,
      textField: textField,
      selectAllText: selectAllText,
      unSelectAllText: unSelectAllText,
      itemsShowLimit: itemsShowLimit,
      allowSearchFilter: isAllowSearchFilter,
      closeDropDownOnSelection: closeDropDownOnSelection,
    };
  }
}
