import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Area } from '../../../core/models/Area';
import { Craft } from '../../../core/models/Craft';
import { Craftsman } from '../../../core/models/Craftman';
import { AreaService } from '../../../core/services/area-service';
import { CraftService } from '../../../core/services/craft-service';
import {
  CreateTechnicianPayload,
  TechnicianService,
  TechnicianServiceAreaPayload,
  UpdateTechnicianPayload,
} from '../../../core/services/technician-service';

interface SubmissionMessage {
  type: 'success' | 'error';
  text: string;
}

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './complete-profile.html',
  styleUrls: ['./complete-profile.css'],
})
export class CompleteProfile implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _technicianService = inject(TechnicianService);
  private readonly _craftService = inject(CraftService);
  private readonly _areaService = inject(AreaService);
  private readonly _router = inject(Router);
  readonly _translate = inject(TranslateService);

  crafts: Craft[] = [];
  areas: Area[] = [];

  technicianId: number | null = null;
  hasExistingProfile = false;
  isLoadingProfile = true;
  isSubmitting = false;
  submissionMessage: SubmissionMessage | null = null;

  readonly maxServiceAreas = 3;

  profileForm: FormGroup = this._fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    craftId: [null, Validators.required],
    hourlyRate: [null, [Validators.min(20), Validators.max(5000)]],
    yearsOfExperience: [null, [Validators.min(0), Validators.max(60)]],
    bio: ['', [Validators.maxLength(600)]],
    serviceAreas: this._fb.array([], Validators.minLength(1)),
  });
  groupedAreas: { region: string; areas: Area[] }[] = [];

  existingProfileImageUrl: string | null = null;
  profileImagePreview: string | null = null;
  selectedProfileImage: File | null = null;

  readonly sectionDescriptors = [
    {
      labelKey: 'CompleteProfile.Form.ProfileBasicsTitle',
      validator: () =>
        this.profileForm.get('fullName')?.valid && this.profileForm.get('craftId')?.valid,
    },
    {
      labelKey: 'CompleteProfile.Form.ExperienceStoryTitle',
      // treat 0 as valid for yearsOfExperience; check for null/undefined instead of truthiness
      validator: () =>
        this.profileForm.get('hourlyRate')?.value !== null &&
        this.profileForm.get('hourlyRate')?.value !== undefined &&
        this.profileForm.get('yearsOfExperience')?.value !== null &&
        this.profileForm.get('yearsOfExperience')?.value !== undefined &&
        !!this.profileForm.get('bio')?.value,
    },
    {
      labelKey: 'CompleteProfile.Form.ServiceCoverageTitle',
      validator: () => this.serviceAreasControls.length > 0 && this.serviceAreasControls.valid,
    },
  ];

  get progressSteps() {
    return this.sectionDescriptors.map((section) => ({
      labelKey: section.labelKey,
      completed: Boolean(section.validator()),
    }));
  }

  get progressValue(): number {
    const completed = this.progressSteps.filter((step) => step.completed).length;
    return Math.round((completed / this.progressSteps.length) * 100);
  }

  get serviceAreasControls(): FormArray {
    return this.profileForm.get('serviceAreas') as FormArray;
  }

  get canAddMoreAreas(): boolean {
    return this.serviceAreasControls.length < this.maxServiceAreas;
  }

  get serviceAreasTouched(): boolean {
    return (
      this.serviceAreasControls.invalid &&
      (this.serviceAreasControls.dirty || this.serviceAreasControls.touched)
    );
  }

  ngOnInit(): void {
    const claims = this._technicianService.getTechnicianTokenClaims();
    this.technicianId = claims?.nameIdentifier ? Number(claims.nameIdentifier) : null;
    if (claims?.fullName?.trim()) {
      this.profileForm.patchValue({ fullName: claims.fullName.trim() });
    }

    this.loadCrafts();
    this.loadAreas();
    this.loadTechnicianProfile();
  }

  addServiceArea(areaId: number | null = null, radius: number | null = 10): void {
    if (!this.canAddMoreAreas) return;

    this.serviceAreasControls.push(
      this._fb.group({
        areaId: [areaId, Validators.required],
        serviceRadiusKm: [radius, [Validators.required, Validators.min(5), Validators.max(80)]],
      }),
    );
    // console.log for dev
    // console.log(this.profileForm.value);
  }

  removeServiceArea(index: number): void {
    if (this.serviceAreasControls.length > 1) {
      this.serviceAreasControls.removeAt(index);
    } else {
      // if they try to remove the last one, clear value instead to keep one control present
      this.serviceAreasControls.removeAt(index);
      // maintain at least one empty control
      if (!this.serviceAreasControls.length) {
        this.addServiceArea();
      }
    }
  }

  isAreaSelected(areaId: number, currentIndex: number): boolean {
    return this.serviceAreasControls.controls.some((control, index) => {
      if (index === currentIndex) return false;
      return control.value?.areaId === areaId;
    });
  }
  displayAreaLabel(areaId: number | null): string {
    if (areaId === null || areaId === undefined) {
      return this._translate.instant('CompleteProfile.ServiceAreas.SelectServiceAreaLabel');
    }
    const area = this.areas.find((a) => a.id === areaId);
    if (area) {
      return `${area.region} • ${area.city}`;
    }
    return this._translate.instant('CompleteProfile.ServiceAreas.SelectedAreaFallback');
  }

  onImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement)?.files?.[0];
    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.submissionMessage = {
        type: 'error',
        text: this._translate.instant('CompleteProfile.Messages.ImageTypeError'),
      };
      return;
    }

    this.selectedProfileImage = file;
    const reader = new FileReader();
    reader.onload = () => {
      this.profileImagePreview = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  clearSubmissionMessage(): void {
    this.submissionMessage = null;
  }

  onSkip(): void {
    this._router.navigate(['/home']);
  }

  onSubmit(): void {
    this.clearSubmissionMessage();

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      this.serviceAreasControls.markAsTouched();
      this.serviceAreasControls.controls.forEach((group) => group.markAllAsTouched());
      return;
    }

    const payload = this.buildPayload();
    let request$:
      | ReturnType<TechnicianService['createTechnician']>
      | ReturnType<TechnicianService['updateTechnician']>;

    if (this.technicianId && this.hasExistingProfile) {
      const updatePayload: UpdateTechnicianPayload = { ...payload, id: this.technicianId };
      request$ = this._technicianService.updateTechnician(updatePayload, this.selectedProfileImage);
    } else {
      request$ = this._technicianService.createTechnician(payload, this.selectedProfileImage);
    }

    this.isSubmitting = true;

    request$.subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.hasExistingProfile = true;
        this.submissionMessage = {
          type: 'success',
          text: this._translate.instant('CompleteProfile.Messages.SubmitSuccess'),
        };
        this.existingProfileImageUrl = response?.data?.profileImageUrl ?? this.existingProfileImageUrl;
        if (!this.technicianId && response?.data?.id) {
          this.technicianId = response.data.id;
        }
        setTimeout(() => this._router.navigate(['/home']), 1500);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.submissionMessage = {
          type: 'error',
          text:
            typeof error?.error === 'string'
              ? error.error
              : this._translate.instant('CompleteProfile.Messages.SubmitError'),
        };
      },
    });
  }

  private loadCrafts(): void {
    this._craftService.GetAllCrafts().subscribe({
      next: (response) => {
        this.crafts = response?.data ?? [];
      },
    });
  }

  private loadAreas(): void {
    this._areaService.GetAllAreas().subscribe({
      next: (response) => {
        this.areas = response?.data ?? [];
        this.rebuildGroupedAreas();
        if (!this.serviceAreasControls.length) {
          this.addServiceArea();
        }
      },
    });
  }

  private rebuildGroupedAreas(): void {
    const grouped = this.areas.reduce((acc, area) => {
      if (!acc[area.region]) {
        acc[area.region] = [];
      }
      acc[area.region].push(area);
      return acc;
    }, {} as Record<string, Area[]>);

    this.groupedAreas = Object.keys(grouped)
      .sort((a, b) => a.localeCompare(b))
      .map((region) => ({ region, areas: grouped[region] }));
  }

  private loadTechnicianProfile(): void {
    if (!this.technicianId) {
      this.isLoadingProfile = false;
      if (!this.serviceAreasControls.length) {
        this.addServiceArea();
      }
      return;
    }

    this.isLoadingProfile = true;
    this._technicianService.getTechnicianById(this.technicianId).subscribe({
      next: (response) => {
        const profile = response?.data;
        if (profile) {
          this.hasExistingProfile = true;
          this.patchForm(profile);
        }
        if (!this.serviceAreasControls.length) {
          this.addServiceArea();
        }
        this.isLoadingProfile = false;
      },
      error: () => {
        this.isLoadingProfile = false;
        if (!this.serviceAreasControls.length) {
          this.addServiceArea();
        }
      },
    });
  }

  private patchForm(profile: Craftsman): void {
    this.profileForm.patchValue({
      fullName: profile.fullName,
      craftId: profile.craftId,
      hourlyRate: profile.hourlyRate ?? null,
      yearsOfExperience: profile.yearsOfExperience ?? null,
      bio: profile.bio ?? '',
    });

    this.existingProfileImageUrl = profile.profileImageUrl ?? null;

    this.serviceAreasControls.clear();
    profile.serviceAreas?.forEach((area) =>
      this.addServiceArea(area.areaId, area.serviceRadiusKm ?? 10),
    );
  }

  private buildPayload(): CreateTechnicianPayload {
    const formValue = this.profileForm.value;
    const serviceAreas = (formValue.serviceAreas ?? []) as Array<{
      areaId: number | null;
      serviceRadiusKm?: number | null;
    }>;

    const serviceAreasPayload: TechnicianServiceAreaPayload[] = serviceAreas
      .filter((area) => area?.areaId !== null && area?.areaId !== undefined)
      .map((area) => ({
        areaId: Number(area.areaId),
        serviceRadiusKm: area?.serviceRadiusKm ? Number(area.serviceRadiusKm) : undefined,
      }));

    return {
      fullName: formValue.fullName?.trim() ?? '',
      craftId: Number(formValue.craftId),
      hourlyRate:
        formValue.hourlyRate !== null && formValue.hourlyRate !== undefined
          ? Number(formValue.hourlyRate)
          : null,
      bio: formValue.bio?.trim() ? formValue.bio.trim() : null,
      yearsOfExperience:
        formValue.yearsOfExperience !== null && formValue.yearsOfExperience !== undefined
          ? Number(formValue.yearsOfExperience)
          : null,
      serviceAreas: serviceAreasPayload,
    };
  }
}
