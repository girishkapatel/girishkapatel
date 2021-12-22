import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { PlmapService } from "./processlocationmapping.service";
import { NgForm } from "@angular/forms";
import { UtilsService } from "../../../services/utils/utils.service";
import { CommonApiService } from "../../../services/utils/commonapi.service";
import * as $ from "jquery";
import { IDropdownSettings } from "ng-multiselect-dropdown";
import { ToastrService } from "ngx-toastr";
import{NgxSpinnerService}from"ngx-spinner";
@Component({
  selector: "app-plmap",
  templateUrl: "./processlocationmapping.component.html",
  styleUrls: ["./processlocationmapping.component.css"],
  providers: [PlmapService],
})
export class PlmapComponent implements OnInit {
  constructor(
    private plmap: PlmapService,
    private utils: UtilsService,
    private commonApi: CommonApiService,
    private notifyService: ToastrService,
    private spinner:NgxSpinnerService
  ) {}

  @ViewChild("plmapForm", { static: false }) plmapForm: NgForm;

  dropdownSettings: IDropdownSettings = {};

  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "plmap_table";

  tableColumns: tableColumn[] = [
    {
      title: "Audit Name",
      data: "auditName",
    },
    {
      title: "Business Cycle",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.businessCycles != null)
          return this.getBusinessCycleNames(rowData.businessCycles);
      },
    },
    {
      title: "Process L1",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.processL1s != null)
          return this.getProcessL1Names(rowData.processL1s);
      },
    },
    {
      title: "Process L2",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.processL2s != null)
          return this.getProcessL2Names(rowData.processL2s);
      },
    },
    // {
    //   title:'Process Model',
    //   data:'processModel',
    //   render:function(data){
    //     return data ? '<span style="text-transform:capitalize">'+data+'</span>' : '';
    //   }
    // },
    {
      title: "Locations",
      data: "",
      render: (data, row, rowData) => {
        if (rowData.locations != null)
          return this.getLocationNames(rowData.locations);
      },
    },
    {
      title: "Action",
      orderable: false,
      data: "id",
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" data-id="' +
            data +
            '" title="Edit Record" class="btn btn-sm btn-info editPlmap"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" title="Delete Record" class="btn btn-sm btn-danger deletePlmap"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  tableColumnsLoc: tableColumn[] = [
    {
      title: "Company Name",
      data: "company.name",
    },
    {
      title: "Country",
      data: "country.name",
    },
    {
      title: "Location",
      data: "locationDescription",
    },
    {
      title: "City",
      data: "cityOrTown.name",
      render: function (city) {
        return city ? city : "";
      },
    },
    {
      title: "Profit Center Code",
      data: "profitCenterCode",
    },
   
    {
      title: "Action",
      orderable: false,
      data: "id",
      render: (data) => {
        let checked = "";
        if (this.locationArray.indexOf(data) > -1) {
          checked = "checked";
        }
        return (
          '<input  data-id="' + data + '" type="checkbox" ' + checked + ">"
        );
      },
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});
  tableData: tableData[] = [];
  tableScroll: boolean = false;
  formVisible: boolean = false;

  id: string = "";
  businessCycleOptions: any = [];
  processlevel1Options: any = [];
  processlevel2Options: any = [];
  locationArray: any = [];
  rowWiseLocations: any = [];

  selectedBusinessCycle = [];
  selectedProcessL1 = [];
  selectedProcessL2 = [];

  businessCycleId: string = "";
  businessCycleIdMulti: string = "";
  processlevel1Id: string = "";
  processlevel1IdMulti: string = "";
  processlevel2Id: string = "";
  processlevel2IdMulti: string = "";
  processModel: string = "";
  AuditName: string = "";
  locations: any = [];
  businessCyclesAll: any = [];
  processL1sAll: any = [];
  processL2sAll: any = [];

  isAll: boolean = true;
  isBusinessCycle: boolean = false;
  isProcessL1: boolean = false;
  isProcessL2: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.loadProcessLocation();
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
      this.tableFilters.next({});
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  savePlmap(e) {
    if (this.plmapForm.invalid) return false;
    if (this.isEdit) {
      this.updatePlmap();
    } else {
      this.addNewPlmap();
    }
  }

  getSelectedLocation() {
    let locations = [];
    let selectedLoc = window["jQuery"](
      '#tableLoc tbody tr td:nth-last-child(1) input[type="checkbox"]:checked'
    );
    for (let loc of selectedLoc) {
      let locId = window["jQuery"](loc).attr("data-id");
      locations.push(locId);
    }
    return locations;
  }

  addNewPlmap() {
    let locations = this.getSelectedLocation();
    let postData = this.plmapForm.form.value;
    postData.Locations = this.getSelectedLocation();

    postData.AuditName = this.AuditName;

    postData.businessCycles = this.getBusinessCycleIds();
    postData.processL1s = this.getProcessL1Ids();
    postData.processL2s = this.getProcessL2Ids();

    postData.isAll = this.isAll;
    postData.isBusinessCycle = this.isBusinessCycle;
    postData.isProcessL1 = this.isProcessL1;
    postData.isProcessL2 = this.isProcessL2;

    this.plmap.addPlmap("api/processlocationmapping", postData).subscribe(
      (response) => {
        this.notifyService.success(
          "Process Location Mapping saved successfully"
        );
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  getBusinessCycleIds() {
    let businessCyclesArray = [];
    this.selectedBusinessCycle.forEach((element) => {
      businessCyclesArray.push(element.id);
    });
    return businessCyclesArray;
  }

  getProcessL1Ids() {
    let processL1Array = [];
    this.selectedProcessL1.forEach((element) => {
      processL1Array.push(element.id);
    });
    return processL1Array;
  }

  getProcessL2Ids() {
    let processL2Array = [];
    this.selectedProcessL2.forEach((element) => {
      processL2Array.push(element.id);
    });
    return processL2Array;
  }

  updatePlmap() {
    let locations = this.getSelectedLocation();
    let postData = this.plmapForm.form.value;
    postData.Locations = locations;
    postData.id = this.id;

    postData.AuditName = this.AuditName;

    postData.businessCycles = this.getBusinessCycleIds();
    postData.processL1s = this.getProcessL1Ids();
    postData.processL2s = this.getProcessL2Ids();

    postData.isAll = this.isAll;
    postData.isBusinessCycle = this.isBusinessCycle;
    postData.isProcessL1 = this.isProcessL1;
    postData.isProcessL2 = this.isProcessL2;

    this.plmap.updatePlmap("api/processlocationmapping", postData).subscribe(
      (response) => {
        this.notifyService.success(
          "Process Location Mapping updated successfully"
        );
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  addPlmap() {
    this.fillBusinessCycleOpts();
    this.handleFormView.show();
  }

  editPlmap(plData) {
    this.isEdit = true;
    this.id = plData.id;
    this.locationArray = plData.locations;
    this.processModel = plData.processModel;
    this.AuditName = plData.auditName;

    this.isAll = plData.isAll;
    this.isBusinessCycle = plData.isBusinessCycle;
    this.isProcessL1 = plData.isProcessL1;
    this.isProcessL2 = plData.isProcessL2;

    this.initDropdownSettings(
      false,
      "id",
      "name",
      "Select All",
      "Deselect All",
      3,
      true,
      true
    );

    if (this.isAll) {
      this.initDropdownSettings(
        true,
        "id",
        "name",
        "Select All",
        "Deselect All",
        3,
        true,
        true
      );

      this.fillBusinessCycleOpts();
      this.getProcessLevel1Opts(plData.businessCycles[0]);
      this.getProcessLevel2Opts(plData.processL1s[0]);

      this.selectedBusinessCycle = [
        {
          id: plData.businessCycles[0],
          name: this.getBusinessCycleNames(plData.businessCycles),
        },
      ];

      this.selectedProcessL1 = [
        {
          id: plData.processL1s[0],
          name: this.getProcessL1Names(plData.processL1s),
        },
      ];

      this.selectedProcessL2 = [
        {
          id: plData.processL2s[0],
          name: this.getProcessL2Names(plData.processL2s),
        },
      ];
    } else if (this.isBusinessCycle) {
      this.fillBusinessCycleOpts();

      let bcData: any = plData.businessCycles;

      bcData.forEach((element) => {
        this.selectedBusinessCycle.push({
          id: element,
          name: this.getBusinessCycleNames(element),
        });
      });
    } else if (this.isProcessL1) {
      this.getAllProcessLevel1();

      let pl1Data: any = plData.processL1s;

      pl1Data.forEach((element) => {
        this.selectedProcessL1.push({
          id: element,
          name: this.getProcessL1Names(element),
        });
      });
    } else if (this.isProcessL2) {
      this.getAllProcessLevel2();

      let pl2Data: any = plData.processL2s;

      pl2Data.forEach((element) => {
        this.selectedProcessL2.push({
          id: element,
          name: this.getProcessL2Names(element),
        });
      });
    }

    this.handleFormView.show();
  }

  deletePlmap(plId) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);
    if (isConfirm) {
      this.plmap.deletePlmap("api/processlocationmapping/" + plId).subscribe(
        (response) => {
          this.notifyService.success(
            "Process Location Mapping deleted successfully."
          );
          this.handleFormView.hide();
        },
        (error) => {
          if (error.status == 406) {
            this.notifyService.error(
              "Looks like the selected record reference has been given in following places: Audit, ERMRisks, FollowUp, KeyBusinessInitiative, OverallAssesment, ProcessRiskMapping, ScopeAndSchedule, TrialBalance. Hence, you cannot delete the selected record"
            );
          } else {
            this.notifyService.error(error.error);
          }
        }
      );
    }
  }

  clearform() {
    this.id = "";
    this.AuditName = "";
    this.businessCycleOptions = [];
    this.processlevel1Options = [];
    this.processlevel2Options = [];
    this.locationArray = [];

    this.selectedBusinessCycle = [];
    this.selectedProcessL1 = [];
    this.selectedProcessL2 = [];
    this.processModel = "";
  }

  createTableColumnConfig() {
    this.commonApi.getLocations().subscribe((locationData) => {
      this.buildTableConfig(locationData);
    });
  }

  buildTableConfig(locationData) {
    let tableConfObj = {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" id="btnEdit" title="Edit Record" class="btn btn-sm btn-info editPlmap"><i class="fa fa-edit"></i></button>' +
          '<button type="button" data-id="' +
          data +
          '" id="btnDelete" title="Delete Record" class="btn btn-sm btn-danger deletePlmap"><i class="fa fa-trash"></i></button>'
        );
      },
    };

    for (let location of locationData) {
      this.rowWiseLocations.push(location.id);
      let tableConfObj = {
        title: location.profitCenterCode
          ? location.profitCenterCode
          : "No Title",
        data: "",
        render: (data, type, row, meta) => {
          let locArray = Array.isArray(row.locations) ? row.locations : [];
          let colsOffset = 4;
          let locIndex = meta.col - colsOffset;
          let checked = "";
          let currentRowLocId = this.rowWiseLocations[locIndex];
          if (locArray.indexOf(currentRowLocId) > -1) {
            checked = "checked";
          }
          return (
            '<input type="checkbox" ' +
            checked +
            ' style="pointer-events:none" tabindex="-1">'
          );
        },
      };
      this.tableColumns.push(tableConfObj);
    }

    this.tableColumns.push(tableConfObj);
    this.loadProcessLocation();
  }

  loadProcessLocation() {
    this.spinner.show();
    this.plmap.getPlmap("api/processlocationmapping").subscribe((pldata) => {
      this.commonApi.initialiseTable(
        this.tableId,
        pldata,
        this.tableColumns,
        this.tableScroll
      );
      this.spinner.hide();
    });
    this.spinner.hide();
  }

  fillBusinessCycleOpts() {
    this.commonApi.getBusinessCycle().subscribe((posts) => {
      this.businessCycleOptions = posts;
    });
  }

  getAllProcessLevel1() {
    this.commonApi.getAllProcessLevel1().subscribe((posts) => {
      this.processlevel1Options = posts;
    });
  }

  getAllProcessLevel2() {
    this.commonApi.getAllProcessLevel2().subscribe((posts) => {
      this.processlevel2Options = posts;
    });
  }

  getProcessLevel1Opts(e) {
    this.selectedProcessL1 = [];
    this.processlevel1Options = [];

    this.selectedProcessL2 = [];
    this.processlevel2Options = [];

    this.businessCycleId = e;
    this.businessCycleIdMulti = this.businessCycleIdMulti
      ? this.businessCycleIdMulti + "," + e
      : e;
    this.commonApi.getProcessLevel1(e).subscribe((posts) => {
      this.processlevel1Options = posts;
    });
  }

  getProcessLevel2Opts(e) {
    if (e) {
      this.processlevel1Id = e;
      this.commonApi.getProcessLevel2(e).subscribe((posts) => {
        this.processlevel2Options = posts;
      });
    } else {
      this.processlevel2Id = "";
      this.processlevel2Options = [];
    }
  }

  getLocations() {
    this.commonApi.getLocations().subscribe((locationData) => {
      this.locations = locationData;
      this.loadProcessLocation();
    });
  }

  getLocationNames(locArray) {
    let locationNameArray = [];

    for (let location of this.locations) {
      if (locArray.indexOf(location.id) > -1) {
        locationNameArray.push(
          `<a  href="javascript:void(0);" class="btn btn-sm blue mb-10"> ${location.locationDescription} </a>`
        );
      }
    }

    return locationNameArray.join(" ");
  }

  getAllBusinessCycles() {
    this.commonApi.getBusinessCycle().subscribe((businessCycleData) => {
      this.businessCyclesAll = businessCycleData;
    });
  }

  getBusinessCycleNames(busCycleArray) {
    let businessCycleNamesArray = [];

    for (let bc of this.businessCyclesAll) {
      if (busCycleArray.indexOf(bc.id) > -1) {
        businessCycleNamesArray.push(bc.name);
      }
    }

    return businessCycleNamesArray.join(",");
  }

  getAllProcessL1s() {
    this.commonApi.getAllProcessLevel1().subscribe((processL1sData) => {
      this.processL1sAll = processL1sData;
    });
  }

  getProcessL1Names(processL1sArray) {
    let processL1NamesArray = [];

    for (let pl1 of this.processL1sAll) {
      if (processL1sArray.indexOf(pl1.id) > -1) {
        processL1NamesArray.push(pl1.name);
      }
    }

    return processL1NamesArray.join(",");
  }

  getAllProcessL2s() {
    this.commonApi.getAllProcessLevel2().subscribe((processL2sData) => {
      this.processL2sAll = processL2sData;
    });
  }

  getProcessL2Names(processL2sArray) {
    let processL2NamesArray = [];

    for (let pl2 of this.processL2sAll) {
      if (processL2sArray.indexOf(pl2.id) > -1) {
        processL2NamesArray.push(pl2.name);
      }
    }

    return processL2NamesArray.join(",");
  }

  showHideControls(e) {
    this.selectedBusinessCycle = [];
    this.businessCycleOptions = [];

    this.selectedProcessL1 = [];
    this.processlevel1Options = [];

    this.selectedProcessL2 = [];
    this.processlevel2Options = [];

    if (e.target.value === "BC") {
      this.isAll = false;
      this.isBusinessCycle = true;
      this.isProcessL1 = false;
      this.isProcessL2 = false;

      this.fillBusinessCycleOpts();

      if (this.isEdit) this.AuditName = "";

      this.initDropdownSettings(
        false,
        "id",
        "name",
        "Select All",
        "Deselect All",
        3,
        true,
        true
      );
    } else if (e.target.value === "PL1") {
      this.isAll = false;
      this.isBusinessCycle = false;
      this.isProcessL1 = true;
      this.isProcessL2 = false;

      if (this.isEdit) this.AuditName = "";

      this.getAllProcessLevel1();

      this.initDropdownSettings(
        false,
        "id",
        "name",
        "Select All",
        "Deselect All",
        3,
        true,
        true
      );
    } else if (e.target.value === "PL2") {
      this.isAll = false;
      this.isBusinessCycle = false;
      this.isProcessL1 = false;
      this.isProcessL2 = true;

      if (this.isEdit) this.AuditName = "";

      this.getAllProcessLevel2();

      this.initDropdownSettings(
        false,
        "id",
        "name",
        "Select All",
        "Deselect All",
        3,
        true,
        true
      );
    } else {
      this.isAll = true;
      this.isBusinessCycle = false;
      this.isProcessL1 = false;
      this.isProcessL2 = false;

      this.fillBusinessCycleOpts();

      if (this.isEdit) this.AuditName = "";

      this.dropdownSettings = {
        singleSelection: true,
        idField: "id",
        textField: "name",
        selectAllText: "Select All",
        unSelectAllText: "UnSelect All",
        itemsShowLimit: 3,
        allowSearchFilter: true,
        closeDropDownOnSelection: true,
      };
    }
  }

  initDropdownSettings(
    isSingleSelection: boolean,
    idField: string,
    textField: string,
    selectAllText: string,
    unSelectAllText: string,
    itemsShowLimit: number,
    isAllowSearchFilter: boolean,
    closeDropDownOnSelection: boolean
  ) {
    this.dropdownSettings = {
      singleSelection: isSingleSelection,
      idField: idField,
      textField: textField,
      selectAllText: selectAllText,
      unSelectAllText: unSelectAllText,
      itemsShowLimit: itemsShowLimit,
      allowSearchFilter: isAllowSearchFilter,
      closeDropDownOnSelection: closeDropDownOnSelection,
    };
  }

  onBCDeSelect(item: any) {
    this.selectedProcessL1 = [];
    this.processlevel1Options = [];

    this.selectedProcessL2 = [];
    this.processlevel2Options = [];

    this.getProcessLevel1Opts(0);
    this.getProcessLevel2Opts(0);
  }

  onProcessL1DeSelect(item: any) {
    this.selectedProcessL2 = [];
    this.processlevel2Options = [];
  }

  onSelectAll(items: any) {}

  onBCDeSelectAll(items: any) {
    this.businessCycleIdMulti = "";
  }

  setProcessLevel2Id(e) {
    this.processlevel2Id = e.id;
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1(
      "master",
      "processlocationmapping"
    );
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    this.getLocations();
    this.getAllBusinessCycles();
    this.getAllProcessL1s();
    this.getAllProcessL2s();
    //this.createTableColumnConfig();

    $(document).ready(() => {
      $("#plmComponent").on("click", ".editPlmap, .deletePlmap", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        if (
          $(e.target).hasClass("editPlmap") ||
          $(e.target).hasClass("fa fa-edit")
        ) {
          let plData = window["jQuery"]("#" + dataId).data();
          this.editPlmap(plData);
        } else {
          this.deletePlmap(dataId);
        }
      });
    });

    this.initDropdownSettings(
      true,
      "id",
      "name",
      "Select All",
      "Deselect All",
      3,
      true,
      true
    );
  }
}
