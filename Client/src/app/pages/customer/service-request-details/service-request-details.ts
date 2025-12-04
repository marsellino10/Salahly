import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CustomerOffersService, OfferDto, OfferStatus } from '../../../core/services/customer-offers.service';
import {
  ServiceRequestDto,
  ServiceRequestStatus,
  ServicesRequestsService,
  UpdateServiceRequestPayload,
} from '../../../core/services/services-requests.service';
import { AuthService } from '../../../core/services/auth-service';
import { CraftsmanServiceRequestService } from '../../../core/services/craftsman-service-request.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CarouselModule, OwlOptions } from 'ngx-owl-carousel-o';

type TimelineStep = {
  key: ServiceRequestStatus | 'HasOffers';
  labelKey: string;
};

@Component({
  selector: 'app-service-request-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, TranslateModule, CarouselModule],
  templateUrl: './service-request-details.html',
  styleUrl: './service-request-details.css',
})
export class ServiceRequestDetails implements OnInit {
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _requestsService = inject(ServicesRequestsService);
  private readonly _offersService = inject(CustomerOffersService);
  private readonly _translate = inject(TranslateService);
  private readonly _fb = inject(FormBuilder);
  private readonly _authService = inject(AuthService);
  private readonly _httpClient = inject(HttpClient);
  private readonly _craftsmanRequestService = inject(CraftsmanServiceRequestService);

  private requestId: number | null = null;
  readonly OfferStatusEnum = OfferStatus;

  readonly request = signal<ServiceRequestDto | null>(null);
  readonly requestImages = computed(() => {
    const images = this.request()?.images ?? [];
    return images.filter((image): image is string => Boolean(image && image.trim()));
  });
  readonly offers = signal<OfferDto[]>([]);
  readonly isRequestLoading = signal(true);
  readonly isOffersLoading = signal(true);
  readonly requestError = signal<string | null>(null);
  readonly offersError = signal<string | null>(null);
  readonly actionBanner = signal<{ type: 'success' | 'error'; message: string } | null>(null);
  readonly offerActionLoadingId = signal<number | null>(null);
  readonly activeRejectOfferId = signal<number | null>(null);
  rejectReason: string = '';
  readonly editModalOpen = signal(false);
  readonly deleteModalOpen = signal(false);
  readonly editError = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isDeletingRequest = signal(false);
  userType: string = '';
  isCustomer = false;
  isTechnician = false;
  canCompleteRequest = false;
  showAddress = false;

  readonly editForm = this._fb.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(80)]],
    description: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(800)]],
    availableFromDate: ['', Validators.required],
    availableToDate: ['', Validators.required],
    address: ['', [Validators.required, Validators.maxLength(160)]],
    customerBudget: [null as number | null, [Validators.min(0)]],
  });

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
        value: this.showAddress && current.address
          ? current.address
          : `${current.city || this._translate.instant('ServiceRequestDetails.Meta.CityFallback')}, ${
              current.area || this._translate.instant('ServiceRequestDetails.Meta.AreaFallback')
            }`,
      },
      {
        icon: 'bi-calendar-event',
        labelKey: 'ServiceRequestDetails.Meta.PreferredDate',
        value: `${this.formatDate(current.availableFromDate)} - ${this.formatDate(current.availableToDate)}`,
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
    this._route.params.subscribe((params) => {
      const idParam = Number(params['id']);
      if (!idParam) {
      this.requestError.set(this._translate.instant('ServiceRequestDetails.Messages.MissingRequest'));
      this.isRequestLoading.set(false);
      this.isOffersLoading.set(false);
      return;
    }

    this.requestId = idParam;
    this.loadRequest();
    });

  }

  loadRequest(): void {
    if (!this.requestId) {
      return;
    }

    this.isRequestLoading.set(true);
    this.requestError.set(null);
    this.userType = this._authService.getUserType()?.toLowerCase() ?? '';
    this.isCustomer = this.userType === 'customer';
    this.isTechnician = this.userType === 'technician' || this.userType === 'craftsman';

    if (this.isCustomer) {
      this.loadCustomerRequest();
    } else if (this.isTechnician) {
      this.loadTechnicianRequest();
    } else {
      this.loadCustomerRequest();
    }
  }

  loadOffers(): void {
    if (!this.requestId || !this.isCustomer) {
      this.offers.set([]);
      this.isOffersLoading.set(false);
      this.offersError.set(null);
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
        console.log(error);
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
      next: (response: any) => {
        window.open(response.data.paymentLink, '_blank');
        this.setActionBanner('success', response.message ?? this._translate.instant('ServiceRequestDetails.Messages.AcceptSuccess'));
        this.resetOfferActionState();
      },
      error: (error) => {
        this.setActionBanner('error', this.extractErrorMessage(error));
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
        this.setActionBanner('success', response.message ?? this._translate.instant('ServiceRequestDetails.Messages.RejectSuccess'));
        this.resetOfferActionState();
      },
      error: (error) => {
        this.setActionBanner('error', this.extractErrorMessage(error));
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

  completeRequest(): void {
    const current = this.request();
    if (!current?.serviceRequestId) {
      return;
    }

    this._httpClient.post(`/api/customer/servicerequests/${current.serviceRequestId}/complete`, {}).subscribe({
      next: () => {
        this.setActionBanner('success', 'Service request completed successfully!');
        this.loadRequest();
      },
      error: (error) => {
        this.setActionBanner('error', this.extractErrorMessage(error));
      },
    });
  }

  openEditModal(): void {
    const current = this.request();
    if (!current) {
      return;
    }

    this.populateEditForm(current);
    this.editError.set(null);
    this.editModalOpen.set(true);
  }

  closeEditModal(): void {
    this.editModalOpen.set(false);
    this.editForm.reset();
    this.editError.set(null);
    this.isSaving.set(false);
  }

  submitEdit(): void {
    if (!this.requestId) {
      return;
    }

    if (this.editForm.invalid || this.isSaving()) {
      this.editForm.markAllAsTouched();
      return;
    }

    const payload = this.buildUpdatePayload();
    this.isSaving.set(true);
    this._requestsService.updateRequest(this.requestId, payload).subscribe({
      next: (response) => {
        const message = response?.message ?? this._translate.instant('ServiceRequestDetails.Edit.Messages.Success');
        this.setActionBanner('success', message);
        this.isSaving.set(false);
        this.closeEditModal();
        this.loadRequest();
      },
      error: (error) => {
        this.editError.set(this.extractErrorMessage(error));
        this.isSaving.set(false);
      },
    });
  }

  promptDelete(): void {
    if (!this.request()) {
      return;
    }
    this.deleteModalOpen.set(true);
  }

  cancelDelete(): void {
    this.deleteModalOpen.set(false);
    this.isDeletingRequest.set(false);
  }

  confirmDelete(): void {
    if (!this.requestId || this.isDeletingRequest()) {
      return;
    }

    this.isDeletingRequest.set(true);
    this._requestsService.deleteRequest(this.requestId).subscribe({
      next: () => {
        this.setActionBanner('success', this._translate.instant('ServiceRequestDetails.Delete.Messages.Success'));
        this.isDeletingRequest.set(false);
        this.deleteModalOpen.set(false);
        void this._router.navigate(['/show-services-requested']);
      },
      error: (error) => {
        this.setActionBanner('error', this.extractErrorMessage(error));
        this.isDeletingRequest.set(false);
        this.deleteModalOpen.set(false);
      },
    });
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

  readonly heroCarouselOptions: OwlOptions = {
    items: 1,
    loop: true,
    dots: false,
    nav: false,
    autoplay: true,
    autoplayHoverPause: true,
    responsive: {
      0: { items: 1 },
    },
  };

  private resetOfferActionState(): void {
    this.offerActionLoadingId.set(null);
    this.activeRejectOfferId.set(null);
    this.rejectReason = '';
  }

  private loadCustomerRequest(): void {
    if (!this.requestId) {
      return;
    }

    this._requestsService.getRequestById(this.requestId).subscribe({
      next: (response) => {
        this.request.set(response.data ?? null);
        this.isRequestLoading.set(false);
        const current = this.request();
        if (current) {
          const status = current.status;
          const normalizedStatus = typeof status === 'string' ? status : undefined;
          const numericStatus = typeof status === 'number' ? status : undefined;
          this.canCompleteRequest = normalizedStatus === 'OfferAccepted' || numericStatus === 2;
          this.showAddress = true;
          if (this.editModalOpen()) {
            this.populateEditForm(current);
          }
          this.loadOffers();
        }
      },
      error: (error) => {
        this.requestError.set(this.extractErrorMessage(error));
        this.isRequestLoading.set(false);
      },
    });
  }

  private loadTechnicianRequest(): void {
    if (!this.requestId) {
      return;
    }

    this._craftsmanRequestService.getServiceRequestById(this.requestId).subscribe({
      next: (response) => {
        this.request.set(response.data ?? null);
        this.isRequestLoading.set(false);
        this.canCompleteRequest = false;
        this.checkTechnicianOfferStatus();
      },
      error: (error) => {
        this.requestError.set(this.extractErrorMessage(error));
        this.isRequestLoading.set(false);
      },
    });
  }

  private checkTechnicianOfferStatus(): void {
    const current = this.request();
    if (!current?.serviceRequestId) {
      this.showAddress = false;
      return;
    }

    this._httpClient.get<any>('/api/craftsman/service-requests/offers').subscribe({
      next: (response) => {
        const payload = response?.data ?? response ?? [];
        const offers = Array.isArray(payload) ? payload : [];
        const acceptedOffer = offers.find(
          (offer: any) =>
            offer?.serviceRequestId === current.serviceRequestId &&
            (offer?.status === 'OfferAccepted' || offer?.status === 2 || offer?.status === 'Accepted'),
        );

        this.showAddress = !!acceptedOffer;
      },
      error: () => {
        this.showAddress = false;
      },
    });
  }

  private setActionBanner(type: 'success' | 'error', message: string): void {
    this.actionBanner.set({ type, message });
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

  trackImage(_: number, image: string): string {
    return image;
  }

  private populateEditForm(request: ServiceRequestDto): void {
    this.editForm.reset({
      title: request.title,
      description: request.description,
      availableFromDate: this.toDateInputValue(request.availableFromDate),
      availableToDate: this.toDateInputValue(request.availableToDate),
      address: request.address,
      customerBudget: request.customerBudget ?? null,
    });
  }

  private buildUpdatePayload(): UpdateServiceRequestPayload {
    const raw = this.editForm.value;
    const payload: UpdateServiceRequestPayload = {
      title: raw.title?.trim(),
      description: raw.description?.trim(),
      address: raw.address?.trim(),
      availableFromDate: raw.availableFromDate ? new Date(raw.availableFromDate).toISOString() : undefined,
      availableToDate: raw.availableToDate ? new Date(raw.availableToDate).toISOString() : undefined,
    };

    if (raw.customerBudget !== null && raw.customerBudget !== undefined) {
      payload.customerBudget = Number(raw.customerBudget);
    } else {
      payload.customerBudget = null;
    }

    return payload;
  }

  private toDateInputValue(value: string | null | undefined): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (isNaN(date.getTime())) {
      return '';
    }

    return date.toISOString().split('T')[0];
  }
}
