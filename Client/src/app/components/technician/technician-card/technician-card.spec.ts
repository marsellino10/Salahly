import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TechnicianCard } from './technician-card';

describe('TechnicianCard', () => {
  let component: TechnicianCard;
  let fixture: ComponentFixture<TechnicianCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TechnicianCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TechnicianCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
