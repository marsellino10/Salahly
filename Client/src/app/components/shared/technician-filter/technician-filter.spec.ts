import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TechnicianFilter } from './technician-filter';

describe('TechnicianFilter', () => {
  let component: TechnicianFilter;
  let fixture: ComponentFixture<TechnicianFilter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TechnicianFilter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TechnicianFilter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
