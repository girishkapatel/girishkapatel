import { Component, OnInit } from "@angular/core";
import { UtilsService } from "src/app/services/utils/utils.service";

@Component({
  selector: "app-library",
  templateUrl: "./library.component.html",
  styleUrls: ["./library.component.css"],
})
export class LibraryComponent implements OnInit {
  constructor(private utils: UtilsService) {}

  moduleName = "knowledgelibrary";

  accessRights: any = {};
  library: Object = {};

  checkAccess() {
    var library = this.utils.getSubmoduleAccess(this.moduleName)[0];

    let submodules = library.submodules;

    if (submodules) {
      let submoduleAccess = {
        processuniverse: true,
        racm: true,
      };

      submodules.forEach((element) => {
        let name = element.name.replace(/\s/g, "").toLowerCase();

        if (name === "processuniverse")
          submoduleAccess.processuniverse = element.access;

        if (name === "racm") submoduleAccess.racm = element.access;
      });

      this.library = submoduleAccess;
    }
  }

  ngOnInit() {
    this.checkAccess();
  }
}
