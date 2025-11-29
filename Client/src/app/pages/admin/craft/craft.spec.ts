import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Craft } from './craft';

describe('Craft', () => {
  let component: Craft;
  let fixture: ComponentFixture<Craft>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Craft]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Craft);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
