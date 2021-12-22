import { Injectable } from "@angular/core";
import { ApiService } from "./../../../../services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class SectorService {
  constructor(private appApi: ApiService) {}

  getSector(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addSector(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateSector(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteSector(
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
