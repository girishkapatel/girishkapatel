import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { LibraryComponent } from "./library.component";
import { LibraryRoutingModule } from "./library-routing.module";
import { RisksComponent } from "../master/risks/risks.component";
import { ControlsComponent } from "../master/controls/controls.component";
import { TableModule } from "../../common/table/table.module";
import { DatarequestComponent } from "../master/datarequest/datarequest.component";
import { ProcessUniverseComponent } from "../master/processuniverse/processuniverse.component";
// import { BenchmarksComponent } from '../master/benchmarks/benchmarks.component';
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { CKEditorModule } from "@ckeditor/ckeditor5-angular";

@NgModule({
  imports: [
    CommonModule,
    LibraryRoutingModule,
    CKEditorModule,
    FormsModule,
    TableModule,
    NgbModule,
  ],
  declarations: [
    LibraryComponent,
    RisksComponent,
    ControlsComponent,
    DatarequestComponent,
    ProcessUniverseComponent,
    // BenchmarksComponent
  ],
  providers: [],
})
export class LibraryModule {}
