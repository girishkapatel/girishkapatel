import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableComponent } from "./table.component";
import{NgxSpinnerService,NgxSpinnerModule}from"ngx-spinner"
@NgModule({
    imports: [
        CommonModule  ,
        NgxSpinnerModule      
    ],
    declarations: [
        TableComponent
    ],
    providers:[NgxSpinnerService],
    exports: [TableComponent]
})

export class TableModule {}