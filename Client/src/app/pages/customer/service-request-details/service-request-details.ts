import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CustomerOffersService, OfferDto, OfferStatus } from '../../../core/services/customer-offers.service';
import {
  ServiceRequestDto,
  ServiceRequestStatus,
  ServicesRequestsService,
} from '../../../core/services/services-requests.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

type TimelineStep = {
  key: ServiceRequestStatus | 'HasOffers';
  labelKey: string;
};

@Component({
  selector: 'app-service-request-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule],
  templateUrl: './service-request-details.html',
  styleUrl: './service-request-details.css',
})
export class ServiceRequestDetails implements OnInit {
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _requestsService = inject(ServicesRequestsService);
  private readonly _offersService = inject(CustomerOffersService);
  private readonly _translate = inject(TranslateService);

  private requestId: number | null = null;
  readonly OfferStatusEnum = OfferStatus;

  readonly request = signal<ServiceRequestDto | null>(null);
  readonly offers = signal<OfferDto[]>([]);
  readonly isRequestLoading = signal(true);
  readonly isOffersLoading = signal(true);
  readonly requestError = signal<string | null>(null);
  readonly offersError = signal<string | null>(null);
  readonly actionBanner = signal<string | null>(null);
  readonly offerActionLoadingId = signal<number | null>(null);
  readonly activeRejectOfferId = signal<number | null>(null);
  rejectReason: string = '';

  readonly lifecycleSteps: TimelineStep[] = [
    { key: 'Open', labelKey: 'ServiceRequestDetails.Timeline.Posted' },
    { key: 'HasOffers', labelKey: 'ServiceRequestDetails.Timeline.OffersIncoming' },
    { key: 'OfferAccepted', labelKey: 'ServiceRequestDetails.Timeline.OfferAccepted' },
    { key: 'InProgress', labelKey: 'ServiceRequestDetails.Timeline.InProgress' },
    { key: 'Completed', labelKey: 'ServiceRequestDetails.Timeline.Completed' },
  ];

  readonly statusChipStyles: Record<string, { bg: string; text: string }> = {
    Open: { bg: 'var(--chip-blue)', text: 'var(--chip-blue-text)' },
    HasOffers: { bg: 'var(--chip-purple)', text: 'var(--chip-purple-text)' },
    OfferAccepted: { bg: 'var(--chip-indigo)', text: 'var(--chip-indigo-text)' },
    InProgress: { bg: 'var(--chip-amber)', text: 'var(--chip-amber-text)' },
    Completed: { bg: 'var(--chip-green)', text: 'var(--chip-green-text)' },
    Cancelled: { bg: 'var(--chip-red)', text: 'var(--chip-red-text)' },
    Expired: { bg: 'var(--chip-gray)', text: 'var(--chip-gray-text)' },
  };

  readonly requestMeta = computed(() => {
    const current = this.request();
    if (!current) {
      return [];
    }

    return [
      {
        icon: 'bi-geo-alt',
        labelKey: 'ServiceRequestDetails.Meta.Location',
        value: `${current.city || this._translate.instant('ServiceRequestDetails.Meta.CityFallback')}, ${
          current.area || this._translate.instant('ServiceRequestDetails.Meta.AreaFallback')
        }`,
      },
      {
        icon: 'bi-calendar-event',
        labelKey: 'ServiceRequestDetails.Meta.PreferredDate',
        value: `${this.formatDate(current.preferredDate)} · ${
          current.preferredTimeSlot || this._translate.instant('ServiceRequestDetails.Meta.TimeFallback')
        }`,
      },
      {
        icon: 'bi-currency-dollar',
        labelKey: 'ServiceRequestDetails.Meta.Budget',
        value: this.formatBudget(current.customerBudget),
      },
      {
        icon: 'bi-people',
        labelKey: 'ServiceRequestDetails.Meta.Offers',
        value: current.maxOffers ? `${current.offersCount}/${current.maxOffers}` : `${current.offersCount}`,
      },
    ];
  });

  ngOnInit(): void {
    const idParam = Number(this._route.snapshot.paramMap.get('id'));
    if (!idParam) {
      this.requestError.set(this._translate.instant('ServiceRequestDetails.Messages.MissingRequest'));
      this.isRequestLoading.set(false);
      this.isOffersLoading.set(false);
      return;
    }

    this.requestId = idParam;
    this.loadRequest();
    this.loadOffers();
  }

  loadRequest(): void {
    if (!this.requestId) {
      return;
    }

    this.isRequestLoading.set(true);
    this.requestError.set(null);

    this._requestsService.getRequestById(this.requestId).subscribe({
      next: (response) => {
        this.request.set(response.data ?? null);
        this.isRequestLoading.set(false);
      },
      error: (error) => {
        this.requestError.set(this.extractErrorMessage(error));
        this.isRequestLoading.set(false);
      },
    });
  }

  loadOffers(): void {
    if (!this.requestId) {
      return;
    }

    this.isOffersLoading.set(true);
    this.offersError.set(null);

    this._offersService.getOffersForRequest(this.requestId).subscribe({
      next: (response) => {
        this.offers.set(response.data ?? []);
        this.isOffersLoading.set(false);
      },
      error: (error) => {
        this.offersError.set(this.extractErrorMessage(error));
        this.isOffersLoading.set(false);
      },
    });
  }

  navigateBack(): void {
    void this._router.navigate(['/show-services-requested']);
  }

  getStatusStyle(status: ServiceRequestStatus | string) {
    return this.statusChipStyles[status] ?? this.statusChipStyles['Open'];
  }

  isStepCompleted(step: TimelineStep): boolean {
    const currentStatus = this.request()?.status as ServiceRequestStatus | undefined;
    if (!currentStatus) {
      return false;
    }

    const order = ['Open', 'HasOffers', 'OfferAccepted', 'InProgress', 'Completed', 'Cancelled', 'Expired'];
    const currentIndex = order.indexOf(currentStatus);
    const stepIndex = order.indexOf(step.key);
    return currentIndex >= stepIndex;
  }

  hasStep(step: TimelineStep): boolean {
    if (step.key !== 'HasOffers') {
      return true;
    }
    const request = this.request();
    return Boolean(request && request.offersCount > 0);
  }

  acceptOffer(offer: OfferDto): void {
    if (this.offerActionLoadingId()) {
      return;
    }

    this.offerActionLoadingId.set(offer.craftsmanOfferId);
    this.actionBanner.set(null);

    this._offersService.acceptOffer(offer.craftsmanOfferId).subscribe({
      next: (response) => {
        this.actionBanner.set(response.message ?? this._translate.instant('ServiceRequestDetails.Messages.AcceptSuccess'));
        this.resetOfferActionState();
      },
      error: (error) => {
        this.actionBanner.set(this.extractErrorMessage(error));
        this.offerActionLoadingId.set(null);
      },
      complete: () => {
        this.loadOffers();
        this.loadRequest();
      },
    });
  }

  toggleRejectForm(offer: OfferDto): void {
    if (this.offerActionLoadingId()) {
      return;
    }

    const current = this.activeRejectOfferId();
    if (current === offer.craftsmanOfferId) {
      this.activeRejectOfferId.set(null);
      this.rejectReason = '';
      return;
    }

    this.activeRejectOfferId.set(offer.craftsmanOfferId);
    this.rejectReason = '';
  }

  submitRejectOffer(): void {
    const offerId = this.activeRejectOfferId();
    if (!offerId || this.offerActionLoadingId()) {
      return;
    }

    this.offerActionLoadingId.set(offerId);
    this.actionBanner.set(null);

    this._offersService.rejectOffer(offerId, { rejectionReason: this.rejectReason.trim() || undefined }).subscribe({
      next: (response) => {
        this.actionBanner.set(response.message ?? this._translate.instant('ServiceRequestDetails.Messages.RejectSuccess'));
        this.resetOfferActionState();
      },
      error: (error) => {
        this.actionBanner.set(this.extractErrorMessage(error));
        this.offerActionLoadingId.set(null);
      },
      complete: () => {
        this.loadOffers();
        this.loadRequest();
      },
    });
  }

  canManageOffer(offer: OfferDto): boolean {
    return offer.status === OfferStatus.Pending;
  }

  dismissBanner(): void {
    this.actionBanner.set(null);
  }

  trackByOfferId(_: number, offer: OfferDto): number {
    return offer.craftsmanOfferId;
  }

  formatDate(value: string): string {
    return new Date(value).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
  }

  formatDateTime(value: string): string {
    return new Date(value).toLocaleString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  formatBudget(value?: number | null): string {
    if (value === undefined || value === null) {
      return this._translate.instant('ServiceRequestDetails.Meta.BudgetUnknown');
    }
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(value);
  }

  getOfferStatusLabel(status: OfferStatus): string {
    switch (status) {
      case OfferStatus.Accepted:
        return this._translate.instant('ServiceRequestDetails.Offers.Status.Accepted');
      case OfferStatus.Rejected:
        return this._translate.instant('ServiceRequestDetails.Offers.Status.Rejected');
      case OfferStatus.Withdrawn:
        return this._translate.instant('ServiceRequestDetails.Offers.Status.Withdrawn');
      case OfferStatus.Expired:
        return this._translate.instant('ServiceRequestDetails.Offers.Status.Expired');
      default:
        return this._translate.instant('ServiceRequestDetails.Offers.Status.Pending');
    }
  }

  getOfferStatusClass(status: OfferStatus): string {
    switch (status) {
      case OfferStatus.Accepted:
        return 'offer-status accepted';
      case OfferStatus.Rejected:
        return 'offer-status rejected';
      case OfferStatus.Withdrawn:
        return 'offer-status withdrawn';
      case OfferStatus.Expired:
        return 'offer-status expired';
      default:
        return 'offer-status pending';
    }
  }

  getCoverImage(): string | null {
    const current = this.request();
    console.log(current);
    if (!current?.images?.length) {
      return null;
    }
    return current.images[0] ?? null;
  }

  private resetOfferActionState(): void {
    this.offerActionLoadingId.set(null);
    this.activeRejectOfferId.set(null);
    this.rejectReason = '';
  }

  getRequestStatusLabel(status: ServiceRequestStatus | string): string {
    const key = this.statusLabelKeys[String(status)];
    return key ? this._translate.instant(key) : String(status);
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return this._translate.instant('ServiceRequestDetails.Messages.GenericError');
    }

    if (typeof error === 'string') {
      return error;
    }

    const errorRecord = error as Record<string, unknown> | null;
    if (errorRecord && typeof errorRecord['message'] === 'string') {
      return errorRecord['message'] as string;
    }

    return this._translate.instant('ServiceRequestDetails.Messages.ActionError');
  }

  private readonly statusLabelKeys: Record<string, string> = {
    Open: 'ServiceRequestDetails.StatusLabels.Open',
    HasOffers: 'ServiceRequestDetails.StatusLabels.HasOffers',
    OfferAccepted: 'ServiceRequestDetails.StatusLabels.OfferAccepted',
    InProgress: 'ServiceRequestDetails.StatusLabels.InProgress',
    Completed: 'ServiceRequestDetails.StatusLabels.Completed',
    Cancelled: 'ServiceRequestDetails.StatusLabels.Cancelled',
    Expired: 'ServiceRequestDetails.StatusLabels.Expired',
  };
}
