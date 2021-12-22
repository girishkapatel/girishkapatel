import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn } from "./../../../../common/table/table.model";
import { ProcessLevel1Service } from "./processlevel1.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../../services/utils/utils.service";
import { CommonApiService } from "../../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-processlevel1",
  templateUrl: "./processlevel1.component.html",
  styleUrls: ["./processlevel1.component.css"],
  providers: [ProcessLevel1Service],
})
export class ProcessLevel1Component implements OnInit {
  constructor(
    private processlevel1: ProcessLevel1Service,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("processlevel1Form", { static: false }) processlevel1Form: NgForm;

  accessRights: any = {};
  submitted: boolean = false;
  tableId: string = "processlevel1_table";
  isStackHolder: boolean = false;
  tableGetApi: string = "posts";

  businessCycleOptions: any = [];

  businessCycleId: string = "";
  processlevel1Id: string = "";
  processlevel1Name: string = "";

  tableColumns: tableColumn[] = [
    {
      title: "Business Cycle",
      data: "businessCycle.name",
    },
    {
      title: "Process L1",
      data: "name",
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
            '" class="btn btn-sm btn-info editProcessLevel1"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteProcessLevel1"><i class="fa fa-trash"></i></button>';

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

  saveProcessLevel1(e) {
    this.submitted = true;
    if (this.processlevel1Form.invalid) {
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateProcessLevel1();
      } else {
        this.addNewProcessLevel1();
      }
    }
  }

  addNewProcessLevel1() {
    let postData = this.processlevel1Form.form.value;
    this.processlevel1.addProcessLevel1("api/processl1", postData).subscribe(
      (response) => {
        this.notifyService.success("Process Level 1 Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateProcessLevel1() {
    let postData = this.processlevel1Form.form.value;
    postData.id = this.processlevel1Id;
    this.processlevel1.updateProcessLevel1("api/processl1", postData).subscribe(
      (response) => {
        this.notifyService.success("Process Level 1 Updated Successfully");
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

  addProcessLevel1() {
    this.fillBusinessCycleOpts();
    this.handleFormView.show();
  }

  editProcessLevel1(processlevel1Data) {
    this.isEdit = true;
    this.processlevel1Id = processlevel1Data.id;
    this.processlevel1Name = processlevel1Data.name;
    this.businessCycleId = processlevel1Data.businessCycleId;
    this.handleFormView.show();
  }

  deleteProcessLevel1(processlevel1Id) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.processlevel1
        .deleteProcessLevel1("api/processl1/" + processlevel1Id)
        .subscribe(
          (response) => {
            this.notifyService.success("Process Level 1 Deleted Successfully");
            this.handleFormView.hide();
          },
          (error) => {
            if (error.status == 406) {
              this.notifyService.error(
                "Looks like the selected record reference has been given in following places: ERMRisks, EYBenchmark , EYBenchmarkAuditwise, FollowUp, KeyBusinessInitiative, OverallAssesment, ProcessL2, ProcessLocationMapping, RACM, RACMAuditProcedure, Risk. Hence, you cannot delete the selected record"
              );
            } else {
              this.notifyService.error(error.error);
            }
          }
        );
    }
  }

  clearform() {
    this.processlevel1Id = "";
    this.processlevel1Name = "";
    this.businessCycleId = "";
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "processmaster");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#pl1Component").on("click", ".editProcessLevel1", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let processlevel1Data = $("#" + dataId).data();
        this.fillBusinessCycleOpts();
        this.editProcessLevel1(processlevel1Data);
      });

      $("#pl1Component").on("click", ".deleteProcessLevel1", (e) => {
        let processlevel1Id = $(e.currentTarget).attr("id");
        this.deleteProcessLevel1(processlevel1Id);
      });
      $('a[href="#pl1"]').on("click", (e) => {
        if (typeof this.tableFilters !== "undefined") {
          this.tableFilters.next({ init: true });
        }
      });
    });
  }
}
