import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";
@Component({
  selector: "selector",
  templateUrl: "auditprocedure-edit.component.html",
})
export class AuditProcedureEditComponent implements OnInit {
  constructor(private router: Router) {}

  formVisible: boolean = false;

  tableColumns: tableColumn[] = [
    {
      title: "Control ID",
      data: "controlid",
    },
    {
      title: "Control Title",
      data: "control",
    },
    {
      title: "Control Owner",
      data: "owner",
    },
    {
      title: "Audit Step",
      data: "astep",
    },
    {
      title: "Data Required",
      data: "dreq",
    },
    {
      title: "Data Period",
      data: "date",
    },
    {
      title: "Analytics Reference",
      data: "ref",
    },
    {
      title: "Data Source",
      data: "source",
    },
    {
      title: "Days Required",
      data: "days",
    },
    {
      title: "Performed By",
      data: "resource",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-primary viewAuditStep"><i class="fa fa-eye"></i></button>'
          // '<button type="button" id="' +
          // data +
          // '" class="btn btn-sm btn-danger deleteAuditStep"><i class="fa fa-trash"></i></button>'
        );
      },
    },
  ];

  tableData: tableData[] = [
    {
      id: "1",
      controlid: "PTP.C.01",
      control: "Authorised and accurate creation of supplier master data",
      owner: "Mr. Sambhaji Patil",
      astep:
        "Verify whether the approval is obtained from the Procurement head before creation of any vendor in the master",
      dreq: "Signed copy of supplier code creation form and vendor master",
      date: "April 2019 to March 2020",
      ref: "NA",
      source: "Manual",
      days: "0.5",
      resource: "Ms. Ashwini Marathe",
    },
    {
      id: "2",
      controlid: "PTP.C.01",
      control: "Authorised and accurate creation of supplier master data",
      owner: "Mr. Sambhaji Patil",
      astep:
        "Verify whether all the supporting documents are obtained from the vendor",
      dreq: "Signed copy of supplier code creation form along with suporting documents",
      date: "April 2019 to March 2020",
      ref: "NA",
      source: "Manual",
      days: "0.5",
      resource: "Ms. Ashwini Marathe",
    },
    {
      id: "3",
      controlid: "PTP.C.01",
      control: "Authorised and accurate creation of supplier master data",
      owner: "Mr. Sambhaji Patil",
      astep:
        "Verify whether all the relevant details are filled in the registration form",
      dreq: "Signed copy of supplier code creation form along with suporting documents",
      date: "April 2019 to March 2020",
      ref: "NA",
      source: "Manual",
      days: "0.5",
      resource: "Ms. Ashwini Marathe",
    },
  ];

  // tableData:tableData[] = [{id:'1', controlid:'PTP.C.01',control:'Authorised and accurate creation of supplier master data',owner:'Mr. Sambhaji Patil',astep:'Verify whether the approval is obtained from the Procurement head before creation of any vendor in the master',dreq:'Signed copy of supplier code creation form and vendor master',date:'April 2019 to March 2020',ref:'NA',source:'Manual',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'2', controlid:'PTP.C.01',control:'Authorised and accurate creation of supplier master data',owner:'Mr. Sambhaji Patil',astep:'Verify whether all the supporting documents are obtained from the vendor',dreq:'Signed copy of supplier code creation form along with suporting documents',date:'April 2019 to March 2020',ref:'NA',source:'Manual',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'3', controlid:'PTP.C.01',control:'Authorised and accurate creation of supplier master data',owner:'Mr. Sambhaji Patil',astep:'Verify whether all the relevant details are filled in the registration form',dreq:'Signed copy of supplier code creation form along with suporting documents',date:'April 2019 to March 2020',ref:'NA',source:'Manual',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'4', controlid:'PTP.C.02',control:'Segregation of duties for supplier master access',owner:'Mr. Sambhaji Patil',astep:'Review the conflicting access rights given to employees.',dreq:'Access rights, employees role and log report',date:'April 2019 to March 2020',ref:'NA',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'5', controlid:'PTP.C.03',control:'Verification of master data',owner:'Mr. Abhishek Sawant',astep:'Verify vendor master details with registration form and supporting documents',dreq:'Vendor Master and Signed copy of supplier code creation form ',date:'April 2019 to March 2020',ref:'NA',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'6', controlid:'PTP.C.04',control:'Periodic review of modification in vendor master',owner:'Mr. Abhishek Sawant',astep:'Verify the log report for changes made to vendor master with supporting documents',dreq:'Log report for changes in master data along with supporting documents',date:'April 2019 to March 2020',ref:'NA',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'7', controlid:'PTP.C.05',control:'Periodic review of supplier master access rights',owner:'Mr. Prasad Singh',astep:'Review the conflicting access rights given to employees.',dreq:'Access rights and log report',date:'April 2019 to March 2020',ref:'NA',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'8', controlid:'PTP.C.06',control:'Review of duplicate supplier codes',owner:'Mr. Abhishek Sawant',astep:'Verify the duplicates code on the basis of PAN, aadhar, company identification number, GSTIN,etc',dreq:'Vendor master',date:'April 2019 to March 2020',ref:'DA.01',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'9', controlid:'PTP.C.07',control:'Review of redundant supplier codes',owner:'Mr. Abhishek Sawant',astep:'Verify redundant supplier codes',dreq:'Vendor Master and vendor ledger',date:'April 2019 to March 2020',ref:'DA.02',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'},
  // {id:'10', controlid:'PTP.C.08',control:'Review of GL distribution',owner:'Mr. Sambhaji More',astep:'Verify accurate GL distributed at the time of creation of supplier code',dreq:'Vendor master',date:'April 2019 to March 2020',ref:'NA',source:'System',days:'0.5',resource:'Ms. Ashwini Marathe'}]
  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".viewAuditStep", () => {
        this.addControl();
      });
    });
  }

  addControl() {
    this.formVisible = true;
  }

  cancelControlAddEdit() {
    this.formVisible = false;
  }

  backToAuditProcedureView() {
    this.router.navigate(["./pages/auditprocedure"]);
  }
}
