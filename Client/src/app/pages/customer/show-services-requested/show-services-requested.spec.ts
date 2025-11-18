import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShowServicesRequested } from './show-services-requested';

describe('ShowServicesRequested', () => {
  let component: ShowServicesRequested;
  let fixture: ComponentFixture<ShowServicesRequested>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShowServicesRequested]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShowServicesRequested);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
