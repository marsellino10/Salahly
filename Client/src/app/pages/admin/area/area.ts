import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { AreaService, CreateAreaPayload } from '../../../core/services/area-service';
import { Area as AreaModel } from '../../../core/models/Area';
import { TranslateModule } from '@ngx-translate/core';

type FormMessage = { type: 'success' | 'error'; text: string };

@Component({
  selector: 'app-area',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule,TranslateModule],
  templateUrl: './area.html',
  styleUrl: './area.css'
})
export class Area implements OnInit {
  private areaService = inject(AreaService);
  private fb = inject(FormBuilder);

  areas: AreaModel[] = [];
  filteredAreas: AreaModel[] = [];
  regions: string[] = [];
  selectedRegion: string | null = null;

  loading = false;
  saving = false;
  deletingId: number | null = null;
  listError: string | null = null;
  formMessage: FormMessage | null = null;
  searchTerm = '';
  editingArea: AreaModel | null = null;

  form = this.fb.nonNullable.group({
    region: ['', [Validators.required, Validators.maxLength(80)]],
    city: ['', [Validators.required, Validators.maxLength(80)]],
  });

  get isEdit() { return !!this.editingArea; }
  get totalAreas() { return this.areas.length; }
  get totalRegions() { return this.regions.length; }

  ngOnInit(): void {
    this.loadAreas();
  }

  // Load areas from API
  loadAreas(): void {
    this.loading = true;
    this.listError = null;

    this.areaService.GetAllAreas()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: res => {
          this.areas = (res?.data ?? []).sort(this.sortAreas);
          this.updateRegions();
          this.applyFilters();
        },
        error: err => this.listError = this.getErrorMessage(err)
      });
  }

  // Filters
  applyFilters(): void {
    const q = this.searchTerm.toLowerCase().trim();
    this.filteredAreas = this.areas.filter(a => 
      (!this.selectedRegion || a.region === this.selectedRegion) &&
      (!q || `${a.region} ${a.city}`.toLowerCase().includes(q))
    );
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedRegion = null;
    this.applyFilters();
  }

  selectRegion(region: string | null): void {
    this.selectedRegion = region;
    this.applyFilters();
  }

  // Edit / Create
  startEdit(area: AreaModel): void {
    this.editingArea = area;
    this.form.patchValue(area);
  }

  cancelEdit(): void {
    this.clearForm();
  }

  clearForm(): void {
    this.editingArea = null;
    this.form.reset({ region: '', city: '' });
  }

  submitForm(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();

    this.saving = true;
    const payload = this.form.value as CreateAreaPayload;
    const request$ = this.isEdit
      ? this.areaService.UpdateArea({ ...payload, id: this.editingArea!.id })
      : this.areaService.CreateArea(payload);

    request$.pipe(finalize(() => this.saving = false))
      .subscribe({
        next: area => {
          if (this.isEdit) {
            this.areas = this.areas.map(a => a.id === area.id ? area : a).sort(this.sortAreas);
            this.formMessage = { type: 'success', text: 'Area updated.' };
          } else {
            this.areas = [area, ...this.areas].sort(this.sortAreas);
            this.formMessage = { type: 'success', text: 'Area added.' };
          }
          this.updateRegions();
          this.applyFilters();
          this.clearForm();
        },
        error: err => this.formMessage = { type: 'error', text: this.getErrorMessage(err) }
      });
  }

  // Delete
  deleteArea(area: AreaModel): void {
    if (!confirm(`Delete ${area.city}, ${area.region}?`)) return;

    this.deletingId = area.id;
    this.areaService.DeleteArea(area.id)
      .pipe(finalize(() => this.deletingId = null))
      .subscribe({
        next: () => {
          this.areas = this.areas.filter(a => a.id !== area.id);
          if (this.editingArea?.id === area.id) this.clearForm();
          this.formMessage = { type: 'success', text: 'Area deleted.' };
          this.updateRegions();
          this.applyFilters();
        },
        error: err => this.formMessage = { type: 'error', text: this.getErrorMessage(err) }
      });
  }

  // Helpers
  trackById(_: number, item: AreaModel) { return item.id; }

  private updateRegions(): void {
    this.regions = Array.from(new Set(this.areas.map(a => a.region))).sort();
  }

  private sortAreas(a: AreaModel, b: AreaModel): number {
    return a.region.localeCompare(b.region) || a.city.localeCompare(b.city);
  }

  private getErrorMessage(error: any): string {
    return error?.error?.message || error?.message || 'Something went wrong';
  }
}
