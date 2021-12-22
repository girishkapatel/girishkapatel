import { Component, OnInit } from "@angular/core";
import { UtilsService } from "src/app/services/utils/utils.service";

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.css"],
})
export class SidebarComponent implements OnInit {
  constructor(private utils: UtilsService) {}
  isAuditee: boolean = false;
  menuAccess: Object = {
    dashboard: true,
    auditplanningengine: true,
    manageaudits: true,
    action: true,
    reports: true,
    knowledgelibrary: true,
    eybenchmarks: true,
    users: true,
    roles: true,
    master: true,
  };

  ngOnInit() {
    let scopeObj = this.utils.getScopeObj();
    this.isAuditee = localStorage.getItem("role") == "Auditee" ? true : false;
    if (scopeObj.length) {
      for (let routes of scopeObj) {
        let rname = routes.name.replace(/\s/g, "").toLowerCase();

        if (this.menuAccess[rname]) {
          this.menuAccess[rname] = routes.access;

          if (routes.access && routes.submodules.length) {
            let hasAccess = [];

            for (let submodule of routes.submodules) {
              if (submodule.access) {
                hasAccess.push(submodule);
              }
            }

            this.menuAccess[rname] = hasAccess.length ? true : false;
          }
        }
      }
    }
  }
}
