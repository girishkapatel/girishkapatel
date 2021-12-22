import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../common/table/table.model";
import { UserService } from "./users.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../services/utils/utils.service";
import { CommonApiService } from "../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-users",
  templateUrl: "./users.component.html",
  styleUrls: ["./users.component.css"],
  providers: [UserService],
})
export class UsersComponent implements OnInit {
  constructor(
    private user: UserService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  @ViewChild("userForm", { static: false }) userForm: NgForm;
  @ViewChild("fileInput", { static: false }) fileInput;
  @ViewChild("fileInputStackHolder", { static: false }) fileInputStackHolder;

  accessRights: any = {};
  isStackHolder: boolean = false;
  userType: string = "";
  userTitle: string = "";
  tableId: string = "user_table";
  stakeHolderTableId: string = "stakeHolderTable";

  userOpts: any = [];

  tableColumns: tableColumn[] = [
    {
      title: "Name",
      data: "firstName",
      render: function (data, type, row) {
        let firstName = row.firstName;
        let middleName = row.middleName;
        let lastName = row.lastName;

        return `${firstName} ${middleName} ${lastName}`;
      },
    },
    {
      title: "Role",
      data: "role",
      render: (data, row, rowData) => {
        if (rowData.role)
          return rowData.role.name;
        else return "";
      },
    },
    {
      title: "Designation",
      data: "designation",
    },
    {
      title: "Work Experience",
      data: "experiance",
    },
    {
      title: "Qualification",
      data: "qualification",
    },
    {
      title: "Email Address",
      data: "emailId",
    },
    {
      title: "Report To",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.reportTo)
          return rowData.reportTo.firstName + " " + rowData.reportTo.lastName;
        else return "";
      },
    },
    {
      title: "Status",
      data: "isActive",
      render: function (status) {
        return status ? "ACTIVE" : "INACTIVE";
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editUser"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteUser"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  StackHoldertableColumns: tableColumn[] = [
    {
      title: "Name",
      data: "firstName",
      render: function (data, type, row) {
        let firstName = row.firstName;
        let middleName = row.middleName;
        let lastName = row.lastName;

        return `${firstName} ${middleName} ${lastName}`;
      },
    },
    {
      title: "Role",
      data: "role",
      render: (data, row, rowData) => {
        if (rowData.role)
          return rowData.role.name;
        else return "";
      },
    },
    {
      title: "Designation",
      data: "designation",
    },
    {
      title: "Work Experience",
      data: "experiance",
    },
    {
      title: "Qualification",
      data: "qualification",
    },
    {
      title: "Email Address",
      data: "emailId",
    },
    {
      title: "Report To",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.reportTo)
          return rowData.reportTo.firstName + " " + rowData.reportTo.lastName;
        else return "";
      },
    },
    {
      title: "Status",
      data: "isActive",
      render: function (status) {
        return status ? "ACTIVE" : "INACTIVE";
      },
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editUser"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteUser"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  roleOptions: any = [];
  designationOptions: any = [];

  userId: string = "";
  firstName: string = "";
  middleName: string = "";
  lastName: string = "";
  roleId: string = "";
  roleName: string = "";
  reportToId: string = "";
  designation: string = "";
  qualification: string = "";
  exp: string = "";
  emailId: string = "";
  password: string = "";
  status: boolean = true;
  employeeCode: string = "";

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.tableFilters.next({});
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
      this.loadUserTable();
      this.loadStackHolderUserTable();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveUser(e) {
    if (this.userForm.invalid) return false;
    if (this.isEdit) {
      this.updateUser();
    } else {
      this.addNewUser();
    }
  }

  addNewUser() {
    let postData = this.userForm.form.value;
    postData.IsActive = postData.IsActive.toString() === "true" ? true : false;
    postData.Experiance = parseInt(postData.Experiance);
    postData.ReportToId =
      postData.reportToId === "" ? null : postData.reportToId;
    let apiUrl = this.resolveApi();

    this.user.addUser(apiUrl, postData).subscribe(
      (response) => {
        this.notifyService.success("User Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateUser() {
    let postData = this.userForm.form.value;
    postData.id = this.userId;
    postData.IsActive = postData.IsActive.toString() === "true" ? true : false;
    postData.ReportToId =
      postData.reportToId === "" ? null : postData.reportToId;
    postData.Experiance = parseInt(postData.Experiance);
    let apiUrl = this.resolveApi();

    this.user.updateUser(apiUrl, postData).subscribe(
      (response) => {
        this.notifyService.success("User Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addUser(userType) {
    this.setUserType(userType);
    this.fillRolesOpt();
    this.fillDesignationsOpt();
    this.fillUserOptions();
    this.handleFormView.show();
  }

  setUserType(userType) {
    this.userType = userType ? userType : "user";
    this.userTitle = userType && userType === "user" ? "Users" : "Stakeholders";
  }

  editUser(userData) {
    this.isEdit = true;
    this.userId = userData.id;
    this.firstName = userData.firstName;
    this.middleName = userData.middleName;
    this.lastName = userData.lastName;
    this.roleId = userData.role?userData.role.id:null;
    this.reportToId = userData.reportToId;
    this.roleName = userData.rolename;
    this.designation = userData.designation;
    this.qualification = userData.qualification;
    this.exp = userData.experiance;
    this.emailId = userData.emailId;
    this.password = userData.password;
    this.status = userData.isActive;
    let userType = userData.stakeHolder ? "stakeholder" : "user";
    this.employeeCode = userData.employeeCode;

    this.setUserType(userType);
    this.fillRolesOpt();
    this.fillDesignationsOpt();
    this.fillUserOptions();
    this.handleFormView.show();
  }

  deleteUser(userData) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);

    if (isConfirm) {
      let userType = userData.stakeHolder ? "stakeholder" : "user";

      this.setUserType(userType);

      let apiUrl = this.resolveApi();

      this.user.deleteUser(apiUrl + "/" + userData.id).subscribe(
        (response) => {
          this.notifyService.success("User Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          this.notifyService.error(error.error);
        }
      );
    }
  }

  resolveApi() {
    let apiUrl =
      this.userType === "user"
        ? "user"
        : this.userType === "stakeholder"
        ? "stakeholder"
        : "";
    apiUrl = "api/" + apiUrl;
    return apiUrl;
  }

  clearform() {
    this.userId = "";
    this.firstName = "";
    this.middleName = "";
    this.lastName = "";
    this.roleId = "";
    this.reportToId = "";
    this.roleName = "";
    this.designation = "";
    this.qualification = "";
    this.exp = "";
    this.emailId = "";
    this.password = "";
    this.status = true;
    this.userType = "";
    this.userTitle = "";
    this.employeeCode = "";
  }

  fillRolesOpt() {
    this.spinner.show();
    this.commonApi.getRoles().subscribe(
      (posts) => {
        this.roleOptions = posts;
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  fillDesignationsOpt() {this.spinner.show();
    this.commonApi.getDesignations().subscribe((posts) => {
      this.designationOptions = posts;
      this.spinner.hide();
    },
    (error) => {
      this.spinner.hide();
      console.log(error);
    });
  }
  fillUserOptions() {
    this.spinner.show();
    this.commonApi.getUsers().subscribe((posts) => {
      this.userOpts = posts;
      this.spinner.hide();
    },
    (error) => {
      this.spinner.hide();
      console.log(error);
    });
  }
  importUserExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    this.user.importFromExcel("api/user/importexcel", formData).subscribe(
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
        this.fileInput.nativeElement.value = "";
        this.handleFormView.hide();
      },
      (error) => {
        this.spinner.hide();
        this.notifyService.error(error.error);
      }
    );
  }
  ExportUser() {this.spinner.show();
    this.user.exportToExcel("api/user/downloadexcel").subscribe((blob) => {
      const objblob: any = new Blob([blob], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });

      let link = document.createElement("a");

      if (link.download !== undefined) {
        let url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", "User.xlsx");
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
  SampleExportUser() {this.spinner.show();
    this.user.exportToExcel("api/user/sampledownloadexcel").subscribe((blob) => {
      const objblob: any = new Blob([blob], {
        type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      });

      let link = document.createElement("a");

      if (link.download !== undefined) {
        let url = URL.createObjectURL(blob);
        link.setAttribute("href", url);
        link.setAttribute("download", "SampleUser.xlsx");
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
  importStackHolderExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInputStackHolder.nativeElement.files[0]);
    this.user
      .importFromExcel("api/stakeholder/importexcel", formData)
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
          this.fileInputStackHolder.nativeElement.value = "";
          this.handleFormView.hide();
        },
        (error) => {
          this.spinner.hide();
          this.notifyService.error(error.error);
        }
      );
  }

  ExportStackHolder() {
    this.spinner.show();
    this.user
      .exportToExcel("api/stakeholder/downloadexcel")
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "StakeHolder.xlsx");
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
  SampleExportStackHolder() {
    this.spinner.show();
    this.user
      .exportToExcel("api/stakeholder/sampledownloadexcel")
      .subscribe((blob) => {
        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleStakeHolder.xlsx");
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
  checkAccess() {
    this.accessRights = this.utils.getSubmoduleAccess("users")[0];
  }
  async loadUserTable() {
    this.spinner.show();
    await this.user.getUser(`api/user`).subscribe((dtData) => {
      this.commonApi.initialiseTable(this.tableId, dtData, this.tableColumns);
      this.spinner.hide();
    });
  }
  async loadStackHolderUserTable() {
    this.spinner.show();
    await this.user.getUser(`api/stakeholder`).subscribe((dtData) => {
      this.commonApi.initialiseTable(
        this.stakeHolderTableId,
        dtData,
        this.StackHoldertableColumns
      );
      this.spinner.hide();
    });
  }
  ngOnInit() {
    this.checkAccess();
    this.loadUserTable();
    this.loadStackHolderUserTable();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#usersComponent").on("click", ".editUser", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        // let userData = $("#" + dataId).data();
        let userData = window["jQuery"]("#" + dataId).data();
        this.editUser(userData);
      });

      $("#usersComponent").on("click", ".deleteUser", (e) => {
        let userId = $(e.currentTarget).attr("id");
        // let userData = $("#" + userId).data();
        let userData = window["jQuery"]("#" + userId).data();
        this.deleteUser(userData);
      });
      // $(".wrapper1").scroll(function () {
      //   $(".wrapper2").scrollLeft($(".wrapper1").scrollLeft());
      // });
      // $(".wrapper2").scroll(function () {
      //   $(".wrapper1").scrollLeft($(".wrapper2").scrollLeft());
      // });
      // $(".div1").width($("table").width());
      // $(".div2").width($("table").width());

      // $(".wrapper3").scroll(function () {
      //   $(".wrapper4").scrollLeft($(".wrapper3").scrollLeft());
      // });
      // $(".wrapper4").scroll(function () {
      //   $(".wrapper3").scrollLeft($(".wrapper4").scrollLeft());
      // });
      // $(".div3").width($("table").width());
      // $(".div4").width($("table").width());
    });
  }
}
