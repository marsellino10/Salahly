import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { animate, style, transition, trigger } from '@angular/animations';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Portfolio } from '../../../core/models/Portfolio';
import { CraftsmanReview } from '../../../core/models/Craftman';
import {
  CreatePortfolioItemPayload,
  PortfolioItemsService,
  UpdatePortfolioItemPayload,
} from '../../../core/services/portfolio-items.service';
import { TechnicianService } from '../../../core/services/technician-service';

@Component({
  selector: 'app-technician-portfolio',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './technician-portfolio.html',
  styleUrl: './technician-portfolio.css',
  animations: [
    trigger('slideFade', [
      transition(':enter', [
        style({ height: 0, opacity: 0, transform: 'translateY(-12px)', overflow: 'hidden' }),
        animate('300ms ease-in-out', style({ height: '*', opacity: 1, transform: 'translateY(0)', overflow: 'hidden' })),
      ]),
      transition(':leave', [
        style({ height: '*', opacity: 1, transform: 'translateY(0)', overflow: 'hidden' }),
        animate('300ms ease-in-out', style({ height: 0, opacity: 0, transform: 'translateY(-16px)', overflow: 'hidden' })),
      ]),
    ]),
  ],
})
export class TechnicianPortfolio implements OnInit {
  private readonly _portfolioService = inject(PortfolioItemsService);
  private readonly _technicianService = inject(TechnicianService);
  private readonly _fb = inject(FormBuilder);
  private readonly _translate = inject(TranslateService);

  @ViewChild('createImageInput') createImageInput?: ElementRef<HTMLInputElement>;
  @ViewChild('editImageInput') editImageInput?: ElementRef<HTMLInputElement>;

  private craftsmanId: number | null = null;

  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly isUpdating = signal(false);
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly actionBanner = signal<{ type: 'success' | 'error'; text: string } | null>(null);
  readonly items = signal<Portfolio[]>([]);
  readonly reviews = signal<CraftsmanReview[]>([]);
  balance = signal<number>(0.0);
  readonly editingItem = signal<Portfolio | null>(null);
  readonly deleteTargetId = signal<number | null>(null);
  readonly showCreateForm = signal(false);

  readonly stats = computed(() => {
    const entries = this.items();
    const active = entries.filter((item) => item.isActive).length;
    return {
      total: entries.length,
      active,
      inactive: entries.length - active,
    };
  });

  readonly createForm = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(80)]],
    description: ['', [Validators.maxLength(800)]],
    displayOrder: [0, [Validators.min(0)]],
    image: [null as File | null, Validators.required],
  });

  readonly editForm = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(80)]],
    description: ['', [Validators.maxLength(800)]],
    displayOrder: [0, [Validators.min(0)]],
    isActive: [true],
    image: [null as File | null],
  });

  ngOnInit(): void {
    const claims = this._technicianService.getTechnicianTokenClaims();
    this.craftsmanId = claims?.nameIdentifier ? Number(claims.nameIdentifier) : null;
    if(this.craftsmanId != null){
      this._technicianService.getTechnicianById(this.craftsmanId).subscribe({
        next:(value) => {
          this.balance.set(value.data.balance ? value.data.balance : 0);
        },
      })
    }

    if (!this.craftsmanId) {
      this.errorMessage.set(this._translate.instant('TechnicianPortfolio.Messages.MissingCraftsman'));
      this.isLoading.set(false);
      return;
    }

    this.loadPortfolio();
    this.loadReviews();
  }

  loadPortfolio(): void {
    if (!this.craftsmanId) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this._portfolioService.getByCraftsman(this.craftsmanId).subscribe({
      next: (response) => {
        this.items.set(response.data ?? []);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(this.extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  private normalizeReview(review: Partial<CraftsmanReview>): CraftsmanReview {
    return {
      id: review?.id ?? 0,
      reviewerUserId: review?.reviewerUserId ?? 0,
      reviewerName: review?.reviewerName ?? `Reviewer #${review?.reviewerUserId ?? 'N/A'}`,
      reviewerProfileImageUrl: review?.reviewerProfileImageUrl ?? null,
      rating: review?.rating ?? 0,
      comment: review?.comment ?? 'No comment provided.',
      createdAt: review?.createdAt ?? new Date().toISOString(),
      bookingId: review?.bookingId ?? 0,
    };
  }
  loadReviews(): void {
    if (!this.craftsmanId) {
      return;
    }

    this._technicianService.getTechnicianReviews(this.craftsmanId).subscribe({
      next: (response) => {
        const list = response.data ?? [];
        console.log(list);
        const normalizedList = list.map((review) => this.normalizeReview(review));
        this.reviews.set(normalizedList.slice().sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()));
      },
      error: () => {
        this.reviews.set([]);
      },
    });
  }

  onCreateImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.createForm.patchValue({ image: file });
    this.createForm.get('image')?.updateValueAndValidity();
  }

  onEditImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.editForm.patchValue({ image: file });
  }

  toggleCreateForm(): void {
    this.showCreateForm.update((open) => !open);
    if (!this.showCreateForm()) {
      this.resetCreateForm();
    }
  }

  openCreateForm(): void {
    this.showCreateForm.set(true);
  }

  closeCreateForm(): void {
    this.showCreateForm.set(false);
    this.resetCreateForm();
  }

  submitNewItem(): void {
    if (!this.craftsmanId) {
      return;
    }

    if (this.createForm.invalid || this.isSubmitting()) {
      this.createForm.markAllAsTouched();
      return;
    }

    const payload = this.buildCreatePayload();
    this.isSubmitting.set(true);
    this._portfolioService.createPortfolioItem(this.craftsmanId, payload).subscribe({
      next: (response) => {
        const message = response.message ?? this._translate.instant('TechnicianPortfolio.Messages.CreateSuccess');
        this.actionBanner.set({ type: 'success', text: message });
        this.resetCreateForm();
        this.closeCreateForm();
        this.loadPortfolio();
        this.isSubmitting.set(false);
      },
      error: (error) => {
        this.actionBanner.set({ type: 'error', text: this.extractErrorMessage(error) });
        this.isSubmitting.set(false);
      },
    });
  }

  openEditModal(item: Portfolio): void {
    this.editingItem.set(item);
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

  closeEditModal(): void {
    this.editingItem.set(null);
    this.editForm.reset({ title: '', description: '', displayOrder: 0, isActive: true, image: null });
    if (this.editImageInput?.nativeElement) {
      this.editImageInput.nativeElement.value = '';
    }
    this.isUpdating.set(false);
  }

  submitEdit(): void {
    const target = this.editingItem();
    if (!target) {
      return;
    }

    if (this.editForm.invalid || this.isUpdating()) {
      this.editForm.markAllAsTouched();
      return;
    }

    const payload = this.buildUpdatePayload();
    this.isUpdating.set(true);
    this._portfolioService.updatePortfolioItem(target.id, payload).subscribe({
      next: (response) => {
        const message = response.message ?? this._translate.instant('TechnicianPortfolio.Messages.UpdateSuccess');
        this.actionBanner.set({ type: 'success', text: message });
        this.closeEditModal();
        this.loadPortfolio();
        this.isUpdating.set(false);
      },
      error: (error) => {
        this.actionBanner.set({ type: 'error', text: this.extractErrorMessage(error) });
        this.isUpdating.set(false);
      },
    });
  }

  promptDelete(itemId: number): void {
    this.deleteTargetId.set(itemId);
  }

  cancelDelete(): void {
    this.deleteTargetId.set(null);
    this.isDeleting.set(false);
  }

  confirmDelete(): void {
    const targetId = this.deleteTargetId();
    if (!targetId || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this._portfolioService.deletePortfolioItem(targetId).subscribe({
      next: (response) => {
        const message = response.message ?? this._translate.instant('TechnicianPortfolio.Messages.DeleteSuccess');
        this.actionBanner.set({ type: 'success', text: message });
        this.isDeleting.set(false);
        this.cancelDelete();
        this.loadPortfolio();
      },
      error: (error) => {
        this.actionBanner.set({ type: 'error', text: this.extractErrorMessage(error) });
        this.isDeleting.set(false);
        this.cancelDelete();
      },
    });
  }

  dismissBanner(): void {
    this.actionBanner.set(null);
  }

  trackByPortfolioId(_: number, item: Portfolio): number {
    return item.id;
  }

  reviewInitials(name: string | null | undefined): string {
    if (!name?.trim()) {
      return '--';
    }
    return name
      .split(' ')
      .filter(Boolean)
      .map((segment) => segment.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString();
  }

  shortDescription(value: string | null | undefined, maxLength: number = 140): string {
    if (!value) {
      return '';
    }
    return value.length > maxLength ? `${value.slice(0, maxLength).trim()}…` : value;
  }

  getImageUrl(item: Portfolio): string {
    return item.imageUrl || 'https://via.placeholder.com/600x400?text=Portfolio+Item';
  }

  getStatusChipClass(item: Portfolio): string {
    return item.isActive ? 'status-chip active' : 'status-chip inactive';
  }

  private buildCreatePayload(): CreatePortfolioItemPayload {
    const raw = this.createForm.value;
    return {
      title: raw.title?.trim() ?? '',
      description: raw.description?.trim() || undefined,
      displayOrder: raw.displayOrder !== null ? Number(raw.displayOrder) : 0,
      image: raw.image as File,
    };
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

  resetCreateForm(): void {
    this.createForm.reset({ title: '', description: '', displayOrder: 0, image: null });
    if (this.createImageInput?.nativeElement) {
      this.createImageInput.nativeElement.value = '';
    }
  }

  getRatingArray(rating: number): number[] {
    const normalized = Math.max(0, Math.round(rating ?? 0));
    return Array.from({ length: normalized }, (_, index) => index);
  }

  trackByRating(index: number): number {
    return index;
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return this._translate.instant('TechnicianPortfolio.Messages.GenericError');
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

    return this._translate.instant('TechnicianPortfolio.Messages.GenericError');
  }
}
