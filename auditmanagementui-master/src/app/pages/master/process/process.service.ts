import { Injectable } from "@angular/core";
import { ApiService } from "./../../../services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class ProcessService {
  constructor(private appApi: ApiService) {}

  getProcess(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addProcess(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateProcess(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteProcess(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }
}
