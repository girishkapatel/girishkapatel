import { BrowserModule } from "@angular/platform-browser";
import { CommonModule, DatePipe } from "@angular/common";
import { APP_BASE_HREF } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { SidebarComponent } from "./common/sidebar/sidebar.component";
import { TopbarComponent } from "./common/topbar/topbar.component";
import { FooterComponent } from "./common/footer/footer.component";
import { MainComponent } from "./common/main/main.component";
import { LoginComponent } from "./login/login.component";
import { ApiService } from "./services/api/api.service";
import { UtilsService } from "./services/utils/utils.service";
import { CommonApiService } from "./services/utils/commonapi.service";
import { LoginService } from "./login/login.service";
import { AuthInterceptorService } from "./services/auth/auth-interceptor.service";
import { NgbModule, NgbDateParserFormatter } from "@ng-bootstrap/ng-bootstrap";
import { RoleGaurd } from "./services/auth/auth.gaurd";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { ReactiveFormsModule } from "@angular/forms";
import { BusinessProcessService } from "./pages/master/process/businessprocess/businessprocess.service";
import { ProcessLevel1Service } from "./pages/master/process/processlevel1/processlevel1.service";
import { PL2Service } from "./pages/master/process/processlevel2/processlevel2.service";
import { CKEditorModule } from "@ckeditor/ckeditor5-angular";
import { UiSwitchModule } from "ngx-ui-switch";
import { MustMatchDirective } from "./login/must-match-directive";
import { ResetPasswordComponent } from "./reset-password/reset-password.component";
import { ToastrModule } from "ngx-toastr";
import { NgbDateCustomParserFormatter } from "./services/utils/NgbDateCustomParserFormatter";
import {TimeAgoPipe} from 'time-ago-pipe';
@NgModule({
  declarations: [
    AppComponent,
    SidebarComponent,
    TopbarComponent,
    FooterComponent,
    MainComponent,
    LoginComponent,
    MustMatchDirective,
    ResetPasswordComponent, 
    TimeAgoPipe,  
  ],
  imports: [
    BrowserModule,
    CommonModule,
    AppRoutingModule,
    HttpClientModule,
    CKEditorModule,
    FormsModule,
    NgbModule,
    NgxSpinnerModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    UiSwitchModule,
    ToastrModule.forRoot({
      timeOut: 2000,
      positionClass: "toast-top-right",
      closeButton: true,
      progressBar: true,
    }),
  ],
  providers: [
    { provide: APP_BASE_HREF, useValue: "/auditmanagement/" },
    ApiService,
    DatePipe,
    UtilsService,
    CommonApiService,
    LoginService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptorService,
      multi: true,
    },
    RoleGaurd,
    NgxSpinnerService,
    BusinessProcessService,
    ProcessLevel1Service,
    PL2Service,
    {
      provide: NgbDateParserFormatter,
      useClass: NgbDateCustomParserFormatter,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
