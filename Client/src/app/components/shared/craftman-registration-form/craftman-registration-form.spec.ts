import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CraftmanRegistrationForm } from './craftman-registration-form';

describe('CraftmanRegistrationForm', () => {
  let component: CraftmanRegistrationForm;
  let fixture: ComponentFixture<CraftmanRegistrationForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CraftmanRegistrationForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CraftmanRegistrationForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
