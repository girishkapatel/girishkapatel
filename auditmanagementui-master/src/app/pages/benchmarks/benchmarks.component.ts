import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../common/table/table.model";
import { BenchmarksService } from "./benchmarks.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../services/utils/utils.service";
import { CommonApiService } from "../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-benchmarks",
  templateUrl: "./benchmarks.component.html",
  styleUrls: ["./benchmarks.component.css"],
  providers: [BenchmarksService],
})
export class BenchmarksComponent implements OnInit {
  constructor(
    private benchmarks: BenchmarksService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("benchmarksForm", { static: false }) benchmarksForm: NgForm;

  accessRights: any = {};

  processlevel2Id: string = "";
  processlevel2Options: any = [];

  tableId: string = "benchmarks_table";

  businessCycleOptions: any = [];
  processlevel1Options: any = [];

  eybm: string = "";
  eybmId: string = "";
  businessCycleId: string = "";
  processlevel1Id: string = "";
  topPerformance: number = 0;
  bottomPerformance: number = 0;
  median: number = 0;

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
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editBenchmarks"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteBenchmarks"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

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

  saveBenchmarks(e) {
    if (this.benchmarksForm.invalid) {
      return false;
    } else {
      e.preventDefault();
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
      postData.id = this.eybmId;
      if (this.isEdit) {
        this.updateBenchmarks(postData);
      } else {
        this.addNewBenchmarks(postData);
      }
    }
  }
  addNewBenchmarks(postData) {
    this.benchmarks.addBenchmarks("api/eybenchmark", postData).subscribe(
      (response) => {
        this.notifyService.success("EY Benchmanrk added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  updateBenchmarks(postData) {
    this.benchmarks.updateBenchmarks("api/eybenchmark", postData).subscribe(
      (response) => {
        this.notifyService.success("EY Benchmanrk Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  addBenchmarks() {
    this.fillBusinessCycleOpts();
    this.handleFormView.show();
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
    this.fillBusinessCycleOpts();
    this.getProcessLevel1Opts();
    this.getProcessLevel2Opts();
    this.handleFormView.show();
  }

  deleteBenchmarks(eybmid) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.benchmarks.deleteBenchmarks("api/eybenchmark/" + eybmid).subscribe(
        (response) => {
          this.notifyService.success("EY Benchmanrk Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
        }
      );
    }
  }

  clearform() {
    this.eybm = "";
    this.eybmId = "";
    this.businessCycleId = "";
    this.processlevel1Id = "";
    this.topPerformance = 0;
    this.bottomPerformance = 0;
    this.median = 0;
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

  checkAccess() {
    this.accessRights = this.utils.getSubmoduleAccess("eybenchmarks")[0];
  }

  ngOnInit() {
    this.checkAccess();

    $(document).ready(() => {
      $("#benchmarkComponent").on("click", ".editBenchmarks", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let eybmData = $("#" + dataId).data();
        this.editBenchmarks(eybmData);
      });

      $("#benchmarkComponent").on("click", ".deleteBenchmarks", (e) => {
        let eybmId = $(e.currentTarget).attr("data-id");
        this.deleteBenchmarks(eybmId);
      });
    });
  }
}
