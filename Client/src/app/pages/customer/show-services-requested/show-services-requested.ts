import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ServicesRequestsService, ServiceRequestDto, ServiceRequestStatus } from '../../../core/services/services-requests.service';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

type RequestTab = 'active' | 'history';

const ACTIVE_STATUSES: ServiceRequestStatus[] = ['Open', 'HasOffers', 'OfferAccepted', 'InProgress'];
const HISTORY_STATUSES: ServiceRequestStatus[] = ['Completed', 'Cancelled', 'Expired'];

@Component({
  selector: 'app-show-services-requested',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  templateUrl: './show-services-requested.html',
  styleUrl: './show-services-requested.css',
})
export class ShowServicesRequested implements OnInit {
  private readonly _requestsService = inject(ServicesRequestsService);
  private readonly _translate = inject(TranslateService);

  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly searchTerm = signal('');
  readonly statusFilter = signal<ServiceRequestStatus | 'all'>('all');
  readonly areaFilter = signal<string>('all');
  readonly activeTab = signal<RequestTab>('active');
  readonly requests = signal<ServiceRequestDto[]>([]);
  readonly skeletonPlaceholders = Array.from({ length: 4 });

  readonly uniqueAreas = computed(() => {
    const areas = new Set<string>();
    this.requests().forEach((request) => {
      if (request.area && request.area.trim()) {
        areas.add(request.area.trim());
      }
    });
    return Array.from(areas).sort((a, b) => a.localeCompare(b));
  });

  readonly statusOptions = computed(() => (this.activeTab() === 'active' ? ACTIVE_STATUSES : HISTORY_STATUSES));

  readonly hasActiveFilters = computed(
    () => Boolean(this.searchTerm().trim()) || this.statusFilter() !== 'all' || this.areaFilter() !== 'all',
  );

  readonly statusChipStyles: Record<string, { bg: string; text: string }> = {
    Open: { bg: 'var(--chip-blue)', text: 'var(--chip-blue-text)' },
    HasOffers: { bg: 'var(--chip-purple)', text: 'var(--chip-purple-text)' },
    OfferAccepted: { bg: 'var(--chip-indigo)', text: 'var(--chip-indigo-text)' },
    InProgress: { bg: 'var(--chip-amber)', text: 'var(--chip-amber-text)' },
    Completed: { bg: 'var(--chip-green)', text: 'var(--chip-green-text)' },
    Cancelled: { bg: 'var(--chip-red)', text: 'var(--chip-red-text)' },
    Expired: { bg: 'var(--chip-gray)', text: 'var(--chip-gray-text)' },
  };

  readonly filteredRequests = computed(() => {
    const tab = this.activeTab();
    const search = this.searchTerm().trim().toLowerCase();
    const statusFilter = this.statusFilter();
    const areaFilter = this.areaFilter();

    return this.requests()
      .filter((request) =>
        tab === 'active'
          ? ACTIVE_STATUSES.includes((request.status as ServiceRequestStatus) ?? 'Open')
          : HISTORY_STATUSES.includes((request.status as ServiceRequestStatus) ?? 'Completed'),
      )
      .filter((request) =>
        !search
          ? true
          : request.title.toLowerCase().includes(search) ||
            request.description.toLowerCase().includes(search) ||
            (request.craftName ?? '').toLowerCase().includes(search),
      )
      .filter((request) => (statusFilter === 'all' ? true : request.status === statusFilter))
      .filter((request) =>
        areaFilter === 'all' ? true : (request.area ?? 'Unknown area').toLowerCase() === areaFilter.toLowerCase(),
      )
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  });

  readonly overviewStats = computed(() => {
    const all = this.requests();
    const active = all.filter((r) => ACTIVE_STATUSES.includes(r.status as ServiceRequestStatus)).length;
    const history = all.filter((r) => HISTORY_STATUSES.includes(r.status as ServiceRequestStatus)).length;
    const totalBudget = all.reduce((sum, request) => sum + (request.customerBudget ?? 0), 0);
    return {
      total: all.length,
      active,
      history,
      totalBudget,
    };
  });

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this._requestsService.getCustomerRequests().subscribe({
      next: (response) => {
        this.requests.set(response.data ?? []);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.errorMessage.set(this.extractErrorMessage(error));
        this.isLoading.set(false);
      },
    });
  }

  setTab(tab: RequestTab): void {
    if (this.activeTab() === tab) {
      return;
    }
    this.activeTab.set(tab);
    this.statusFilter.set('all');
  }

  onSearch(value: string): void {
    this.searchTerm.set(value);
  }

  onStatusFilterChange(value: ServiceRequestStatus | 'all'): void {
    this.statusFilter.set(value);
  }

  onAreaFilterChange(value: string): void {
    this.areaFilter.set(value);
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.statusFilter.set('all');
    this.areaFilter.set('all');
  }

  getStatusStyle(status: ServiceRequestStatus | string) {
    return this.statusChipStyles[status] ?? this.statusChipStyles['Open'];
  }

  trackByRequestId(_: number, request: ServiceRequestDto): number {
    return request.serviceRequestId;
  }

  formatIso(date: string): string {
    return new Date(date).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
  }

  shortDescription(value: string, maxLength: number = 160): string {
    if (!value) {
      return '';
    }
    return value.length > maxLength ? `${value.slice(0, maxLength).trim()}…` : value;
  }

  formatBudget(value?: number | null): string {
    if (value === undefined || value === null) {
      return this._translate.instant('ShowRequests.Cards.Meta.BudgetUnknown');
    }
    return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(value);
  }

  getOffersProgress(request: ServiceRequestDto): number {
    if (!request.maxOffers) {
      return 0;
    }
    return Math.min(100, Math.round((request.offersCount / request.maxOffers) * 100));
  }

  getCoverImage(request: ServiceRequestDto): string | null {
    return request.images && request.images.length ? request.images[0] : null;
  }

  getStatusLabel(status: ServiceRequestStatus | string): string {
    const key = this.statusLabelKeys[String(status)];
    return key ? this._translate.instant(key) : String(status);
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return this._translate.instant('ShowRequests.Messages.LoadError');
    }

    if (typeof error === 'string') {
      return error;
    }

    const errorRecord = error as Record<string, unknown> | null;
    if (errorRecord && typeof errorRecord['message'] === 'string') {
      return errorRecord['message'] as string;
    }

    return this._translate.instant('ShowRequests.Messages.GenericError');
  }

  private readonly statusLabelKeys: Record<string, string> = {
    Open: 'ShowRequests.StatusLabels.Open',
    HasOffers: 'ShowRequests.StatusLabels.HasOffers',
    OfferAccepted: 'ShowRequests.StatusLabels.OfferAccepted',
    InProgress: 'ShowRequests.StatusLabels.InProgress',
    Completed: 'ShowRequests.StatusLabels.Completed',
    Cancelled: 'ShowRequests.StatusLabels.Cancelled',
    Expired: 'ShowRequests.StatusLabels.Expired',
  };
}
