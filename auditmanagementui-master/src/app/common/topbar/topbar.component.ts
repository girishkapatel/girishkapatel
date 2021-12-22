import { Component, OnInit } from "@angular/core";
import * as $ from "jquery";
import { Router } from "@angular/router";
import { ApiService } from "src/app/services/api/api.service";
import{interval,Subscription, timer}from "rxjs";
@Component({
  selector: "app-topbar",
  templateUrl: "./topbar.component.html",
  styleUrls: ["./topbar.component.css"],
})
export class TopbarComponent implements OnInit {
  timerSubscription:Subscription;
  constructor(private router: Router, private api: ApiService) { }
  rolename = "Admin";
  userName: string = "";
  totalNotification: number = 0;

  TableDataForOverAlls: any = [];
  ngOnInit() {
    this.userName = localStorage.getItem("userName")
      ? localStorage.getItem("userName")
      : "User";
    $("body").attr(
      "class",
      "page-header-fixed page-sidebar-closed-hide-logo page-content-white page-footer-fixed page-sidebar-fixed"
    );
    const source=interval(10000);
    this.timerSubscription=source.subscribe(p=>
      this.fillNotification()
      );
  }
  ngOnDestroy() {
    // For method 1
    this.timerSubscription && this.timerSubscription.unsubscribe();
  }
  logout() {
    var userid = localStorage.getItem("userId");
    this.api.getData("api/authentication/LogOut/" + userid).subscribe(
      (response) => {
        console.log("Logout");
      },
      (error) => {
        console.log(error);
      }
    );
    this.router.navigate(["../../"]);
    localStorage.clear();
  }
  redirectNotification(auditid, module, id) {
    var moduleIndex;
    var userid = localStorage.getItem("userId");
    this.api
      .getData(`api/notification/updateNotificationStatus/${id}/${userid}`)
      .subscribe(
        (response) => {
          this.fillNotification();
        },
        (error) => {
          console.log(error);
        }
      );
    switch (module) {
      case "DiscussionNote":
        moduleIndex = 5;
        break;
      case "DraftReport":
        moduleIndex = 6;
        break;
      case "DraftReport":
        moduleIndex = 7;
        break;
      default:
        moduleIndex = 1;
        break;
    }
    this.router.navigate(["../pages/manageaudits/edit/"], {
      queryParams: { auditId: auditid, tabIndex: moduleIndex },
    });
  }
  fillNotification() {
    var userid = localStorage.getItem("userId");

    this.api
      .getData("api/Notification/GetNotification/" + userid)
      .subscribe((data) => {
        this.totalNotification=0;
        this.TableDataForOverAlls = data;
        this.TableDataForOverAlls.forEach((res) => {
          if(!res.isReadable){
            this.totalNotification++;
          }
        });
        // this.totalNotification =
        //   this.TableDataForOverAlls.length != 0
        //     ? this.TableDataForOverAlls.length
        //     : "";

        // this.TableDataForOverAlls.forEach((a) => {
        //   (this.TotalControlsIdentifiedTested += a.controlsIdentifiedTested),
        //     (this.TotalDesignControlRating += a.designControlRating),
        //     (this.TotalControlswithObservation += a.controlswithObservation),
        //     (this.TotalControlwithnoException += a.controlwithnoException),
        //     (this.TotalOEControlRating += a.oeControlRating);
        // });
      });
  }
}
