import { NgModule } from '@angular/core';
import { MyauditsComponent } from "./myaudits.component";
import { MyauditsRoutingModule } from './myaudits-routing.module';
import { ChartsModule } from "../../common/charts/charts.module";
import { TableModule } from './../../common/table/table.module';

@NgModule({
    imports: [
        MyauditsRoutingModule,
        ChartsModule,
        TableModule        
    ],
    declarations: [
        MyauditsComponent
    ]
})

export class MyauditsModule {}