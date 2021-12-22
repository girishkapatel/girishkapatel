import { Injectable } from '@angular/core';
import { ApiService } from './../../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class ProcessLevel1Service {
    
    constructor(private appApi:ApiService) {}

    getProcessLevel1(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addProcessLevel1(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateProcessLevel1(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteProcessLevel1(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}