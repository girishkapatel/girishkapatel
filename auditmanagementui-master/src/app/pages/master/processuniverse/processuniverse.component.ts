import { Component, OnInit, ViewChild } from '@angular/core';
import * as $ from 'jquery'; 

@Component({
  selector: 'app-processuniverse',
  templateUrl: './processuniverse.component.html',
  styleUrls: ['./processuniverse.component.css'],
  providers:[]
})
export class ProcessUniverseComponent implements OnInit{

  constructor() { }
  
  universeView:boolean = true; 
  universeTitle:string = 'Sector Chart';  
  
   showProcessChart(){
    this.universeTitle = 'Process Chart';
    this.universeView = false;
  }

  showProcessDetail(){
    window["jQuery"]("#basic").modal('show');
  }

  hideProcessChart(){
    this.universeTitle = 'Sector Chart';
    this.universeView = true;
  }
  
  ngOnInit() {
   $(document).ready(()=>{
        $(".modal-body").css({"max-height":$("body").height() - 100})
    })
  }
 
  
}
