import { CommonModule } from '@angular/common';
import { Component, DestroyRef, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CraftService, CreateCraftPayload, UpdateCraftPayload } from '../../../core/services/craft-service';
import { Craft as CraftModel } from '../../../core/models/Craft';

@Component({
  selector: 'app-craft',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './craft.html',
  styleUrl: './craft.css',
})
export class Craft implements OnInit, OnDestroy {
  private readonly craftService = inject(CraftService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  crafts: CraftModel[] = [];
  loadingList = false;
  listError: string | null = null;
  saving = false;
  deletingId: number | null = null;
  formMessage: { type: 'success' | 'error'; text: string } | null = null;
  editingCraft: CraftModel | null = null;
  iconPreviewUrl: string | null = null;
  selectedIconFile: File | null = null;
  private iconObjectUrl: string | null = null;

  @ViewChild('iconInput') iconInput?: ElementRef<HTMLInputElement>;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(60)]],
    nameAr: ['', [Validators.required, Validators.maxLength(60)]],
    description: ['', [Validators.required, Validators.maxLength(500)]],
    displayOrder: [1, [Validators.required, Validators.min(0)]],
    isActive: [true],
  });

  get isEditMode(): boolean {
    return this.editingCraft !== null;
  }

  ngOnInit(): void {
    this.loadCrafts();
  }

  ngOnDestroy(): void {
    this.releaseIconObjectUrl();
  }

  trackCraft(_: number, craft: CraftModel): number {
    return craft.id;
  }

  loadCrafts(): void {
    this.loadingList = true;
    this.listError = null;
    this.craftService
      .GetAllCrafts()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (crafts) => {
          this.crafts = [...crafts.data].sort(this.sortCrafts);
          this.loadingList = false;
          this.prefillDisplayOrder();
        },
        error: (error) => {
          this.loadingList = false;
          this.listError = this.extractErrorMessage(error);
        },
      });
  }

  submitForm(): void {
    this.formMessage = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    const request$ = this.isEditMode && this.editingCraft
      ? this.craftService.UpdateCraft({ ...payload, id: this.editingCraft.id }, this.selectedIconFile)
      : this.craftService.CreateCraft(payload, this.selectedIconFile);

    this.saving = true;
    request$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (craft) => {
          if (this.editingCraft) {
            this.crafts = this.crafts.map((item) => (item.id === craft.id ? craft : item)).sort(this.sortCrafts);
            this.formMessage = { type: 'success', text: 'Craft updated successfully.' };
          } else {
            this.crafts = [craft, ...this.crafts].sort(this.sortCrafts);
            this.formMessage = { type: 'success', text: 'Craft created successfully.' };
          }

          this.resetForm();
          this.prefillDisplayOrder();
          this.saving = false;
        },
        error: (error) => {
          this.saving = false;
          this.formMessage = { type: 'error', text: this.extractErrorMessage(error) };
        },
      });
  }

  startEdit(craft: CraftModel): void {
    this.editingCraft = craft;
    this.form.patchValue({
      name: craft.name,
      nameAr: craft.nameAr ?? '',
      description: craft.description,
      displayOrder: craft.displayOrder,
      isActive: craft.isActive,
    });
    this.selectedIconFile = null;
    this.releaseIconObjectUrl();
    this.iconPreviewUrl = craft.iconUrl ?? null;
    this.resetIconInput();
  }

  cancelEdit(): void {
    this.resetForm();
  }

  clearForm(): void {
    this.resetForm();
    this.formMessage = null;
  }

  deleteCraft(craft: CraftModel): void {
    if (this.deletingId) {
      return;
    }

    const confirmed = window.confirm(`Delete craft "${craft.name}"? This action cannot be undone.`);
    if (!confirmed) {
      return;
    }

    this.deletingId = craft.id;
    this.craftService
      .DeleteCraft(craft.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.crafts = this.crafts.filter((item) => item.id !== craft.id);
          this.deletingId = null;
          this.formMessage = { type: 'success', text: 'Craft deleted successfully.' };
          if (this.editingCraft?.id === craft.id) {
            this.resetForm();
          }
          this.prefillDisplayOrder();
        },
        error: (error) => {
          this.deletingId = null;
          this.formMessage = { type: 'error', text: this.extractErrorMessage(error) };
        },
      });
  }

  statusLabel(craft: CraftModel): string {
    return craft.isActive ? 'Active' : 'Hidden';
  }

  statusClass(craft: CraftModel): string {
    return craft.isActive ? 'status-badge--active' : 'status-badge--inactive';
  }

  hasError(controlName: keyof typeof this.form.controls, error: string): boolean {
    const control = this.form.controls[controlName];
    return !!control && control.touched && control.hasError(error);
  }

  formatDate(value: string): string {
    if (!value) {
      return '—';
    }
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '—' : date.toLocaleDateString();
  }

  getCraftsmenCount(craft: CraftModel): number {
    return typeof craft.craftsmenCount === 'number' ? craft.craftsmenCount : 0;
  }

  getActiveRequestsCount(craft: CraftModel): number {
    return typeof craft.activeServiceRequestsCount === 'number' ? craft.activeServiceRequestsCount : 0;
  }

  onIconSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input?.files?.length) {
      return;
    }

    const file = input.files[0];
    this.selectedIconFile = file;
    this.updatePreviewFromFile(file);
  }

  removeSelectedIcon(): void {
    this.selectedIconFile = null;
    this.resetIconInput();
    this.releaseIconObjectUrl();
    this.iconPreviewUrl = this.editingCraft?.iconUrl ?? null;
  }

  private buildPayload(): CreateCraftPayload {
    const raw = this.form.getRawValue();
    return {
      name: raw.name.trim(),
      nameAr: raw.nameAr.trim(),
      description: raw.description.trim() || null,
      displayOrder: Number(raw.displayOrder ?? 0),
      isActive: raw.isActive,
    };
  }

  private resetForm(): void {
    this.editingCraft = null;
    this.form.reset({
      name: '',
      nameAr: '',
      description: '',
      displayOrder: this.getNextDisplayOrder(),
      isActive: true,
    });
    this.selectedIconFile = null;
    this.releaseIconObjectUrl();
    this.iconPreviewUrl = null;
    this.resetIconInput();
  }

  private prefillDisplayOrder(): void {
    if (!this.isEditMode) {
      this.form.controls.displayOrder.setValue(this.getNextDisplayOrder());
    }
  }

  private getNextDisplayOrder(): number {
    if (!this.crafts.length) {
      return 1;
    }
    const maxOrder = Math.max(...this.crafts.map((craft) => craft.displayOrder ?? 0));
    return maxOrder + 1;
  }

  private sortCrafts = (a: CraftModel, b: CraftModel): number => {
    if (a.displayOrder === b.displayOrder) {
      return a.name.localeCompare(b.name);
    }
    return a.displayOrder - b.displayOrder;
  };

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return 'Something went wrong. Please try again.';
    }

    if (typeof error === 'string') {
      return error;
    }

    if (error instanceof Error) {
      return error.message;
    }

    if (typeof error === 'object') {
      const httpError = error as { message?: unknown; error?: { message?: unknown } };
      const nestedMessage = httpError.error?.message ?? httpError.message;
      if (typeof nestedMessage === 'string') {
        return nestedMessage;
      }
    }

    return 'Something went wrong. Please try again.';
  }

  private updatePreviewFromFile(file: File): void {
    this.releaseIconObjectUrl();
    const objectUrl = URL.createObjectURL(file);
    this.iconObjectUrl = objectUrl;
    this.iconPreviewUrl = objectUrl;
  }

  private resetIconInput(): void {
    if (this.iconInput?.nativeElement) {
      this.iconInput.nativeElement.value = '';
    }
  }

  private releaseIconObjectUrl(): void {
    if (this.iconObjectUrl) {
      URL.revokeObjectURL(this.iconObjectUrl);
      this.iconObjectUrl = null;
    }
  }
}
