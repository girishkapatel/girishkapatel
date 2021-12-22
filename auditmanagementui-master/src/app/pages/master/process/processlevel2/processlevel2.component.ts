import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn } from "./../../../../common/table/table.model";
import { PL2Service } from "./processlevel2.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../../services/utils/utils.service";
import { CommonApiService } from "../../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-processlevel2",
  templateUrl: "./processlevel2.component.html",
  styleUrls: ["./processlevel2.component.css"],
  providers: [PL2Service],
})
export class ProcessLevel2Component implements OnInit {
  constructor(
    private pl2: PL2Service,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  submitted: boolean = false;
  @ViewChild("processlevel2Form", { static: false }) processlevel2Form: NgForm;

  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "processlevel2_table";

  businessCycleOptions: any = [];
  processlevel1Options: any = [];

  businessCycleId: string = "";
  processlevel1Id: string = "";
  model: string = "";
  processlevel2Id: string = "";
  processlevel2Name: string = "";

  tableColumns: tableColumn[] = [
    {
      title: "Business Cycle",
      data: "businessCycle.name",
    },
    {
      title: "Process L1",
      data: "processL1.name",
    },
    {
      title: "Process L2",
      data: "name",
    },
    // {
    //   title:'Process Model',
    //   data:'processModel'
    // },
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
            '" class="btn btn-sm btn-info editProcessLevel2"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteProcessLevel2"><i class="fa fa-trash"></i></button>';

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

  saveProcessLevel2(e) {
    if (this.processlevel2Form.invalid) {
      this.submitted = true;
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateProcessLevel2();
      } else {
        this.addNewProcessLevel2();
      }
    }
  }

  addNewProcessLevel2() {
    let postData = this.processlevel2Form.form.value;
    this.pl2.addProcessLevel2("api/processl2", postData).subscribe(
      (response) => {
        this.notifyService.success("Process Level 2 Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateProcessLevel2() {
    let postData = this.processlevel2Form.form.value;
    postData.id = this.processlevel2Id;
    this.pl2.updateProcessLevel2("api/processl2", postData).subscribe(
      (response) => {
        this.notifyService.success("Process Level 2 Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  fillBusinessCycleOpts() {
    this.commonApi.getBusinessCycle().subscribe((posts) => {
      this.businessCycleOptions = posts;
    });
  }

  getProcessLevel2Opts() {
    //this.processlevel1Id = '';
    this.commonApi.getProcessLevel1(this.businessCycleId).subscribe((posts) => {
      this.processlevel1Options = posts;
    });
  }

  addProcessLevel2() {
    this.fillBusinessCycleOpts();
    this.handleFormView.show();
  }

  editProcessLevel2(processlevel2Data) {
    this.isEdit = true;
    this.processlevel2Id = processlevel2Data.id;
    this.processlevel2Name = processlevel2Data.name;
    this.businessCycleId = processlevel2Data.businessCycleId;
    this.processlevel1Id = processlevel2Data.processL1Id;
    this.model = processlevel2Data.processModel;
    this.fillBusinessCycleOpts();
    this.getProcessLevel2Opts();
    this.handleFormView.show();
  }

  deleteProcessLevel2(processlevel2Id) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.pl2
        .deleteProcessLevel2("api/processl2/" + processlevel2Id)
        .subscribe(
          (response) => {
            this.notifyService.success("Process Level 2 Deleted Successfully");
            this.handleFormView.hide();
          },
          (error) => {
            if (error.status == 406) {
              this.notifyService.error(
                "Looks like the selected record reference has been given in following places: ERMRisks, EYBenchmark , EYBenchmarkAuditwise, FollowUp, KeyBusinessInitiative, OverallAssesment, ProcessLocationMapping, RACM, RACMAuditProcedure, Risk. Hence, you cannot delete the selected record"
              );
            } else {
              this.notifyService.error(error.error);
            }
          }
        );
    }
  }

  clearform() {
    this.processlevel2Id = "";
    this.processlevel2Name = "";
    this.processlevel1Id = "";
    this.businessCycleId = "";
    this.model = "";
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "processmaster");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#pl2Component").on("click", ".editProcessLevel2", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let processlevel2Data = $("#" + dataId).data();
        //this.fillBusinessCycleOpts();
        this.editProcessLevel2(processlevel2Data);
      });

      $("#pl2Component").on("click", ".deleteProcessLevel2", (e) => {
        let processlevel2Id = $(e.currentTarget).attr("id");
        this.deleteProcessLevel2(processlevel2Id);
      });

      $('a[href="#pl2"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });
    });
  }
}
