import { Component, OnInit } from "@angular/core";
import { UtilsService } from "src/app/services/utils/utils.service";

@Component({
  selector: "app-master",
  templateUrl: "./master.component.html",
  styleUrls: ["./master.component.css"],
})
export class MasterComponent implements OnInit {
  constructor(private utils: UtilsService) {}

  moduleName = "auditplanningengine";
  master: object = {};

  ngOnInit() {
    this.checkAccess();
  }

  checkAccess() {
    var moduleObj = this.utils.getSubmoduleAccess(this.moduleName)[0];

    let submodules = moduleObj.submodules;

    if (submodules) {
      let submoduleAccess = {
        ermrisk: true,
        processriskmapping: true,
        keybusinessinitiative: true,
        overallassessment: true,
        auditplan: true,
      };

      submodules.forEach((element) => {      
        let name = element.name.replace(/\s/g, "").toLowerCase();

        if (name === "ermrisk") submoduleAccess.ermrisk = element.access;

        if (name === "processriskmapping")
          submoduleAccess.processriskmapping = element.access;

        if (name === "keybusinessinitiative")
          submoduleAccess.keybusinessinitiative = element.access;

        if (name === "overallassessment")
          submoduleAccess.overallassessment = element.access;

        if (name === "auditplan") submoduleAccess.auditplan = element.access;
      });

      this.master = submoduleAccess;
    }
  }
}
