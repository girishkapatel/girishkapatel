import { Component, OnInit, ViewChild, Input } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { ApiService } from "src/app/services/api/api.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import * as ClassicEditor from "../../../../../assets/ckeditor5/build/ckeditor";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { NgbDateStruct } from "@ng-bootstrap/ng-bootstrap";
import { IDropdownSettings } from "ng-multiselect-dropdown";

@Component({
  selector: "app-datatracker",
  templateUrl: "./datatracker.component.html",
  styleUrls: ["./datatracker.component.css"],
  providers: [],
})
export class DataTrackerComponent implements OnInit {
  constructor(
    private api: ApiService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private spinner: NgxSpinnerService,
    private notifyService: ToastrService
  ) { }

  @ViewChild("datatrackerForm", { static: false }) datatrackerForm: NgForm;
  @ViewChild("fileInputDataTracker", { static: false }) fileInputDataTracker;
  @ViewChild("fileInput", { static: false }) fileInput;
  OwnerdropdownSettings: IDropdownSettings = {};
  accessRights: any = {};
  public Editor = ClassicEditor;
  isStackHolder: boolean = false;
  tableId: string = "datatracker_table";
  // tableFilters;
  tableApiUrl: string = "";
  selectedImplementationOwnerId = [];
  notOverdueTotal: number = 0;
  onlyOverdueTotal: number = 0;
  dtOverdueTotal: number = 0;

  partiallyReceivedTotal: number = 0;
  pendingTotal: number = 0;
  receivedTotal: number = 0;
  statusTotal: number = 0;
  isProcessOwner: boolean = false;
  isEdit: boolean = false;
  formVisible: boolean = false;

  filterOption: number = 0;
  filterStatus: string = "all";

  @Input() shOpts: any;
  id: string = "";
  AuditID: string = "";

  Area: string = "";
  DataRequested: string = "";
  Status: string = "";
  ProcessOwnerId: string = "";
  DataRequestDate: NgbDateStruct;
  DataReceivedDate: NgbDateStruct;
  PendingData: string = "";

  uploadedFiles: any = [];
  selectedFiles: any = [];

  uploadedFileIndex: number;

  selectedIds: any = [];

  mailTotal: number = 0;
  mailSent: number = 0;

  tableColumnsDataTracker: tableColumn[] = [
    {
      title: "<input type='checkbox' id='chkAllDataTracker' />",
      data: "id",
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
      title: "Audit",
      data: "audit",
      render: function (data) {
        return data && data.processLocationMapping
          ? data.processLocationMapping.auditName
          : "";
      },
    },
    {
      title: "Location",
      data: "audit.location",
      render: function (data) {
        return data && data.profitCenterCode ? data.profitCenterCode : "";
      },
    },
    {
      title: "Area",
      data: "area",
    },
    {
      title: "Data Requested",
      data: "dataRequested",
      render: (data) => {
        if (data.length > 50) {
          return (
            "<span>" +
            data.slice(0, 50) +
            '</span><br><a href="javascript:void(0)" data-title="Data Requested" data-content="' +
            encodeURI(data) +
            '" class="viewContent">...View More</a>'
          );
        } else {
          return data;
        }
      },
    },
    {
      title: "Process Owner",
      data: "processOwner",
      render: (data) => {
        if (data != null)
          return data ? `${data.firstName} ${data.lastName}` : "";
        return "";
      },
    },
    {
      title: "Data Request Date",
      data: "dataRequestDate",
      render: (data) => {
        if (data != null) return this.utils.formatDbDateToDMY(data);
        return "";
      },
    },
    {
      title: "Data Received Date",
      data: "dataReceivedDate",
      render: (data) => {
        if (data != null) return this.utils.formatDbDateToDMY(data);
        return "";
      },
    },
    {
      title: "Status",
      data: "status",
      render: (data) => {
        if (data) {
          let d = data.replace(" ", "").toLowerCase();
          let bgColor = this.utils.dtRatingColor[d];
          let textColor = "#ffffff";
          return `<div style="padding: 3px 10px; background:${bgColor}; color:${textColor}"  class="text-capitalize">${data ? data.toUpperCase() : ""
            }</div>`;
        } else {
          return "INPROGRES";
        }
      },
    },
    {
      title: "Overdue (In Days)",
      data: "overdueInDays",
    },
    {
      title: "Actions",
      data: "id",
      className: "text-center",
      render: (data, type, row, meta) => {
        let buttons = "";

        if (this.accessRights.isEdit)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editDataTracker" title="Edit record"><i class="fa fa-edit"></i></button>';
        if (!this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-primary emailDataTracker" title="Send email"><i class="fa fa-send"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteDataTracker" title="Delete record"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
      // this.tableFilters = new BehaviorSubject({ init: true });
      this.loadDataTrackerTable();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveDataTracker(e) {
    if (this.datatrackerForm.invalid) {
      return false;
    }
    e.preventDefault();
    if (this.isEdit) {
      this.updateDataTracker();
    } else {
      this.addNewDataTracker();
    }
  }

  addNewDataTracker() {
    let postData = this.datatrackerForm.form.value;
    postData.AuditID = this.AuditID;
    postData.DataRequestDate = this.utils.formatNgbDateToYMD(
      postData.DataRequestDate
    );
    postData.DataReceivedDate = this.utils.formatNgbDateToYMD(
      postData.DataReceivedDate
    );
    postData.ProcessOwnerId = this.getImplementionOwnerIds();
    this.api.insertData("api/initialdatarequest", postData).subscribe(
      (response) => {
        let result: any = response;
        this.id = result.id;
        this.AuditID = result.auditId;

        if (this.selectedFiles.length > 0) this.saveSelectedFiles();

        this.notifyService.success("Data Request added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        if (error.status == 406) {
          this.notifyService.error(
            "Looks like the selected record reference has been given in following places: State, City, Location, Company. Hence, you cannot delete the selected record"
          );
        } else {
          this.notifyService.error(error.error);
        }
      }
    );
  }

  updateDataTracker() {
    let postData = this.datatrackerForm.form.value;
    postData.id = this.id;
    postData.AuditID = this.AuditID;
    postData.DataRequestDate = this.utils.formatNgbDateToYMD(
      postData.DataRequestDate
    );
    postData.DataReceivedDate = this.utils.formatNgbDateToYMD(
      postData.DataReceivedDate
    );
    postData.ProcessOwnerId = this.getImplementionOwnerIds();
    this.api.updateData("api/initialdatarequest", postData).subscribe(
      (response) => {
        if (this.selectedFiles.length > 0) this.saveSelectedFiles();

        this.notifyService.success("Data Request Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  addDataTracker() {
    this.handleFormView.show();
  }

  editDataTracker(data) {
    if (data.processOwnerId == localStorage.getItem("userId"))
      this.isProcessOwner = true;
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    this.isEdit = true;
    this.id = data.id;
    this.Area = data.area;
    this.DataRequested = data.dataRequested;
    this.Status = data.status;
    this.selectedImplementationOwnerId =
      this.getSelectedImplementationOwnerOpts(data.processOwnerId);
    this.DataRequestDate = this.utils.formatToNgbDate(data.dataRequestDate);
    this.DataReceivedDate = this.utils.formatToNgbDate(data.dataReceivedDate);
    this.PendingData = data.pendingData;
    this.uploadedFiles = [];
    this.selectedFiles = [];
    this.handleFormView.show();
  }

  deleteDataTracker(id) {
    let isConfirm = confirm(
      "Are you sure you want to delete this data request?"
    );
    if (isConfirm) {
      this.api.deleteData("api/initialdatarequest/" + id).subscribe(
        (response) => {
          this.notifyService.success("Data Request Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
        }
      );
    }
  }

  clearform() {
    this.id = "";
    this.Area = "";
    this.DataRequested = "";
    this.Status = "";
    this.ProcessOwnerId = "";
    this.DataRequestDate = null;
    this.DataReceivedDate = null;
    this.PendingData = "";
    this.fileInputDataTracker.nativeElement.value = "";
    this.selectedImplementationOwnerId = [];
  }

  uploadDataTrackerFile() {
    let filesElem = this.fileInputDataTracker.nativeElement;

    if (filesElem.files.length) {
      for (let file of filesElem.files) {
        this.selectedFiles.push(file);

        let fileModel = {
          originalFileName: file.name,
          uploadedDatetime: this.utils.getCurrentDate(),
        };

        this.addUploadedFileToTable(fileModel, true);
      }

      this.fileInputDataTracker.nativeElement.value = "";
    }
  }

  addUploadedFileToTable(uploadFileObj, isNew?: boolean) {
    let uploadedFilesTable = $("#uploadedDataTrackerFiles tbody");
    let noOfRecords = uploadedFilesTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#uploadedDataTrackerFiles tbody .norecords").length) {
        $("#uploadedDataTrackerFiles tbody .norecords").remove();
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
      $("#uploadedDataTrackerFiles tbody").html("");

      for (let file of this.uploadedFiles) {
        this.addUploadedFileToTable(file);
      }
    } else {
      $("#uploadedDataTrackerFiles tbody").html(
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
        this.api
          .deleteData("api/initialdatarequest/removefile/" + fileId)
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

  getDataTrackerFiles() {
    this.api
      .getData(
        "api/initialdatarequest/getallfiles/" + this.AuditID + "/" + this.id
      )
      .subscribe(
        (posts) => {
          this.uploadedFiles = posts;

          this.fillUploadedFilesTable();
        },
        (error) => {
          console.log(error);
        }
      );
  }

  emailDataTracker(id) {
    let postData = {
      Id: id,
    };

    this.api.insertData("api/initialdatarequest/sendemail", postData).subscribe(
      (response) => {
        let result: any = response;

        if (result.sent)
          this.notifyService.success(
            "Initial data request mail sent successfully to process owner."
          );
        this.loadDataTrackerTable();
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

    formData.append("Id", this.id);
    formData.append("auditId", this.AuditID);
    formData.append("module", "datatracker");

    this.api
      .insertData("api/initialdatarequest/uploadfile", formData)
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
  }

  sendMultipleEmail() {
    if (this.selectedIds.length == 0)
      this.notifyService.error(
        "Please select at least one record to send email."
      );
    else {
      window["jQuery"]("#sendMultipleMailModal").modal("show");
      this.mailTotal = this.selectedIds.length;
      this.mailSent = 0;
      let sentMailCounter = 0;
      this.spinner.show();
      this.selectedIds.forEach((element, index) => {
        let postData = {
          Id: element,
        };
        this.api
          .insertData("api/initialdatarequest/sendemail", postData)
          .subscribe(
            (response) => {
              let result: any = response;
              if (result.sent) {
                sentMailCounter++;
                this.mailSent = sentMailCounter;
              }
            },
            (error) => {
              this.spinner.hide();
              console.log(error);
            }
          );
      });
      this.spinner.hide();
      this.selectedIds = [];
      this.loadDataTrackerTable();
    }
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
        this.api
          .downloadFile("api/initialdatarequest/downloadfile/" + fileId)
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

  filterData(option) {
    this.filterOption = option;
    this.loadDataTrackerTable();
    // this.AuditID = localStorage.getItem("auditId");
    // this.tableApiUrl = `api/initialdatarequest/GetByAudit/${this.AuditID}/${this.filterOption}`;
    // this.tableFilters.next({});
  }

  filterDataByStatus(status) {
    this.filterStatus = status;
    this.loadDataTrackerTable();
  }

  loadDataTrackerTable() {
    this.getSummary();
    this.spinner.show();
    this.api
      .getData(
        `api/initialdatarequest/GetByAudit/${this.AuditID}/${this.filterOption}/${this.filterStatus}`
      )
      .subscribe(
        (dtData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            dtData,
            this.tableColumnsDataTracker, true
          );
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  getSummary() {
    this.spinner.show();
    this.api
      .getData("api/initialdatarequest/getsummary/" + this.AuditID)
      .subscribe(
        (response) => {
          let objResult: any = response;
          this.spinner.hide();
          this.notOverdueTotal = objResult.notOverdue || 0;
          this.onlyOverdueTotal = objResult.onlyOverdue || 0;
          this.dtOverdueTotal = this.notOverdueTotal + this.onlyOverdueTotal;

          this.partiallyReceivedTotal = objResult.partiallyReceived || 0;
          this.pendingTotal = objResult.pending || 0;
          this.receivedTotal = objResult.received || 0;
          this.statusTotal =
            this.partiallyReceivedTotal +
            this.pendingTotal +
            this.receivedTotal;
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  exportToExcelDT() {
    this.spinner.show();
    this.api
      .downloadFile(`api/initialdatarequest/downloadexcel/${this.AuditID}`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "DataTracker.xlsx");
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
  sampleExportToExcelDT() {
    this.spinner.show();
    this.api
      .downloadFile(`api/initialdatarequest/sampledownloadexcel/${this.AuditID}`)
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "SampleDataTracker.xlsx");
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

  exportToPDFDT() {
    this.spinner.show();
    this.api
      .downloadFile(`api/initialdatarequest/downloadpdf/${this.AuditID}`)
      .subscribe(
        (blob) => {
          // const blobUrl = URL.createObjectURL(blob);
          // window.open(blobUrl);

          const objblob: any = new Blob([blob], {
            type: "application/pdf",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "DataTracker.pdf");
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

  refreshTable() {
    this.filterOption = 0;
    this.filterStatus = "all";
    this.loadDataTrackerTable();
  }

  checkAccess() {
    let planningModule = this.utils.getAccessOnLevel1(
      "manageaudits",
      "planning"
    );

    this.accessRights = planningModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "datatracker"
    )[0];
  }
  importExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    var userid = localStorage.getItem("userId");
    this.api
      .insertData(
        "api/initialdatarequest/importexcel/" + this.AuditID + "/" + userid,
        formData
      )
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
          this.handleFormView.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
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
    this.loadDataTrackerTable();

    // this.tableApiUrl = `api/initialdatarequest/GetByAudit/${this.AuditID}/0`;
    // this.tableFilters = new BehaviorSubject({ init: true });

    $(document).ready(() => {
      $("#dataTrackerComponent").on("click", ".editDataTracker", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        // let data = $("#" + dataId).data();
        let data = window["jQuery"]("#" + dataId).data();

        this.editDataTracker(data);
        this.getDataTrackerFiles();
      });

      $("#dataTrackerComponent").on("click", ".deleteDataTracker", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.deleteDataTracker(dataId);
      });

      $("#dataTrackerComponent").on("click", ".emailDataTracker", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.emailDataTracker(dataId);
      });

      $("#dataTrackerComponent").on("change", "#chkAllDataTracker", (e) => {
        $("#datatracker_table > tbody > tr")
          .find(".chkSingleDataTracker")
          .prop("checked", $(e.currentTarget).is(":checked"));

        let Ids: any = [];
        $("#datatracker_table > tbody > tr").each(function () {
          let row = $(this);
          Ids.push(row.attr("id"));
        });

        if ($(e.currentTarget).is(":checked")) this.selectedIds = Ids;
        else this.selectedIds = [];
      });

      $("#dataTrackerComponent").on("change", ".chkSingleDataTracker", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        if ($(e.currentTarget).is(":checked")) this.selectedIds.push(dataId);
        else {
          this.selectedIds.forEach((element, index) => {
            if (element == dataId) delete this.selectedIds[index];
          });
        }

        if (
          $("#datatracker_table > tbody > tr").find(".chkSingleDataTracker")
            .length ==
          $("#datatracker_table > tbody > tr").find(
            ".chkSingleDataTracker:checked"
          ).length
        )
          $("#datatracker_table > thead > tr")
            .find("#chkAllDataTracker")
            .prop("checked", true);
        else
          $("#datatracker_table > thead > tr")
            .find("#chkAllDataTracker")
            .prop("checked", false);
      });

      $('#planningTab a[href="#DataTracker"]').on("click", (e) => {
        // if (typeof this.tableFilters !== "undefined") {
        // this.tableFilters.next({ init: true });
        // }
        this.loadDataTrackerTable();
      });

      $("#uploadedDataTrackerFiles").on("click", ".removeFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeUploadedFile(parseInt(dataIndex));
        }
      });

      $("#uploadedDataTrackerFiles").on("click", ".downloadFile", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.downloadFile(parseInt(dataIndex));
        }
      });
      // $(".wrapper1").scroll(function () {
      //   $(".wrapper2").scrollLeft($(".wrapper1").scrollLeft());
      // });
      // $(".wrapper2").scroll(function () {
      //   $(".wrapper1").scrollLeft($(".wrapper2").scrollLeft());
      // });
      // $(".div1").width($("table").width());
      // $(".div2").width($("table").width());
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
