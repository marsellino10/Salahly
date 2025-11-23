import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BrowseOpportunities } from './browse-opportunities';

describe('BrowseOpportunities', () => {
  let component: BrowseOpportunities;
  let fixture: ComponentFixture<BrowseOpportunities>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BrowseOpportunities]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BrowseOpportunities);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
