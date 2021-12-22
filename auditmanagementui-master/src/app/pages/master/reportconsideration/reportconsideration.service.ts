import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "src/app/services/api/api.service";

@Injectable()
export class ReportConsiderationService {
  constructor(private appApi: ApiService) {}

  getReportConsideration(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addReportConsideration(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateReportConsideration(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteReportConsideration(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }
}
