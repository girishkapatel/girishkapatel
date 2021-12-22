import { ViewChild } from "@angular/core";
import { Component, OnInit } from "@angular/core";
import { NgForm } from "@angular/forms";
import { NgxSpinnerService } from "ngx-spinner";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ApiService } from "../../../services/api/api.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";

@Component({
  selector: "app-activitylogs",
  templateUrl: "./activitylogs.component.html",
  styleUrls: ["./activitylogs.component.css"],
})
export class ActivitylogsComponent implements OnInit {
  accessRights: any = {};
  isStackHolder: boolean = false;
  constructor(
    private cpiApi: ApiService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private spinner: NgxSpinnerService
  ) {}
  /* Activity Table Config*/
  tableId: string = "activitylog_table";
  tableFilters = new BehaviorSubject({});
  tableApiUrl: string = "";
  FromDate: string = "";
  ToDate: string = "";
  @ViewChild("activityForm", { static: false }) activityForm: NgForm;
  tableColumns: tableColumn[] = [
    {
      title: "User",
      data: "",
      render: (data, row, rowData) => {
        return rowData
          ? rowData.user.firstName + " " + rowData.user.lastName
          : "";
      },
    },
    {
      title: "Reference",
      data: "reference",
    },
    {
      title: "Type",
      data: "type",
    },
    {
      title: "Event",
      data: "event",
    },
    {
      title: "History",
      data: "history",
    },
    {
      title: "Log Date",
      data: "",
      render: (data, row, rowData) => {
        return `${this.utils.UTCtoLocalDateDDMMMYY(rowData.logDate)}`;
      },
    },
  ];

  handleFormView = {
    refresh: () => {
      this.tableFilters.next({ init: true });
    },
  };

  saveActivityLog(e) {
    if (this.activityForm.invalid) {
      return false;
    }
    e.preventDefault();
    let postData = this.activityForm.form.value;
    this.FromDate = this.utils.formatNgbDateToYMD(postData.FromDate)
    this.ToDate =this.utils.formatNgbDateToYMD(postData.ToDate)
    this.loadActivitylogTable();
    this.handleFormView.refresh();
  }
  deleteActivityLog() {
    var filterObj = {
      StartDate: this.FromDate,
      EndDate: this.ToDate,
    };
    this.cpiApi
      .deleteActivityLogData("api/activitylogs/deleteActivityLog", filterObj)
      .subscribe((response) => {
        this.loadActivitylogTable();
      });
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "activitylogs");
  }

  loadActivitylogTable() {
    this.spinner.show();
    var filterObj = {
      StartDate: this.FromDate,
      EndDate: this.ToDate,
    };
    this.cpiApi
      .getData("api/activitylogs/getActivityLog/", filterObj)
      .subscribe((dtData) => {
        this.commonApi.initialiseTable(this.tableId, dtData, this.tableColumns);
        this.spinner.hide();
      });
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    var myCurrentDate = new Date();
    var myPastDate = new Date(myCurrentDate);
    myPastDate.setDate(myPastDate.getDate() - 7);
    this.FromDate = this.utils.UTCtoLocalDate(myPastDate);
    this.ToDate = this.utils.UTCtoLocalDate(new Date());
    this.loadActivitylogTable();
  }
}
