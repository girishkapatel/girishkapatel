import { Injectable } from "@angular/core";
import { ApiService } from "./../../../services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class AuditPlanService {
  constructor(private appApi: ApiService) {}

  getAuditPlan(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addAuditPlan(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    var userid=localStorage.getItem("userId");
    postData.CreatedBy=userid;
    return this.appApi.insertData(url, postData, params);
  }

  updateAuditPlan(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    var userid=localStorage.getItem("userId");
    postData.UpdatedBy=userid;
    return this.appApi.updateData(url, postData, params);
  }

  deleteAuditPlan(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }

  uploadFile(url: string, postData: any): Observable<any> {
    return this.appApi.insertData(url, postData);
  }

  removeUploadedFile(url: string): Observable<any> {
    return this.appApi.deleteData(url);
  }

  downloadFile(url: string): Observable<Blob> {
    return this.appApi.downloadFile(url);
  }
  exportToExcel(url: string): Observable<Blob> {
    return this.appApi.downloadFile(url);
  }
}
