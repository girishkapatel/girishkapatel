import { Injectable } from "@angular/core";
import {
  HttpClient,
  HttpClientModule,
  HttpParams,
  HttpHeaders,
} from "@angular/common/http";
import { map, catchError } from "rxjs/operators";
import { environment } from "../../../environments/environment";
import { UtilsService } from "../utils/utils.service";
import { MonoTypeOperatorFunction } from "rxjs";

@Injectable()
export class ApiService {
  pipe(arg0: MonoTypeOperatorFunction<unknown>) {
    throw new Error("Method not implemented.");
  }
  constructor(private http: HttpClient, private utils: UtilsService,private httpsapi:HttpClientModule) {}

  createUrl(requrl: string): string {
    let apiurl = "";
    if (environment.apiurl) {
      apiurl = environment.apiurl + "/" + requrl;
    }
    return apiurl;
  }

  getParams(params: { [key: string]: string }): HttpParams {
    let queryParams = new HttpParams();
    for (let paramkey in params) {
      queryParams = queryParams.append(paramkey, params[paramkey]);
    }
    return queryParams;
  }

  getData(requrl: string, params?: { [key: string]: string }) {
    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.get(apiurl, { params: this.getParams(params) });
    } else {
      return this.http.get(apiurl);
    }
  }

  downloadFile(requrl: string, params?: { [key: string]: string }) {
    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.get(apiurl, {
        responseType: "blob",
        params: this.getParams(params),
      });
    } else {
      return this.http.get(apiurl, { responseType: "blob" });
    }
  }
  postDownloadFile(requrl: string, params?: any) {
    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.get(apiurl, {
        params: {
          Overview: JSON.stringify(params) 
        },
        responseType: "blob",
      });
    }  
  }
  postDownloadFiledata(requrl: string, params?: { [key: string]: string }) {
    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.get(apiurl, {
      params: this.getParams(params) , 
        responseType: "blob",
      }).toPromise();
    }  
  }
  postDownloadFile1(requrl: string, params?: any) {
    let apiurl = this.createUrl(requrl);
    return  this.http.post(apiurl, JSON.stringify(params), { responseType: "blob" })
  }

  insertData(
    requrl: string,
    postData: any,
    params?: { [key: string]: string }
  ) {
    var userid = localStorage.getItem("userId");
    postData.CreatedBy = userid;

    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.post(apiurl, postData, {
        params: this.getParams(params),
      });
    } else {
      return this.http.post(apiurl, postData);
    }
  }
  insertDataUploading(requrl: string, postData: any) {
    var userid = localStorage.getItem("userId");
    let apiurl = this.createUrl(requrl);
    // postData.CreatedBy = userid;
    return this.http.post(apiurl, postData);
  }

  deleteData(requrl: string, params?: { [key: string]: string }) {
    var userid = localStorage.getItem("userId");
    let apiurl = this.createUrl(requrl + "/" + userid);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.delete(apiurl, { params: this.getParams(params) });
    } else {
      return this.http.delete(apiurl);
    }
  }
  deleteActivityLogData(requrl: string, params?: { [key: string]: string }) {
    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.delete(apiurl, { params: this.getParams(params) });
    } else {
      return this.http.delete(apiurl);
    }
  }

  updateData(
    requrl: string,
    postData: any,
    params?: { [key: string]: string }
  ) {
    var userid = localStorage.getItem("userId");
    postData.UpdatedBy = userid;
    let apiurl = this.createUrl(requrl);
    if (!this.utils.isEmptyObj(params)) {
      return this.http.put(apiurl, postData, {
        params: this.getParams(params),
      });
    } else {
      return this.http.put(apiurl, postData);
    }
  }

  getLocationLatLong(locationName) {
    let geoApi = `http://api.positionstack.com/v1/forward?access_key=${environment.mapkey}&query=${locationName}`;
    return this.http.get(geoApi);
  }
  insertWithoutUser(
    requrl: string,
    postData: any,
    params?: { [key: string]: string }
  ) {
      return this.http.post(requrl, postData);
  }
}
