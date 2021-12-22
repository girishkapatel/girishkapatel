import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "src/app/services/api/api.service";

@Injectable({
  providedIn: 'root'
})
export class CpiService {
  constructor(private appApi: ApiService) {}
  getCPI(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }

  addCPI(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.insertData(url, postData, params);
  }

  updateCPI(
    url: string,
    postData: any,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.updateData(url, postData, params);
  }

  deleteCPI(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.deleteData(url, params);
  }
}
