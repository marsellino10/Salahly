import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceRequestDetails } from './service-request-details';

describe('ServiceRequestDetails', () => {
  let component: ServiceRequestDetails;
  let fixture: ComponentFixture<ServiceRequestDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServiceRequestDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ServiceRequestDetails);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
