import { TestBed } from '@angular/core/testing';

import { CustomerOffersService } from './customer-offers.service';

describe('CustomerOffersService', () => {
  let service: CustomerOffersService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CustomerOffersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
