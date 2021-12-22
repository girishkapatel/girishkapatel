import { Component, OnInit, ViewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { tableColumn } from "src/app/common/table/table.model";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { EscalationService } from "./escalation.service";
import { IDropdownSettings } from "ng-multiselect-dropdown";
import { UtilsService } from "src/app/services/utils/utils.service";
import * as $ from "jquery";
import { NgForm } from "@angular/forms";
import { number } from "@amcharts/amcharts4/core";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-escalation",
  templateUrl: "./escalation.component.html",
  styleUrls: ["./escalation.component.css"],
  providers: [EscalationService],
})
export class EscalationComponent implements OnInit {
  constructor(
    private escalApi: EscalationService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("escalationForm", { static: false }) escalationForm: NgForm;
  @ViewChild("escalationRulesForm", { static: false })
  escalationRulesForm: NgForm;

  accessRights: any = {};
  isStackHolder: boolean = false;
  selectmodule: any;

  isDisabledAddRule = false;

  ruleObj: {
    condition: any;
    interval: any;
    beforeAfter: any;
    counter: any;
    type: any;
    updatedBy: string;
    updatedon: Date;
  };

  rulesArray: any[];

  escalationRuleModel: {
    condition: 0;
    interval: "";
    beforeAfter: "";
    counter: "";
    type: "";
  };

  audits: any = [];
  userOpts: any = [];

  formVisible: boolean = false;
  isEdit: boolean = false;

  type: string = "Reminder";

  ruleIndex: number;

  isEditRule: boolean = false;

  ruleCondition: number = 1;

  ruleInterval: string = "Day";
  ruleBeforeAfter: string = "Before";

  ruleCounter: number = 0;

  ruleEscalatedTo: any = [];
  ruleEscalatedToIds: any = [];
  counter: any = [];

  ruleEscalatedToNames: string = "";

  rules: any = [];

  modal;

  dropdownSettings: IDropdownSettings = {};

  id: string = "";
  auditId: string = "";
  module: string = "";

  tableId: string = "escalation_table";
  tableFilters = new BehaviorSubject({});
  tableColumns: tableColumn[] = [
    {
      title: "Module",
      data: "module",
      // render: (data, row, rowData) => {
      // return (
      // rowData.audit.auditName + " / " + rowData.audit.location.division
      // );
      // },
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
            '" class="btn btn-sm btn-info editEscalation"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-danger deleteEscalation"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.tableFilters.next({ init: true });
      this.formVisible = false;
      this.isEdit = false;
      this.clearForm();
    },
  };

  getAllAudits() {
    this.escalApi.getEscalation(`api/audit`).subscribe((response) => {
      this.audits = response;
    });
  }

  selectModule() {
    this.selectmodule = this.modal;
  }

  fillUserOptions() {
    this.commonApi.getUsers().subscribe((posts) => {
      this.userOpts = posts;
    });
  }

  selectType(e) {
    this.type = e.currentTarget.value;
    // this.rules = this.rules.filter((a) => a.type == this.type);

    this.allowAddNewRule();
    this.fillRuleTable();
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

  addNewEscalation() {
    this.type = "Reminder";

    $("#tblEscalationRules tbody").html("");

    this.allowAddNewRule();
    this.handleFormView.show();
  }

  addNewEscalationRule() {
    this.ruleIndex = 0;
    this.isEditRule = false;
    this.clearRuleForm();
    this.showRuleForm();
  }
  hideEscalationRuleForm() {
    window["jQuery"]("#manageEscalationRules").modal("hide");
  }
  clearRuleForm() {
    this.ruleCondition = 1;
    this.ruleInterval = "Day";
    this.ruleBeforeAfter = "Before";
    this.counter = [];
    // this.selectedEscalatedTo = [];
    this.ruleEscalatedToNames = "";

    this.ruleEscalatedToIds = [];
    this.initDropdownSettings(
      false,
      "id",
      "firstName",
      "Select All",
      "Deselect All",
      3,
      true,
      true
    );
  }

  showRuleForm() {
    window["jQuery"]("#manageEscalationRules").modal("show");
  }

  onEscalatedToSelect(e) {
    let escalatedToUser = {
      userId: e.id,
      user: {
        firstName: e.firstName,
      },
    };

    // this.selectedEscalatedTo.push(escalatedToUser);
  }

  onEscalatedToDeselect(e) {
    // for (let index = 0; index < this.selectedEscalatedTo.length; index++) {
    // if (this.selectedEscalatedTo[index].userId === e.id)
    // this.selectedEscalatedTo.splice(index, 1);
    // }
  }

  allowAddNewRule() {
    if (this.rules.filter((a) => a.type == this.type).length === 1)
      this.isDisabledAddRule = true;
    else this.isDisabledAddRule = false;
  }

  saveEscalationRule(e) {
    if (this.escalationRulesForm.invalid) {
      return false;
    }
    let ruleSelected = this.getRuleInfo();
    this.addRuleToTable(ruleSelected, false);
    this.allowAddNewRule();
    this.hideEscalationRuleForm();
  }

  getEscalatedToUsers(e) {
    let returnArray = [];

    for (let item of e) {
      returnArray.push(e);
    }

    return returnArray;
  }

  getRuleInfo() {
    let ruleSelected = {
      condition: this.ruleCondition,
      interval: this.ruleInterval,
      beforeAfter: this.ruleBeforeAfter,
      counter: this.counter,
      type: this.type,
    };
    return ruleSelected;
  }

  addRuleToTable(ruleObj, isNew?: boolean) {
    let ruleTable = $("#tblEscalationRules tbody");
    let noOfRecords = ruleTable.children("tr").length;
    let isInitRender = false;

    if (typeof isNew === "undefined") {
      isNew = true;
      isInitRender = true;
    }

    if (isNew) {
      if ($("#tblEscalationRules tbody .norecords").length) {
        $("#tblEscalationRules tbody .norecords").remove();
        noOfRecords = noOfRecords - 1;
      }

      if (ruleObj.type === this.type) {
        let resHtml = `<tr>
        <td>${ruleObj.condition}</td>
        <td>${ruleObj.interval}</td>
        <td>${ruleObj.beforeAfter}</td>
        <td>${ruleObj.counter}</td>
        <td>
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-info editRule">
            <i class="fa fa-edit"></i></button>
          <button type="button" data-index="${noOfRecords}" class="btn btn-sm btn-danger removeRule">
            <i class="fa fa-trash"></i></button>
        </td>
      </tr>`;

        ruleTable.append(resHtml);
      }

      if (!isInitRender) {
        this.rules.push(ruleObj);
      }
    } else {
      this.rules[this.ruleIndex] = ruleObj;
      this.fillRuleTable();
    }
  }

  getEscalatedToNames(escalatedToObj) {
    let names = "";

    escalatedToObj.forEach((element) => {
      names += element.firstName + ", ";
    });

    return names;
  }

  fillRuleTable() {
    if (Array.isArray(this.rules) && this.rules.length) {
      $("#tblEscalationRules tbody").html("");

      for (let rule of this.rules) {
        this.addRuleToTable(rule);
      }
    } else {
      $("#tblEscalationRules tbody").html(
        '<tr class="norecords"><td colspan="5" class="text-center">No Records Found</td></tr>'
      );
    }
  }

  editRule(appIndex) {
    let appData = this.rules[appIndex];
    this.clearRuleForm();
    this.fillRuleEdit(appData);
  }

  fillRuleEdit(ruleData) {
    this.ruleCondition = ruleData.condition;
    this.ruleInterval = ruleData.interval;
    this.ruleBeforeAfter = ruleData.beforeAfter;
    // this.ruleEscalatedToIds = ruleData.escalatedTo;
    this.counter = ruleData.counter;
    this.type = ruleData.type;
    this.showRuleForm();
  }

  removeRule(appIndex) {
    if (this.rules[appIndex]) {
      this.rules.splice(appIndex, 1);
    }

    this.allowAddNewRule();
    this.fillRuleTable();
  }

  saveEscalation(e) {
    if (this.escalationForm.invalid) {
      return false;
    }

    e.preventDefault();

    let postData = this.getEscalaionObj();

    if (this.id) postData["id"] = this.id;

    if (this.isEdit) this.updateEscalation(postData);
    else this.addEscalation(postData);
  }

  addEscalation(postData) {
    this.escalApi.addEscalation("api/escalation", postData).subscribe(
      (response) => {
        this.notifyService.success("Escalation details saved successfully.");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  updateEscalation(postData) {
    this.escalApi.updateEscalation("api/escalation", postData).subscribe(
      (response) => {
        this.notifyService.success("Escalation details saved successfully.");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  getEscalaionObj() {
    var role = localStorage.getItem("role");
    let escalation = {
      createdBy: role,
      updatedBy: role,
      createdOn: new Date(),
      updatedon: new Date(),
      module: this.module,
      escalationRules: this.getEscalationRules(),
    };

    // if (this.isEdit) escalation.Id = this.id;

    return escalation;
  }

  getEscalatedToIds(escalatedToObj) {
    let Ids = "";

    escalatedToObj.forEach((element) => {
      Ids += element.id + ", ";
    });

    return Ids;
  }

  getEscalationRules() {
    this.rulesArray = [];
    var role = localStorage.getItem("role");
    for (let rule of this.rules) {
      const ruleObj = {
        condition: rule.condition,
        interval: rule.interval,
        beforeAfter: rule.beforeAfter,
        counter: rule.counter,
        type: rule.type,
        updatedBy: role,
        updatedon: new Date(),
      };

      this.rulesArray.push(ruleObj);
    }

    return this.rulesArray;
  }

  getEscalatedToObj(ruleEscalatedTos) {
    let ruleEscalatedToArray = [];

    for (let escTo of ruleEscalatedTos) {
      let ruleEscalatedToObj = {
        UserId: escTo.id,
      };

      ruleEscalatedToArray.push(ruleEscalatedToObj);
    }

    return ruleEscalatedToArray;
  }

  clearForm() {
    this.id = "";
    this.modal = "";
    this.rules = [];
  }

  editEscalation(escalationData) {
    this.isEdit = true;

    this.id = escalationData.id;
    this.module = escalationData.module;
    this.type = "Reminder";
    this.getEditEscalationRules(escalationData.escalationRules);

    this.allowAddNewRule();
    this.fillRuleTable();
    this.handleFormView.show();
  }

  getEditEscalationRules(rulesArray) {
    for (let index = 0; index < rulesArray.length; index++) {
      let ruleModel = {
        condition: rulesArray[index].condition,
        interval: rulesArray[index].interval,
        beforeAfter: rulesArray[index].beforeAfter,
        counter: rulesArray[index].counter,
        type: rulesArray[index].type,
      };
      this.rules.push(ruleModel);
    }
  }

  cancelEscalationEdit(e) {
    this.handleFormView.hide();
  }

  deleteEscalation(Id) {
    let isConfirm = confirm(`Are you sure you want to delete this record ?`);

    if (isConfirm)
      this.escalApi.deleteEscalation(`api/escalation/${Id}`).subscribe(
        (response) => {
          this.handleFormView.hide();
        },
        (error) => {
          console.log(error);
        }
      );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "escalation");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    this.getAllAudits();
    this.fillUserOptions();

    this.ruleEscalatedToIds = [];

    this.initDropdownSettings(
      false,
      "id",
      "firstName",
      "Select All",
      "Deselect All",
      3,
      false,
      true
    );

    $(document).ready(() => {
      $("#escalationComponent").on("click", ".editEscalation", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");
        let escalationData = $("#" + dataId).data();

        this.editEscalation(escalationData);
      });

      $("#escalationComponent").on("click", ".deleteEscalation", (e) => {
        let dataId = $(e.currentTarget).attr("data-id");

        this.deleteEscalation(dataId);
      });

      $("#escalationComponent").on("click", ".editRule", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.ruleIndex = dataIndex;
          this.isEditRule = true;

          this.editRule(parseInt(dataIndex));
        }
      });

      $("#escalationComponent").on("click", ".removeRule", (e) => {
        let dataIndex = window["jQuery"](e.currentTarget).attr("data-index");

        if (dataIndex) {
          this.removeRule(parseInt(dataIndex));
        }
      });
    });
  }
}
