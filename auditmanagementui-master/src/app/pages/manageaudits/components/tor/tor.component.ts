import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { ApiService } from "../../../../services/api/api.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { NgbDate, NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { NgForm } from "@angular/forms";
import { IDropdownSettings } from "ng-multiselect-dropdown";
@Component({
  selector: "app-tor",
  templateUrl: "tor.component.html",
  styles: [
    ".myClass {font-weight: bold;}.red-border-class {border: 1px solid red;}.disabledTab{pointer-events: none;}.table>thead>tr>th {vertical-align: top !important;}",
  ],
})
export class TorComponent implements OnInit {
  constructor(
    private api: ApiService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private spinner: NgxSpinnerService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("fileInputTOR", { static: false }) fileInputTOR;
  @ViewChild("auditScopeForm", { static: false }) auditScopeForm: NgForm;
  @ViewChild("auditInfoForm", { static: false }) auditInfoForm: NgForm;
  @ViewChild("torForm", { static: false }) torForm: NgForm;
  OwnerdropdownSettings: IDropdownSettings = {};
  // @ViewChild("fileInputTOR", { static: true }) fileInputTOR: any;
  @Input() userOpts: any = [];
  @Input() shOpts: any = [];
  @Input() isChildTabActive: boolean;
  selectedImplementationOwnerId = [];
  public Editor = ClassicEditor;

  accessRights: any = {};
  isStackHolder: boolean = false;
  Areas: string = "";
  Scope: string = "";

  Area: string = "";
  DataRequested: string = "";
  Status: string = "";
  ProcessOwnerId: string = "";

  DataRequestDate: NgbDateStruct;
  DataReceivedDate: NgbDateStruct;

  PendingData: string = "";

  EditScope: boolean = false;
  EditInfo: boolean = false;
  ScopeIndex = -1;
  InfoIndex = -1;
  AuditInfoId: any;

  uploadedFiles: any = [];

  uploadedFileIndex: number;
  loader: boolean = false;

  id: string = "";
  activities: any = [];
  AuditID: string = "";
  auditUnit: string = "";
  auditName: string = "";
  businessCycle: string = "";
  processl1: string = "";
  auditTeam: string = "";
  auditApprovers: string = "";
  location: string = "";
  auditPeriod: string = "";

  AuditObjective: string = "";
  Policies: string = "";
  Deliverable: string = "";
  Disclaimer: string = "";
  Limitation: string = "";
  TORIssuedDate: NgbDateStruct;
  AuditPeriodFromDate: NgbDateStruct;
  AuditPeriodToDate: NgbDateStruct;
  Auditee: string = "";

  auditeesEmail: any = [];
  fillTorForm(torData) {
    this.id = torData.id;
    this.activities = torData.activities;
    this.AuditObjective = torData.auditObjective;
    this.Policies = torData.policies;
    this.Deliverable = torData.deliverable;
    this.Disclaimer = torData.disclaimer;
    this.Limitation = torData.limitation;
    this.TORIssuedDate = torData.torIssuedDate
      ? this.utils.formatToNgbDate(torData.torIssuedDate)
      : null;
    this.AuditPeriodFromDate = torData.auditPeriodFromDate
      ? this.utils.formatToNgbDate(torData.auditPeriodFromDate)
      : null;
    this.AuditPeriodToDate = torData.auditPeriodToDate
      ? this.utils.formatToNgbDate(torData.auditPeriodToDate)
      : null;
    this.Auditee = this.getAuditees(torData.audit.auditees);

    let auditData = torData.audit;
    this.auditUnit = auditData.location.divisionDescription;
    this.auditName = auditData.audit.processLocationMapping.auditName;
    // this.businessCycle = auditData.audit.processLocationMapping.businessCycle.name;
    // this.processl1 = auditData.audit.processLocationMapping.processL1.name;
    this.auditTeam = this.getAuditTeam(auditData.auditResources);
    this.auditApprovers = this.getAuditApprovers(
      auditData.auditApprovalMapping
    );
    this.auditPeriod =
      this.utils.formatDbDateToDMY(auditData.auditStartDate) +
      " - " +
      this.utils.formatDbDateToDMY(auditData.auditEndDate);
    this.location = auditData.location.profitCenterCode;
    this.handleTORTables.display(torData.auditScopes, "auditScope");
    this.handleTORTables.display(
      torData.auditSpecificInformations,
      "auditInfo"
    );
  }

  handleTORTables = {
    data: {
      auditScope: [],
      auditInfo: [],
      add: (newdata, dataOf) => {
        if (Array.isArray(this.handleTORTables.data[dataOf])) {
          this.handleTORTables.data[dataOf].push(newdata);
        }
        return this.handleTORTables.data[dataOf];
      },
      edit: (newdata, index, dataOf) => {
        if (
          Array.isArray(this.handleTORTables.data[dataOf]) &&
          this.handleTORTables.data[dataOf][index]
        ) {
          this.handleTORTables.data[dataOf][index] = newdata;
        }
        return this.handleTORTables.data[dataOf];
      },
      remove: (index, dataOf) => {
        if (
          Array.isArray(this.handleTORTables.data[dataOf]) &&
          this.handleTORTables.data[dataOf][index]
        ) {
          this.handleTORTables.data[dataOf].splice(index, 1);
        }
        return this.handleTORTables.data[dataOf];
      },
      get: (dataOf, index?) => {
        if (Array.isArray(this.handleTORTables.data[dataOf])) {
          if (index && this.handleTORTables.data[dataOf][index]) {
            return this.handleTORTables.data[dataOf][index];
          }
          return this.handleTORTables.data[dataOf];
        } else {
          return [];
        }
      },
    },
    coldef: {
      auditScope: {},
      auditInfo: {},
    },
    display: (tableData, dataOf) => {
      this.handleTORTables.data[dataOf] = Array.isArray(tableData)
        ? tableData
        : [];
      this.handleTORTables.fill(tableData, `#${dataOf}Table`, dataOf);
    },
    fill: (tableData, tableId, dataOf) => {
      let tbody =
        Array.isArray(tableData) && tableData.length
          ? this.handleTORTables.tbody(tableData, dataOf)
          : this.handleTORTables.noRecords(tableId);
      $(`${tableId} tbody`).html("").html(tbody);
    },
    add: (newdata, dataOf) => {
      let dataAfterAdd = this.handleTORTables.data.add(newdata, dataOf);
      this.handleTORTables.fill(dataAfterAdd, `#${dataOf}Table`, dataOf);
    },
    edit: (newdata, index, dataOf) => {
      let dataAfterEdit = this.handleTORTables.data.edit(
        newdata,
        index,
        dataOf
      );
      this.handleTORTables.fill(dataAfterEdit, `#${dataOf}Table`, dataOf);
    },
    remove: (index, dataOf) => {
      let newdata = this.handleTORTables.data.remove(index, dataOf);
      this.handleTORTables.fill(newdata, `#${dataOf}Table`, dataOf);
    },
    tbody: (tableData, dataOf) => {
      let tbody = "";
      for (let i = 0; i < tableData.length; i++) {
        tbody += this.handleTORTables.row(tableData[i], i, dataOf);
      }
      return tbody;
    },
    row: (trData, index, dataOf) => {
      let tr = "";
      if (typeof dataOf === "string" && dataOf.toLowerCase() === "auditinfo") {
        trData = this.handleAuditInfo.get(trData);
      }
      let tableObjKeys = Object.keys(trData);
      let recId =
        tableObjKeys.indexOf("id") > -1
          ? trData["id"]
          : tableObjKeys.indexOf("Id") > -1
          ? trData["Id"]
          : "";
      tr += `<tr data-of="${dataOf}" data-id="${recId}" data-index="${index}">`;
      for (let keys of tableObjKeys) {
        if (keys.toLowerCase() !== "id") {
          if (keys.toLowerCase() === "processownerid") {
            var pOwnerId = trData[keys][0] ? trData[keys][0].id : trData[keys];
            let user = this.shOpts.filter((uObj) => uObj.id === pOwnerId);
            if (user.length) {
              tr += `<td>${user[0].firstName} ${user[0].lastName}</td>`;
            } else {
              tr += `<td></td>`;
            }
          } else {
            tr += `<td>${trData[keys]}</td>`;
          }
        }
      }

      let buttons = "";

      if (this.accessRights.isEdit && !this.isStackHolder)
        buttons = `<button type="button" class="btn btn-sm btn-info editTorRow">
          <i class="fa fa-edit"></i></button>`;

      if (this.accessRights.isDelete && !this.isStackHolder)
        buttons += `<button type="button" class="btn btn-sm btn-danger deleteTorRow">
          <i class="fa fa-trash"></i></button>`;

      tr += `<td>${buttons}</td>`;
      tr += "</tr>";

      return tr;
    },
    noRecords: function (tableId) {
      let colspan = $(`${tableId} thead tr th`).length;
      return `<tr class="noRecords"><td colspan="${colspan}">No Records</td></tr>`;
    },
  };

  getAuditTeam(resources) {
    let users = [];
    for (let resource of resources) {
      let resName =
        resource.user.firstName +
        " " +
        resource.user.lastName +
        " (" +
        resource.user.designation +
        ")";
      users.push(resName);
    }
    return users.join(", ");
  }

  getAuditApprovers(approvers) {
    let users = [];
    for (let approver of approvers.userData) {
      let resName =
        approver.user.firstName +
        " " +
        approver.user.lastName +
        " (" +
        approver.responsibility +
        ")";
      users.push(resName);
    }
    return users.join(", ");
  }

  getTor() {
    try {
      this.spinner.show();
      this.api.getData(`api/tor/GetByAudit/${this.AuditID}`).subscribe(
        (torData) => {
          this.fillTorForm(torData);
          this.getTORFiles();
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          this.notifyService.error(error.error);
        }
      );
    } catch (error) {
      this.spinner.hide();
    }
  }

  saveTor(e) {
    if (this.torForm.invalid) {
      return false;
    }
    this.spinner.show();
    let torObj = this.getTorObj();

    this.api.updateData(`api/tor`, torObj).subscribe(
      (response) => {
        this.spinner.hide();
        this.notifyService.success("TOR Saved Successfully");
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  downloadTor() {
    this.spinner.show();
    this.api
      .downloadFile(`api/tor/downloadExcel/${this.AuditID}`)
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "TOR.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      }),
      (error) => {
        this.spinner.hide();
        console.log(error);
      };
  }
  exportToPDF() {
    this.spinner.show();
    this.api
      .downloadFile(`api/tor/downloadpdf/${this.AuditID}`)
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/pdf",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "TOR.pdf");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      }),
      (error) => {
        this.spinner.hide();
        console.log(error);
      };
  }

  getTorObj() {
    let torObj = {
      Id: this.id,
      AuditId: this.AuditID,
      AuditObjective: this.AuditObjective,
      Policies: this.Policies,
      Deliverable: this.Deliverable,
      AuditScopes: this.handleAuditScope.getAll(),
      AuditSpecificInformations: this.handleAuditInfo.getAll(),
      Disclaimer: this.Disclaimer,
      Limitation: this.Limitation,
      TORIssuedDate: this.utils.formatNgbDateToYMD(this.TORIssuedDate),
      AuditPeriodFromDate: this.utils.formatNgbDateToYMD(
        this.AuditPeriodFromDate
      ),
      AuditPeriodToDate: this.utils.formatNgbDateToYMD(this.AuditPeriodToDate),
    };
    return torObj;
  }

  handleModal = {
    show: (elemId) => {
      this.handleModal.clear();
      window["jQuery"](elemId).modal("show");
    },
    hide: (elemId) => {
      window["jQuery"](elemId).modal("hide");
    },
    edit: (elemId, dataIndex, dataOf) => {
      this.handleModal.show(elemId);
      this.handleModal.fill(dataIndex, dataOf);
    },
    fill: (dataIndex, dataOf) => {
      let data = this.handleTORTables.data.get(dataOf, dataIndex);
      switch (dataOf) {
        case "auditScope":
          this.EditScope = true;
          this.ScopeIndex = dataIndex;
          this.Areas = data.areas;
          this.Scope = data.scope;
          break;
        case "auditInfo":
          this.EditInfo = true;
          this.InfoIndex = dataIndex;
          this.AuditInfoId = data.id ? data.id : data.Id ? data.Id : "";
          this.Area = data.area ? data.area : data.Area ? data.Area : "";
          this.DataRequested = data.dataRequested
            ? data.dataRequested
            : data.DataRequested
            ? data.DataRequested
            : "";
          this.Status = data.status
            ? data.status
            : data.Status
            ? data.Status
            : "";
          this.selectedImplementationOwnerId = data.processOwnerId
            ? this.getSelectedImplementationOwnerOpts(data.processOwnerId)
            : this.getSelectedImplementationOwnerOpts(data.ProcessOwnerId)
            ? this.getSelectedImplementationOwnerOpts(data.ProcessOwnerId)
            : null;
          this.DataRequestDate = data.dataRequestDate
            ? this.utils.formatToNgbDate(data.dataRequestDate)
            : this.utils.formatToNgbDate(data.DataRequestDate)
            ? this.utils.formatToNgbDate(data.DataRequestDate)
            : null;
          this.DataReceivedDate = data.dataReceivedDate
            ? this.utils.formatToNgbDate(data.dataReceivedDate)
            : this.utils.formatToNgbDate(data.DataReceivedDate)
            ? this.utils.formatToNgbDate(data.DataReceivedDate)
            : null;
          this.PendingData = data.pendingData
            ? data.pendingData
            : data.PendingData
            ? data.PendingData
            : "";
          break;
      }
    },
    clear: () => {
      this.Areas = "";
      this.Scope = "";
      this.Area = "";
      this.DataRequested = "";
      this.Status = "";
      this.ProcessOwnerId = "";
      this.DataRequestDate = null;
      this.DataReceivedDate = null;
      this.PendingData = "";
      this.EditInfo = false;
      this.EditScope = false;
      this.ScopeIndex = -1;
      this.InfoIndex = -1;
      this.fileInputTOR.nativeElement.value = "";
      this.selectedImplementationOwnerId = [];
    },
  };

  handleAuditScope = {
    config: {
      dataOf: "auditScope",
      table: "#auditScopeTable",
      modal: "#auditScopeModal",
    },
    getAll: () => {
      return this.handleTORTables.data.get(this.handleAuditScope.config.dataOf);
    },
    get: () => {
      let Obj = {
        areas: this.Areas,
        scope: this.Scope,
      };
      return Obj;
    },
    save: () => {
      if (this.auditScopeForm.invalid) {
        return false;
      }
      let Obj = this.handleAuditScope.get();
      if (this.EditScope) {
        this.handleTORTables.edit(
          Obj,
          this.ScopeIndex,
          this.handleAuditScope.config.dataOf
        );
      } else {
        this.handleTORTables.add(Obj, this.handleAuditScope.config.dataOf);
      }
      this.handleModal.hide(this.handleAuditScope.config.modal);
    },
  };

  handleAuditInfo = {
    config: {
      dataOf: "auditInfo",
      table: "#auditInfoTable",
      modal: "#auditInfoModal",
    },
    getAll: () => {
      return this.handleTORTables.data.get(this.handleAuditInfo.config.dataOf);
    },
    get: (data?) => {
      let Obj = {
        Area: data && data.area ? data.area : this.Area,
        DataRequested:
          data && data.dataRequested ? data.dataRequested : this.DataRequested,
        ProcessOwnerId:
          data && data.processOwnerId
            ? this.getSelectedImplementationOwnerOpts(data.processOwnerId)
            : this.getImplementionOwnerIds(),
        DataRequestDate:
          data && data.dataRequestDate
            ? this.utils.formatDbDateToDMY(data.dataRequestDate)
            : this.utils.formatNgbDateToDMY(this.DataRequestDate),
        DataReceivedDate:
          data && data.dataReceivedDate
            ? this.utils.formatDbDateToDMY(data.dataReceivedDate)
            : this.utils.formatNgbDateToDMY(this.DataReceivedDate),
        PendingData:
          data && data.pendingData ? data.pendingData : this.PendingData,
        Status:
          data && data.status
            ? data.status.toUpperCase()
            : this.Status.toUpperCase(),
      };

      if (data && data.id) {
        Obj["Id"] = data.id;
      }
      return Obj;
    },
    save: () => {
      if (this.auditInfoForm.invalid) {
        return false;
      }
      let Obj = this.handleAuditInfo.get();
      this.handleAuditInfo.saveIDR(Obj, this.EditInfo);
      // if(this.EditInfo){
      //     //this.handleTORTables.edit(Obj, this.InfoIndex, this.handleAuditInfo.config.dataOf);
      // }else{
      //     //this.handleTORTables.add(Obj, this.handleAuditInfo.config.dataOf);
      // }
      //this.handleModal.hide(this.handleAuditInfo.config.modal);
    },
    saveIDR: (obj, EditInfo) => {
      obj.AuditId = this.AuditID;
      let dataRequestDate = obj.DataRequestDate;
      let dataReceivedDate = obj.DataReceivedDate;
      obj.DataRequestDate = this.utils.formatDMYToYMD(obj.DataRequestDate);
      obj.DataReceivedDate = this.utils.formatDMYToYMD(obj.DataReceivedDate);

      if (EditInfo) {
        obj.Id = this.AuditInfoId;

        this.api.updateData("api/initialdatarequest", obj).subscribe(
          (res) => {
            obj.DataRequestDate = dataRequestDate;
            obj.DataReceivedDate = dataReceivedDate;
            this.handleTORTables.edit(
              obj,
              this.InfoIndex,
              this.handleAuditInfo.config.dataOf
            );
            this.handleModal.hide(this.handleAuditInfo.config.modal);
          },
          (err) => {
            this.notifyService.error(
              "Unable to Update Audit Specific Information"
            );
            this.handleModal.hide(this.handleAuditInfo.config.modal);
          }
        );
      } else {
        this.api.insertData("api/initialdatarequest", obj).subscribe(
          (res) => {
            this.handleTORTables.add(obj, this.handleAuditInfo.config.dataOf);
            this.handleModal.hide(this.handleAuditInfo.config.modal);
            let filterOption = 0;
            let filterStatus = "all";
            this.api
              .getData(
                `api/initialdatarequest/GetByAudit/${this.AuditID}/${filterOption}/${filterStatus}`
              )
              .subscribe(
                (res) => {
                  this.handleTORTables.display(res, "auditInfo");
                },
                (error) => {
                  this.notifyService.error(
                    "Unable to fetch Audit Specific Information"
                  );
                }
              );
          },
          (err) => {
            this.notifyService.error(
              "Unable to save Audit Specific Information"
            );
            this.handleModal.hide(this.handleAuditInfo.config.modal);
          }
        );
      }
    },
    delete: (dataId, dataIndex, dataOf) => {
      if (dataId) {
        let isConfirm = confirm(
          "Are you sure you want to delete this information?"
        );
        if (isConfirm) {
          this.api.deleteData("api/initialdatarequest/" + dataId).subscribe(
            (response) => {
              this.notifyService.success(
                "Audit Specific Information Deleted Successfully"
              );
              this.handleTORTables.remove(dataIndex, dataOf);
            },
            (error) => {
              this.notifyService.error(
                "Unable to delete Audit Specific Information."
              );
              console.log(error);
            }
          );
        }
      }
    },
  };

  uploadTORFile() {
    this.spinner.show();
    let filesElem = this.fileInputTOR.nativeElement;
    let formData = new FormData();

    if (filesElem.files.length) {
      for (let files of filesElem.files) {
        formData.append("files", files);
      }

      formData.append("Id", this.id);
      formData.append("AuditId", this.AuditID);
      formData.append("module", "tor");

      const uploaded = this.api
        .insertDataUploading("api/tor/uploadfile", formData)
        .subscribe(
          (upload) => {
            let result: any = upload;
            if (result.isUploaded) {
              for (let file of result.files) {
                this.addUploadedFileToTable(file, true);
              }
            }
            this.spinner.hide();
          },
          (error) => {
            this.spinner.hide();
            console.log(error);
          }
        );
      // uploaded.unsubscribe();
    }
    this.spinner.hide();
    this.fileInputTOR.nativeElement.value = "";
  }

  addUploadedFileToTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#uploadedTORFiles tbody");
    let noOfRecords = uploadedFilesTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#uploadedTORFiles tbody .norecords").length) {
        $("#uploadedTORFiles tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      let fileName = `<a href="javascript:void(0);" class="downloadFile" data-index="${noOfRecords}">${uploadFileObj.originalFileName}</a>`;
      if (typeof uploadFileObj.uploadedFileName === "undefined")
        fileName = uploadFileObj.originalFileName;

      let button = "";
      if (this.accessRights.isDelete)
        button = `<button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeFile">
          <i class="fa fa-trash"></i></button>`;

      let resHtml = `<tr>;
          <td>${fileName}</td>
          <td>${this.utils.formatDateToStr(uploadFileObj.uploadedDatetime)}</td>
          <td>${button}</td>
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
      $("#uploadedTORFiles tbody").html("");
      for (let file of this.uploadedFiles) {
        this.addUploadedFileToTable(file);
      }
    } else {
      $("#uploadedTORFiles tbody").html(
        '<tr class="norecords"><td colspan="3">No Records Found</td></tr>'
      );
    }
  }

  removeUploadedFile(appIndex) {
    let fileId;

    if (this.uploadedFiles[appIndex]) {
      fileId = this.uploadedFiles[appIndex].id;

      this.uploadedFiles.splice(appIndex, 1);

      this.api.deleteData("api/tor/removefile/" + fileId).subscribe(
        (upload) => {
          let result: any = upload;
          if (result.isDeleted) this.fillUploadedFilesTable();
        },
        (error) => {
          console.log(error);
        }
      );
    }

    this.fillUploadedFilesTable();
  }

  getTORFiles() {
    this.spinner.show();
    this.api
      .getData("api/tor/getallfiles/" + this.AuditID + "/" + this.id)
      .subscribe(
        (posts) => {
          this.uploadedFiles = posts;
          this.fillUploadedFilesTable();
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  downloadFile(appIndex) {
    this.spinner.show();
    let fileId = this.uploadedFiles[appIndex].id;

    if (this.uploadedFiles[appIndex]) {
      if (typeof fileId === "undefined")
        this.notifyService.error(
          "Internal server error. Please reload the page and try again."
        );
      else {
        this.api.downloadFile("api/tor/downloadfile/" + fileId).subscribe(
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
    this.spinner.hide();
  }

  sendEmail() {
    let postData = {
      Id: this.AuditID,
      ToEmail: this.auditeesEmail,
    };
    this.spinner.show();
    this.api.insertData("api/tor/sendemail", postData).subscribe(
      (response) => {
        this.spinner.hide();
        let result: any = response;
        if (result.sent)
          this.notifyService.success(
            "Email sent successfully to all Audit approvers, team and auditee."
          );
      },
      (error) => {
        this.spinner.hide();

        console.log(error);
      }
    );
    this.spinner.hide();
  }

  getAuditees(auditees) {
    let auditee = "";

    auditees.forEach((element) => {
      if (element.user != null) {
        auditee +=
          element.user.firstName +
          " " +
          element.user.lastName +
          " (" +
          element.user.designation +
          "), ";

        this.auditeesEmail.push(element.user.emailId);
      }
    });

    return auditee.trim().slice(0, -1);
  }

  checkAccess() {
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "planning"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "tor"
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
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.AuditID = localStorage.getItem("auditId");

    this.getTor();

    $('#planningTab a[href="#TOR"]').on("click", (e) => {
      this.getTor();
    });

    $("#torComponent").on("click", ".editTorRow", (e) => {
      let trElem = $(e.target).closest("tr");
      let dataIndex = trElem.attr("data-index");
      let dataOf = trElem.attr("data-of");
      let modalId = `#${dataOf}Modal`;
      this.handleModal.edit(modalId, dataIndex, dataOf);
    });

    $("#torComponent").on("click", ".deleteTorRow", (e) => {
      let trElem = $(e.target).closest("tr");
      let dataIndex = trElem.attr("data-index");
      let dataOf = trElem.attr("data-of");
      if (dataOf.toLowerCase() === "auditinfo") {
        let dataId = trElem.attr("data-id");
        this.handleAuditInfo.delete(dataId, dataIndex, dataOf);
      } else {
        this.handleTORTables.remove(dataIndex, dataOf);
      }
    });

    $("#uploadedTORFiles").on("click", ".removeFile", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.removeUploadedFile(parseInt(dataIndex));
      }
    });

    $("#uploadedTORFiles").on("click", ".downloadFile", (e) => {
      let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
      if (dataIndex) {
        this.downloadFile(parseInt(dataIndex));
      }
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
