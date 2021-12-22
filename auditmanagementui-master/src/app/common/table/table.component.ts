import {
  Component,
  OnInit,
  AfterViewInit,
  Input,
  OnDestroy,
  Output,
  EventEmitter,
} from "@angular/core";
import * as $ from "jquery";
import { tableData, tableColumn } from "./table.model";
import { TableService } from "./table.service";
import { Observable, Subscription } from "rxjs";
import { UtilsService } from "../../services/utils/utils.service";
import { NgxSpinnerService } from "ngx-spinner";
@Component({
  selector: "app-table",
  templateUrl: "./table.component.html",
  styleUrls: ["./table.component.css"],
  providers: [TableService, UtilsService],
})
export class TableComponent implements OnInit, AfterViewInit, OnDestroy {
  constructor(
    private tableApi: TableService,
    private utils: UtilsService,
    private spinner: NgxSpinnerService
  ) {}

  @Input() tableBehavior?: Observable<string>;
  @Input() tableId: string = "appTable";
  @Input() tableDownload: boolean = false;
  @Input() tableFileName: string = "diaFile";
  @Input() tableColumns: tableColumn[] = [];
  @Input("tableApi") tableUrl?: string = "";
  @Input() tableFilters?: Observable<{ [key: string]: string }>;
  @Input() tableScroll?: boolean = false;
  @Input() tableData?: tableData[] = [];
  @Input() tableOrdering?: boolean = true;
  @Output() initComplete: any = new EventEmitter<any>();

  tableFiltersData: { [key: string]: any } = {};

  tableSubscription: Subscription;
  tableFilterSubscription: Subscription;

  setupTable() {
    this.spinner.show();
    if (this.tableUrl && this.tableFiltersData.init) {
      this.tableApi
        .getTableData(this.tableUrl, this.tableFiltersData)
        .subscribe((posts) => {
          this.tableData = posts;
          window["jQuery"]("#" + this.tableId)
            .DataTable()
            .clear()
            .destroy();
          this.initialiseTable();
          this.spinner.hide();
        });
    } else {
      this.initialiseTable();
      this.spinner.hide();
    }
  }

  initialiseTable() {
    let buttonsArray = [];
    let exportFilename = this.tableFileName;
    if (this.tableDownload) {
      buttonsArray = [
        {
          extend: "excel",
          text: "Excel",
          titleAttr: "Excel",
          className: "sq-btn",
          exportOptions: {
            modifier: {
              // DataTables core
              order: "index", // 'current', 'applied', 'index',  'original'
              page: "all", // 'all',     'current'
              search: "none", // 'none',    'applied', 'removed'
            },
          },
          filename: function () {
            return exportFilename;
          },
        },
        {
          text: "PDF",
          extend: "pdfHtml5",
          orientation: "landscape", //portrait
          pageSize: "A4", //A3 , A5 , A6 , legal , letter
          exportOptions: {
            columns: ":visible",
            search: "applied",
            order: "applied",
          },
          customize: function (doc) {
            //Handles Title
            if (typeof exportFilename === "string" && exportFilename !== "") {
              doc.content[0].text = exportFilename;
              doc.styles.title.alignment = "left";
            } else {
              doc.content.splice(0, 1);
            }
          },
          filename: function () {
            return exportFilename;
          },
        },
      ];
    }

    let tableConfig = {
      data: this.tableData,
      bSort: this.tableOrdering,
      // bSort: false,
      columns: this.tableColumns,
      scrollX: this.tableScroll,
      fnCreatedRow: function (nRow, data) {
        $(nRow).attr("id", data.id);
        $(nRow).data(data);
      },
      dom: "Bfrtip",
      buttons: buttonsArray,
      initComplete: (settings, json) => {
        this.initComplete.emit({ settings, json });
      },
    };

    window["jQuery"]("#" + this.tableId).DataTable(tableConfig);
    this.spinner.hide()
  }

  ngOnInit() {
    if (this.tableFilters) {
      this.tableFilterSubscription = this.tableFilters.subscribe(
        (filters) => {
          if (!this.utils.isEmptyObj(filters)) {
            this.tableFiltersData = filters;
          }
          if (typeof this.tableFiltersData.init === "undefined") {
            this.tableFiltersData.init = true;
          }
          this.setupTable();
        },
        (error) => {
          console.log(error);
        }
      );
    }

    if (this.tableBehavior) {
      this.tableSubscription = this.tableBehavior.subscribe((item) => {
        if (item === "load") {
          this.setupTable();
        }
      });
    }
  }

  ngAfterViewInit(): void {
    this.initialiseTable();
  }

  ngOnDestroy() {
    if (this.tableFilterSubscription) {
      this.tableFilterSubscription.unsubscribe();
    }

    if (this.tableSubscription) {
      this.tableSubscription.unsubscribe();
    }
  }
}
