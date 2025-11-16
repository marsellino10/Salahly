import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TechnicianNavBar } from './technician-nav-bar';

describe('TechnicianNavBar', () => {
  let component: TechnicianNavBar;
  let fixture: ComponentFixture<TechnicianNavBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TechnicianNavBar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TechnicianNavBar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
