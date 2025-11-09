import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeServicesSection } from './home-services-section';

describe('HomeServicesSection', () => {
  let component: HomeServicesSection;
  let fixture: ComponentFixture<HomeServicesSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeServicesSection]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeServicesSection);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
