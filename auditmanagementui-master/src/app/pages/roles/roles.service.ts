import { Injectable } from '@angular/core';
import { ApiService } from './../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class RolesService {
    
    constructor(private appApi:ApiService) {}

    getRoles(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addRoles(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateRoles(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteRoles(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}