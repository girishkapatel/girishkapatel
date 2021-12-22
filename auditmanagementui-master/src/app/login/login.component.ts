import { Component, OnInit, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";
import { LoginService } from "./login.service";
import { Router, ActivatedRoute } from "@angular/router";
import * as $ from "jquery";
import { environment } from "src/environments/environment";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"],
})
export class LoginComponent implements OnInit {
  constructor(
    private userlogin: LoginService,
    private router: Router,
    private activateroute: ActivatedRoute
  ) {}

  showMessage: boolean;
  msgText: string = "";
  msgClass: string = "";
  role: string = "";
  loginFormshow: boolean = true;
  forgotFormshow: boolean = false;
  copyrightYear: any;
  emailid: string = "";
  forgotModel: any;
  userID: string = "";
  @ViewChild("loginform", { static: false }) loginForm: NgForm;
  @ViewChild("forgotPasswordform", { static: false })
  forgotPasswordform: NgForm;

  ngOnInit() {
    $("body").attr("class", "login");
    localStorage.removeItem("auth-token");
    var date = new Date();
    this.copyrightYear = date.getFullYear();

    var IsChangePassword = this.activateroute.snapshot.queryParams["pc"];
    if (IsChangePassword) {
      this.displayMessage(
        "Password has been Changed Successfully",
        "alert-success-ey"
      );
    }
  }

  userLogin(e) {
    e.preventDefault();
    this.displayMessage("Authenticating...", "alert-success-ey", false);
    // this.role = "lead";
    // this.handleLogin({login:true});
    if (this.validateCreds(this.loginForm.form.value)) {
      this.userlogin
        .login("api/authentication/login", this.loginForm.form.value)
        .subscribe(
          (response) => {
            this.handleLogin(response);
          },
          (error) => {
            this.handleError(error);
          }
        );
    }
  }

  handleError(error) {
    let errorMessage = "";
    if (error.status == 401) {
      errorMessage = "Please enter valid credentials";
    }
    else if (error.error && error.error.message) {
      errorMessage = error.error.error;
    } else {
      errorMessage = "Unable to login";
    }
    this.displayMessage(errorMessage, "alert-danger-ey");
  }

  handleLogin(response) {
    this.displayMessage("Login Successful", "alert-success-ey");
    localStorage.setItem("user", JSON.stringify(response));
    localStorage.setItem("auth-token", response.Token);
    localStorage.setItem("userId", response.UserId);
    localStorage.setItem("role", response.Role);
    localStorage.setItem("uis", JSON.stringify(response.UIScopes));
    localStorage.setItem(
      "userName",
      `${response.FirstName} ${response.LastName}`
    );
    localStorage.setItem("stackholder", response.StakeHolder);
    this.redirectToDashoard();
  }

  redirectToDashoard() {
    this.router.navigate(["./pages"]);
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

  validateCreds(creds: { [key: string]: string }) {
    let isValid = false;
    if (creds.EmailId && creds.Password) {
      isValid = true;
    } else {
      this.displayMessage(
        "Username and Password cannot be blank",
        "alert-danger-ey"
      );
    }
    return isValid;
  }
  showforgotPassword() {
    this.loginFormshow = false;
    this.forgotFormshow = true;
  }
  validateforgot(creds: { [key: string]: string }) {
    let isValid = false;
    if (creds.EmailId) {
      isValid = true;
    } else {
      this.displayMessage(
        "Username and Password cannot be blank",
        "alert-danger-ey"
      );
    }
    return isValid;
  }

  forgotPassword(e) {
    e.preventDefault();
    if (this.forgotPasswordform.invalid) {
      return false;
    } else {
      this.userlogin
        .login("api/authentication/checkmail", { EmailId: this.emailid })
        .subscribe(
          (response) => {
            this.userID = response.id;
            this.forgot();
          },
          (error) => {
            this.handleErrorForgot(error);
          }
        );
    }
  }

  forgot() {
    let postData = {
      EmailId: this.emailid,
      ResetURL: environment.weburl + "/resetpassword?user=" + this.userID + "",
    };
    this.userlogin
      .login("api/authentication/forgotpassword", postData)
      .subscribe(
        (response) => {
          this.displayMessage(
            "The link has been sent, please check your email to reset your password.",
            "alert-success-ey"
          );
          this.loginFormshow = true;
          this.forgotFormshow = false;
        },
        (error) => {
          this.handleError(error);
        }
      );
  }
  handleErrorForgot(error) {
    let errorMessage = "";
    if (error.error && error.error.message) {
      errorMessage = error.error.error;
    } else {
      errorMessage = error.error;
    }
    this.displayMessage(errorMessage, "alert-danger-ey");
  }
  back() {
    this.loginFormshow = true;
    this.forgotFormshow = false;
  }
}
