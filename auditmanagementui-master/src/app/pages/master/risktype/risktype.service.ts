import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "src/app/services/api/api.service";

@Injectable()
export class RiskTypeService {
  constructor(private appApi: ApiService) {}

  getRiskType(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addRiskType(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateRiskType(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteRiskType(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }
}
