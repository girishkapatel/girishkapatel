import { Component, OnInit, ViewChild } from "@angular/core";
import { tableColumn, tableData } from "./../../../common/table/table.model";
import { ControlsService } from "./controls.service";
import { BehaviorSubject } from "rxjs";
import { NgForm } from "@angular/forms";
import * as $ from "jquery";

@Component({
  selector: "app-controls",
  templateUrl: "./controls.component.html",
  styleUrls: ["./controls.component.css"],
  providers: [ControlsService],
})
export class ControlsComponent implements OnInit {
  constructor(private controls: ControlsService) {}

  @ViewChild("controlsForm", { static: false }) controlsForm: NgForm;

  tableId: string = "controls_table";

  tableGetApi: string = "posts";

  tableColumns: tableColumn[] = [
    {
      title: "Control Title",
      data: "name",
    },
    {
      title: "Control ID",
      data: "cid",
    },
    {
      title: "Control Description",
      data: "desc",
      render: function (desc) {
        return '<div style="height:100px;>' + desc + "</div>";
      },
    },
    {
      title: "Control Nature",
      data: "nature",
    },
    {
      title: "Control Frequency",
      data: "freq",
    },
    // ,{
    //   title:'Control Owner',
    //   data:'owner'
    // }
    {
      title: "Control Type",
      data: "type",
    },
    {
      title: "Action",
      data: "id",
      orderable: false,
      render: (data) => {
        return (
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-info editControls"><i class="fa fa-edit"></i></button>' +
          '<button type="button" id="' +
          data +
          '" class="btn btn-sm btn-primary" title="Map to Audit"><i class="fa fa-link"></i></button></p>'
        );
        //return '<a id="'+ data+'" href="javascript:void(0);" class="btn btn-sm red editControls"><i class="fa fa-edit"></i> Edit </a>';
      },
    },
  ];

  tableData: tableData[] = [
    {
      id: "1",
      cid: "PTP.C.01",
      name: "Authorised and accurate creation of supplier master data",
      desc: "Based on the vendor invoice (received from a supplier with no existing supplier code in the supplier master), a supplier code creation form(available on the intranet) is filled up by the Accountant, Third Parties Payables with all the available information. Supplier code creation form is approved by Procurement Head. In case of any missing information, the form is sent to the respective Purchaser/ Buyer for obtaining the necessary supplier details. Upon receiving the required details from the Purchaser/ Buyer via email, the new supplier code is created in SAP based on approved form. The filled supplier creation form kept for records purpose.",
      nature: "Manual",
      freq: "Event driven",
      owner: "Mr. Sambhaji More",
      type: "Financial Reporting",
    },
    {
      id: "2",
      cid: "PTP.C.02",
      name: "Segregation of duties for supplier master access",
      desc: "Access rights for creating/ updating the supplier master are centralised with the personnel of F&C, who are functionally independent from the procurement function i.e. they are not involved with the activities of buying and receiving.",
      nature: "Automated",
      freq: "Event driven",
      owner: "NA",
      type: "Financial Reporting",
    },
    {
      id: "3",
      cid: "PTP.C.03",
      name: "Verification of master data",
      desc: "The supplier codes are reviewed by Accountant, Cash Management on a sample basis by checking against the supplier code creation form or invoice.",
      nature: "IT Dependent",
      freq: "Monthly",
      owner: "Mr. Abhishek Sawant",
      type: "Financial Reporting",
    },
    {
      id: "4",
      cid: "PTP.C.04",
      name: "Periodic review of modification in vendor master",
      desc: "Log of changes made to bank details of the supplier codes are reviewed by Accountant, Cash Management to ensure that there are no unauthorised changes to bank details of the supplier  codes.",
      nature: "IT Dependent",
      freq: "Monthly",
      owner: "Mr. Abhishek Sawant",
      type: "Financial Reporting",
    },
    {
      id: "5",
      cid: "PTP.C.05",
      name: "Periodic review of supplier master access rights",
      desc: "An Annual review of access rights is performed by Head, F & C to ensure that access to create supplier codes in SAP is restricted to authorised personnel in F&C.",
      nature: "IT Dependent",
      freq: "Annaully",
      owner: "Mr. Prasad Singh",
      type: "Financial Reporting",
    },
    {
      id: "6",
      cid: "PTP.C.06",
      name: "Review of duplicate supplier codes",
      desc: "Supplier master is reviewed by the Accountant, Payables Third Parties to check for duplicate codes.",
      nature: "IT Dependent",
      freq: "Monthly",
      owner: "Mr. Abhishek Sawant",
      type: "Financial Reporting",
    },
    {
      id: "7",
      cid: "PTP.C.07",
      name: "Review of redundant supplier codes",
      desc: "Supplier master is reviewed by Accountant, Payables Third Parties to check for redundant codes with whom there have been no transactions in last two years.",
      nature: "IT Dependent",
      freq: "Monthly",
      owner: "Mr. Abhishek Sawant",
      type: "Financial Reporting",
    },
    {
      id: "8",
      cid: "PTP.C.08",
      name: "Review of GL distribution",
      desc: "The GL Distribution providing the link to the relevant General Ledger code is done in the SAP at the time of creation of the supplier code in the system.",
      nature: "Manual",
      freq: "Event driven",
      owner: "Mr. Sambhaji More",
      type: "Financial Reporting",
    },
  ];

  isEdit: boolean = false;

  tableFilters = new BehaviorSubject({});

  formVisible: boolean = false;

  handleFormView = {
    show: () => {
      this.formVisible = true;
    },
    hide: () => {
      this.tableFilters.next({});
      this.formVisible = false;
      this.isEdit = false;
      this.clearform();
    },
  };

  cancelAddEdit() {
    this.handleFormView.hide();
  }

  saveControls(e) {
    e.preventDefault();
    if (this.isEdit) {
      this.updateControls();
    } else {
      this.addNewControls();
    }
  }

  addNewControls() {
    let postData = this.controlsForm.form.value;

    this.controls.addControls("posts", postData).subscribe(
      (response) => {
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  updateControls() {
    let postData = {
      id: 1,
      title: "foo",
      body: "bar",
      userId: 1,
    };

    this.controls.updateControls("posts/1", postData).subscribe(
      (response) => {
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  addControls() {
    this.handleFormView.show();
  }

  editControls() {
    this.isEdit = true;
    this.handleFormView.show();
  }

  deleteControls() {
    this.controls.deleteControls("post/1").subscribe(
      (response) => {
        this.handleFormView.hide();
      },
      (error) => {
        console.log(error);
      }
    );
  }

  clearform() {}

  ngOnInit() {
    $(document).ready(() => {
      $("body").on("click", ".editControls", (e) => {
        let dataId = $(e.currentTarget).attr("id");
        this.editControls();
      });
    });
  }
}
