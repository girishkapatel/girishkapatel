import { Component, OnInit, AfterViewInit } from "@angular/core";
import { tableColumn, tableData } from "./../../common/table/table.model";
import * as $ from "jquery";
import * as moment from "moment";
import * as am4core from "@amcharts/amcharts4/core";
@Component({
  selector: "app-myaudits",
  templateUrl: "./myaudits.component.html",
  styleUrls: ["./myaudits.component.css"],
})
export class MyauditsComponent implements OnInit, AfterViewInit {
  constructor() {}

  /* Scheduling Table Config*/

  auditChartDataOverall: any[] = [
    {
      status: "Completed",
      count: 4,
      color: am4core.color("#ffe600"),
    },
    {
      status: "In Progress",
      count: 4,
      color: am4core.color("#2e2e38"),
    },
    {
      status: "Pending Initiation",
      count: 0,
      color: am4core.color("#c4c4cd"),
    },
  ];

  auditChartDataApprovals: any[] = [
    {
      status: "Due",
      count: 2,
      color: am4core.color("#ffe600"),
    },
    {
      status: "Pending Review",
      count: 6,
      color: am4core.color("#2e2e38"),
    },
    {
      status: "Completed",
      count: 1,
      color: am4core.color("#c4c4cd"),
    },
  ];

  tableColumnsAudit: tableColumn[] = [
    {
      title: "Audit",
      data: "audit",
    },
    {
      title: "Location",
      data: "location",
    },
    {
      title: "HOD",
      data: "HOD",
    },
    {
      title: "Eng. Co-ordinator",
      data: "coordinator",
    },
    {
      title: "Eng. Partner/ HOA",
      data: "partner",
    },
    {
      title: "Start Date",
      data: "start",
    },
    {
      title: "End Date",
      data: "end",
    },
    {
      title: "Status",
      data: "status",
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
          '" class="btn btn-sm btn-info editSchedule"><i class="fa fa-edit"></i></button>'
        );
      },
    },
  ];

  tableData_audit: tableData[] = [
    {
      id: "1",
      location: "India",
      audit: "Procure to Pay",
      HOD: "Dinesh Gupta",
      coordinator: "Vikas Gupta",
      partner: "Shailesh Jain",
      start: "04/01/2019",
      end: "5/18/2019",
      status: "Completed",
    },
    {
      id: "6",
      location: "India",
      audit: "Warranty Claims",
      HOD: "Sagar S",
      coordinator: "Virti S",
      partner: "Shailesh Jain",
      start: "12/1/2019",
      end: "",
      status: "In-progress",
    },
    {
      id: "14",
      location: "USA",
      audit: "Revenue recognition",
      HOD: "Rachael",
      coordinator: "Ron",
      partner: "Punit Mehta",
      start: "3/1/2020",
      end: "",
      status: "In-progress",
    },
    {
      id: "12",
      location: "USA",
      audit: "Business review",
      HOD: "Monica",
      coordinator: "Mike",
      partner: "Punit Mehta",
      start: "10/15/2019",
      end: "12/10/2019",
      status: "Completed",
    },
    {
      id: "13",
      location: "USA",
      audit: "Plant operations",
      HOD: "Ross",
      coordinator: "Sean",
      partner: "Punit Mehta",
      start: "9/1/2019",
      end: "11/12/2019",
      status: "Completed",
    },
    {
      id: "17",
      location: "Russia",
      audit: "Procure to Pay",
      HOD: "Phoebe",
      coordinator: "Krutsova",
      partner: "Anshul Shukla",
      start: "1/1/2020",
      end: "2/15/2020",
      status: "Completed",
    },
    {
      id: "15",
      location: "USA",
      audit: "Insurance",
      HOD: "Chandler",
      coordinator: "Emma",
      partner: "Punit Mehta",
      start: "3/15/2020",
      end: "",
      status: "In-progress",
    },
    {
      id: "18",
      location: "Russia",
      audit: "Cyber-security",
      HOD: "Barbara",
      coordinator: "Zespher",
      partner: "Anshul Shukla",
      start: "3/20/2020",
      end: "",
      status: "In-progress",
    },
    {
      id: "23",
      location: "Europe",
      audit: "Speciality chemicals business audit",
      HOD: "Kate",
      coordinator: "Leonard H",
      partner: "Raghuwar Singh",
      start: "3/20/2020",
      end: "",
      status: "In-progress",
    },
    {
      id: "25",
      location: "UAE",
      audit: "Warranty claims",
      HOD: "Lara",
      coordinator: "Animesh D",
      partner: "Tushar Shah",
      start: "2/1/2020",
      end: "",
      status: "In-progress",
    },
  ];

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

  /* Scheduling Table Config End*/
  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".viewStatus", () => {
        window["jQuery"]("#basic").modal("show");
      });
    });
  }

  ngAfterViewInit() {
    $(document).ready(() => {
      window["jQuery"]("#dashboard-report-range").daterangepicker(
        {
          ranges: {
            Today: [moment(), moment()],
            Yesterday: [
              moment().subtract("days", 1),
              moment().subtract("days", 1),
            ],
            "Last 7 Days": [moment().subtract("days", 6), moment()],
            "Last 30 Days": [moment().subtract("days", 29), moment()],
            "This Month": [moment().startOf("month"), moment().endOf("month")],
            "Last Month": [
              moment().subtract("month", 1).startOf("month"),
              moment().subtract("month", 1).endOf("month"),
            ],
          },
          locale: {
            format: "MM/DD/YYYY",
            separator: " - ",
            applyLabel: "Apply",
            cancelLabel: "Cancel",
            fromLabel: "From",
            toLabel: "To",
            customRangeLabel: "Custom",
            daysOfWeek: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"],
            monthNames: [
              "January",
              "February",
              "March",
              "April",
              "May",
              "June",
              "July",
              "August",
              "September",
              "October",
              "November",
              "December",
            ],
            firstDay: 1,
          },
          //"startDate": "11/08/2015",
          //"endDate": "11/14/2015",
          opens: false ? "right" : "left",
        },
        function (start, end, label) {
          if ($("#dashboard-report-range").attr("data-display-range") != "0") {
            $("#dashboard-report-range span").html(
              start.format("MMMM D, YYYY") + " - " + end.format("MMMM D, YYYY")
            );
          }
        }
      );
      if ($("#dashboard-report-range").attr("data-display-range") != "0") {
        $("#dashboard-report-range span").html(
          moment().subtract("days", 29).format("MMMM D, YYYY") +
            " - " +
            moment().format("MMMM D, YYYY")
        );
      }
      $("#dashboard-report-range").show();
    });
  }
}
