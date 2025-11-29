import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Portfolio } from '../../../core/models/Portfolio';
import { CraftsmanReview } from '../../../core/models/Craftman';
import { PortfolioItemsService, UpdatePortfolioItemPayload } from '../../../core/services/portfolio-items.service';
import { TechnicianService } from '../../../core/services/technician-service';

@Component({
  selector: 'app-technician-portfolio-details',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './technician-portfolio-details.html',
  styleUrl: './technician-portfolio-details.css',
})
export class TechnicianPortfolioDetails implements OnInit {
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _portfolioService = inject(PortfolioItemsService);
  private readonly _technicianService = inject(TechnicianService);
  private readonly _fb = inject(FormBuilder);
  private readonly _translate = inject(TranslateService);

  @ViewChild('editImageInput') editImageInput?: ElementRef<HTMLInputElement>;

  private portfolioId: number | null = null;

  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly actionBanner = signal<{ type: 'success' | 'error'; text: string } | null>(null);
  readonly item = signal<Portfolio | null>(null);
  readonly reviews = signal<CraftsmanReview[]>([]);
  readonly showEditModal = signal(false);
  readonly showDeleteModal = signal(false);

  readonly editForm = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(80)]],
    description: ['', [Validators.maxLength(800)]],
    displayOrder: [0, [Validators.min(0)]],
    isActive: [true],
    image: [null as File | null],
  });

  ngOnInit(): void {
    const id = Number(this._route.snapshot.paramMap.get('id'));
    if (!id) {
      this.errorMessage.set(this._translate.instant('TechnicianPortfolioDetails.Messages.LoadError'));
      this.isLoading.set(false);
      return;
    }
    this.portfolioId = id;
    this.loadItem();
  }

  loadItem(): void {
    if (!this.portfolioId) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this._portfolioService.getItemById(this.portfolioId).subscribe({
      next: (response) => {
        const data = response.data;
        if (!data) {
          this.errorMessage.set(this._translate.instant('TechnicianPortfolioDetails.Messages.LoadError'));
          this.isLoading.set(false);
          return;
        }
        this.item.set(data);
        this.populateEditForm(data);
        this.loadReviews(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(this.extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  loadReviews(item: Portfolio): void {
    if (!item?.craftsmanId) {
      this.reviews.set([]);
      return;
    }

    this._technicianService.getTechnicianReviews(item.craftsmanId).subscribe({
      next: (response) => {
        const list = response.data ?? [];
        const filtered = list.filter((review) => this.reviewReferencesPortfolio(review, item));
        this.reviews.set(filtered);
      },
      error: () => this.reviews.set([]),
    });
  }

  goBack(): void {
    void this._router.navigate(['/portfolio']);
  }

  openEdit(): void {
    const current = this.item();
    if (!current) {
      return;
    }
    this.populateEditForm(current);
    this.showEditModal.set(true);
  }

  closeEdit(): void {
    this.showEditModal.set(false);
    this.editForm.patchValue({ image: null });
    if (this.editImageInput?.nativeElement) {
      this.editImageInput.nativeElement.value = '';
    }
    this.isSaving.set(false);
  }

  onEditImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.editForm.patchValue({ image: file });
  }

  submitEdit(): void {
    const target = this.item();
    if (!target || !this.portfolioId) {
      return;
    }

    if (this.editForm.invalid || this.isSaving()) {
      this.editForm.markAllAsTouched();
      return;
    }

    const payload = this.buildUpdatePayload();
    this.isSaving.set(true);
    this._portfolioService.updatePortfolioItem(this.portfolioId, payload).subscribe({
      next: (response) => {
        const message = response.message ?? this._translate.instant('TechnicianPortfolioDetails.Messages.UpdateSuccess');
        this.actionBanner.set({ type: 'success', text: message });
        this.closeEdit();
        this.loadItem();
        this.isSaving.set(false);
      },
      error: (error) => {
        this.actionBanner.set({ type: 'error', text: this.extractErrorMessage(error) });
        this.isSaving.set(false);
      },
    });
  }

  promptDelete(): void {
    this.showDeleteModal.set(true);
  }

  cancelDelete(): void {
    this.showDeleteModal.set(false);
    this.isDeleting.set(false);
  }

  confirmDelete(): void {
    if (!this.portfolioId || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this._portfolioService.deletePortfolioItem(this.portfolioId).subscribe({
      next: (response) => {
        const message = response.message ?? this._translate.instant('TechnicianPortfolioDetails.Messages.DeleteSuccess');
        this.actionBanner.set({ type: 'success', text: message });
        this.isDeleting.set(false);
        this.cancelDelete();
        this.goBack();
      },
      error: (error) => {
        this.actionBanner.set({ type: 'error', text: this.extractErrorMessage(error) });
        this.isDeleting.set(false);
      },
    });
  }

  trackReview(index: number, review: CraftsmanReview): number {
    return review.id ?? index;
  }

  trackRating(index: number): number {
    return index;
  }

  getRatingArray(rating: number): number[] {
    const normalized = Math.max(0, Math.round(rating ?? 0));
    return Array.from({ length: normalized }, (_, idx) => idx);
  }

  formatDate(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }
    const date = value instanceof Date ? value : new Date(value);
    return Number.isNaN(date.getTime()) ? '' : date.toLocaleDateString();
  }

  getImageUrl(item: Portfolio | null): string {
    if (!item) {
      return 'https://via.placeholder.com/1200x800?text=Portfolio+Item';
    }
    return item.imageUrl || 'https://via.placeholder.com/1200x800?text=Portfolio+Item';
  }

  dismissBanner(): void {
    this.actionBanner.set(null);
  }

  private populateEditForm(item: Portfolio): void {
    this.editForm.patchValue({
      title: item.title,
      description: item.description ?? '',
      displayOrder: item.displayOrder ?? 0,
      isActive: item.isActive,
      image: null,
    });
    if (this.editImageInput?.nativeElement) {
      this.editImageInput.nativeElement.value = '';
    }
  }

  private buildUpdatePayload(): UpdatePortfolioItemPayload {
    const raw = this.editForm.value;
    return {
      title: raw.title?.trim() ?? '',
      description: raw.description?.trim() || undefined,
      displayOrder: raw.displayOrder !== null ? Number(raw.displayOrder) : 0,
      isActive: Boolean(raw.isActive),
      image: (raw.image as File | null) ?? undefined,
    };
  }

  private reviewReferencesPortfolio(review: CraftsmanReview, item: Portfolio): boolean {
    if (!review || !item) {
      return false;
    }

    if (review.bookingId && review.bookingId === item.id) {
      return true;
    }

    const normalizedComment = (review.comment ?? '').toLowerCase();
    const normalizedTitle = (item.title ?? '').toLowerCase();
    return Boolean(normalizedTitle && normalizedComment.includes(normalizedTitle));
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return this._translate.instant('TechnicianPortfolioDetails.Messages.GenericError');
    }

    if (typeof error === 'string') {
      return error;
    }

    const record = error as Record<string, unknown> | null;
    if (record?.['error'] && typeof record['error'] === 'object') {
      const nested = record['error'] as Record<string, unknown>;
      const messages = [nested['message'], nested['error']].filter((value) => typeof value === 'string') as string[];
      if (messages.length) {
        return messages[0];
      }
    }

    if (typeof record?.['message'] === 'string') {
      return record['message'] as string;
    }

    return this._translate.instant('TechnicianPortfolioDetails.Messages.GenericError');
  }
}
