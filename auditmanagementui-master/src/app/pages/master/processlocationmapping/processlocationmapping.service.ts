import { Injectable } from '@angular/core';
import { ApiService } from './../../../services/api/api.service';
import { Observable } from 'rxjs';
import { IDropdownSettings } from 'ng-multiselect-dropdown';

@Injectable()
export class PlmapService {
    dropdownSettings:IDropdownSettings = {};
    constructor(private appApi:ApiService) {}

    getPlmap(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addPlmap(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updatePlmap(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deletePlmap(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }

   
}