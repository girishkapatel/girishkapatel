import { Injectable } from "@angular/core";
import { ApiService } from "../api/api.service";
import { UtilsService } from "./utils.service";

@Injectable()
export class CommonApiService {
  constructor(private appApi: ApiService,private util: UtilsService) {}
  getRoles() {
    return this.appApi.getData("api/role");
  }

  getDesignations() {
    return this.appApi.getData("api/designation");
  }

  getUsers() {
    return this.appApi.getData("api/user");
  }
  getStakeholders() {
    return this.appApi.getData("api/stakeholder/getallstockuser");
  }
  getUsersByAudit(auditId) {
    return this.appApi.getData("api/scopeandschedule/GetByAudit/" + auditId);
  }
  getSector() {
    return this.appApi.getData("api/sector");
  }

  getLocations() {
    return this.appApi.getData("api/location");
  }

  getLocationByProcess(processl1Id: string) {
    let apiUrl = "api/processlocationmapping/GetByProcessL1/" + processl1Id;
    //let apiUrl = 'api/location';
    return this.appApi.getData(apiUrl);
  }

  getLocationByAuditName(ProcessLocMappingId: string) {
    let apiUrl =
      "api/processlocationmapping/GetByAuditName/" + ProcessLocMappingId;
    //let apiUrl = 'api/location';
    return this.appApi.getData(apiUrl);
  }

  getLocationsByPLMapId(processLocationMappingId: string) {
    let apiUrl =
      "api/processlocationmapping/getlocationsbyplmapid/" +
      processLocationMappingId;

    return this.appApi.getData(apiUrl);
  }

  getCountry() {
    return this.appApi.getData("api/country");
  }

  getState(countryId?: string) {
    let apiUrl = "api/state";
    if (countryId) {
      apiUrl += "/GetByCountry/" + countryId;
    }
    return this.appApi.getData(apiUrl);
  }

  getCity(stateId?: string, countryId?: string) {
    let apiUrl = "api/cityortown";
    if (stateId) {
      apiUrl += "/GetByState/" + stateId;
    }
    return this.appApi.getData(apiUrl);
  }

  getCompany(cityId?: string) {
    let apiUrl = "api/company/";
    if (cityId) {
      apiUrl += "/GetByCity/" + cityId;
    }
    return this.appApi.getData(apiUrl);
  }

  getAllRisks() {
    return this.appApi.getData("api/risk");
  }

  getAllRACM() {
    return this.appApi.getData("api/racm");
  }

  getRACMById(id) {
    return this.appApi.getData("api/racm/GetById/" + id);
  }

  getBusinessCycle() {
    return this.appApi.getData("api/businesscycle");
  }

  getImpacts() {
    return this.appApi.getData("api/impactmaster");
  }

  getReportConsiderations() {
    return this.appApi.getData("api/reportconsideration");
  }

  getRootCause() {
    return this.appApi.getData("api/rootcause");
  }

  getRecommendation() {
    return this.appApi.getData("api/recommendation");
  }

  getAuditName() {
    return this.appApi.getData("api/ProcessLocationMapping");
  }

  getAllProcessLevel1() {
    return this.appApi.getData("api/processl1");
  }

  getAllProcessLevel2() {
    return this.appApi.getData("api/processl2");
  }

  getProcessLevel1(bcId?: string) {
    let apiUrl = "api/processl1";
    if (bcId) {
      apiUrl += "/GetByBusinessCycle/" + bcId;
    }
    return this.appApi.getData(apiUrl);
  }

  getProcessLevel2(processLevel1Id?: string, bcId?: string) {
    let apiUrl = "api/processl2";
    if (processLevel1Id) {
      apiUrl += "/GetByProcessL1/" + processLevel1Id;
    }
    return this.appApi.getData(apiUrl);
  }

  initialiseTable(tableId, tableData, tableColumns, tableScroll?) {
    //window["jQuery"]('#'+tableId).DataTable().clear().destroy();
    if (window["jQuery"].fn.DataTable.isDataTable("#" + tableId)) {
      window["jQuery"]("#" + tableId)
        .DataTable()
        .clear()
        .destroy();
      window["jQuery"]("#" + tableId).empty();
    }
    let tableConfig = {
      data: tableData,
      //"bSort": this.tableOrdering,
      bSort: true,
      columns: tableColumns,
      scrollX: tableScroll ? tableScroll : false,
      fnCreatedRow: function (nRow, data) {
        $(nRow).attr("id", data.id);
        $(nRow).data(data);
      },
      initComplete: function () {},
    };

    window["jQuery"]("#" + tableId).DataTable(tableConfig);
  }

  initialiseTableActionTable(tableId, tableData, tableColumns, tableScroll?) {
    //window["jQuery"]('#'+tableId).DataTable().clear().destroy();
    if (window["jQuery"].fn.DataTable.isDataTable("#" + tableId)) {
      window["jQuery"]("#" + tableId)
        .DataTable()
        .clear()
        .destroy();
      // window["jQuery"]("#" + tableId).empty();
    }
    let tableConfig = {
      data: tableData,
      //"bSort": this.tableOrdering,
      columnDefs: [
        { orderable: true, className: 'reorder', targets:12 },
        { orderable: false, targets: '_all' }
    ],
      columns: tableColumns, 
      scrollX: tableScroll ? tableScroll : false,
      fnCreatedRow: function (nRow, data) {
        $(nRow).attr("id", data.id);
        data.auditName =
          data.audit && data.audit.audit.processLocationMapping
            ? data.audit.audit.processLocationMapping.auditName
            : data.processLocationMapping?data.processLocationMapping.auditName:"";  
        data.locationName=  data.audit && data.audit.audit.location.locationDescription
          ? data.audit.audit.location.locationDescription
          :(data.location?data.location.locationDescription:null); 
        $(nRow).data(data);
      },
      initComplete: function ()
       {
        this.api()
          .columns()
          .every(function () {
            var column = this;
            if (column[0][0] == 0 || column[0][0] == 10 || column[0][0] == 12 || column[0][0] == 13) {
              return;
            }
            var select = $('<select name="drpFilter" style="width: 18px;margin-left: 15px;border: none;"><option value="">-- select --</option></select>')
            .appendTo($(column.header()))
              .on("change", function () {
                var val = window["jQuery"].fn.dataTable.util.escapeRegex(
                  $(this).val()
                );
                column.search(val ? "^" + val + "$" : "", true, false).draw();
              });
            column
              .data()
              .unique()
              .sort()
              .each(function (d, j) {
                if(d!=null)
                select.append('<option value="' + d + '">' + d + "</option>");
              });
          });
      },
    };
    window["jQuery"]("#" + tableId).DataTable(tableConfig);
  }
  initialiseTableTestingControl(tableId, tableData, tableColumns, tableScroll?) {
    if (window["jQuery"].fn.DataTable.isDataTable("#" + tableId)) {
      window["jQuery"]("#" + tableId)
        .DataTable()
        .clear()
        .destroy();
    }
    let tableConfig = {
      data: tableData,
      bSort: false,
      columns: tableColumns, 
      scrollX: tableScroll ? tableScroll : false,
      fnCreatedRow: function (nRow, data) {
        $(nRow).attr("id", data.id);
        data.status =data.status?data.status.toUpperCase():"INP";
        $(nRow).data(data);
      },
      initComplete: function ()
       {
        this.api()
          .columns()
          .every(function () {
            var column = this;
            if (column[0][0] == 0 || column[0][0] == 11 || column[0][0] == 12) {
              return;
            }
            var select = $('<select name="drpFilter" style="width: 18px;margin-left: 15px;border: none;"><option value="">-- select --</option></select>')
            .appendTo($(column.header()))
              .on("change", function () {
                var val = window["jQuery"].fn.dataTable.util.escapeRegex(
                  $(this).val()
                );
                column.search(val ? "^" + val + "$" : "", true, false).draw();
              });
            column
              .data()
              .unique()
              .sort()
              .each(function (d, j) {
                if(d!=null)
                select.append('<option value="' + d + '">' + d + "</option>");
              });
          });
      },
    };
    window["jQuery"]("#" + tableId).DataTable(tableConfig);
  }

  getAllRiskTypes() {
    return this.appApi.getData("api/risktype");
  }

  getAllRootCause() {
    return this.appApi.getData("api/rootcause");
  }

  getQuickView() {
    return this.appApi.getData("api/ActionPlanning/GetQuickView");
  }

  getObservationGradings() {
    return this.appApi.getData("api/observationgrading");
  }
  getAuditNamebyAudit() {
    return this.appApi.getData("api/Audit");
  }
  getRiskData(id) {
    return this.appApi.getData("api/racmauditprocedure/getRiskData/" + id);
  }
  getControlData(id) {
    return this.appApi.getData("api/racmauditprocedure/getControlData/" + id);
  }
}
