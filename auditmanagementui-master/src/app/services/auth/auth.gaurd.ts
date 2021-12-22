import { Injectable } from "@angular/core";
import { CanActivate, ActivatedRouteSnapshot, Router } from "@angular/router";
@Injectable({
  providedIn: "root",
})
export class RoleGaurd implements CanActivate {
  constructor(private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot) {
    let uiscopes = localStorage.getItem("uis");
    if (uiscopes) {
      let segments = route["_urlSegment"].segments;
      let module = "";

      /*This is flag to check redirection from module rout to submodule route */
      let isRedirection = false;

      let path = route.routeConfig.path;
      if (segments.length === 3) {
        module = segments[1].path;
        if (module === path) {
          path = segments[2].path;
          isRedirection = true;
        }
      }
      let scopeObj = JSON.parse(uiscopes);
      let comparePath = module ? module : path;
      /*Main Menu Route Check*/
      let routeObj = scopeObj.filter((x) => this.checkModule(x, comparePath));
      if (routeObj.length === 1 && routeObj[0].access) {
        if (module) {
          /*Sub Module Route Check*/
          let modObj = routeObj[0].submodules.filter((x) =>
            this.checkModule(x, path)
          );
          if (modObj.length === 1 && modObj[0].access) {
            return true;
          } else if (isRedirection) {
            /*This logic is to handle case when the default sub module route set on mudule route click is not given access */
            modObj = routeObj[0].submodules.filter((x) => x.access);
            if (modObj.length > 0) {
              this.navigateToModule(modObj[0], module);
            }
          }
        } else {
          return true;
        }
      } else if (path.toLowerCase() === "dashboard") {
        routeObj = scopeObj.filter((x) => x.access);
        if (routeObj.length > 0) {
          this.navigateToModule(routeObj[0], module);
        }
      }
    }
    return false;
  }

  checkModule(x, path) {
    let rname = x.name.replace(/\s/g, "").replace(/,/g, "").toLowerCase();
    if (rname === path) {
      return true;
    }
    return false;
  }

  navigateToModule(modObj, module) {
    let newpath = modObj.name
      .replace(/\s/g, "")
      .replace(/,/g, "")
      .toLowerCase();
    let parsedUrl = `/pages/${module}/${newpath}`;
    this.router.navigate([parsedUrl]);
  }
}
