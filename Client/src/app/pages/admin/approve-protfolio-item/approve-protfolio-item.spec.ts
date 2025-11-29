import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApproveProtfolioItem } from './approve-protfolio-item';

describe('ApproveProtfolioItem', () => {
  let component: ApproveProtfolioItem;
  let fixture: ComponentFixture<ApproveProtfolioItem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApproveProtfolioItem]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApproveProtfolioItem);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
