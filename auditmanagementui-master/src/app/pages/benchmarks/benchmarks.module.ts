import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BenchmarksComponent } from "./benchmarks.component";
import { ChartsModule } from "../../common/charts/charts.module";
import { TableModule } from "../../common/table/table.module";
import { BenchmarksRoutingModule } from './benchmarks-routing.module';

@NgModule({
    imports: [
        BenchmarksRoutingModule,
        CommonModule,
        ChartsModule,
        TableModule,
        FormsModule        
    ],
    declarations: [
        BenchmarksComponent
    ]
})

export class BenchmarksModule {}