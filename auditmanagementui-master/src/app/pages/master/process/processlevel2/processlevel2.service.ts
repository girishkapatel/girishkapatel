import { Injectable } from '@angular/core';
import { ApiService } from './../../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class PL2Service {
    
    constructor(private appApi:ApiService) {}

    getProcessLevel2(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addProcessLevel2(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateProcessLevel2(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteProcessLevel2(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}