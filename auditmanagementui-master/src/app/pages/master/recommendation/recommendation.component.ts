import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { RecommendationService } from "./recommendation.service";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgForm } from "@angular/forms";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-recommendation",
  templateUrl: "./recommendation.component.html",
  styleUrls: ["./recommendation.component.css"],
  providers: [RecommendationService],
})
export class RecommendationComponent implements OnInit {
  constructor(
    private recommendationApi: RecommendationService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("recommendationForm", { static: false })
  recommendationForm: NgForm;
  isStackHolder: boolean = false;
  accessRights: any = {};
  submitted: boolean = false;
  recommId: string = "";
  recommName: string = "";

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
            '" class="btn btn-sm btn-info editRecommendation"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteRecommendation"><i class="fa fa-trash"></i></button>';

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

  showRecommendationModal() {
    window["jQuery"]("#manageRecommendationModal").modal("show");
  }

  hideRecommendationModal() {
    window["jQuery"]("#manageRecommendationModal").modal("hide");
  }

  addRecommendation() {
    this.submitted = false;
    this.clearValues();
    this.showRecommendationModal();
  }

  saveRecommendation() {
    if (this.recommendationForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      let postData = { Id: this.recommId, Name: this.recommName };

      if (this.recommId) {
        this.recommendationApi
          .updateRecommendation("api/recommendation", postData)
          .subscribe(
            (response) => {
              this.notifyService.success("Recommendation Updated Successfully");
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      } else {
        this.recommendationApi
          .addRecommendation("api/recommendation", postData)
          .subscribe(
            (response) => {
              this.notifyService.success(
                "Recommendation Inserted Successfully"
              );
              this.handleFormView.refresh();
            },
            (error) => {
              this.notifyService.error(error.error);
            }
          );
      }
      this.hideRecommendationModal();
    }
  }

  editRecommendation(data) {
    this.isEdit = true;
    this.recommId = data.id;
    this.recommName = data.name;

    this.showRecommendationModal();
  }

  cancelAddEdit() {
    this.handleFormView.refresh();
  }

  clearValues() {
    this.isEdit = false;

    this.recommId = "";
    this.recommName = "";
  }

  deleteRecommendation(id) {
    this.recommendationApi
      .deleteRecommendation("api/recommendation/" + id)
      .subscribe(
        (response) => {
          this.notifyService.success("Recommendation Deleted Successfully");
          this.handleFormView.refresh();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: ActionPlanning, DraftReport, AuditClosure, FollowUp. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "master",
      "recommendation"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#recommendationComponent").on("click", ".editRecommendation", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let recommData = $("#" + dataId).data();

        this.editRecommendation(recommData);
      });

      $("#recommendationComponent").on(
        "click",
        ".deleteRecommendation",
        (e) => {
          let dataId = $(e.currentTarget).attr("data-id");

          this.deleteRecommendation(dataId);
        }
      );
    });
  }
}
