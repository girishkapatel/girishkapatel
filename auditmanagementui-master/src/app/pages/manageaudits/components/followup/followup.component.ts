 import { Component, OnInit, ViewChild, Input } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import * as $ from "jquery";
import { ApiService } from "src/app/services/api/api.service";
import { NgForm } from "@angular/forms";
import { Observable } from "rxjs";
import { debounceTime, distinctUntilChanged, map } from "rxjs/operators";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { UtilsService } from "src/app/services/utils/utils.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { IDropdownSettings } from "ng-multiselect-dropdown";
@Component({
  selector: "app-followup",
  templateUrl: "./followup.component.html",
  styleUrls: ["./followup.component.css"],
  providers: [],
})
export class FollowupComponent implements OnInit {
  constructor(
    private api: ApiService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService
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
  OwnerdropdownSettings: IDropdownSettings = {};
  @Input() auditMaster: any = [];
  @Input() shOpts: any = [];

  accessRights: any = {};
  isStackHolder: boolean = false;
  public Editor = ClassicEditor;
  CurrentDate:  {year: number, month: number, day: number};
  allRootCauses: any = [];
  allRiskTypess: any = [];
  RootCauses: any = [];

  RiskTypeId: string = "";

  tableApiUrl: string;
  id: string = "";
  AuditId: string = "";
  AuditNumber: string = "";
  ObservationHeading: string = "";
  DetailedObservation: string = "";
  ImplementationEndDate: NgbDateStruct;
  Status: string = "";
  RiskType: string = "";
  ResponsibilityId: string = "";
  Implications: string = "";
  RootCause: string = "";
  RevisedDates: NgbDateStruct;
  DraftReportId: any;

  //Action plan fields
  apId: string = "";
  isEditAP: boolean = false;
  Comments: any;
  RevisedDate: any;
  ActionPlan: any;
  ImplementationOwnerId: string = "";
  ActionPlans: any = [];
  apIndex: number = -1;

  uploadedFiles: any = [];
  selectedFiles: any = [];
  uploadedFileIndex: number;
  selectedImplementationOwnerId = [];
  tableId: string = "followup_table";
  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Audit Number",
      data: "audit.auditNumber",
    },
    {
      title: "Observation Heading",
      data: "observationHeading",
    },
    {
      title: "Detailed Observation",
      data: "detailedObservation",
      render: (data) => {
        if (data.length > 100) {
          return (
            "<span>" +
            data.slice(0, 100) +
            '</span><br><a href="javascript:void(0)" data-title="Detailed Observation" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    {
      title: "Root Cause",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.rootCauses)
          return this.getRootCauseNames(
            rowData.draftReport
              ? rowData.draftReport.rootCauses
              : rowData.rootCauses
          );
        else return "";
      },
    },
    {
      title: "Risk Type",
      data: "",
      render: (data, row, rowData) => {
        if(rowData.draftReport)
        if(rowData.draftReport.discussionNote){
          var rtype=rowData.draftReport.discussionNote.riskTypeIds;
          return this.getRiskTypeNames(rtype);
        }
        return "";
      },
    }, 
    {
      title: "Status",
      data: "",
      render: (data, type, row, meta) => {
        return row.status.toUpperCase();
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, type, row, meta) => {
        let buttons = "";

        if (this.accessRights.isEdit)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editFollowup"><i class="fa fa-edit"></i></button>';

        return buttons;
      },
    },
  ];

  tableData: tableData[] = [];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
      //window["jQuery"]("#editFollowup").modal('show');
    },
    hide: () => {
      this.formVisible = false;
      //window["jQuery"]("#editFollowup").modal('hide');
      this.clearform();
    },
  };

  formatter = (schedule: any) => {
    return schedule.auditNumber;
  };

  validateActionSave() {
    return this.AuditId !== "";
  }

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
    // this.Quarter = auditData.quarter;
    this.AuditId = auditData.audit.id;
  }

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveFollowup() {
    if (this.Status === "") {
      this.notifyService.error("Status is compulsory");
      return;
    }

    if (this.actionForm.invalid) {
      return false;
    }

    let followupObj = this.actionForm.form.value;
    followupObj.AuditId = this.AuditId;
    followupObj.auditNumber = this.AuditNumber;
    followupObj.id = this.id;
    followupObj.DraftReportId = this.DraftReportId;
    followupObj.RiskTypeId = this.RiskTypeId;
    followupObj.ObservationHeading = this.ObservationHeading;
    followupObj.RootCauses = this.RootCauses;
    followupObj.RootCause = null;
    followupObj.ImplementationEndDate = this.utils.formatNgbDateToYMD(
      followupObj.ImplementationEndDate
    );
    followupObj.RevisedDate = this.utils.formatNgbDateToYMD(
     this.RevisedDates
    );
    followupObj.ImplementationOwnerId = this.getImplementionOwnerIds();
    this.api.updateData("api/followup/UpdateFollowup/followup", followupObj).subscribe(
      (response) => {
        this.updateAPDetails();

        this.notifyService.success("Followup updated successfully");
        this.handleFormView.hide();
        this.tableFilters.next({ init: true });
      },
      (error) => {
        this.notifyService.error(
          "Unable to update, please contact administrator"
        );
        this.handleFormView.hide();
      }
    );
  }

  editFollowup(actionData) {
    this.id = actionData.id;
    this.DraftReportId = actionData.draftReportId;
    this.AuditId = actionData.auditId;
    this.AuditNumber = actionData.audit.auditNumber;
    this.ObservationHeading = actionData.observationHeading;
    this.DetailedObservation = actionData.detailedObservation;
    this.ImplementationEndDate = actionData.implementationEndDate
      ? this.utils.formatToNgbDate(actionData.implementationEndDate)
      : null;
    this.Status = actionData.status.toLowerCase();
    var rtype=actionData.draftReport.discussionNote.riskTypeIds;
    this.RiskType = this.getRiskTypeNames(rtype);
    // this.ImplementationOwnerId = actionData.implementationOwnerId;
    this.ResponsibilityId = "";
    this.Implications = actionData.implications;
    this.RootCause =
      actionData.draftReport != null
        ? this.getRootCauseNames(actionData.draftReport.rootCauses)
        : this.getRootCauseNames(actionData.rootCauses);

        this.RootCauses= actionData.draftReport != null
        ? (actionData.draftReport.rootCauses)
        : (actionData.rootCauses);
    // this.Comments = actionData.comments;
    // this.RevisedDate = actionData.revisedDate;
    // this.RootCauses =actionData.draftReport!=null? actionData.draftReport.rootCauses:actionData.;
    this.RiskTypeId = actionData.riskTypeId;
    this.ActionPlans = this.getAPToDisplay(actionData.actionPlansInfo);

    this.fillAPTable();

    this.handleFormView.show();

    // this.Recommendation = actionData.recommendation;
    // this.ImplementationStartDate = actionData.implementationStartDate;
    // this.Company = actionData.audit.location.profitCenterCode;
    // this.Quarter = actionData.audit.quater;
    // this.AuditStartDate = actionData.audit.auditStartDate;
    // this.AuditEndDate = actionData.audit.auditEndDate;
  }

  getAPToDisplay(apData) {
    let apArray = apData.map((apObj) => this.formatApData.get(apObj));

    return apArray;
  }

  clearform() {
    this.id = "";
    this.DraftReportId = "";
    this.Comments = "";
    this.Status = "";
    this.AuditId = "";
    this.AuditNumber = "";

    this.Comments = "";
    this.ObservationHeading = "";
    this.DetailedObservation = "";
    this.ImplementationEndDate = null;
    this.Status = "";
    this.RiskType = "";
    this.ImplementationOwnerId = "";
    this.ResponsibilityId = "";
    this.Implications = "";
    this.RootCause = "";

    // this.Company = "";
    // this.Quarter = "";
    // this.AuditStartDate = "";
    // this.AuditEndDate = "";
    // this.ImplementationStartDate = "";
    // this.Recommendation = "";
  }

  showContentModal(title, content) {
    window["jQuery"]("#fuContentModal #fucontent-title").html("").html(title);
    window["jQuery"]("#fuContentModal #fucontent").html("").html(content);
    window["jQuery"]("#fuContentModal").modal("show");
  }

  getAllRootCauses() {
    this.commonApi.getRootCause().subscribe((data) => {
      this.allRootCauses = data;
    });
  }
  getAllRiskTypes() {
    this.commonApi.getAllRiskTypes().subscribe((data) => {
      this.allRiskTypess = data;
    });
  }

  getRootCauseNames(rootCauseIds) {
    let rootCauseNames: any = [];
    if (rootCauseIds != null) {
      this.allRootCauses.forEach((element) => {
        if (rootCauseIds.indexOf(element.id) > -1)
          rootCauseNames.push(element.name);
      });
    }
    return rootCauseNames.join(", ");
  }
  getRiskTypeNames(rootCauseIds) {
    let rootCauseNames: any = [];
    if (rootCauseIds != null) {
      this.allRiskTypess.forEach((element) => {
        if (rootCauseIds.indexOf(element.id) > -1)
          rootCauseNames.push(element.name);
      });
    }
    return rootCauseNames.join(", ");
  }
  addNewAP() {
    this.clearAPForm();
    this.showAPForm();
  }

  clearAPForm() {
    this.apId = "";
    this.Comments = "";
    this.ActionPlan = "";
    this.RevisedDate = "";
    this.ImplementationOwnerId = "";
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

    var appData={
      Comments:this.Comments,
      ActionPlan:this.ActionPlan,
      ImplementationOwnerId:this.getImplementionOwnerIds(),
      RevisedDate:this.RevisedDate,
      FilesInfo:this.uploadedFiles
    }
    let isNew = !this.isEditAP;

    if (this.apId && isNew) this.addAPDetail(appData);
    else this.addAPToTable(appData, isNew);

    this.hideAPForm();
  }

  addAPDetail(apData) {
    let apObj = this.formatApData.post(apData);
    apObj.FollowupId = this.id;

    this.api
      .insertData("api/actionplanning/savefollowupactionplan/", apObj)
      .subscribe((response) => {
        apData.id = response["id"];

        this.ActionPlans.push(apData);

        this.fillAPTable();
        this.hideAPForm();
        this.clearAPForm();
      });
  }

  formatApData = {
    get: (apData) => {
      let apObj = {
        id: apData.id,
        Comments: apData.comments,
        ActionPlan: apData.actionPlan,
        ImplementationOwnerId: apData.implementationOwnerId,
        RevisedDate: this.utils.formatToNgbDate(apData.revisedDate),
        Files: apData.files,
        FilesInfo: apData.filesInfo,
      };

      this.apId = apData.id;

      return apObj;
    },
    post: (apData) => {
      var userid = localStorage.getItem("userId");
      let apObj = {
        Comments: apData.Comments,
        ActionPlan: apData.ActionPlan,
        ImplementationOwnerId:this.getImplementionOwnerIds(),
        RevisedDate: this.utils.formatNgbDateToYMD(apData.RevisedDate),
        FollowupId: this.id,
        Files: this.getFileNamesArrayToPost(apData.FilesInfo),
        CreatedBy:userid,
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

    let isInitRender = false;
    let startDate;
    let enddate;
    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }
    if (this.isEdit) startDate = this.utils.formatDateToStr(apData.RevisedDate);
    else
      startDate = this.utils.formatDateToStr(
        this.utils.formatNgbDateToDate(apData.RevisedDate)
      );

    if (isNew) {
      if ($("#FAPTable tbody .norecords").length) {
        $("#FAPTable tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }
      let owner = this.shOpts.filter(
        (userObj) => userObj.id === apData.ImplementationOwnerId
      )[0];

      let ownerName = owner ? `${owner.firstName} ${owner.lastName}` : "";
      let fileNames = this.getUploadedFileNames(apData.FilesInfo).join(", ");
      this.RevisedDates = apData.RevisedDate;
      let buttons = "";

      if (this.accessRights.isEdit)
        buttons = `<button type="button" data-index="${noOfRecords}" class="editAP btn btn-sm btn-info">
            <i class="fa fa-edit"></i></button>`;

      if (this.accessRights.isDelete && !this.isStackHolder)
        buttons += `<button type="button" data-index="${noOfRecords}" class="removeAP btn btn-sm btn-danger">
          <i class="fa fa-trash"></i></button>`;

      let resHtml = `<tr>;
          <td>${apData.Comments} </td>
          <td>${apData.ActionPlan}</td>
          <td>${ownerName}</td>
          <td>${startDate}</td>
          <td>${fileNames}</td>
          <td>${buttons}</td>
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
    this.isEditAP = false;

    window["jQuery"]("#manageFollowUpActionPlan").modal("hide");
  }

  addAPDetails() {
    let racmObj = this.getAPToPost();

    this.api
      .insertData("api/actionplanning/multiple", racmObj)
      .subscribe((response) => {
        this.handleFormView.hide();
      });
  }

  updateAPDetails() {
    let racmObj = this.getAPToPost();

    this.api
      .updateData("api/actionplanning/multiple", racmObj)
      .subscribe((response) => {
        this.handleFormView.hide();
      });
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
    this.selectedImplementationOwnerId =
    this.getSelectedImplementationOwnerOpts(apData.ImplementationOwnerId);
  this.ImplementationOwnerId = this.getImplementionOwnerIds(); 
    this.RevisedDate = apData.RevisedDate;
    this.uploadedFiles = apData.FilesInfo;

    this.fillUploadedFilesTable();
    this.showAPForm();
  }

  removeAP(apIndex) {
    if (this.ActionPlans[apIndex]) {
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

  uploadAPFile() {
    let filesElem = this.fileInputAP.nativeElement;
    let formData = new FormData();

    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        formData.append("files", file);
      }

      this.api
        .insertData("api/actionplanning/multiplefiles", formData)
        .subscribe(
          (upload) => {
            let result: any = upload;
            if (result.isUploaded) {
              for (let file of result.files) {
                this.addUploadedFileToTable(file, true);
              }
            }
          },
          (error) => {
            console.log(error);
          }
        );

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
            },
            (error) => {
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
              console.log(error);
            }
          );
      }
    }
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "manageaudits",
      "followup"
    );
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
      for (let i of this.shOpts) {
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
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditId = localStorage.getItem("auditId");

    this.getAllRootCauses();
    this.getAllRiskTypes();

    this.tableApiUrl = `api/followup/GetByAudit/${this.AuditId}`;
    this.tableFilters = new BehaviorSubject({ init: true });

    $(document).ready(() => {
      $("#followupComponent").on("click", ".editFollowup", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let followupData = $("#" + dataId).data();
        this.editFollowup(followupData);
      });

      $('a[href="#followupTab"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });

      $("#followupComponent").on("click", ".viewContent", (e) => {
        let title = $(e.currentTarget).attr("data-title");
        let content = $(e.currentTarget).attr("data-content");
        content = decodeURI(content);
        this.showContentModal(title, content);
      });

      $("#followupComponent").on("click", ".editAP", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.apIndex = dataIndex;
          this.isEditAP = true;
          this.editAP(parseInt(dataIndex));
        }
      });

      $("#followupComponent").on("click", ".removeAP", (e) => {
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
