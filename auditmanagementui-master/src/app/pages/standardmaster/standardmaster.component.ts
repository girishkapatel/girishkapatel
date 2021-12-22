import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { UtilsService } from "src/app/services/utils/utils.service";

@Component({
  selector: "app-standardmaster",
  templateUrl: "./standardmaster.component.html",
  styleUrls: ["./standardmaster.component.css"],
})
export class StandardMasterComponent implements OnInit {
  constructor(private utils: UtilsService, private router: Router) {}

  moduleName = "master";

  standardmaster: Object = {};

  ngOnInit() {
    this.checkAccess();
  }

  checkAccess() {
    var standardmaster = this.utils.getSubmoduleAccess(this.moduleName)[0];

    let submodules = standardmaster.submodules;

    if (submodules) {
      let submoduleAccess = {
        countrystatecity: true,
        company: true,
        location: true,
        processlocationmapping: true,
        processmaster: true,
        risktype: true,
        reportconsideration: true,
        recommendation: true,
        rootcause: true,
        impact: true,
        trialbalance: true,
        observationgrading: true,
        escalation: true,
        designation: true,
        controlperformanceindicator: true,
        activitylogs: true,
      };

      submodules.forEach((element) => {
        let name = element.name.replace(/\s/g, "").toLowerCase();

        if (name === "country,state,city")
          submoduleAccess.countrystatecity = element.access;

        if (name === "company") submoduleAccess.company = element.access;

        if (name === "location") submoduleAccess.location = element.access;

        if (name === "processlocationmapping")
          submoduleAccess.processlocationmapping = element.access;

        if (name === "processmaster")
          submoduleAccess.processmaster = element.access;

        if (name === "risktype") submoduleAccess.risktype = element.access;

        if (name === "reportconsideration")
          submoduleAccess.reportconsideration = element.access;

        if (name === "recommendation")
          submoduleAccess.recommendation = element.access;

        if (name === "rootcause") submoduleAccess.rootcause = element.access;

        if (name === "impact") submoduleAccess.impact = element.access;

        if (name === "trialbalance")
          submoduleAccess.trialbalance = element.access;

        if (name === "observationgrading")
          submoduleAccess.observationgrading = element.access;

        if (name === "escalation") submoduleAccess.escalation = element.access;

        if (name === "designation")
        submoduleAccess.designation = element.access;
        if (name === "controlperformanceindicator")
        submoduleAccess.controlperformanceindicator = element.access;
        if (name === "activitylogs")
        submoduleAccess.activitylogs = element.access;
      });

      this.standardmaster = submoduleAccess;
    }
  }
}
