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

type TimelineStep = {
  key: ServiceRequestStatus | 'HasOffers';
  label: string;
};

@Component({
  selector: 'app-service-request-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './service-request-details.html',
  styleUrl: './service-request-details.css',
})
export class ServiceRequestDetails implements OnInit {
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _requestsService = inject(ServicesRequestsService);
  private readonly _offersService = inject(CustomerOffersService);

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
    { key: 'Open', label: 'Request Posted' },
    { key: 'HasOffers', label: 'Offers Incoming' },
    { key: 'OfferAccepted', label: 'Offer Accepted' },
    { key: 'InProgress', label: 'In Progress' },
    { key: 'Completed', label: 'Completed' },
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
        label: 'Location',
        value: `${current.city || 'City N/A'}, ${current.area || 'Area N/A'}`,
      },
      {
        icon: 'bi-calendar-event',
        label: 'Preferred Date',
        value: `${this.formatDate(current.preferredDate)} · ${current.preferredTimeSlot || 'Flexible slot'}`,
      },
      {
        icon: 'bi-currency-dollar',
        label: 'Budget',
        value: this.formatBudget(current.customerBudget),
      },
      {
        icon: 'bi-people',
        label: 'Offers',
        value: current.maxOffers ? `${current.offersCount}/${current.maxOffers}` : `${current.offersCount}`,
      },
    ];
  });

  ngOnInit(): void {
    const idParam = Number(this._route.snapshot.paramMap.get('id'));
    if (!idParam) {
      this.requestError.set('Missing service request reference.');
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
        this.actionBanner.set(response.message ?? 'Offer accepted successfully.');
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
        this.actionBanner.set(response.message ?? 'Offer rejected successfully.');
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
      return '—';
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
        return 'Accepted';
      case OfferStatus.Rejected:
        return 'Rejected';
      case OfferStatus.Withdrawn:
        return 'Withdrawn';
      case OfferStatus.Expired:
        return 'Expired';
      default:
        return 'Pending';
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

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return 'Something went wrong. Please try again.';
    }

    if (typeof error === 'string') {
      return error;
    }

    const errorRecord = error as Record<string, unknown> | null;
    if (errorRecord && typeof errorRecord['message'] === 'string') {
      return errorRecord['message'] as string;
    }

    return 'Unable to complete the request right now. Please retry shortly.';
  }
}
