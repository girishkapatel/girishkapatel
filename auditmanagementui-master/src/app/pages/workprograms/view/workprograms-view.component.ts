import { Component, OnInit } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { Router } from "@angular/router";
import * as $ from "jquery";

@Component({
  selector: "selector",
  templateUrl: "workprograms-view.component.html",
})
export class WorkprogramsViewComponent implements OnInit {
  constructor(private router: Router) {}

  tableColumnsWP: tableColumn[] = [
    {
      title: "Business Cycle",
      data: "A",
    },
    {
      title: "L1 Process",
      data: "B",
    },
    {
      title: "L2 Process",
      data: "C:",
    },
    {
      title: "Risk ID",
      data: "D",
    },
    {
      title: "Risk Title",
      data: "E",
    },
    {
      title: "Description",
      data: "F",
      render: function (desc) {
        return '<div style="height:100px; overflow-y:auto">' + desc + "</div>";
      },
    },
    {
      title: "Risk Rating",
      data: "G",
    },
    {
      title: "Action",
      data: "id",
      render: (data, type, row, meta) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" id="' +
          data +
          '" class="btn btn-sm btn-info editWP"><i class="fa fa-edit"></i></button>'
        );
      },
    },
  ];

  tableData_wp: tableData[] = [
    {
      id: "1",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.01",
      E: "Uncontrolled master data",
      F: "Unauthorised/incorrect creation/modification of vendor codes in Vendor Master may result in recording of fictitious/inaccurate transaction",
      G: "High",
    },
    {
      id: "2",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.01",
      E: "Uncontrolled master data",
      F: "Unauthorised/incorrect creation/modification of vendor codes in Vendor Master may result in recording of fictitious/inaccurate transaction",
      G: "High",
    },
    {
      id: "3",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.01",
      E: "Uncontrolled master data",
      F: "Unauthorised/incorrect creation/modification of vendor codes in Vendor Master may result in recording of fictitious/inaccurate transaction",
      G: "High",
    },
    {
      id: "4",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.01",
      E: "Uncontrolled master data",
      F: "Unauthorised/incorrect creation/modification of vendor codes in Vendor Master may result in recording of fictitious/inaccurate transaction",
      G: "High",
    },
    {
      id: "5",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.02",
      E: "Duplicate supplier code",
      F: "Duplicate/redundant Supplier codes (i.e. more than one code) created for same supplier",
      G: "High",
    },
    {
      id: "6",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.02",
      E: "Redundant supplier code",
      F: "Duplicate/redundant Supplier codes (i.e. more than one code) created for same supplier",
      G: "High",
    },
    {
      id: "7",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.03",
      E: "Incorrect details in item master",
      F: "Unauthorised/incorrect creation/modification of item master/price master may result in recording of inaccurate creation of purchase order",
      G: "High",
    },
    {
      id: "8",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Master Management",
      D: "PTP. R.03",
      E: "Incorrect details in item master",
      F: "Unauthorised/incorrect creation/modification of item master/price master may result in recording of inaccurate creation of purchase order",
      G: "High",
    },
    {
      id: "9",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Ordering",
      D: "PTP. R.04",
      E: "Unauthorised  and incorrect purchase order creation/amendment",
      F: "Purchase Order (PO) creation/amendments may not be authorized / correct resulting in recording of incorrect transactions",
      G: "High",
    },
    {
      id: "10",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Ordering",
      D: "PTP. R.04",
      E: "Unauthorised  and incorrect purchase order creation/amendment",
      F: "Purchase Order (PO) creation/amendments may not be authorized / correct resulting in recording of incorrect transactions",
      G: "High",
    },
    {
      id: "11",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Ordering",
      D: "PTP. R.04",
      E: "Unauthorised  and incorrect purchase order creation/amendment",
      F: "Purchase Order (PO) creation/amendments may not be authorized / correct resulting in recording of incorrect transactions",
      G: "High",
    },
    {
      id: "12",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Post PO Activities",
      D: "PTP. R.04",
      E: "Unauthorised  and incorrect purchase order creation/amendment",
      F: "Purchase Order (PO) creation/amendments may not be authorized / correct resulting in recording of incorrect transactions",
      G: "High",
    },
    {
      id: "13",
      A: "Procure to Pay",
      B: "Procurement",
      C: "Vendor Selection",
      D: "PTP. R.05",
      E: "Inaccurate allocation of procurement",
      F: "Inaccurate allocation of procurement to vendor may leads to excess cost of procurement",
      G: "High",
    },
    {
      id: "14",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "GRN (Goods Receipt Note) Material Receipt and Service Entry Sheet preparation ",
      D: "PTP. R.06",
      E: "Unauthorised  and incorrect material receipt/service entry posting",
      F: "Unauthorised access to Inventory receipt may resulting in incorrect inventory declaration in financial statements",
      G: "High",
    },
    {
      id: "15",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "GRN (Goods Receipt Note) Material Receipt and Service Entry Sheet preparation ",
      D: "PTP. R.07",
      E: "Inaccurate inventory receipt",
      F: "Inventory records updated without physically receiving material resulting in incorrect inventory declaration in financial statements",
      G: "High",
    },
    {
      id: "16",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "Invoice Processing",
      D: "PTP. R.08",
      E: "Inaccurate/unauthorised recording of vendor invoices",
      F: "Inaccurate/unauthorised recording of vendor invoices (rate/quantity) may result in inaccurate recording of vendor liability ",
      G: "High",
    },
    {
      id: "17",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "Invoice Processing",
      D: "PTP. R.09",
      E: "Service expenses are posted without service entry",
      F: "Service expenses recorded without receiving services may result in incorrect expense booking ",
      G: "High",
    },
    {
      id: "18",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "Invoice blocking",
      D: "PTP. R.10",
      E: "Non blocking of invoices leads to excess payment",
      F: "Non blocking invoices may leads to excess payment to vendor",
      G: "High",
    },
    {
      id: "19",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "Duplicate invoicing",
      D: "PTP. R.11",
      E: "Duplicate posting of invoices",
      F: "Duplicate invoice may be processed resulting in incorrect liability reporting",
      G: "High",
    },
    {
      id: "20",
      A: "Procure to Pay",
      B: "Accounts Payable",
      C: "Duplicate invoicing",
      D: "PTP. R.11",
      E: "Duplicate posting of invoices",
      F: "Duplicate invoice may be processed resulting in incorrect liability reporting",
      G: "High",
    },
  ];

  tableFilters = new BehaviorSubject({});
  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".editWP", (e) => {
        e.preventDefault();
        let schId = $(e.currentTarget).attr("id");
        this.editWorkprograms();
      });
    });
  }

  editWorkprograms() {
    this.router.navigate(["./pages/workprograms/edit"]);
  }

  addWorkprograms() {
    this.router.navigate(["./pages/workprograms/add"]);
  }
}
