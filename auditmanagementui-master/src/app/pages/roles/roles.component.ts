import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../common/table/table.model";
import { RolesService } from "./roles.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
import moduleArray from "../../common/data/modules.data";
import { string } from "@amcharts/amcharts4/core";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
@Component({
  selector: "app-roles",
  templateUrl: "./roles.component.html",
  styleUrls: ["./roles.component.css"],
  providers: [RolesService],
})
export class RolesComponent implements OnInit {
  constructor(
    private roles: RolesService,
    private utils: UtilsService,
    private notifyService: ToastrService
  ) {}

  @ViewChild("rolesForm", { static: false }) rolesForm: NgForm;

  moduleData: any;
  accessRights: any = {};
  isStackHolder: boolean = false;
  tableId: string = "roles_table";
  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Name",
      data: "name",
    },
    {
      title: "Status",
      data: "isActive",
      render: function (status) {
        return status ? "ACTIVE" : "INACTIVE";
      },
    },
    {
      title: "Action",
      data: "id",
      render: (data) => {
        let buttons = "";

        if (this.accessRights.isEdit && !this.isStackHolder)
          buttons =
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-info editRoles"><i class="fa fa-edit"></i></button>';

        if (this.accessRights.isDelete && !this.isStackHolder)
          buttons +=
            '<button type="button" id="' +
            data +
            '" class="btn btn-sm btn-danger deleteRoles"><i class="fa fa-trash"></i></button>';

        return buttons;
      },
    },
  ];

  tableData: tableData[] = [];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  rolesId: string = "";
  rolesName: string = "";
  status: boolean = true;
  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.tableFilters.next({});
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
      this.tableFilters.next({});
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveRoles(e) {
    if (this.rolesForm.invalid) return false;
    if (this.isEdit) {
      this.updateRoles();
    } else {
      this.addNewRoles();
    }
  }

  addNewRoles() {
    // let modObj = this.getModuleObj();

    let postData = this.rolesForm.form.value;
    postData.UIScopes = this.moduleData;
    postData.IsActive = postData.IsActive.toString() === "true" ? true : false;

    this.roles.addRoles("api/role", postData).subscribe(
      (response) => {
        this.notifyService.success("Roles Added Successfully");
        this.handleFormView.hide();
      },
      (err) => {
        this.notifyService.error(err.error);
      }
    );
  }

  updateRoles() {
    // let modObj = this.getModuleObj();

    let postData = this.rolesForm.form.value;
    postData.UIScopes = this.moduleData;
    postData.Id = this.rolesId;
    postData.IsActive = postData.IsActive.toString() === "true" ? true : false;

    this.roles.updateRoles("api/role", postData).subscribe(
      (response) => {
        this.notifyService.success("Roles Updated Successfully");

        let rolename = postData.Name.toLowerCase();
        let currentUserRole = localStorage.getItem("role").toLowerCase();

        if (rolename === currentUserRole) {
          localStorage.setItem("uis", JSON.stringify(postData.UIScopes));
        }
        this.handleFormView.hide();
      },
      (err) => {
        this.notifyService.error(err.error);
      }
    );
  }

  addRoles() {
    this.moduleData = moduleArray;

    // this.fillModules(this.moduleData);
    this.handleFormView.show();
  }

  editRoles(rolesData) {
    this.moduleData = rolesData.uiScopes;

    this.isEdit = true;
    this.rolesId = rolesData.id;
    this.rolesName = rolesData.name;
    this.status = rolesData.isActive;

    // this.fillModules(rolesData.uiScopes);
    this.handleFormView.show();
  }

  fillModules(roleData) {
    let moduleHtml = this.moduleHtml(roleData);
    $("#user-modules tbody").html(moduleHtml);
  }

  moduleHtml(roleData) {
    let moduleData = roleData;
    let moduleHtml = "";

    for (let i = 0; i < moduleData.length; i++) {
      let moduleId = moduleData[i].name.replace(/\s/g, "").toLowerCase();
      moduleHtml += `<tr id="access-${moduleId}">`;
      moduleHtml += `<td>${moduleData[i].name}</td>`;
      moduleHtml += `<td>
                      <input type="checkbox" ${
                        moduleData[i].access ? "checked" : ""
                      }/> <span class="text-capitalize"></span>
                    </td>`;
      moduleHtml += `<td>`;
      for (let j = 0; j < moduleData[i].submodules.length; j++) {
        moduleHtml += `<input type="checkbox" ${
          moduleData[i].submodules[j].access ? "checked" : ""
        }/> <span class="text-capitalize">${
          moduleData[i].submodules[j].name
        }</span><br/>`;
      }

      moduleHtml += `</td>`;

      moduleHtml += `</tr>`;
    }
    return moduleHtml;
  }

  getModuleObj() {
    let allTr = $("#user-modules tbody tr");
    let moduleArray = [];
    for (let i = 0; i < allTr.length; i++) {
      let submoduleArray = [];
      let moduleTrId = $(allTr[i]).attr("id");
      let moduleTrTd = $("#" + moduleTrId + " td");
      let moduleName = $(moduleTrTd[0]).text();
      let access = $(moduleTrTd[1])
        .children('input[type="checkbox"]')
        .is(":checked");
      let modules = $(moduleTrTd[2]).children("div.row").children("div");

      for (let j = 0; j < modules.length; j++) {
        let submoduleAccess = $(modules[j]).children("input").is(":checked");
        let submoduleName = $(modules[j]).children("span").text();
        let submoduleObj = {
          name: submoduleName,
          access: submoduleAccess,
        };
        submoduleArray.push(submoduleObj);
      }

      let moduleObj = {
        name: moduleName,
        access: access,
        submodules: submoduleArray,
      };

      moduleArray.push(moduleObj);
    }
    return moduleArray;
  }

  deleteRoles(rolesId) {
    this.roles.deleteRoles("api/role/" + rolesId).subscribe(
      (response) => {
        this.notifyService.success("Roles Deleted Successfully");
        this.handleFormView.hide();
      },
      (error) => {
        this.notifyService.error(error.error);
      }
    );
  }

  clearform() {
    this.rolesId = "";
    this.rolesName = "";
    this.status = true;
  }

  onView($event, item, level) {
    let module;

    if (level === 0) {
      module = this.moduleData.filter((x) => x.name === item.name)[0];
      module.access = $event;

      if (!$event) {
        module.isAdd = $event;
        module.isEdit = $event;
        module.isDelete = $event;
      }
    } else if (level === 1) {
      module = this.moduleData.find((x) => {
        return x.submodules.find((m) => m.name === item.name);
      });

      let submodule = module.submodules.find((a) => a.name === item.name);
      submodule.access = $event;

      if (!$event) {
        submodule.isAdd = $event;
        submodule.isEdit = $event;
        submodule.isDelete = $event;
      }
    } else if (level === 2) {
      let level2Module;

      this.moduleData.forEach((main) => {
        if (main.submodules) {
          main.submodules.forEach((level1) => {
            if (level1.submodules) {
              level1.submodules.forEach((level2) => {
                if (level2.name === item.name) {
                  module = main;
                  level2Module = level2;
                }
              });
            }
          });
        }
      });

      level2Module.access = $event;

      if (!$event) {
        level2Module.isAdd = $event;
        level2Module.isEdit = $event;
        level2Module.isDelete = $event;
      }
    }

    this.moduleData[module] = module;
  }

  onAdd($event, item, level) {
    let module;

    if (level === 0) {
      module = this.moduleData.filter((x) => x.name === item.name)[0];
      module.isAdd = $event;
    } else if (level === 1) {
      module = this.moduleData.find((x) => {
        return x.submodules.find((m) => m.name === item.name);
      });

      let submodule = module.submodules.find((a) => a.name === item.name);
      submodule.isAdd = $event;
    } else if (level === 2) {
      let level2Module;

      this.moduleData.forEach((main) => {
        if (main.submodules) {
          main.submodules.forEach((level1) => {
            if (level1.submodules) {
              level1.submodules.forEach((level2) => {
                if (level2.name === item.name) {
                  module = main;
                  level2Module = level2;
                }
              });
            }
          });
        }
      });

      level2Module.isAdd = $event;
    }

    this.moduleData[module] = module;
  }

  onEdit($event, item, level) {
    let module;

    if (level === 0) {
      module = this.moduleData.filter((x) => x.name === item.name)[0];
      module.isEdit = $event;
    } else if (level === 1) {
      module = this.moduleData.find((x) => {
        return x.submodules.find((m) => m.name === item.name);
      });

      let submodule = module.submodules.find((a) => a.name === item.name);
      submodule.isEdit = $event;
    } else if (level === 2) {
      let level2Module;

      this.moduleData.forEach((main) => {
        if (main.submodules) {
          main.submodules.forEach((level1) => {
            if (level1.submodules) {
              level1.submodules.forEach((level2) => {
                if (level2.name === item.name) {
                  module = main;
                  level2Module = level2;
                }
              });
            }
          });
        }
      });

      level2Module.isEdit = $event;
    }

    this.moduleData[module] = module;
  }

  onDelete($event, item, level) {
    let module;

    if (level === 0) {
      module = this.moduleData.filter((x) => x.name === item.name)[0];
      module.isDelete = $event;
    } else if (level === 1) {
      module = this.moduleData.find((x) => {
        return x.submodules.find((m) => m.name === item.name);
      });

      let submodule = module.submodules.find((a) => a.name === item.name);
      submodule.isDelete = $event;
    } else if (level === 2) {
      let level2Module;

      this.moduleData.forEach((main) => {
        if (main.submodules) {
          main.submodules.forEach((level1) => {
            if (level1.submodules) {
              level1.submodules.forEach((level2) => {
                if (level2.name === item.name) {
                  module = main;
                  level2Module = level2;
                }
              });
            }
          });
        }
      });

      level2Module.isDelete = $event;
    }

    this.moduleData[module] = module;
  }

  onGrantAll($event, item, level) {
    let module;

    if (level === 0) {
      module = this.moduleData.filter((x) => x.name === item.name)[0];
      module.access = $event;
      module.isAdd = $event;
      module.isEdit = $event;
      module.isDelete = $event;
    } else if (level === 1) {
      module = this.moduleData.find((x) => {
        return x.submodules.find((m) => m.name === item.name);
      });

      let submodule = module.submodules.find((a) => a.name === item.name);
      submodule.access = $event;
      submodule.isAdd = $event;
      submodule.isEdit = $event;
      submodule.isDelete = $event;
    } else if (level === 2) {
      let level2Module;

      this.moduleData.forEach((main) => {
        if (main.submodules) {
          main.submodules.forEach((level1) => {
            if (level1.submodules) {
              level1.submodules.forEach((level2) => {
                if (level2.name === item.name) {
                  module = main;
                  level2Module = level2;
                }
              });
            }
          });
        }
      });

      level2Module.access = $event;
      level2Module.isAdd = $event;
      level2Module.isEdit = $event;
      level2Module.isDelete = $event;
    }

    this.moduleData[module] = module;
  }

  checkAccess() {
    this.accessRights = this.utils.getSubmoduleAccess("roles")[0];
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder = localStorage.getItem("stackholder") == "true" ? true : false;
    $(document).ready(() => {
      $("#rolesComponent").on("click", ".editRoles", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        let rolesData = $("#" + dataId).data();
        this.editRoles(rolesData);
      });

      $("#rolesComponent").on("click", ".deleteRoles", (e) => {
        let rolesId = $(e.currentTarget).attr("id");
        this.deleteRoles(rolesId);
      });
    });
  }
}
