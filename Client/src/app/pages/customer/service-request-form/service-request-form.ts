import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Area } from '../../../core/models/Area';
import { Craft } from '../../../core/models/Craft';
import { AreaService } from '../../../core/services/area-service';
import { CraftService } from '../../../core/services/craft-service';
import {
  CreateServiceRequestPayload,
  ServicesRequestsService,
} from '../../../core/services/services-requests.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-service-request-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslateModule],
  templateUrl: './service-request-form.html',
  styleUrl: './service-request-form.css',
  
})
export class ServiceRequestForm implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _requestsService = inject(ServicesRequestsService);
  private readonly _craftService = inject(CraftService);
  private readonly _areaService = inject(AreaService);
  private readonly _router = inject(Router);
  private readonly _translate = inject(TranslateService);

  readonly minDate = new Date().toISOString().split('T')[0];
  readonly maxImages = 5;

  crafts: Craft[] = [];
  areas: Area[] = [];
  isLoadingCrafts = false;
  isLoadingAreas = false;
  craftsError: string | null = null;
  areasError: string | null = null;

  isSubmitting = false;
  submitError: string | null = null;
  submitSuccess: string | null = null;

  selectedImages: File[] = [];
  imagePreviews: string[] = [];
  readonly paymentMethods = ["Card","Wallet","Cash"]

  requestForm = this._fb.group({
    craftId: [null as number | null, Validators.required],
    title: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(80)]],
    description: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(800)]],
    address: ['', [Validators.required, Validators.maxLength(160)]],
    areaId: [null as number | null, Validators.required],
    availableFromDate: ['', Validators.required],
    availableToDate: ['', Validators.required],
    customerBudget: [null as number | null, [Validators.min(20), Validators.max(100000)]],
    maxOffers: [3, [Validators.min(1), Validators.max(10)]],
    latitude: [null as number | null, [Validators.min(-90), Validators.max(90)]],
    longitude: [null as number | null, [Validators.min(-180), Validators.max(180)]],
    paymentMethod: ['', Validators.required],
  });

  ngOnInit() {
    this.loadBaseData();
  }

  loadBaseData() {
    this.loadCrafts();
    this.loadAreas();
  }

  get descriptionLength(): number {
    return this.requestForm.get('description')?.value?.length ?? 0;
  }

  get titleLength(): number {
    return this.requestForm.get('title')?.value?.length ?? 0;
  }

  fieldInvalid(controlName: string): boolean {
    const control = this.requestForm.get(controlName);
    return Boolean(control && control.invalid && (control.dirty || control.touched));
  }

  submitRequest(): void {
    this.submitError = null;
    this.submitSuccess = null;

    if (this.requestForm.invalid || this.isSubmitting) {
      this.requestForm.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();

    this.isSubmitting = true;

    this._requestsService.createRequest(payload, this.selectedImages).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.submitSuccess = this._translate.instant('ServiceRequestForm.Messages.SubmitSuccess');
        this.submitError = null;
        this.resetForm();
        setTimeout(() => this._router.navigate(['/show-services-requested']), 1500);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.submitError = this.extractErrorMessage(error);
      },
    });
  }

  loadCrafts(): void {
    if (this.crafts.length > 0) return;

  this.isLoadingCrafts = true;

  this._craftService.GetAllCrafts().subscribe({
    next: res => {
      this.crafts = res.data ?? [];
      this.isLoadingCrafts = false;
    },
    error: err => {
      this.isLoadingCrafts = false;
    }
  });
  }

  loadAreas(): void {
    this.isLoadingAreas = true;
    this.areasError = null;

    this._areaService.GetAllAreas().subscribe({
      next: (response) => {
        this.areas = response?.data ?? [];
        this.isLoadingAreas = false;
      },
      error: (error) => {
        this.isLoadingAreas = false;
        this.areasError = this.extractErrorMessage(error);
      },
    });
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);

    if (!files.length) {
      return;
    }

    const remainingSlots = this.maxImages - this.selectedImages.length;
    const acceptedFiles = files.slice(0, remainingSlots).filter((file) => file.type.startsWith('image/'));

    acceptedFiles.forEach((file) => {
      this.selectedImages.push(file);
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviews.push(reader.result as string);
      };
      reader.readAsDataURL(file);
    });

    input.value = '';
  }

  removeImage(index: number): void {
    this.selectedImages.splice(index, 1);
    this.imagePreviews.splice(index, 1);
  }

  clearUploads(): void {
    this.selectedImages = [];
    this.imagePreviews = [];
  }

  navigateBack(): void {
    void this._router.navigate(['/show-services-requested']);
  }

  private resetForm(): void {
    this.requestForm.reset({
      craftId: null,
      title: '',
      description: '',
      address: '',
      areaId: null,
      availableFromDate: '',
      availableToDate: '',
      customerBudget: null,
      maxOffers: 3,
      latitude: null,
      longitude: null,
    });
    this.requestForm.markAsPristine();
    this.clearUploads();
  }

  private buildPayload(): CreateServiceRequestPayload {
    const raw = this.requestForm.value;

    return {
      craftId: Number(raw.craftId),
      title: raw.title?.trim() ?? '',
      description: raw.description?.trim() ?? '',
      address: raw.address?.trim() ?? '',
      areaId: Number(raw.areaId),
      availableFromDate: raw.availableFromDate ?? new Date().toISOString(),
      availableToDate: raw.availableToDate ?? new Date().toISOString(),
      customerBudget:
        raw.customerBudget !== null && raw.customerBudget !== undefined
          ? Number(raw.customerBudget)
          : null,
      latitude:
        raw.latitude !== null && raw.latitude !== undefined ? Number(raw.latitude) : null,
      longitude:
        raw.longitude !== null && raw.longitude !== undefined ? Number(raw.longitude) : null,
      maxOffers: raw.maxOffers ? Number(raw.maxOffers) : undefined,
      paymentMethod: raw.paymentMethod
    } as CreateServiceRequestPayload;
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return this._translate.instant('ServiceRequestForm.Messages.GenericError');
    }

    if (typeof error === 'string') {
      return error;
    }

    const errorRecord = error as Record<string, unknown> | null;
    if (errorRecord?.['message'] && typeof errorRecord['message'] === 'string') {
      return errorRecord['message'] as string;
    }

    const apiError = errorRecord?.['error'] as Record<string, unknown> | string | undefined;
    if (typeof apiError === 'string') {
      return apiError;
    }

    if (apiError && typeof apiError['message'] === 'string') {
      return apiError['message'] as string;
    }

    return this._translate.instant('ServiceRequestForm.Messages.ActionError');
  }
}