import { Injectable } from '@angular/core';
import { ApiService } from '../services/api/api.service';
import { Observable } from 'rxjs'

@Injectable()
export class LoginService {
    
    constructor(private appApi:ApiService) { }

    login(apiurl:string, creds:{[key:string]:string}):Observable<any>{
        return this.appApi.insertData(apiurl, creds)
    }
    checkEmpDetails(apiurl:string, creds:{[key:string]:string}):Observable<any>{
        return this.appApi.insertWithoutUser(apiurl, creds)
    }
}