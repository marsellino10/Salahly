import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceRequestForm } from './service-request-form';

describe('ServiceRequestForm', () => {
  let component: ServiceRequestForm;
  let fixture: ComponentFixture<ServiceRequestForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServiceRequestForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServiceRequestForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
