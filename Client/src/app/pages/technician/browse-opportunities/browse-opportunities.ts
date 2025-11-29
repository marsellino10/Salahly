import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CraftsmanOffersService,
  CreateCraftsmanOfferPayload,
  CraftsmanOfferDto,
  CraftsmanOfferStatus,
} from '../../../core/services/craftsman-offers.service';
import { CraftsmanServiceRequestService, ServiceResponse } from '../../../core/services/craftsman-service-request.service';
import { ServiceRequestDto, ServiceRequestStatus } from '../../../core/services/services-requests.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CarouselModule, OwlOptions } from 'ngx-owl-carousel-o';

type OpportunitySort = 'latest' | 'budget' | 'closingSoon';

@Component({
  selector: 'app-browse-opportunities',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, CarouselModule],
  templateUrl: './browse-opportunities.html',
  styleUrl: './browse-opportunities.css',
})
export class BrowseOpportunities implements OnInit {
  private readonly _service = inject(CraftsmanServiceRequestService);
  private readonly _offersService = inject(CraftsmanOffersService);
  private readonly _fb = inject(FormBuilder);
  private readonly _translate = inject(TranslateService);

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly opportunities = signal<ServiceRequestDto[]>([]);
  readonly searchTerm = signal('');
  readonly statusFilter = signal<ServiceRequestStatus | 'all'>('all');
  readonly cityFilter = signal('all');
  readonly sortOption = signal<OpportunitySort>('latest');
  readonly skeletonPlaceholders = Array.from({ length: 4 });
  readonly hasLoadedOnce = signal(false);
  readonly offerModalOpen = signal(false);
  readonly offerSubmitting = signal(false);
  readonly offerSubmitMessage = signal<string | null>(null);
  readonly selectedRequest = signal<ServiceRequestDto | null>(null);
  readonly offeredRequestIds = signal<Set<number>>(new Set());
  readonly offerLookup = signal<Map<number, CraftsmanOfferDto>>(new Map());
  readonly withdrawingOfferId = signal<number | null>(null);
  readonly actionBanner = signal<{ type: 'success' | 'error'; text: string } | null>(null);
  offerForm!: FormGroup;
  minDate = new Date().toISOString().split('T')[0];

  readonly statusOptions: { value: ServiceRequestStatus; labelKey: string }[] = [
    { value: 'Open', labelKey: 'BrowseOpportunities.Filters.Status.Open' },
    { value: 'HasOffers', labelKey: 'BrowseOpportunities.Filters.Status.HasOffers' },
    { value: 'OfferAccepted', labelKey: 'BrowseOpportunities.Filters.Status.OfferAccepted' },
    { value: 'InProgress', labelKey: 'BrowseOpportunities.Filters.Status.InProgress' },
  ];

  readonly sortOptions: { value: OpportunitySort; labelKey: string }[] = [
    { value: 'latest', labelKey: 'BrowseOpportunities.Filters.Sort.Latest' },
    { value: 'budget', labelKey: 'BrowseOpportunities.Filters.Sort.Budget' },
    { value: 'closingSoon', labelKey: 'BrowseOpportunities.Filters.Sort.ClosingSoon' },
  ];

  readonly filteredOpportunities = computed(() => {
    const search = this.searchTerm().trim().toLowerCase();
    const statusFilter = this.statusFilter();
    const cityFilter = this.cityFilter();
    const sortOption = this.sortOption();

    const filtered = this.opportunities()
      .filter((opportunity) =>
        statusFilter === 'all' ? true : (opportunity.status as ServiceRequestStatus) === statusFilter,
      )
      .filter((opportunity) =>
        cityFilter === 'all'
          ? true
          : (opportunity.city ?? opportunity.area ?? 'unknown').toLowerCase() === cityFilter.toLowerCase(),
      )
      .filter((opportunity) =>
        !search
          ? true
          : opportunity.title.toLowerCase().includes(search) ||
            opportunity.description.toLowerCase().includes(search) ||
            (opportunity.area ?? '').toLowerCase().includes(search),
      );
      console.log('Filtered Opportunities:', filtered);
    return [...filtered].sort((a, b) => this.sortItems(a, b, sortOption));
  });

  readonly hasActiveFilters = computed(
    () =>
      Boolean(this.searchTerm().trim()) ||
      this.statusFilter() !== 'all' ||
      this.cityFilter() !== 'all' ||
      this.sortOption() !== 'latest',
  );

  readonly stats = computed(() => {
    const list = this.opportunities();
    const now = Date.now();
    const closingSoonThreshold = now + 3 * 24 * 60 * 60 * 1000;

    const closingSoon = list.filter((item) => new Date(item.expiresAt).getTime() <= closingSoonThreshold).length;

    return {
      total: list.length,
      open: list.filter((item) => item.status === 'Open').length,
      closingSoon,
      avgBudget:
        list.length === 0
          ? 0
          : Math.round(
              list.reduce((sum, item) => sum + (item.customerBudget ?? 0), 0) / Math.max(1, list.length),
            ),
    };
  });

  readonly cities = computed(() => {
    const unique = new Set(
      this.opportunities()
        .map((item) => item.city ?? item.area ?? null)
        .filter((value): value is string => Boolean(value && value.trim())),
    );
    return Array.from(unique).sort((a, b) => a.localeCompare(b));
  });

  ngOnInit(): void {
    this.offerForm = this._fb.nonNullable.group({
      offeredPrice: [null, [Validators.required, Validators.min(1)]],
      estimatedDurationMinutes: [null, [Validators.required, Validators.min(15)]],
      description: ['', [Validators.required, Validators.minLength(20)]],
      preferredDate: ['', Validators.required],
      preferredTimeSlot: [''],
    });
    this.loadOpportunities();
    this.loadExistingOffers();
  }

  private setActionBanner(type: 'success' | 'error', text: string): void {
    this.actionBanner.set({ type, text });
  }

  loadOpportunities(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this._service.getAvailableOpportunities().subscribe({
      next: (response: ServiceResponse<ServiceRequestDto[]>) => {
        this.opportunities.set(response.data ?? []);
        this.isLoading.set(false);
        this.hasLoadedOnce.set(true);
      },
      error: (error) => {
        this.errorMessage.set(this.extractErrorMessage(error));
        this.isLoading.set(false);
        this.hasLoadedOnce.set(true);
      },
    });
  }

  onSearch(value: string): void {
    this.searchTerm.set(value);
  }

  onStatusChange(status: ServiceRequestStatus | 'all'): void {
    this.statusFilter.set(status);
  }

  onCityChange(city: string): void {
    this.cityFilter.set(city);
  }

  onSortChange(sort: OpportunitySort): void {
    this.sortOption.set(sort);
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set('all');
    this.cityFilter.set('all');
    this.sortOption.set('latest');
  }

  refresh(): void {
    this.loadOpportunities();
    this.loadExistingOffers();
  }

  openOfferModal(request: ServiceRequestDto): void {
    this.selectedRequest.set(request);
    this.offerSubmitMessage.set(null);
    this.offerForm.enable();
    this.offerForm.reset();
    this.offerModalOpen.set(true);
  }

  closeOfferModal(): void {
    if (this.offerSubmitting()) {
      return;
    }
    this.offerModalOpen.set(false);
    this.selectedRequest.set(null);
    this.offerSubmitMessage.set(null);
  }

  submitOffer(): void {
    if (!this.selectedRequest() || this.offerForm.invalid) {
      this.offerForm.markAllAsTouched();
      return;
    }

    const request = this.selectedRequest();
    const formValue = this.offerForm.value;
    const payload: CreateCraftsmanOfferPayload = {
      serviceRequestId: request!.serviceRequestId,
      offeredPrice: Number(formValue['offeredPrice']),
      description: String(formValue['description']),
      estimatedDurationMinutes: Number(formValue['estimatedDurationMinutes']),
      preferredDate: new Date(formValue['preferredDate']).toISOString(),
      preferredTimeSlot: formValue['preferredTimeSlot'] || undefined,
    };

    this.offerSubmitting.set(true);
    this.offerSubmitMessage.set(null);

    this._offersService.createOffer(payload).subscribe({
      next: (response: ServiceResponse<CraftsmanOfferDto>) => {
        const successMessage = response?.message ?? this._translate.instant('BrowseOpportunities.Messages.OfferSubmitSuccess');
        this.offerSubmitMessage.set(successMessage);
        this.offerSubmitting.set(false);
        this.offerForm.enable();
        this.offerModalOpen.set(false);
        this.selectedRequest.set(null);
        this.refresh();
        this.loadExistingOffers();
        this.setActionBanner('success', successMessage);
      },
      error: (error) => {
        this.offerSubmitMessage.set(this.extractErrorMessage(error));
        this.offerSubmitting.set(false);
        this.offerForm.enable();
      },
    });
  }

  hasSubmittedOffer(request: ServiceRequestDto): boolean {
    return this.offeredRequestIds().has(request.serviceRequestId);
  }

  getMyOffer(requestId: number): CraftsmanOfferDto | undefined {
    return this.offerLookup().get(requestId);
  }

  canWithdrawOffer(offer: CraftsmanOfferDto): boolean {
    const status = typeof offer.status === 'number' ? offer.status : CraftsmanOfferStatus[offer.status as keyof typeof CraftsmanOfferStatus];
    return status === CraftsmanOfferStatus.Pending;
  }

  isWithdrawing(offer: CraftsmanOfferDto): boolean {
    return this.withdrawingOfferId() === offer.craftsmanOfferId;
  }

  withdrawOffer(offer: CraftsmanOfferDto): void {
    if (this.isWithdrawing(offer) || !this.canWithdrawOffer(offer)) {
      return;
    }

    this.withdrawingOfferId.set(offer.craftsmanOfferId);
    this._offersService.withdrawOffer(offer.craftsmanOfferId).subscribe({
      next: (response) => {
        this.withdrawingOfferId.set(null);
        this.refresh();
        const withdrawMessage = response?.message ?? this._translate.instant('BrowseOpportunities.Messages.OfferWithdrawSuccess');
        this.setActionBanner('success', withdrawMessage);
      },
      error: (error) => {
        this.withdrawingOfferId.set(null);
        this.setActionBanner('error', this.extractErrorMessage(error));
      },
    });
  }

  trackByRequestId(_: number, request: ServiceRequestDto): number {
    return request.serviceRequestId;
  }

  trackImage(_: number, image: string): string {
    return image;
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' });
  }

  formatBudget(value?: number | null): string {
    if (value === null || value === undefined) {
      return this._translate.instant('BrowseOpportunities.Cards.Meta.BudgetTbd');
    }
    return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(value);
  }

  getProgress(request: ServiceRequestDto): number {
    if (!request.maxOffers) {
      return 0;
    }
    return Math.min(100, Math.round((request.offersCount / request.maxOffers) * 100));
  }

  getRequestImages(request: ServiceRequestDto): string[] {
    return (request.images ?? []).filter((image): image is string => Boolean(image && image.trim()));
  }

  getLocationLabel(request: ServiceRequestDto): string {
    if (request.city && request.area) {
      return `${request.city}, ${request.area}`;
    }
    return request.city ?? request.area ?? this._translate.instant('BrowseOpportunities.Cards.Meta.LocationFallback');
  }

  getStatusStyle(status: ServiceRequestStatus | string) {
    return this.statusChipStyles[status] ?? this.statusChipStyles['Open'];
  }

  isClosingSoon(request: ServiceRequestDto): boolean {
    const threshold = Date.now() + 3 * 24 * 60 * 60 * 1000;
    return new Date(request.expiresAt).getTime() <= threshold;
  }

  timeUntilExpiry(request: ServiceRequestDto): string {
    const distance = new Date(request.expiresAt).getTime() - Date.now();
    if (distance <= 0) {
      return this._translate.instant('BrowseOpportunities.Cards.Meta.ExpiresExpired');
    }

    const hours = Math.floor(distance / (1000 * 60 * 60));
    if (hours < 24) {
      return this._translate.instant('BrowseOpportunities.Cards.Meta.ExpiresHours', { hours });
    }

    const days = Math.floor(hours / 24);
    return this._translate.instant('BrowseOpportunities.Cards.Meta.ExpiresDays', { days });
  }

  formatDateTime(value: string): string {
    return new Date(value).toLocaleString(undefined, {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  getOfferStatusLabel(status: CraftsmanOfferStatus | string): string {
    const normalized = this.normalizeOfferStatus(status);
    const key = this.offerStatusLabelKeys[normalized];
    return key ? this._translate.instant(key) : this._translate.instant('BrowseOpportunities.OfferStatus.Pending');
  }

  getOfferStatusClass(status: CraftsmanOfferStatus | string): string {
    const normalized = typeof status === 'number' ? status : CraftsmanOfferStatus[status as keyof typeof CraftsmanOfferStatus];
    switch (normalized) {
      case CraftsmanOfferStatus.Accepted:
        return 'offer-status accepted';
      case CraftsmanOfferStatus.Rejected:
        return 'offer-status rejected';
      case CraftsmanOfferStatus.Withdrawn:
        return 'offer-status withdrawn';
      case CraftsmanOfferStatus.Expired:
        return 'offer-status expired';
      default:
        return 'offer-status pending';
    }
  }

  dismissActionBanner(): void {
    this.actionBanner.set(null);
  }

  readonly statusChipStyles: Record<string, { bg: string; text: string }> = {
    Open: { bg: 'var(--chip-blue)', text: 'var(--chip-blue-text)' },
    HasOffers: { bg: 'var(--chip-purple)', text: 'var(--chip-purple-text)' },
    OfferAccepted: { bg: 'var(--chip-indigo)', text: 'var(--chip-indigo-text)' },
    InProgress: { bg: 'var(--chip-amber)', text: 'var(--chip-amber-text)' },
    Completed: { bg: 'var(--chip-green)', text: 'var(--chip-green-text)' },
    Cancelled: { bg: 'var(--chip-red)', text: 'var(--chip-red-text)' },
    Expired: { bg: 'var(--chip-gray)', text: 'var(--chip-gray-text)' },
  };

  readonly cardCarouselOptions: OwlOptions = {
    items: 1,
    loop: true,
    dots: true,
    nav: true,
    navText: ['‹', '›'],
    autoplay: true,
    autoplayHoverPause: true,
    responsive: {
      0: { items: 1 },
    },
  };

  private sortItems(a: ServiceRequestDto, b: ServiceRequestDto, sort: OpportunitySort): number {
    switch (sort) {
      case 'budget':
        return (b.customerBudget ?? 0) - (a.customerBudget ?? 0);
      case 'closingSoon':
        return new Date(a.expiresAt).getTime() - new Date(b.expiresAt).getTime();
      default:
        return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
    }
  }

  private loadExistingOffers(): void {
    this._offersService.getMyOffers().subscribe({
      next: (response: ServiceResponse<CraftsmanOfferDto[]>) => {
        const offers = response.data ?? [];
        const existingIds = new Set<number>();
        const lookup = new Map<number, CraftsmanOfferDto>();
        offers.forEach((offer) => {
          existingIds.add(offer.serviceRequestId);
          lookup.set(offer.serviceRequestId, offer);
        });
        this.offeredRequestIds.set(existingIds);
        this.offerLookup.set(lookup);
      },
      error: () => {
        this.offeredRequestIds.set(new Set());
        this.offerLookup.set(new Map());
      },
    });
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return this._translate.instant('BrowseOpportunities.Messages.LoadError');
    }

    if (typeof error === 'string') {
      return error;
    }

    const errorRecord = error as { message?: string; error?: unknown } | null;
    if (typeof errorRecord?.error === 'string') {
      return errorRecord.error;
    }
    if (typeof errorRecord?.message === 'string') {
      return errorRecord.message;
    }

    return this._translate.instant('BrowseOpportunities.Messages.GenericError');
  }

  getOffersProgressLabel(request: ServiceRequestDto): string {
    const maxValue = request.maxOffers ?? this._translate.instant('BrowseOpportunities.Cards.ProgressInfinite');
    return this._translate.instant('BrowseOpportunities.Cards.ProgressLabel', {
      current: request.offersCount,
      max: maxValue,
    });
  }

  getStatusLabel(status: ServiceRequestStatus | string): string {
    const key = this.statusLabelKeys[String(status)];
    return key ? this._translate.instant(key) : String(status);
  }

  private normalizeOfferStatus(status: CraftsmanOfferStatus | string): CraftsmanOfferStatus {
    if (typeof status === 'number') {
      return status;
    }
    return CraftsmanOfferStatus[status as keyof typeof CraftsmanOfferStatus] ?? CraftsmanOfferStatus.Pending;
  }

  private readonly offerStatusLabelKeys: Record<CraftsmanOfferStatus, string> = {
    [CraftsmanOfferStatus.Pending]: 'BrowseOpportunities.OfferStatus.Pending',
    [CraftsmanOfferStatus.Accepted]: 'BrowseOpportunities.OfferStatus.Accepted',
    [CraftsmanOfferStatus.Rejected]: 'BrowseOpportunities.OfferStatus.Rejected',
    [CraftsmanOfferStatus.Withdrawn]: 'BrowseOpportunities.OfferStatus.Withdrawn',
    [CraftsmanOfferStatus.Expired]: 'BrowseOpportunities.OfferStatus.Expired',
  };

  private readonly statusLabelKeys: Record<string, string> = {
    Open: 'BrowseOpportunities.StatusLabels.Open',
    HasOffers: 'BrowseOpportunities.StatusLabels.HasOffers',
    OfferAccepted: 'BrowseOpportunities.StatusLabels.OfferAccepted',
    InProgress: 'BrowseOpportunities.StatusLabels.InProgress',
    Completed: 'BrowseOpportunities.StatusLabels.Completed',
    Cancelled: 'BrowseOpportunities.StatusLabels.Cancelled',
    Expired: 'BrowseOpportunities.StatusLabels.Expired',
  };
}