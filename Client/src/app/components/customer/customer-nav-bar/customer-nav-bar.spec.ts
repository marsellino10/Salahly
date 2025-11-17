import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerNavBar } from './customer-nav-bar';

describe('CustomerNavBar', () => {
  let component: CustomerNavBar;
  let fixture: ComponentFixture<CustomerNavBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomerNavBar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerNavBar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
