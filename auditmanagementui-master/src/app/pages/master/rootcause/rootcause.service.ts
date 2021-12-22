import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "src/app/services/api/api.service";

@Injectable()
export class RootCauseService {
  constructor(private appApi: ApiService) {}

  getRootCause(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addRootCause(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateRootCause(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteRootCause(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }
}
