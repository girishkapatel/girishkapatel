import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../../common/table/table.model";
import { SectorService } from "./sector.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-sector",
  templateUrl: "./sector.component.html",
  styleUrls: ["./sector.component.css"],
  providers: [SectorService],
})
export class SectorComponent implements OnInit {
  constructor(
    private sector: SectorService,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner:NgxSpinnerService
  ) {}
  submitted: boolean = false;
  @ViewChild("sectorForm", { static: false }) sectorForm: NgForm;
  isStackHolder: boolean = false;
  accessRights: any = {};

  tableId: string = "sector_table";

  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Name",
      data: "name",
    },
    {
      title: "Action",
      orderable: false,
      data: "id",
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editSector"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteSector"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  sectorId: string = "";
  sectorName: string = "";
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

  saveSector(e) {
    if (this.sectorForm.invalid) {
      this.submitted = true;
      return false;
    } else {
      e.preventDefault();
      if (this.isEdit) {
        this.updateSector();
      } else {
        this.addNewSector();
      }
    }
  }

  addNewSector() {
    let postData = this.sectorForm.form.value;
    this.sector.addSector("api/sector", postData).subscribe(
      (response) => {
        this.notifyService.success("Sector Added Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateSector() {
    let postData = this.sectorForm.form.value;
    postData.id = this.sectorId;
    this.sector.updateSector("api/sector", postData).subscribe(
      (response) => {
        this.notifyService.success("Sector Updated Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addSector() {
    this.handleFormView.show();
  }

  editSector(sectorData) {
    this.isEdit = true;
    this.sectorId = sectorData.id;
    this.sectorName = sectorData.name;
    this.handleFormView.show();
  }

  deleteSector(sectorId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.sector.deleteSector("api/sector/" + sectorId).subscribe(
        (response) => {
          this.notifyService.success("Sector Deleted Successfully");
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: Location, TrialBalance. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  clearform() {
    this.sectorId = "";
    this.sectorName = "";
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "processmaster");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#sectorComponent").on("click", ".editSector", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let sectorData = $("#" + dataId).data();
        this.editSector(sectorData);
      });

      $("#sectorComponent").on("click", ".deleteSector", (e) => {
        let sectorId = $(e.currentTarget).attr("id");
        this.deleteSector(sectorId);
      });
    });
  }
}
