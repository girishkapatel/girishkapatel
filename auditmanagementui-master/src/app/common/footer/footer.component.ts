import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent implements OnInit {
  copyrightYear:any;
  constructor() { }
  ngOnInit() {
    var date=new Date();
    this.copyrightYear=date.getFullYear();
  }
}
