import { Component, Input, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import EYBenchmarks from "./eybenchmark.model";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ApiService } from "src/app/services/api/api.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-eybenchmark",
  templateUrl: "eybenchmark.component.html",
})
export class EyBenchmarkComponent implements OnInit {
  constructor(
    private utils: UtilsService,
    private api: ApiService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("benchmarksForm", { static: false }) benchmarksForm: NgForm;

  accessRights: any = {};

  AuditID: string;
  tableApiUrl: string;
  tableFilters;
  tableApiUrlEbAll: string;
  tableFiltersEbAll: BehaviorSubject<{ init: boolean }>;
  selectedEybm = {};
  companyPerformance: number;
  eyBenchmarkID: any;

  businessCycleOptions: any = [];
  processlevel1Options: any = [];

  eybm: string = "";
  eybmId: string = "";
  businessCycleId: string = "";
  processlevel1Id: string = "";
  topPerformance: number = 0;
  bottomPerformance: number = 0;
  median: number = 0;
  processlevel2Id: string = "";
  processlevel2Options: any = [];

  /* Eb Table Config*/
  tableId: string = "eb_table";
  tableColumns: tableColumn[] = [
    {
      title: "Business Cycle",
      data: "businessCycle.name",
      render: function (businessCycle) {
        return businessCycle ? businessCycle : "";
      },
    },
    {
      title: "L1 Process",
      data: "processL1.name",
      render: function (processL1) {
        return processL1 ? processL1 : "";
      },
    },
    {
      title: "EY Benchmarks",
      data: "name",
    },
    {
      title: "Bottom Performance (In INR)",
      data: "bottomPerformance",
    },
    {
      title: "Median (In INR)",
      data: "median",
    },
    {
      title: "Top Performance (In INR)",
      data: "topPerformance",
    },
    {
      title: "Company Performance",
      data: "companyPerformance",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editEb"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteEb"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  tableColumnsEbAll: tableColumn[] = [
    {
      title: "Select",
      data: "id",
      render: (data) => {
        return (
          '<p class="text-center" style="margin:0px"><input type="checkbox" class="selectedEybm" data-id="' +
          data +
          '" /></p>'
        );
      },
    },
    {
      title: "Business Cycle",
      data: "businessCycle.name",
      render: function (businessCycle) {
        return businessCycle ? businessCycle : "";
      },
    },
    {
      title: "L1 Process",
      data: "processL1.name",
      render: function (processL1) {
        return processL1 ? processL1 : "";
      },
    },
    {
      title: "EY Benchmarks",
      data: "name",
    },
    {
      title: "Bottom Performance (In INR)",
      data: "bottomPerformance",
    },
    {
      title: "Median (In INR)",
      data: "median",
    },
    {
      title: "Top Performance (In INR)",
      data: "topPerformance",
    },
  ];

  tableData_eb: tableData[] = EYBenchmarks.slice(0, 10);

  isEdit: boolean = false;

  formVisible: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.formVisible = false;
      this.isEdit = false;
      this.tableFilters.next({ init: true });
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  updateBenchmarks() {
    let postData = this.benchmarksForm.form.value;
    (postData.BusinessCycleId =
      postData.BusinessCycleId === "" ? null : postData.BusinessCycleId),
      (postData.ProcessL1Id =
        postData.ProcessL1Id === "" ? null : postData.ProcessL1Id),
      (postData.ProcessL2Id =
        postData.ProcessL2Id === "" ? null : postData.ProcessL2Id),
      (postData.TopPerformance = parseFloat(postData.TopPerformance));
    postData.BottomPerformance = parseFloat(postData.BottomPerformance);
    postData.Median = parseFloat(postData.Median);
    postData.CompanyPerformance = parseFloat(postData.CompanyPerformance);
    postData.EYBenchmarkID = this.eyBenchmarkID;
    postData.AuditID = this.AuditID;
    postData.id = this.eybmId;
    this.api.updateData("api/eybenchmarkauditwise", postData).subscribe(
      (response) => {
        this.notifyService.success("EY Benchmark Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  editBenchmarks(eybmData) {
    this.isEdit = true;
    this.eybm = eybmData.name;
    this.eybmId = eybmData.id;
    this.businessCycleId = eybmData.businessCycleId
      ? eybmData.businessCycleId
      : "";
    this.processlevel1Id = eybmData.processL1Id ? eybmData.processL1Id : "";
    this.processlevel2Id = eybmData.processL2Id ? eybmData.processL2Id : "";
    this.topPerformance = parseFloat(eybmData.topPerformance);
    this.bottomPerformance = parseFloat(eybmData.bottomPerformance);
    this.median = parseFloat(eybmData.median);
    this.companyPerformance =
      eybmData.companyPerformance && !isNaN(eybmData.companyPerformance)
        ? parseFloat(eybmData.companyPerformance)
        : 0;
    this.eyBenchmarkID = eybmData.eyBenchmarkID;
    this.fillBusinessCycleOpts();
    this.getProcessLevel1Opts();
    this.getProcessLevel2Opts();
    this.handleFormView.show();
  }

  deleteBenchmarks(eybmid) {
    this.api.deleteData("api/eybenchmarkauditwise/" + eybmid).subscribe(
      (response) => {
        this.notifyService.success("EY Benchmark Deleted Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  clearform() {
    this.eybm = "";
    this.eybmId = "";
    this.businessCycleId = "";
    this.processlevel1Id = "";
    this.topPerformance = 0;
    this.bottomPerformance = 0;
    this.median = 0;
    this.companyPerformance = 0;
    this.eyBenchmarkID = "";
  }

  fillBusinessCycleOpts() {
    this.commonApi.getBusinessCycle().subscribe((posts) => {
      this.businessCycleOptions = posts;
    });
  }

  getProcessLevel1Opts() {
    if (this.businessCycleId) {
      this.commonApi
        .getProcessLevel1(this.businessCycleId)
        .subscribe((posts) => {
          this.processlevel1Options = posts;
        });
    } else {
      this.processlevel1Id = "";
      this.processlevel1Options = [];
      this.processlevel2Id = "";
      this.processlevel2Options = [];
    }
  }

  getProcessLevel2Opts() {
    if (this.processlevel1Id) {
      this.commonApi
        .getProcessLevel2(this.processlevel1Id)
        .subscribe((posts) => {
          this.processlevel2Options = posts;
        });
    } else {
      this.processlevel2Id = "";
      this.processlevel2Options = [];
    }
  }

  handleLibraryDialog = {
    show: () => {
      window["jQuery"]("#ebAllModal").modal("show");
      this.tableFiltersEbAll.next({ init: true });
    },
    hide: () => {
      window["jQuery"]("#ebAllModal").modal("hide");
    },
  };

  handleSelectedEybmData(dataId, eybmData, isChecked) {
    if (isChecked) {
      this.handleEybmData.add(dataId, eybmData);
    } else {
      this.handleEybmData.remove(dataId);
    }
  }

  handleEybmData = {
    checkEmpty: () => {
      return this.utils.isEmptyObj(this.selectedEybm);
    },
    add: (dataId, eybmData) => {
      if (typeof this.selectedEybm[dataId] === "undefined") {
        this.selectedEybm[dataId] = {};
      }
      this.selectedEybm[dataId] = eybmData;
    },
    remove: (dataId) => {
      if (typeof this.selectedEybm[dataId] === "object") {
        delete this.selectedEybm[dataId];
      }
    },
  };

  saveEybmFromLibrary() {
    if (!this.handleEybmData.checkEmpty()) {
      let EybenchmarkdIDs = Object.keys(this.selectedEybm);
      this.api
        .insertData("api/eybenchmarkauditwise/addFromEybmLibrary", {
          EybenchmarkdIDs: EybenchmarkdIDs,
          AuditID: this.AuditID,
        })
        .subscribe((response) => {
          this.notifyService.success("Data import from library successful");
          this.handleFormView.hide();
          this.handleLibraryDialog.hide();
        });
    } else {
      this.notifyService.error("Please select records to save");
    }
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "manageaudits",
      "eybenchmark"
    );
  }

  ngOnInit() {
    this.checkAccess();

    this.AuditID = localStorage.getItem("auditId");

    this.tableApiUrl = `api/eybenchmarkauditwise/GetByAudit/${this.AuditID}`;
    this.tableApiUrlEbAll = `api/eybenchmark/getall`;

    this.tableFilters = new BehaviorSubject({ init: true });
    this.tableFiltersEbAll = new BehaviorSubject({ init: true });

    $(document).ready(() => {
      $("#eyBenchmarkMasterComponent").on("click", ".editEb", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let ebData = $("#" + dataId).data();
        this.editBenchmarks(ebData);
      });

      $("#eyBenchmarkMasterComponent").on("click", ".deleteEb", (e) => {
        let eybmId = $(e.currentTarget).attr("data-id");
        this.deleteBenchmarks(eybmId);
      });

      $('a[href="#eyBenchmarksTab"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });

      $("#ebAllModal").on("change", ".selectedEybm", (e) => {
        let isChecked = $(e.target).is(":checked");
        let dataId = $(e.currentTarget).attr("data-id");
        let eybmData = $("#" + dataId).data();
        this.handleSelectedEybmData(dataId, eybmData, isChecked);
      });
    });
  }
}
