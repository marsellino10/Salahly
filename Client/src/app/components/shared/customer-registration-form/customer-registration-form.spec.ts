import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomerRegistrationForm } from './customer-registration-form';

describe('CustomerRegistrationForm', () => {
  let component: CustomerRegistrationForm;
  let fixture: ComponentFixture<CustomerRegistrationForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomerRegistrationForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomerRegistrationForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
