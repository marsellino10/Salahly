import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TranslateSelect } from './translate-select';

describe('TranslateSelect', () => {
  let component: TranslateSelect;
  let fixture: ComponentFixture<TranslateSelect>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TranslateSelect]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TranslateSelect);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
