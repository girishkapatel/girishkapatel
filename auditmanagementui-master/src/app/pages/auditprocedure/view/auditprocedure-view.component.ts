import { Component, OnInit } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { Router } from "@angular/router";
import * as $ from "jquery";

@Component({
  selector: "selector",
  templateUrl: "auditprocedure-view.component.html",
})
export class AuditProcedureViewComponent implements OnInit {
  constructor(private router: Router) {}

  tableColumnsAP: tableColumn[] = [
    {
      title: "Procedure ID",
      data: "A",
    },
    {
      title: "Procedure Title",
      data: "B",
    },
    {
      title: "Control ID",
      data: "C",
    },
    {
      title: "Control Title",
      data: "D",
    },
    {
      title: "Control Owner",
      data: "E",
    },
    {
      title: "Performed By",
      data: "F",
    },
    {
      title: "Action",
      data: "id",
      render: (data, type, row, meta) => {
        let aHtml =
          '<button type="button" data-id="' +
          data +
          '" id="' +
          data +
          '" class="btn btn-sm btn-info editAP"><i class="fa fa-edit"></i></button>';

        if (row.status === "Inactive") {
          aHtml +=
            '<button type="button" data-id="' +
            data +
            '" class="btn btn-sm btn-primary initiateActivity"><i class="fa fa-cog"></i></button>';
        }

        return aHtml;
      },
    },
  ];

  tableData_ap: tableData[] = [
    {
      id: "1",
      A: "PTP.P.01",
      B: "Supplier master creation process",
      C: "PTP.MM.C.1",
      D: "Authorised and accurate creation/modification of vendor master data",
      E: "Mr. Sambhaji Patil",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "2",
      A: "PTP.P.01",
      B: "Supplier master creation process",
      C: "PTP.MM.C.1",
      D: "Authorised and accurate creation/modification of vendor master data",
      E: "Mr. Sambhaji Patil",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "3",
      A: "PTP.P.01",
      B: "Supplier master creation process",
      C: "PTP.MM.C.1",
      D: "Authorised and accurate creation/modification of vendor master data",
      E: "Mr. Sambhaji Patil",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "4",
      A: "PTP.P.02",
      B: "Annual review of access rights for supplier master",
      C: "PTP.MM.C.2",
      D: "Periodic review of supplier master access rights",
      E: "Mr. Prasad Singh",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "5",
      A: "PTP.P.03",
      B: "Monthly review of supplier master",
      C: "PTP.MM.C.3",
      D: "Verification of master data",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "6",
      A: "PTP.P.04",
      B: "Supplier master modification process",
      C: "PTP.MM.C.4",
      D: "Periodic review of modification in vendor master",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "7",
      A: "PTP.P.05",
      B: "Identification of duplicate vendor codes",
      C: "PTP.MM.C.5",
      D: "Review of duplicate supplier codes",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "8",
      A: "PTP.P.06",
      B: "Identification of redundant vendor codes",
      C: "PTP.MM.C.6",
      D: "Review of redundant supplier codes",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "9",
      A: "PTP.P.07",
      B: "Creation/modification in item master",
      C: "PTP.MM.C.7",
      D: "Creation/modification of prices in item master",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "10",
      A: "PTP.P.07",
      B: "Creation/modification in item master",
      C: "PTP.MM.C.7",
      D: "Creation/modification of prices in item master",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "11",
      A: "PTP.P.07",
      B: "Creation/modification in item master",
      C: "PTP.MM.C.7",
      D: "Creation/modification of prices in item master",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "12",
      A: "PTP.P.08",
      B: "Review of prices of alloys components ",
      C: "PTP.MM.C.8",
      D: "Creation/modification of prices in item master for alloys components",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "13",
      A: "PTP.P.08",
      B: "Review of prices of alloys components ",
      C: "PTP.MM.C.8",
      D: "Creation/modification of prices in item master for alloys components",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "14",
      A: "PTP.P.08",
      B: "Review of prices of alloys components ",
      C: "PTP.MM.C.8",
      D: "Creation/modification of prices in item master for alloys components",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "15",
      A: "PTP.P.08",
      B: "Annual review of access rights for PO creation and modification",
      C: "PTP.PM.C.1",
      D: "Periodic review of access rights for po creation and amendment",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "16",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "17",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "18",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "19",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "20",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "21",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "22",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "23",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "24",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "25",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "26",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "27",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "28",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "29",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "30",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "31",
      A: "PTP.P.09",
      B: "Creation /modification of purchase order",
      C: "PTP.PM.C.2",
      D: "Authorised and accurate creation/modification of purchase order (PO)",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "32",
      A: "PTP.P.10",
      B: "System restrict editing of auto populated field",
      C: "PTP.PM.C.3",
      D: "Restriction of changes to auto-populated fields within PO / agreements",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "33",
      A: "PTP.P.11",
      B: "Review of open commitment",
      C: "PTP.PM.C.4",
      D: "Review of open POs and closure",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "34",
      A: "PTP.P.11",
      B: "Review of open commitment",
      C: "PTP.PM.C.4",
      D: "Review of open POs and closure",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "35",
      A: "PTP.P.11",
      B: "Review of open commitment",
      C: "PTP.PM.C.4",
      D: "Review of open POs and closure",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "36",
      A: "PTP.P.11",
      B: "Review of open commitment",
      C: "PTP.PM.C.4",
      D: "Review of open POs and closure",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "37",
      A: "PTP.P.12",
      B: "Review of share of business",
      C: "PTP.PM.C.5",
      D: "Review and approval of share of business",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "38",
      A: "PTP.P.12",
      B: "Review of share of business",
      C: "PTP.PM.C.5",
      D: "Review and approval of share of business",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
    {
      id: "39",
      A: "PTP.P.12",
      B: "Review of share of business",
      C: "PTP.PM.C.5",
      D: "Review and approval of share of business",
      E: "Mr. Abhishek Sawant",
      F: "Ms. Ashwini Marathe",
    },
  ];

  tableFilters = new BehaviorSubject({});
  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".editAuditProcedure", (e) => {
        e.preventDefault();
        let schId = $(e.currentTarget).attr("id");
        this.editAuditProcedure();
      });
    });
  }

  editAuditProcedure() {
    this.router.navigate(["./pages/auditprocedure/edit"]);
  }

  addAuditProcedure() {
    this.router.navigate(["./pages/auditprocedure/add"]);
  }
}
