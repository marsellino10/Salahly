import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { BrowseTechnicians } from './browse-technicians';

describe('BrowseTechnicians', () => {
  let component: BrowseTechnicians;
  let fixture: ComponentFixture<BrowseTechnicians>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BrowseTechnicians, FormsModule]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BrowseTechnicians);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.searchQuery).toBe('');
    expect(component.selectedCraftId).toBe(0);
    expect(component.selectedSort).toBe('Highest Rated');
    expect(component.filteredCraftsmen.length).toBe(3);
  });

  it('should filter craftsmen by search query', () => {
    component.searchQuery = 'Khalid';
    component.onSearch();
    expect(component.filteredCraftsmen.length).toBe(1);
    expect(component.filteredCraftsmen[0].fullName).toBe('Khalid Abdullah');
  });

  it('should filter craftsmen by craft category', () => {
    component.selectedCraftId = 1;
    component.onCategoryChange();
    expect(component.filteredCraftsmen.length).toBe(1);
    expect(component.filteredCraftsmen[0].craftId).toBe(1);
  });

  it('should sort craftsmen by rating', () => {
    component.selectedSort = 'Highest Rated';
    component.onSortChange();
    expect(component.filteredCraftsmen[0].ratingAverage).toBeGreaterThanOrEqual(component.filteredCraftsmen[1].ratingAverage);
  });

  it('should generate correct star array', () => {
    const stars = component.getStarArray(4.5);
    expect(stars.length).toBe(5);
    expect(stars.filter(star => star).length).toBe(4);
  });
});
