import { Injectable } from "@angular/core";
import { ApiService } from "../../services/api/api.service";
import { Observable } from "rxjs";

@Injectable()
export class TableService {
  constructor(private appApi: ApiService) {}

  getTableData(
    url: string,
    tableFilters: { [key: string]: string }
  ): Observable<any> {
    return this.appApi.getData(url, tableFilters);
  }
}
