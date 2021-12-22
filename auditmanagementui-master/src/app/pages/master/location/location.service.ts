import { Injectable } from '@angular/core';
import { ApiService } from './../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class LocationService {
    
    constructor(private appApi:ApiService) {}

    getLocation(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addLocation(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateLocation(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteLocation(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}