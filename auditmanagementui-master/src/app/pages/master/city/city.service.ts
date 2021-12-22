import { Injectable } from "@angular/core";
import { ApiService } from "./../../../services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class CityService {
  constructor(private appApi: ApiService) {}

  getCity(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addCity(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateCity(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteCity(
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
