import { Component, OnInit } from "@angular/core";
import { Router, ActivatedRoute, ParamMap } from "@angular/router";
import { ApiService } from "../../../services/api/api.service";
import { CommonApiService } from "src/app/services/utils/commonapi.service";
import { UtilsService } from "src/app/services/utils/utils.service";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-auditedit",
  templateUrl: "manageaudits-edit.component.html",
  styleUrls: ["./manageaudits-edit.component.css"],
})
export class ManageauditsEditComponent implements OnInit {
  constructor(
    private router: Router,
    private api: ApiService,
    private commonApi: CommonApiService,
    private utils: UtilsService,
    private spinner: NgxSpinnerService,
    private notifyService: ToastrService,
    private route: ActivatedRoute
  ) {}

  accessRights: any = {};

  tabIndex: number = 0;

  shOpts: any = [];
  auditName: any;
  auditProcess: any;
  location: any;
  overallAuditStartDate: any;
  overallAuditEndDate: any;
  auditNumber: any;
  scheduleId: any;
  quarter: any;
  auditStatus: any;
  id: any;

  setActiveParentTab: boolean = false;
  setActiveChildTab: boolean = false;
  setRACMActiveChildTab: boolean = false;
  setTestingofControlActiveChildTab: boolean = false;
  setDisscussionNoteActiveChildTab: boolean = false;

  manageaudits: Object = {
    noaccess: false,
    planning: {
      status: true,
      active: true,
      activity: {
        status: true,
        active: true,
      },
      tor: {
        status: true,
        active: false,
      },
      dataTracker: {
        status: true,
        active: false,
      },
    },
    auditexecution: {
      status: true,
      active: false,
      racm: {
        status: true,
        active: false,
      },
      testingOfControls: {
        status: true,
        active: false,
      },
      discussionNote: {
        status: true,
        active: false,
      },
    },
    reporting: {
      status: true,
      active: false,
      draftReport: {
        status: true,
        active: false,
      },
      finalReport: {
        status: true,
        active: false,
      },
    },
    eybenchmark: {
      status: true,
      active: false,
    },
    auditclosure: {
      status: true,
      active: false,
    },
    followup: {
      status: true,
      active: false,
    },
  };

  auditId: string = "";
  scopeandscheduleapi: string = "";
  auditData: any = [];
  userOpts: any = [];
  approverOpts: any = [];

  checkIfApprover(schedule) {
    localStorage.setItem("isApprover", "false");
    if (
      schedule[0].auditApprovalMapping &&
      Array.isArray(schedule[0].auditApprovalMapping.userData)
    ) {
      let userId = localStorage.getItem("userId");
      if (
        schedule[0].auditApprovalMapping.userData.filter(
          (x) => x.user.id === userId
        ).length
      ) {
        localStorage.setItem("isApprover", "true");
      }
    }
  }

  checkIfUser(schedule) {
    localStorage.setItem("isUser", "false");
    if (schedule[0].auditResources && schedule[0].auditResources.length) {
      let userId = localStorage.getItem("userId");
      let userArray = schedule[0].auditResources.filter(
        (userObj) => userObj.userId === userId
      );
      if (userArray.length) {
        localStorage.setItem("isUser", "true");
      }
    }
  }

  fillUserOptions(schedule) {
    this.userOpts = schedule[0]["auditResources"]
      ? schedule[0]["auditResources"]
      : [];
    this.approverOpts =
      schedule[0]["auditApprovalMapping"] &&
      schedule[0]["auditApprovalMapping"].userData
        ? schedule[0]["auditApprovalMapping"].userData
        : [];

    this.fillApproverTable(this.approverOpts);
    this.fillResourcesTable(this.userOpts);
  }

  saveAudit() {
    this.api
      .updateData("api/scopeandschedule/UpdateInfo", {
        Id: this.id,
        Status: this.auditStatus,
        AuditId: this.auditId,
      })
      .subscribe((res) => {
        this.notifyService.success("Audit updated successfully");
      });
  }

  fillApproverTable(approverOpts) {
    let tableColumns = [
      {
        title: "Name",
        data: "user",
        render: function (data) {
          return `${data.firstName} ${data.lastName}`;
        },
      },
      {
        title: "Responsibility",
        data: "responsibility",
      },
      {
        title: "Designation",
        data: "user.designation",
      },
      {
        title: "Experience",
        data: "user.experiance",
      },
      {
        title: "Qualification",
        data: "user.qualification",
      },
    ];

    this.commonApi.initialiseTable(
      "approversTable",
      approverOpts,
      tableColumns
    );
  }

  fillResourcesTable(resourcesOpts) {
    let tableColumns = [
      {
        title: "Name",
        data: "user",
        render: function (data) {
          return `${data.firstName} ${data.lastName}`;
        },
      },
      {
        title: "Designation",
        data: "user.designation",
      },
      {
        title: "Experience",
        data: "user.experiance",
      },
      {
        title: "Qualification",
        data: "user.qualification",
      },
      {
        title: "Man Days",
        data: "manDaysRequired",
      },
      {
        title: "Start Date",
        data: "auditStartDate",
        render: (data) => {
          if (data) {
            return this.utils.formatDateYYYYMMDD(data);
          } else {
            return "";
          }
        },
      },
      {
        title: "End Date",
        data: "auditEndDate",
        render: (data) => {
          if (data) {
            return this.utils.formatDateYYYYMMDD(data);
          } else {
            return "";
          }
        },
      },
    ];

    this.commonApi.initialiseTable(
      "resourcesTable",
      resourcesOpts,
      tableColumns
    );
  }

  fillStakeHolderOptions() {
    this.commonApi.getStakeholders().subscribe((posts) => {
      this.shOpts = posts;
      this.shOpts.forEach((user) => {
        user["custom"] = user.firstName + " " + user.lastName;
      });
    });
  }

  backToAuditForm() {
    this.router.navigate(["./pages/manageaudits"]);
  }

  getPlanningAccess() {
    let level1Modules = this.utils.getAccessOnLevel1(
      "manageaudits",
      "planning"
    );

    let level2Modules = level1Modules.submodules;

    let activityAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "activity"
    )[0];

    let torAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "tor"
    )[0];

    let dataTrackerAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "datatracker"
    )[0];

    this.manageaudits["planning"].status = level1Modules.access;
    this.manageaudits["planning"].activity.status = activityAccess.access;
    this.manageaudits["planning"].tor.status = torAccess.access;
    this.manageaudits["planning"].dataTracker.status = dataTrackerAccess.access;

    if (level1Modules.access && !this.setActiveParentTab) {
      this.manageaudits["planning"].active = level1Modules.access;
      this.setActiveParentTab = true;
    }

    if (activityAccess.access && !this.setActiveChildTab) {
      this.manageaudits["planning"].activity.active = activityAccess.access;
      this.setActiveChildTab = true;
    }

    if (torAccess.access && !this.setActiveChildTab) {
      this.manageaudits["planning"].tor.active = torAccess.access;
      this.setActiveChildTab = true;
    }

    if (dataTrackerAccess.access && !this.setActiveChildTab) {
      this.manageaudits["planning"].dataTracker.active =
        dataTrackerAccess.access;
      this.setActiveChildTab = true;
    }
  }

  getAuditExceptionAccess() {
    let level1Modules = this.utils.getAccessOnLevel1(
      "manageaudits",
      "auditexecution"
    );

    let level2Modules = level1Modules.submodules;
    level2Modules.forEach((element, Index) => {
      // level2Modules[0]["access"]
      if (
        element.name.replace(/\s/g, "").toLowerCase() === "racm" &&
        element.access
      ) {
        this.setRACMActiveChildTab = true;
        return false;
      }
      if (
        element.name.replace(/\s/g, "").toLowerCase() === "testingofcontrols" &&
        element.access
      ) {
        this.setTestingofControlActiveChildTab = true;
        return false;
      }
      if (
        element.name.replace(/\s/g, "").toLowerCase() === "discussionnote" &&
        element.access
      ) {
        this.setDisscussionNoteActiveChildTab = true;
        return false;
      }
    });
    let racmAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "racm"
    )[0];

    let testingOfControlsAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "testingofcontrols"
    )[0];

    let discussionNoteAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "discussionnote"
    )[0];

    this.manageaudits["auditexecution"].status = level1Modules.access;
    this.manageaudits["auditexecution"].racm.status = racmAccess.access;
    this.manageaudits["auditexecution"].testingOfControls.status =
      testingOfControlsAccess.access;
    this.manageaudits["auditexecution"].discussionNote.status =
      discussionNoteAccess.access;

    if (level1Modules.access && !this.setActiveParentTab) {
      this.manageaudits["auditexecution"].active = level1Modules.access;
      this.setActiveParentTab = true;
    }

    if (racmAccess.access && !this.setActiveChildTab) {
      this.manageaudits["auditexecution"].racm.active = racmAccess.access;
      this.setActiveChildTab = true;
    }

    if (testingOfControlsAccess.access && !this.setActiveChildTab) {
      this.manageaudits["auditexecution"].testingOfControls.active =
        testingOfControlsAccess.access;
      this.setActiveChildTab = true;
    }

    if (discussionNoteAccess.access && !this.setActiveChildTab) {
      this.manageaudits["auditexecution"].discussionNote.active =
        discussionNoteAccess.access;
      this.setActiveChildTab = true;
    }
  }

  getReportingAccess() {
    let level1Modules = this.utils.getAccessOnLevel1(
      "manageaudits",
      "reporting"
    );

    let level2Modules = level1Modules.submodules;

    let draftReportAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "draftreport"
    )[0];

    let finalReportAccess = level2Modules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "finalreport"
    )[0];

    this.manageaudits["reporting"].status = level1Modules.access;
    this.manageaudits["reporting"].draftReport.status =
      draftReportAccess.access;
    this.manageaudits["reporting"].finalReport.status =
      finalReportAccess.access;

    if (level1Modules.access && !this.setActiveParentTab) {
      this.manageaudits["reporting"].active = level1Modules.access;
      this.setActiveParentTab = true;
    }

    if (draftReportAccess.access && !this.setActiveChildTab) {
      this.manageaudits["reporting"].draftReport.active =
        draftReportAccess.access;
      this.setActiveChildTab = true;
    }

    if (finalReportAccess.access && !this.setActiveChildTab) {
      this.manageaudits["reporting"].finalReport.active =
        finalReportAccess.access;
      this.setActiveChildTab = true;
    }
  }

  checkAccess() {
    let mainModule = this.utils.getSubmoduleAccess("manageaudits")[0];

    this.getPlanningAccess();
    this.getAuditExceptionAccess();
    this.getReportingAccess();

    let eyBenchmarkAccess = mainModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "eybenchmark"
    )[0];

    this.manageaudits["eybenchmark"].status = eyBenchmarkAccess.access;

    if (eyBenchmarkAccess.access && !this.setActiveParentTab) {
      this.manageaudits["eybenchmark"].active = eyBenchmarkAccess.access;
      this.setActiveParentTab = true;
    }

    let auditClosureAccess = mainModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "auditclosure"
    )[0];

    this.manageaudits["auditclosure"].status = auditClosureAccess.access;

    if (auditClosureAccess.access && !this.setActiveParentTab) {
      this.manageaudits["auditclosure"].active = auditClosureAccess.access;
      this.setActiveParentTab = true;
    }

    let followUpAccess = mainModule.submodules.filter(
      (a) => a.name.replace(/\s/g, "").toLowerCase() === "followup"
    )[0];

    this.manageaudits["followup"].status = followUpAccess.access;

    if (followUpAccess.access && !this.setActiveParentTab) {
      this.manageaudits["followup"].active = followUpAccess.access;
      this.setActiveParentTab = true;
    }

    // let scopeObj = this.utils.getScopeObj();
    // if (scopeObj.length) {
    //   let manageAuditObj = scopeObj.filter(
    //     (x) => x.name.toLowerCase() === "manage audits"
    //   );
    //   let submodule = manageAuditObj[0].submodules;
    //   if (submodule.length) {
    //     let setActive = false;
    //     for (let module of submodule) {
    //       let moduleName = module.name.toLowerCase().replace(/\s/g, "");
    //       if (this.manageaudits[moduleName]) {
    //         this.manageaudits[moduleName].status = module["access"];
    //         if (module["access"] && !setActive) {
    //           this.manageaudits[moduleName].active = true;
    //           setActive = true;
    //         }
    //       }
    //     }

    //     if (!setActive) {
    //       this.manageaudits["noaccess"] = true;
    //     }
    //   }
    // }
  }

  onParentTabClick(index) {
    this.onTabClick(index);
    if (index === 0) {
      $('a[href="#Activity"]').trigger("click");
      this.manageaudits["planning"].activity.active = true;
    } else if (index === 3) {
      if (this.setRACMActiveChildTab) {
        $('a[href="#walkthrough"]').trigger("click");
        this.manageaudits["auditexecution"].racm.active = true;
        this.onTabClick(3);
      } else if (this.setTestingofControlActiveChildTab) {
        $('a[href="#testing"]').trigger("click");
        this.manageaudits["auditexecution"].testingOfControls.active = true;
        this.onTabClick(4);
      } else if (this.setDisscussionNoteActiveChildTab) {
        $('a[href="#discussionnote"]').trigger("click");
        this.manageaudits["auditexecution"].discussionNote.active = true;
        this.onTabClick(5);
      }
    } else if (index === 6) {
      $('a[href="#auditreport"]').trigger("click");
      this.manageaudits["reporting"].draftReport.active = true;
    }
  }

  onTabClick(index) {
    this.tabIndex = index;
  }

  fillAuditInfo(auditSchedule) {
    this.id = auditSchedule[0].id;
    this.auditName = auditSchedule[0].audit.processLocationMapping.auditName;
    this.auditProcess = auditSchedule[0].audit.overallAssesment.processL1.name;
    this.location = auditSchedule[0].audit.location.profitCenterCode;
    this.overallAuditStartDate = this.utils.formatDbDateToDMY(
      auditSchedule[0].auditStartDate
    );
    this.overallAuditEndDate = this.utils.formatDbDateToDMY(
      auditSchedule[0].auditEndDate
    );
    this.auditNumber = auditSchedule[0].auditNumber;
    this.scheduleId = auditSchedule[0].id;
    this.quarter = auditSchedule[0].quater;
    this.auditStatus = auditSchedule[0].status || "";
  }

  ngOnInit() {
    this.checkAccess();
    this.route.queryParams.subscribe((params) => {
      if (params["auditId"]) {
        localStorage.setItem("auditId", params.auditId);
        this.auditId = params.auditId;
        this.commonScopeSchedule();
        var index = Number(params.tabIndex);
        this.tabIndex = index;
        this.manageaudits["planning"].active = false;
        if (index === 0) {
          $('a[href="#Activity"]').trigger("click");
          this.manageaudits["planning"].activity.active = true;
        } // else if (index === 5) {
        //   $('a[href="#discussionnote"]').trigger("click");
        //   this.manageaudits["auditexecution"].active = true;
        //   this.manageaudits["auditexecution"].discussionNote.active = true;
        // }
        else if (index === 6) {
          $('a[href="#auditreport"]').trigger("click");
          this.manageaudits["reporting"].active = true;
          this.manageaudits["reporting"].draftReport.active = true;
        }
      } else {
        this.auditId = localStorage.getItem("auditId");
        this.commonScopeSchedule();
      }
    });
  }
  commonScopeSchedule() {
    this.scopeandscheduleapi = `api/scopeandschedule/GetByAudit/${this.auditId}`;
    this.api.getData(this.scopeandscheduleapi).subscribe((schedule) => {
      this.fillAuditInfo(schedule);
      this.checkIfApprover(schedule);
      this.checkIfUser(schedule);
      this.auditData = schedule;
      this.fillUserOptions(schedule);
    });
    this.fillStakeHolderOptions();
  }
}
