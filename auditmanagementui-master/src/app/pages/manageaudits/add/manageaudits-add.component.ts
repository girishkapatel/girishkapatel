import { Component, OnInit, AfterViewInit } from "@angular/core";
import { Router } from "@angular/router";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { BehaviorSubject } from "rxjs";
import * as $ from "jquery";

@Component({
  selector: "app-auditadd",
  templateUrl: "manageaudits-add.component.html",
})
export class ManageauditsAddComponent implements OnInit, AfterViewInit {
  constructor(private router: Router) {}

  auditColumns: tableColumn[] = [
    {
      title: "Internal Audit Areas",
      data: "area",
    },
    {
      title: "Risk Categorisation",
      data: "riskcat",
    },
    {
      title: "2020-21",
      data: "",
      render: (data) => {
        return '<input type="checkbox" data-on-color="success" data-on-text="YES" data-off-color="default" data-off-text="NO" class="make-switch" data-size="small">';
      },
    },
    {
      title: "2021-22",
      data: "",
      render: (data) => {
        return '<input type="checkbox" data-on-color="success" data-on-text="YES" data-off-color="default" data-off-text="NO" class="make-switch" data-size="small">';
      },
    },
    {
      title: "2022-23",
      data: "",
      render: (data) => {
        return '<input type="checkbox" data-on-color="success" data-on-text="YES" data-off-color="default" data-off-text="NO" class="make-switch" data-size="small">';
      },
    },
  ];

  auditMapData: tableData[] = [
    { area: "Accounts Payable", riskcat: "High" },
    { area: "Sales and Marketing", riskcat: "High" },
    { area: "Logistics", riskcat: "High" },
    { area: "Plant Maintenance (Asset Care Management)", riskcat: "High" },
    { area: "Salt packing Centre (SPC)", riskcat: "High" },
    { area: "Safety, Health & Environment", riskcat: "High" },
    { area: "Compliances", riskcat: "High" },
    { area: "Capex", riskcat: "High" },
    { area: "Plant Operations review", riskcat: "High" },
    { area: "Related Party Transactions ( Half Yearly)", riskcat: "High" },
    { area: "SAP Control Review(All Modules)", riskcat: "High" },
    { area: "IT Controls Review Genral Controls", riskcat: "High" },
    { area: "IT Cyber Security", riskcat: "High" },
    { area: "IT  Data privacy ", riskcat: "High" },
    { area: "Outsourced Salt operations ", riskcat: "Medium" },
    {
      area: "Disaster Recovery Planning (Potential Natural and Security Risk)",
      riskcat: "Medium",
    },
    { area: "Internal Financial Control", riskcat: "Medium" },
  ];

  ngOnInit() {}

  ngAfterViewInit() {
    // let switchLength = jQuery(".make-switch").length;
    // for(let i=0; i < switchLength; i++){
    //     try {
    //         jQuery(jQuery(".make-switch")[i]).bootstrapSwitch("toggleRadioState");
    //     }catch (error) {}
    // }
  }

  backToAuditForm() {
    this.router.navigate(["./pages/manageaudits"]);
  }
}
