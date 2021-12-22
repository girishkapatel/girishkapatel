import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { CompanyService } from "./company.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-company",
  templateUrl: "./company.component.html",
  styleUrls: ["./company.component.css"],
  providers: [CompanyService],
})
export class CompanyComponent implements OnInit {
  constructor(
    private company: CompanyService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("companyForm", { static: false }) companyForm: NgForm;

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

  tableId: string = "company_table";

  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Company Name",
      data: "name",
    },
    {
      title: "City",
      data: "cityOrTown.name",
      render: function (city) {
        return city ? city : "";
      },
    },
    {
      title: "Pan",
      data: "panNo",
    },
    {
      title: "GST No",
      data: "gstno",
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
            '" class="btn btn-sm btn-info editCompany"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete  && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteCompany"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  userOpts: any = [];
  countryOpts: any = [];
  stateOpts: any = [];
  cityOpts: any = [];

  companyName: string = "";
  companyId: string = "";
  panNo: string = "";
  gstNo: string = "";
  countryId: string = "";
  country: string = "";
  stateId: string = "";
  state: string = "";
  cityId: string = "";
  city: string = "";
  coordinatorId: string = "";
  coordinator: string = "";
  auditManagerId: string = "";
  auditManager: string = "";
  headOfAuditId: string = "";
  headOfAudit: string = "";

  formVisible: boolean = false;

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

  saveCompany(e) {
    if (this.companyForm.invalid) return false;
    if (this.isEdit) {
      this.updateCompany();
    } else {
      this.addNewCompany();
    }
  }

  addNewCompany() {
    let postData = this.companyForm.form.value;
    postData.AuditApprovalMapping = this.getAuditApprovalMapping();
    postData.IsActive = true;

    this.company.addCompany("api/company", postData).subscribe(
      (response) => {
        this.notifyService.success("Company Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateCompany() {
    let postData = this.companyForm.form.value;
    postData.AuditApprovalMapping = this.getAuditApprovalMapping();
    postData.id = this.companyId;
    postData.IsActive = true;
    this.company.updateCompany("api/company", postData).subscribe(
      (response) => {
        this.notifyService.success("Company Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        if (error.status == 406) {
          this.notifyService.error(
            "Looks like the selected record reference has been given in following places: Location. Hence, you cannot delete the selected record"
          );
        } else {
          this.notifyService.error(error.error);
        }
      }
    );
  }

  addCompany() {
    $("#selectedApprovers tbody").html("");

    this.approvers = [];

    this.fillUserOptions();
    this.fillCountryOptions();
    this.handleFormView.show();
  }

  editCompany(companyData) {
    this.isEdit = true;
    this.companyId = companyData.id;
    this.companyName = companyData.name;
    this.panNo = companyData.panNo;
    this.gstNo = companyData.gstno;
    this.countryId = companyData.countryId;
    this.country = companyData.country;
    this.stateId = companyData.stateId;
    this.state = companyData.state;
    this.cityId = companyData.cityId;
    this.city = companyData.city;
    this.coordinatorId = companyData.coordinatorId;
    this.coordinator = companyData.coordinator;
    this.auditManagerId = companyData.auditManagerId;
    this.auditManager = companyData.auditManager;
    this.headOfAuditId = companyData.headOfAuditId;
    this.headOfAudit = companyData.headOfAudit;
    this.approvers =
      companyData.auditApprovalMapping &&
      companyData.auditApprovalMapping.userData
        ? companyData.auditApprovalMapping.userData
        : [];
    this.fillUserOptions();
    this.fillCountryOptions();
    this.getStateOptions();
    this.getCityOptions();
    this.handleFormView.show();
    this.fillApproverTable();
  }

  deleteCompany(companyId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.company.deleteCompany("api/company/" + companyId).subscribe(
        (response) => {
          this.notifyService.success("Company Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
    }
  }

  clearform() {
    this.companyId = "";
    this.companyName = "";
    this.panNo = "";
    this.gstNo = "";
    this.countryId = "";
    this.country = "";
    this.stateId = "";
    this.state = "";
    this.cityId = "";
    this.city = "";
    this.coordinatorId = "";
    this.coordinator = "";
    this.auditManagerId = "";
    this.auditManager = "";
    this.headOfAuditId = "";
    this.headOfAudit = "";
  }

  fillUserOptions() {
    this.commonApi.getUsers().subscribe((posts) => {
      this.userOpts = posts;
    });
  }

  fillCountryOptions() {
    this.commonApi.getCountry().subscribe((posts) => {
      this.countryOpts = posts;
    });
  }

  getStateOptions() {
    if (this.countryId) {
      this.commonApi.getState(this.countryId).subscribe((posts) => {
        this.stateOpts = posts;
      });
    } else {
      this.stateId = "";
      this.stateOpts = [];
      this.cityId = "";
      this.cityOpts = [];
    }
  }

  getCityOptions() {
    if (this.stateId) {
      this.commonApi.getCity(this.stateId).subscribe((posts) => {
        this.cityOpts = posts;
      });
    } else {
      this.cityId = "";
      this.cityOpts = [];
    }
  }

  getAuditApprovalMapping() {
    let apObj = {
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

  removeApprover(appIndex) {
    if (this.approvers[appIndex]) {
      this.approvers.splice(appIndex, 1);
    }
    this.fillApproverTable();
  }

  editApprover(appIndex) {
    let appData = this.approvers[appIndex];
    this.clearApproverForm();
    this.fillApproverEdit(appData);
  }

  addNewAprrover() {
    this.approverIndex = -1;
    this.isEditApprover = false;
    this.clearApproverForm();
    this.showApproverForm();
  }

  addApprover() {
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appoverSelected = this.selectedApprover;
      appoverSelected.responsibility = this.appResponsibility;
      this.addApproverToTable(appoverSelected, true);
      this.selectedApprover = {};
    }
  }

  saveApprover() {
    if (!this.utils.isEmptyObj(this.selectedApprover)) {
      let appSelected = this.selectedApprover;
      this.addApproverToTable(appSelected, false);
      this.selectedApprover = {};
    }
  }

  showApproverForm() {
    window["jQuery"]("#manageApprover").modal("show");
  }

  fillApproverEdit(approverData) {
    this.selectedApprover = approverData;
    this.approver = approverData.id;
    this.appDesignation = approverData.designation;
    this.appQualification = approverData.qualification;
    this.appExperience = approverData.experiance;
    this.showApproverForm();
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

  clearApproverForm() {
    this.approver = "";
    this.appDesignation = "";
    this.appQualification = "";
    this.appExperience = "";
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "company");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#companyComponent").on("click", ".editCompany", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let companyData = $("#" + dataId).data();
        this.editCompany(companyData);
      });

      $("#companyComponent").on("click", ".deleteCompany", (e) => {
        let companyId = $(e.currentTarget).attr("data-id");
        this.deleteCompany(companyId);
      });

      $("#companyComponent").on("click", ".removeApprover", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");
        if (dataIndex) {
          this.removeApprover(parseInt(dataIndex));
        }
      });

      $("#companyComponent").on("click", ".editApprover", (e) => {
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
