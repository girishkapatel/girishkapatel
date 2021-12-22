import { Injectable } from "@angular/core";
import { ApiService } from "src/app/services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class ManageAuditsService {
  constructor(private appApi: ApiService) {}

  getAudits(
    url: string,
    params: { [key: string]: string } = {}
  ): Observable<any> {
    return this.appApi.getData(url, params);
  }
}
