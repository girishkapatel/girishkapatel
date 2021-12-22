import { Component, NgZone, OnInit, OnDestroy, AfterViewInit, Input } from "@angular/core";
import * as am4core from "@amcharts/amcharts4/core";
import * as am4charts from "@amcharts/amcharts4/charts";
import am4themes_animated from "@amcharts/amcharts4/themes/animated";
import { Observable, Subscription } from 'rxjs';
import { ArrayType } from '@angular/compiler';

am4core.useTheme(am4themes_animated);
@Component({
  selector: 'app-charts',
  templateUrl: './charts.component.html',
  styleUrls: ['./charts.component.css']
})
export class ChartsComponent implements OnInit, OnDestroy, AfterViewInit {

  private chart: am4charts.XYChart;
  @Input() chartId:string = 'chartId';
  @Input() chartLegend?:boolean = true;
  @Input() chartHeight?:string = "400px";
  @Input() chartDataChange?:Observable<any>;
  piechart:any;
  chartDataSubscription:Subscription;
  @Input() chartData?:any[] = [];
  constructor(private zone: NgZone) {}

  ngOnInit() {
    if(this.chartDataChange){
      this.chartDataSubscription = this.chartDataChange.subscribe(data => {
        if(Array.isArray(data) && data.length){
          this.chartData = data;
          if(this.piechart){
            this.piechart.dispose();
          }
          this.initPieChart();
        }
      }, error=>{
        console.log(error);
      });
    }
    
  }

  ngAfterViewInit() {
    if(!this.chartDataChange){
      this.initPieChart();
    }
  }

  initLineGraph(){
    this.zone.runOutsideAngular(() => {
      let chart = am4core.create(this.chartId, am4charts.XYChart);

      chart.paddingRight = 20;

      let data = [];
      let visits = 10;
      for (let i = 1; i < 366; i++) {
        visits += Math.round((Math.random() < 0.5 ? 1 : -1) * Math.random() * 10);
        data.push({ date: new Date(2018, 0, i), name: "name" + i, value: visits });
      }

      chart.data = data;

      let dateAxis = chart.xAxes.push(new am4charts.DateAxis());
      dateAxis.renderer.grid.template.location = 0;

      let valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
      valueAxis.tooltip.disabled = true;
      valueAxis.renderer.minWidth = 35;

      let series = chart.series.push(new am4charts.LineSeries());
      series.dataFields.dateX = "date";
      series.dataFields.valueY = "value";

      series.tooltipText = "{valueY.value}";
      chart.cursor = new am4charts.XYCursor();

      let scrollbarX = new am4charts.XYChartScrollbar();
      scrollbarX.series.push(series);
      chart.scrollbarX = scrollbarX;

      this.chart = chart;
    });
  }

  initPieChart(){
    this.piechart = am4core.create(this.chartId, am4charts.PieChart);

    // Add and configure Series
    let pieSeries = this.piechart.series.push(new am4charts.PieSeries());
    pieSeries.dataFields.value = "count";
    pieSeries.dataFields.category = "status";

    // Let's cut a hole in our Pie chart the size of 30% the radius
    this.piechart.innerRadius = am4core.percent(30);

    // Put a thick white border around each Slice
    pieSeries.slices.template.stroke = am4core.color("#fff");
    pieSeries.slices.template.strokeWidth = 2;
    pieSeries.slices.template.strokeOpacity = 1;
    pieSeries.slices.template
      // change the cursor on hover to make it apparent the object can be interacted with
      .cursorOverStyle = [
        {
          "property": "cursor",
          "value": "pointer"
        }
      ];
    pieSeries.slices.template.propertyFields.fill = "color";
    
    // pieSeries.alignLabels = false;
    // pieSeries.labels.template.bent = false;
     pieSeries.labels.template.disabled = true;
    // pieSeries.labels.template.padding(0,0,0,0);

    pieSeries.ticks.template.disabled = false;

    // Create a base filter effect (as if it's not there) for the hover to return to
    let shadow = pieSeries.slices.template.filters.push(new am4core.DropShadowFilter);
    shadow.opacity = 0;

    // Create hover state
    let hoverState = pieSeries.slices.template.states.getKey("hover"); // normally we have to create the hover state, in this case it already exists

    // Slightly shift the shadow and make it more prominent on hover
    let hoverShadow = hoverState.filters.push(new am4core.DropShadowFilter);
    hoverShadow.opacity = 0.7;
    hoverShadow.blur = 5;

    // Add a legend
    if(this.chartLegend){
        this.piechart.legend = new am4charts.Legend();
    }

    this.piechart.data = this.chartData;

  }

  ngOnDestroy() {
    if(this.piechart){
      this.piechart.dispose();
    }
   
   if(this.chartDataSubscription){
    this.chartDataSubscription.unsubscribe();
   }
  }

}

