import { Injectable } from '@angular/core';
import { ApiService } from '../../../services/api/api.service';
import { Observable } from 'rxjs';

@Injectable()
export class OverallAssessmentService {
    
    constructor(private appApi:ApiService) {}

    getOverallAssessment(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.getData(url, params)
    }

    addOverallAssessment(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.insertData(url, postData, params)
    }

    updateOverallAssessment(url:string, postData:any, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.updateData(url, postData, params)
    }

    deleteOverallAssessment(url:string, params:{[key:string]:string}={}):Observable<any>{
        return this.appApi.deleteData(url,params);
    }
    exportToExcel(url: string): Observable<Blob> {
        return this.appApi.downloadFile(url);
      }
      exportToPDF(url: string): Observable<Blob> {
        return this.appApi.downloadFile(url);
      }
      exportToPPT(url: string): Observable<Blob> {
        return this.appApi.downloadFile(url);
      }
      getData(url:string):Observable<any>{
        return this.appApi.getData(url)
    }
}