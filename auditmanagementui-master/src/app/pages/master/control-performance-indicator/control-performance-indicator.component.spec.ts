import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ControlPerformanceIndicatorComponent } from './control-performance-indicator.component';

describe('ControlPerformanceIndicatorComponent', () => {
  let component: ControlPerformanceIndicatorComponent;
  let fixture: ComponentFixture<ControlPerformanceIndicatorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ControlPerformanceIndicatorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ControlPerformanceIndicatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
