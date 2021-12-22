import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { RootCauseService } from "./rootcause.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-rootcause",
  templateUrl: "./rootcause.component.html",
  styleUrls: ["./rootcause.component.css"],
  providers: [RootCauseService],
})
export class RootCauseComponent implements OnInit {
  constructor(
    private rootCauseApi: RootCauseService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("rootCauseForm", { static: false })
  rootCauseForm: NgForm;
  submitted: boolean = false;
  accessRights: any = {};
  isStackHolder: boolean = false;
  rootCauseId: string = "";
  rootCauseName: string = "";

  isEdit: boolean = false;

  tableColumns: tableColumn[] = [
    {
      title: "Name",
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
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-info editRootCause"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteRootCause"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  tableFilters = new BehaviorSubject({});

  handleFormView = {
    refresh: () => {
      this.tableFilters.next({ init: true });

      this.clearValues();
    },
  };

  showRootCauseModal() {
    window["jQuery"]("#manageRootCauseModal").modal("show");
  }

  hideRRootCauseModal() {
    window["jQuery"]("#manageRootCauseModal").modal("hide");
  }

  addRootCause() {
    this.submitted = false;
    this.clearValues();
    this.showRootCauseModal();
  }

  saveRootCause() {
    if (this.rootCauseForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = { Id: this.rootCauseId, Name: this.rootCauseName };

      if (this.rootCauseId) {
        this.rootCauseApi.updateRootCause("api/rootcause", postData).subscribe(
          (response) => {
            this.notifyService.success("Rootcause Updated Successfully");
            this.handleFormView.refresh();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
      } else {
        this.rootCauseApi.addRootCause("api/rootcause", postData).subscribe(
          (response) => {
            this.notifyService.success("Rootcause Inserted Successfully");
            this.handleFormView.refresh();
          },
          (error) => {
            this.notifyService.error(error.error);
          }
        );
      }
      this.hideRRootCauseModal();
    }
  }

  editRootCause(data) {
    this.isEdit = true;
    this.rootCauseId = data.id;
    this.rootCauseName = data.name;

    this.showRootCauseModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.rootCauseId = "";
    this.rootCauseName = "";
  }

  deleteRootCause(id) {
    this.rootCauseApi.deleteRootCause("api/rootcause/" + id).subscribe(
      (response) => {
        this.notifyService.success("Rootcause Deleted Successfully");
        this.handleFormView.refresh();
      },
      (error) => {
        if (error.status == 406) {
          this.notifyService.error(
            "Looks like the selected record reference has been given in following places: ActionPlanning, DiscussionNote, DraftReport, AuditClosure, FollowUp. Hence, you cannot delete the selected record"
          );
        } else {
          this.notifyService.error(error.error);
        }
      }
    );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "rootcause");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#rootcauseComponent").on("click", ".editRootCause", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let rootCauseData = $("#" + dataId).data();

        this.editRootCause(rootCauseData);
      });

      $("#rootcauseComponent").on("click", ".deleteRootCause", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deleteRootCause(dataId);
      });
    });
  }
}
