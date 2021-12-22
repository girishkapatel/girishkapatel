import { Component, OnInit, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";
import { Router, ActivatedRoute } from "@angular/router";
import * as $ from "jquery";
import { environment } from "src/environments/environment";
import { LoginService } from "../login/login.service";

@Component({
  selector: "app-reset-password",
  templateUrl: "./reset-password.component.html",
  styleUrls: ["./reset-password.component.css"],
})
export class ResetPasswordComponent implements OnInit {
  constructor(
    private userlogin: LoginService,
    private router: Router,
    private activateroute: ActivatedRoute
  ) {}
  copyrightYear: any;
  reset_password: string = "";
  reset_confirmPassword: string = "";
  email: string = "";
  msgText: string = "";
  msgClass: string = "";
  showMessage: boolean;
  @ViewChild("resetPasswordForm", { static: false }) resetPasswordForm: NgForm;

  ngOnInit() {
    $("body").attr("class", "login");
    localStorage.removeItem("auth-token");
    var date = new Date();
    this.copyrightYear = date.getFullYear();

    this.email = this.activateroute.snapshot.queryParams["user"];
  }

  displayMessage(msgText, msgClass, autohide = true) {
    this.msgText = msgText;
    this.msgClass = msgClass;
    this.showMessage = true;
    if (autohide) {
      setTimeout(() => {
        this.showMessage = false;
      }, 5000);
    }
  }
  resetPassword(e) {
    e.preventDefault();

    if (this.resetPasswordForm.invalid) {
      return false;
    } else {
      let postData = { EmailId: this.email, Password: this.reset_password };
      this.userlogin
        .login("api/authentication/resetpassword", postData)
        .subscribe(
          (response) => {
            this.displayMessage("Password Changed Successfully", "alert-success-ey");

            this.router.navigate(["./login"], { queryParams: { pc: true } });
          },
          (error) => {}
        );
    }
  }
  back() {
    this.router.navigate(["./login"]);
  }
}
