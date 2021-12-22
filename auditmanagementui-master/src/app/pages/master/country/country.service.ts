import { Injectable } from "@angular/core";
import { ApiService } from "./../../../services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class CountryService {
  constructor(private appApi: ApiService) {}

  getCountry(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addCountry(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    var userid=localStorage.getItem("userId");
    postData.CreatedBy=userid;
    return this.appApi.insertData(url, postData, params);
  }

  updateCountry(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    var userid=localStorage.getItem("userId");
    postData.UpdatedBy=userid;
    return this.appApi.updateData(url, postData, params);
  }

  deleteCountry(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }

  exportToExcel(url: string): Observable<Blob> {
    return this.appApi.downloadFile(url);
  }

  importFromExcel(url: string, postData: any): Observable<any> {
    return this.appApi.insertData(url, postData);
  }
}
