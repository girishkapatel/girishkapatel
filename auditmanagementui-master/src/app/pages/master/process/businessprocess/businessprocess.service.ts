import { Injectable } from '@angular/core';
import { ApiService } from '../../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class BusinessProcessService {
    
    constructor(private appApi:ApiService) {}

    getBusinessProcess(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addBusinessProcess(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateBusinessProcess(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteBusinessProcess(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}