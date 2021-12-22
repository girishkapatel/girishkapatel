import { Component, OnInit, ViewChild } from "@angular/core";
import { SectorService } from "../process/sector/sector.service";
import { Router } from "@angular/router";
import { UtilsService } from "src/app/services/utils/utils.service";
import { ToastrService } from "ngx-toastr";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-process",
  templateUrl: "./process.component.html",
  styleUrls: ["./process.component.css"],
  providers: [SectorService],
})
export class ProcessComponent implements OnInit {
  constructor(
    private sector: SectorService,
    private router: Router,
    private utils: UtilsService,
    private notifyService: ToastrService,
    private spinner: NgxSpinnerService
  ) {}

  accessRights: any = {};
  isStackHolder: boolean = false;
  @ViewChild("fileInput", { static: false }) fileInput;

  exportProcessMaster() {
    this.spinner.show();
    this.sector.exportToExcel("api/sector/downloadexcel").subscribe(
      (blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "ProcessMaster.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }

  sampleexportProcessMaster() {
    this.spinner.show();
    this.sector.exportToExcel("api/sector/sampledownloadexcel").subscribe(
      (blob) => {
        // const blobUrl = URL.createObjectURL(blob);
        // window.open(blobUrl);

        const objblob: any = new Blob([blob], {
          type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });

        let link = document.createElement("a");

        if (link.download !== undefined) {
          let url = URL.createObjectURL(blob);
          link.setAttribute("href", url);
          link.setAttribute("download", "SampleProcessMaster.xlsx");
          document.body.appendChild(link);
          link.click();
          document.body.removeChild(link);
        }
        this.spinner.hide();
      },
      (error) => {
        this.spinner.hide();
        console.log(error);
      }
    );
  }
  importExcel() {
    this.spinner.show();
    let formData = new FormData();
    formData.append("upload", this.fileInput.nativeElement.files[0]);
    this.spinner.show();
    this.sector.importFromExcel("api/sector/importexcel", formData).subscribe(
      (response) => {
        this.spinner.hide();
        //sector
        var sectionTotalRow = response["sectionTotalRow"];
        var sectionExcptionCount = response["sectionExcptionCount"];
        var sectionExcptionRowNumber = response["sectionExcptionRowNumber"];
        sectionExcptionRowNumber = sectionExcptionRowNumber.replace(/,\s*$/,"");
        var sectorSuccessCount = sectionTotalRow - sectionExcptionCount;

        //business cycle
        var businessCyclTotalRow = response["businessCyclTotalRow"];
        var businessCycleExcptionCount = response["businessCycleExcptionCount"];
        var businessCyclExcptionRowNumber = response["businessCyclExcptionRowNumber"];
        businessCyclExcptionRowNumber = businessCyclExcptionRowNumber.replace(/,\s*$/, "");
        var businessCyclSuccessCount = businessCyclTotalRow - businessCycleExcptionCount;

        //process l1
        var processL1TotalRow = response["processL1TotalRow"];
        var processL1ExcptionCount = response["processL1ExcptionCount"];
        var processL1ExcptionRowNumber = response["processL1ExcptionRowNumber"];
        processL1ExcptionRowNumber = processL1ExcptionRowNumber.replace(/,\s*$/, "");
        var processL1SuccessCount = processL1TotalRow - processL1ExcptionCount;

        //process l2
        var processL2TotalRow = response["processL2TotalRow"];
        var processL2ExcptionCount = response["processL2ExcptionCount"];
        var processL2ExcptionRowNumber = response["processL2ExcptionRowNumber"];
        processL2ExcptionRowNumber = processL2ExcptionRowNumber.replace(/,\s*$/, "");
        var processL2SuccessCount = processL2TotalRow - processL2ExcptionCount;

        var msg =
          "Sector Total Rows : " +
          sectionTotalRow +
          "<br>Sector Success Count : " +
          sectorSuccessCount +
          " <br>Sector Exception Count : " +
          sectionExcptionCount +
          "<br>Sector Exception RowNumber : " +
          sectionExcptionRowNumber +

          "<br><br>Business Cycle Total Rows : " +
          businessCyclTotalRow +
          "<br>Business Cycle Success Count : " +
          businessCyclSuccessCount +
          " <br>Business Cycle Exception Count : " +
          businessCycleExcptionCount +
          "<br>Business Cycle Exception RowNumber : " +
          businessCyclExcptionRowNumber+
          
          "<br><br>Process L1 Total Rows : " +
          processL1TotalRow+
          "<br>Process L1 Success Count : " +
          processL1SuccessCount +
          " <br>Process L1 Exception Count : " +
          processL1ExcptionCount +
          "<br>Process L1 Exception RowNumber : " +
          processL1ExcptionRowNumber+

          "<br><br>Process L2 Total Rows : " +
          processL2SuccessCount+
          "<br>Process L2 Success Count : " +
          processL2SuccessCount +
          " <br>Process L2 Exception Count : " +
          processL2ExcptionCount +
          "<br>Process L2 Exception RowNumber : " +
          processL2ExcptionRowNumber
          ;

        this.notifyService.success(msg, "", {
          enableHtml: true,
        });
        // this.notifyService.success("Process Master Imported Successfully.");
        this.router.navigate(["./pages/master/processmaster"]);
      },
      (error) => {
        this.spinner.hide();
        this.notifyService.error(error.error);
      }
    );
  }

  checkAccess() {
    this.accessRights = this.utils.getAccessOnLevel1("master", "processmaster");
  }

  ngOnInit() {
    this.checkAccess();
    this.isStackHolder =
      localStorage.getItem("stackholder") == "true" ? true : false;
  }
}
