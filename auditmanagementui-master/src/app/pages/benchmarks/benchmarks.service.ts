import { Injectable } from '@angular/core';
import { ApiService } from './../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class BenchmarksService {
    
    constructor(private appApi:ApiService) {}

    getBenchmarks(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addBenchmarks(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateBenchmarks(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteBenchmarks(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
}