import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { CraftsmanOfferDto, CraftsmanOffersService, CraftsmanOfferStatus } from '../../../core/services/craftsman-offers.service';
import { CraftsmanServiceRequestService } from '../../../core/services/craftsman-service-request.service';
import { ServiceRequestDto, ServiceRequestStatus } from '../../../core/services/services-requests.service';
import { RouterLink } from "@angular/router";
import { TranslateModule, TranslateService } from '@ngx-translate/core';

type OfferHistoryRecord = {
  offer: CraftsmanOfferDto;
  request?: ServiceRequestDto;
};

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule, RouterLink,TranslateModule],
  templateUrl: './history.html',
  styleUrl: './history.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class History implements OnInit {
  private readonly _requestsService = inject(CraftsmanServiceRequestService);
  private readonly _offersService = inject(CraftsmanOffersService);
  readonly translate = inject(TranslateService);

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly records = signal<OfferHistoryRecord[]>([]);
  readonly statusFilter = signal<'all' | CraftsmanOfferStatus>('all');
  readonly hasLoaded = signal(false);
  readonly skeletonPlaceholders = Array.from({ length: 4 });

  readonly filteredRecords = computed(() => {
    const filter = this.statusFilter();
    return this.records().filter((record) =>
      filter === 'all' ? true : this.normalizeOfferStatus(record.offer.status) === filter,
    );
  });

  readonly summaryStats = computed(() => {
    const stats = {
      total: 0,
      pending: 0,
      accepted: 0,
      rejected: 0,
      withdrawn: 0,
      expired: 0,
    };

    this.records().forEach((record) => {
      stats.total += 1;
      const status = this.normalizeOfferStatus(record.offer.status);
      switch (status) {
        case CraftsmanOfferStatus.Accepted:
          stats.accepted += 1;
          break;
        case CraftsmanOfferStatus.Rejected:
          stats.rejected += 1;
          break;
        case CraftsmanOfferStatus.Withdrawn:
          stats.withdrawn += 1;
          break;
        case CraftsmanOfferStatus.Expired:
          stats.expired += 1;
          break;
        default:
          stats.pending += 1;
      }
    });

    return stats;
  });

  readonly statusOptions: { value: 'all' | CraftsmanOfferStatus; label: string }[] = [
    { value: 'all', label: this.translate.instant('History.Statuses.All') },
    { value: CraftsmanOfferStatus.Pending, label: this.translate.instant('History.Statuses.Pending') },
    { value: CraftsmanOfferStatus.Accepted, label: this.translate.instant('History.Statuses.Accepted') },
    { value: CraftsmanOfferStatus.Rejected, label: this.translate.instant('History.Statuses.Rejected') },
    { value: CraftsmanOfferStatus.Withdrawn, label: this.translate.instant('History.Statuses.Withdrawn') },
    { value: CraftsmanOfferStatus.Expired, label: this.translate.instant('History.Statuses.Expired') }
  ];

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      requestsResponse: this._requestsService.getRequestsCraftsmanOfferedOn(),
      offersResponse: this._offersService.getMyOffers(),
    }).subscribe({
      next: ({ requestsResponse, offersResponse }) => {
        const requests = requestsResponse?.data ?? [];
        const offers = offersResponse?.data ?? [];

        const requestLookup = new Map<number, ServiceRequestDto>();
        requests.forEach((request) => requestLookup.set(request.serviceRequestId, request));

        const merged = offers.map((offer) => ({
          offer,
          request: requestLookup.get(offer.serviceRequestId),
        }));

        merged.sort(
          (a, b) => new Date(b.offer.createdAt).getTime() - new Date(a.offer.createdAt).getTime(),
        );

        this.records.set(merged);
        this.isLoading.set(false);
        this.hasLoaded.set(true);
      },
      error: (error) => {
        this.errorMessage.set(this.extractErrorMessage(error));
        this.isLoading.set(false);
        this.hasLoaded.set(true);
      },
    });
  }

  refresh(): void {
    this.loadHistory();
  }

  onStatusChange(value: string): void {
    if (value === 'all') {
      this.statusFilter.set('all');
      return;
    }
    const parsed = Number(value) as CraftsmanOfferStatus;
    this.statusFilter.set(parsed);
  }

  trackByOfferId(_: number, item: OfferHistoryRecord): number {
    return item.offer.craftsmanOfferId;
  }

  getOfferStatusLabel(status: CraftsmanOfferStatus | string): string {
    switch (this.normalizeOfferStatus(status)) {
      case CraftsmanOfferStatus.Accepted:
        return this.translate.instant('History.Statuses.Accepted');
      case CraftsmanOfferStatus.Rejected:
        return this.translate.instant('History.Statuses.Rejected');
      case CraftsmanOfferStatus.Withdrawn:
        return this.translate.instant('History.Statuses.Withdrawn');
      case CraftsmanOfferStatus.Expired:
        return this.translate.instant('History.Statuses.Expired');
      default:
        return this.translate.instant('History.Statuses.Pending');
    }
  }

  getOfferStatusClass(status: CraftsmanOfferStatus | string): string {
    switch (this.normalizeOfferStatus(status)) {
      case CraftsmanOfferStatus.Accepted:
        return 'pill accepted';
      case CraftsmanOfferStatus.Rejected:
        return 'pill rejected';
      case CraftsmanOfferStatus.Withdrawn:
        return 'pill withdrawn';
      case CraftsmanOfferStatus.Expired:
        return 'pill expired';
      default:
        return 'pill pending';
    }
  }

  getRequestStatusClass(status: ServiceRequestStatus | string | undefined): string {
    const normalized = (status ?? 'Open').toString();
    switch (normalized) {
      case 'InProgress':
      case 'OfferAccepted':
        return 'chip in-progress';
      case 'Completed':
        return 'chip completed';
      case 'Cancelled':
        return 'chip cancelled';
      case 'Expired':
        return 'chip expired';
      default:
        return 'chip open';
    }
  }

  getRequestStatusLabel(status: ServiceRequestStatus | string | undefined): string {
    if (!status) {
      return 'Open';
    }
    const label = status.toString();
    return label.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  getLocationLabel(request?: ServiceRequestDto): string {
    if (!request) {
      return 'Location unavailable';
    }
    if (request.city && request.area) {
      return `${request.city}, ${request.area}`;
    }
    return request.city ?? request.area ?? 'Location unavailable';
  }

  getRequestTitle(request?: ServiceRequestDto): string {
    if (!request) {
      return 'Service request unavailable';
    }
    return request.title ?? 'Service request';
  }

  formatDuration(minutes?: number | null): string {
    if (!minutes || minutes <= 0) {
      return 'Duration TBD';
    }
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    if (!hours) {
      return `${remainingMinutes} min`;
    }
    if (!remainingMinutes) {
      return `${hours} hr${hours > 1 ? 's' : ''}`;
    }
    return `${hours} hr${hours > 1 ? 's' : ''} ${remainingMinutes} min`;
  }

  formatCurrency(value?: number | null): string {
    if (value === null || value === undefined) {
      return 'Budget TBD';
    }
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(value);
  }

  formatDate(value: string | undefined | null): string {
    if (!value) {
      return 'Not provided';
    }
    return new Date(value).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  formatDateTime(value: string | undefined | null): string {
    if (!value) {
      return 'Unknown';
    }
    return new Date(value).toLocaleString(undefined, {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  private normalizeOfferStatus(status: CraftsmanOfferStatus | string): CraftsmanOfferStatus {
    if (typeof status === 'number') {
      return status;
    }
    return CraftsmanOfferStatus[status as keyof typeof CraftsmanOfferStatus] ?? CraftsmanOfferStatus.Pending;
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return 'Unable to load offer history.';
    }

    if (typeof error === 'string') {
      return error;
    }

    const record = error as { message?: string; error?: unknown };
    if (typeof record?.message === 'string') {
      return record.message;
    }
    if (typeof record?.error === 'string') {
      return record.error;
    }

    return 'Something went wrong. Please try again.';
  }
}
