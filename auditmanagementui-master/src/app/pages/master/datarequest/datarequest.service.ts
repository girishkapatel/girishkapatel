import { Injectable } from '@angular/core';
import { ApiService } from './../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class DatarequestService {
    
    constructor(private appApi:ApiService) {}

    getDatarequest(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addDatarequest(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateDatarequest(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteDatarequest(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}