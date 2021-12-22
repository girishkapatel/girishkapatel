import { Injectable } from '@angular/core';
import { ApiService } from './../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class ProcessUniverseService {
    
    constructor(private appApi:ApiService) {}

    getProcessUniverse(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addProcessUniverse(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateProcessUniverse(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteProcessUniverse(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}