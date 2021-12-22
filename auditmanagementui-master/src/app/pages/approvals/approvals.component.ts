import { Component, OnInit } from "@angular/core";
import { tableColumn, tableData } from "./../../common/table/table.model";
import * as $ from "jquery";
@Component({
  selector: "app-approvals",
  templateUrl: "./approvals.component.html",
  styleUrls: ["./approvals.component.css"],
})
export class ApprovalsComponent implements OnInit {
  constructor() {}

  tableColumnsApproval: tableColumn[] = [
    {
      title: "Location",
      data: "A",
    },
    {
      title: "Audit",
      data: "B",
    },
    {
      title: "Activity Type",
      data: "C",
    },
    {
      title: "Detail",
      data: "D",
    },
    {
      title: "	Responsibilty",
      data: "E",
    },
    {
      title: "	Status",
      data: "F",
    },
    {
      title: "	Update",
      data: "id",
      render: (data, type, row, meta) => {
        return (
          '<button type="button" data-id="' +
          data +
          '" id="' +
          data +
          '" class="btn btn-sm btn-primary viewStatus"><i class="fa fa-gear"></i></button>'
        );
      },
    },
    {
      title: "	Comment",
      data: "",
      render: function () {
        return "";
      },
    },
  ];

  tableData_approval: tableData[] = [
    {
      id: "1",
      A: "Russia",
      B: "Cyber-security",
      C: "TOR",
      D: "Approval pending with Engagement Partner / HOA",
      E: "Anshul Shukla",
      F: "Approval due today",
    },
    {
      id: "2",
      A: "India",
      B: "Direct Taxation",
      C: "Work Programme",
      D: "Revision pending with audit performer",
      E: "Shailesh Jain",
      F: "Pending review",
    },
    {
      id: "3",
      A: "India",
      B: "Financial Closure Process",
      C: "Testing of controls",
      D: "Revision pending with audit performer",
      E: "Shailesh Jain",
      F: "Pending review",
    },
    {
      id: "4",
      A: "India",
      B: "GST",
      C: "Discussion Notes",
      D: "Approval pending with Engagement Partner / HOA",
      E: "Shailesh Jain",
      F: "Approval due today",
    },
    {
      id: "5",
      A: "USA",
      B: "Insurance",
      C: "Testing of controls",
      D: "Revision pending with audit performer",
      E: "Punit Mehta",
      F: "Pending review",
    },
    {
      id: "6",
      A: "India",
      B: "Procure to Pay",
      C: "Final Report",
      D: "Approval proceeds",
      E: "Shailesh Jain",
      F: "Completed",
    },
    {
      id: "7",
      A: "USA",
      B: "Revenue recognition",
      C: "TOR",
      D: "Revision pending with audit performer",
      E: "Punit Mehta",
      F: "Pending review",
    },
    {
      id: "8",
      A: "Europe",
      B: "Speciality chemicals business audit",
      C: "Work Programme",
      D: "Revision pending with audit performer",
      E: "Raghuwar Singh",
      F: "Pending review",
    },
    {
      id: "9",
      A: "UAE",
      B: "Warranty claims",
      C: "Testing of controls",
      D: "Revision pending with audit performer",
      E: "Tushar Shah",
      F: "Pending review",
    },
  ];

  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".viewStatus", () => {
        window["jQuery"]("#basic").modal("show");
      });
    });
  }
}
