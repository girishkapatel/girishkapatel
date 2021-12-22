import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "../../../common/table/table.model";
import { OverallAssessmentService } from "./overallassessment.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-overallassessment",
  templateUrl: "./overallassessment.component.html",
  styleUrls: ["./overallassessment.component.css"],
  providers: [OverallAssessmentService],
})
export class OverallAssessmentComponent implements OnInit {
  constructor(
    private overallassessment: OverallAssessmentService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("auditForm", { static: false }) auditForm: NgForm;
  @ViewChild("apForm", { static: false }) apForm: NgForm;
  accessRights: any = {};
  isStackHolder: boolean = false;
  rowWiseLocations: any = [];
  locations: any = [];
  selectedIds: any = [];
  id: string = "";
  email_id: string = "";
  Email: string = "";
  coverage: string = "";
  justification: string = "";
  lastAudited: string = "";

  tableScroll: boolean = false;
  singleMail: boolean = false;

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
      title: "Audit Name",
      data: "",
      render: (data, row, rowData) => {
        return rowData.processLocationMappings[0].auditName;
      },
    },
    {
      title: "Locations",
      data: "",
      render: (data, row, rowData) => {
        return this.getLocationNames(
          rowData.processLocationMappings[0].locations
        );
      },
    },
    //   {
    //     title:'Process Model',
    //     data:'processLocationMappings',
    //     render:function(data) {
    //       return Array.isArray(data) ? data : '';
    //     }
    // },
    {
      title: "Overall Risk",
      data: "processRiskMapping.finalProcessrating",
      render: function (data) {
        if (data) {
          let colors = {
            high: "#f04c3e",
            medium: "#FFC200",
            low: "#2c973e",
          };
          return (
            '<span style="padding: 3px 10px; color: #fff;background:' +
            colors[data.toLowerCase()] +
            '">' +
            data +
            "</span>"
          );
        } else {
          return "";
        }
      },
    },
    {
      title: "ERM Risk",
      data: "",
      orderable: false,
      render: (data, type, row) => {
        let isChecked = row.isERMRisks ? "checked" : "";

        return (
          '<input style="pointer-events:none" tabindex="-1" type="checkbox" data-on-color="success" data-on-text="YES" data-off-color="default" data-off-text="NO" class="make-switch" data-size="small" ' +
          isChecked +
          ">"
        );
      },
    },
    {
      title: "Business Initiative",
      data: "keyBusinessInitiative",
      orderable: false,
      render: (data, type, row) => {
        let isChecked = row.isKeyBusiness ? "checked" : "";

        return (
          '<input style="pointer-events:none" tabindex="-1" type="checkbox" data-on-color="success" data-on-text="YES" data-off-color="default" data-off-text="NO" class="make-switch" data-size="small" ' +
          isChecked +
          ">"
        );
      },
    },
    {
      title: "Coverage",
      data: "coverage",
      orderable: false,
      render: (data, type, row) => {
        let isChecked = "";
        let isDisabled = row.isOverallAssesmentWiseAudit ? "disabled" : "";
        if (isDisabled) {
          isChecked = "checked";
        }
        return (
          '<input tabindex="-1" class="auditCoverage" ' +
          isDisabled +
          ' data-id="' +
          row.id +
          '" type="checkbox"  data-on-color="success" data-on-text="YES" data-off-color="default" data-off-text="NO" class="make-switch" data-size="small" ' +
          isChecked +
          ">"
        );
      },
    },
    {
      title: "Justification",
      data: "justification",
      render: (data) => {
        if (data) {
          return data;
        } else {
          return "";
        }
      },
    },
    {
      title: "Last Audited",
      data: "lastaudityear",
      render: (data) => {
        data = data ? data : "";
        return data;
      },
    },

    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data, row, rowData) => {
        let buttons = "";
        let id = rowData.id;
        let isDisabled = rowData.isOverallAssesmentWiseAudit ? "disabled" : "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" data-id="' +
            id +
            '" class="btn btn-sm btn-info editOverallAssessment" ' +
            isDisabled +
            '><i class="fa fa-edit"></i></button>';

        // if (this.accessRights.isAdd)
        //   buttons +=
        //     '<p class="mb-10"><a data-id="' +
        //     id +
        //     '" href="javascript:void(0);" class="btn btn-sm green createAudit" ' +
        //     isDisabled +
        //     '><i class="fa fa-check"></i> Create Audit </a></p>';
        if (!this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            id +
            '" class="btn btn-sm btn-primary emailOverallAssesment" title="Send email"><i class="fa fa-send"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            id +
            '" class="btn btn-sm btn-danger deleteOverallAssessment" ' +
            isDisabled +
            '><i class="fa fa-trash"></i></button>';

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
      this.tableFilters.next({});
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  updateOverallAssessment(e) {
    e.preventDefault();
    let postData = this.auditForm.form.value;
    postData.Coverage =
      postData.Coverage.toLowerCase() === "yes" ? true : false;
    postData.id = this.id;
    this.overallassessment
      .updateOverallAssessment("api/overallassesment", postData)
      .subscribe(
        (response) => {
          window["jQuery"]("#basic").modal("hide");
          this.notifyService.success("Overall Assessment Updated Successfully");
          this.handleFormView.hide();
          this.loadOverallAssessments();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
  }

  addOverallAssessment() {
    this.handleFormView.show();
  }

  editOverallAssessment(auditData) {
    this.id = auditData.id;
    this.coverage = auditData.coverage ? "Yes" : "No";
    this.justification = auditData.justification;
    this.lastAudited = auditData.lastaudityear;
    window["jQuery"]("#basic").modal("show");
  }

  deleteOverallAssessment(oaId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.overallassessment
        .deleteOverallAssessment("api/overallassesment/" + oaId)
        .subscribe(
          (response) => {
            this.handleFormView.hide();
          },
          (error) => {
            console.log(error);
          }
        );
    }
  }

  getLocations() {
    this.spinner.show();
    this.commonApi.getLocations().subscribe(
      (locationData) => {
        this.locations = locationData;
        this.spinner.hide();
        this.loadOverallAssessments();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  createTableColumnConfig() {
    this.spinner.show();
    this.commonApi.getLocations().subscribe(
      (locationData) => {
        this.buildTableConfig(locationData);
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  buildTableConfig(locationData) {
    let tableConfObj = {
      title: "Action",
      orderable: false,
      data: "id",
      render: (data) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" class="btn btn-sm btn-info editOverallAssessment"><i class="fa fa-edit"></i></button>' +
          '<button type="button" data-id="' +
          data +
          '" class="btn btn-sm btn-primary createAudit" title="Create Audit"><i class="fa fa-plus"></i></button>'
        );
      },
    };

    for (let location of locationData) {
      this.rowWiseLocations.push(location.id);
      let tableConfObj = {
        title: location.profitCenterCode
          ? location.profitCenterCode
          : "No Title",
        data: "",
        render: (data, type, row, meta) => {
          let locArray = Array.isArray(row.processLocationMappings[0].locations)
            ? row.processLocationMappings[0].locations
            : [];
          let colsOffset = 7;
          let locIndex = meta.col - colsOffset;
          let checked = "";
          let currentRowLocId = this.rowWiseLocations[locIndex];
          if (locArray.indexOf(currentRowLocId) > -1) {
            checked = "checked";
          }
          return (
            '<input type="checkbox" ' +
            checked +
            ' style="pointer-events:none" tabindex="-1">'
          );
        },
      };
      this.tableColumns.push(tableConfObj);
    }

    this.tableColumns.push(tableConfObj);
    this.loadOverallAssessments();
  }

  loadOverallAssessments() {
    this.spinner.show();
    this.overallassessment
      .getOverallAssessment("api/overallassesment")
      .subscribe(
        (auditData) => {
          this.commonApi.initialiseTable(
            this.tableId,
            auditData,
            this.tableColumns,
            this.tableScroll
          );
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          console.log(error);
        }
      );
  }

  createAudit(id, auditData) {
    let postData = {
      OverallAssesmentId: id,
    };
    let ifYes = confirm("Are you sure you want to Create Audit ?");
    if (ifYes) {
      this.overallassessment
        .addOverallAssessment("api/audit", postData)
        .subscribe(
          (response) => {
            this.notifyService.success("Audit Created Successfully");
            this.loadOverallAssessments();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
    }
  }
  clearform() {
    this.id = "";
    this.coverage = "";
    this.justification = "";
    this.lastAudited = "";
  }

  getLocationNames(locArray) {
    let locationNameArray = [];
    for (let location of this.locations) {
      if (locArray.indexOf(location.id) > -1) {
        locationNameArray.push(
          `<a href="javascript:void(0);" class="btn btn-sm blue mb-10"> ${location.locationDescription} </a>`
        );
      }
    }
    return locationNameArray.join(" ");
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "auditplanningengine",
      "overallassessment"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    //this.createTableColumnConfig();
    this.getLocations();

    $(document).ready(() => {
      $("#oaComponent").on("click", ".editOverallAssessment", (e) => {
        if (!$(e.currentTarget).attr("disabled")) {
          let dataId = $(e.currentTarget).attr("data-id");
          let auditData = window["jQuery"]("#" + dataId).data();
          this.editOverallAssessment(auditData);
        }
      });

      $("#oaComponent").on("click", ".deleteOverallAssessment", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        this.deleteOverallAssessment(dataId);
      });
      $("#oaComponent").on("click", ".emailOverallAssesment", (e) => {
        this.showMailForm();
        let dataId = $(e.currentTarget).attr("data-id");
        this.email_id = dataId;
        this.singleMail = true;
        // this.emailoverallassesment(dataId);
      });

      $("#oaComponent").on("click", ".auditCoverage", (e) => {
        if (!$(e.currentTarget).attr("disabled")) {
          let dataId = $(e.currentTarget).attr("data-id");
          let auditData = window["jQuery"]("#" + dataId).data();
          this.createAudit(dataId, auditData);
        }
        // window["jQuery"]("#" + dataId).;
      });
      $("#oaComponent").on("change", "#chkAllDataTracker", (e) => {
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

      $("#oaComponent").on("change", ".chkSingleDataTracker", (e) => {
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
  }

  exportOverAllAssesment() {
    this.spinner.show();
    this.overallassessment
      .exportToExcel("api/OverallAssesment/downloadexcel")
      .subscribe(
        (blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
          });

          let link = document.createElement("a");

          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "OverAllAssesment.xlsx");
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
  exportToPDF() {
    this.spinner.show();
    this.overallassessment
      .exportToExcel("api/OverallAssesment/downloadpdf")
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/pdf",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "OverallAssesment.pdf");
          // link.setAttribute("download", "DiscussionNotes.pptx");
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
  exportToPPT() {
    this.spinner.show();
    try {
      this.overallassessment
        .exportToPPT("api/OverallAssesment/downloadppt")
        .subscribe((blob) => {
          const objblob: any = new Blob([blob], {
            type: "application/ppt",
          });

          let link = document.createElement("a");
          if (link.download !== undefined) {
            let url = URL.createObjectURL(blob);
            link.setAttribute("href", url);
            link.setAttribute("download", "OverallAssesment.pptx");
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
    this.overallassessment
      .addOverallAssessment("api/OverallAssesment/sendemail", postData)
      .subscribe(
        (response) => {
          let result: any = response;
          if (result.sent)
            this.notifyService.success(
              "OverallAssesment Data mail sent successfully."
            );
          this.hideMailForm();
          this.Email = "";
          this.spinner.hide();
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
    this.overallassessment
      .addOverallAssessment("api/OverallAssesment/sendemail/", postData)
      .subscribe(
        (response) => {
          this.hideMailForm();
          this.Email = "";
          this.spinner.hide();
        },
        (error) => {
          this.spinner.hide();
          this.notifyService.error(error);
        }
      );
    this.loadOverallAssessments();
    this.selectedIds = [];
  }
}
