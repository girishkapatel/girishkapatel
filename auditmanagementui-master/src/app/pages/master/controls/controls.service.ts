import { Injectable } from '@angular/core';
import { ApiService } from './../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class ControlsService {
    
    constructor(private appApi:ApiService) {}

    getControls(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addControls(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateControls(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteControls(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}