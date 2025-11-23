import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { ApiResponse, CreateCustomerPayload, CustomerResponse, CustomerService, CustomerUpdatePayload } from '../../../core/services/customer-service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

interface SubmissionMessage {
  type: 'success' | 'error';
  text: string;
}

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './customer-profile.html',
  styleUrl: './customer-profile.css',
})
export class CustomerProfile implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _customerService = inject(CustomerService);
  private readonly _router = inject(Router);
  private readonly _translate = inject(TranslateService);

  customerId: number | null = null;
  hasExistingProfile = false;
  isLoadingProfile = true;
  isSubmitting = false;
  submissionMessage: SubmissionMessage | null = null;

  existingProfileImageUrl: string | null = null;
  profileImagePreview: string | null = null;
  selectedProfileImage: File | null = null;

  profileForm: FormGroup = this._fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    address: ['', [Validators.required, Validators.minLength(5)]],
    city: ['', [Validators.required, Validators.minLength(2)]],
    area: ['', [Validators.required, Validators.minLength(2)]],
    phoneNumber: [
      '',
      [
        Validators.required,
        Validators.pattern(/^[0-9+()\-\s]{7,20}$/),
      ],
    ],
    dateOfBirth: [''],
  });

  readonly sectionDescriptors = [
    {
      labelKey: 'CustomerProfile.Form.ProfileBasicsTitle',
      validator: () => this.profileForm.get('fullName')?.valid || this.profileForm.get('dateOfBirth')?.valid,
    },
    {
      labelKey: 'CustomerProfile.Form.ContactDetailsTitle',
      validator: () =>
        this.profileForm.get('phoneNumber')?.valid &&
        this.profileForm.get('address')?.valid,
    },
    {
      labelKey: 'CustomerProfile.Form.LocationTitle',
      validator: () =>
        this.profileForm.get('city')?.valid && this.profileForm.get('area')?.valid,
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

  ngOnInit(): void {
    const claims = this._customerService.getCustomerTokenClaims();
    this.customerId = claims?.nameIdentifier ? Number(claims.nameIdentifier) : null;
    if (claims?.fullName?.trim()) {
      this.profileForm.patchValue({ fullName: claims.fullName.trim() });
    }

    if (!this.customerId) {
      this.isLoadingProfile = false;
      return;
    }

    this.loadCustomerProfile();
  }

  getControl(controlName: string) {
    return this.profileForm.get(controlName);
  }

  isControlInvalid(controlName: string): boolean {
    const control = this.getControl(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  onImageSelected(event: Event): void {
    const file = (event.target as HTMLInputElement)?.files?.[0];
    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.submissionMessage = {
        type: 'error',
        text: this._translate.instant('CustomerProfile.Messages.ImageTypeError'),
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

  onSubmit(): void {
    this.submissionMessage = null;

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const updatePayload = this.buildUpdatePayload();
    let request$: Observable<CustomerResponse | ApiResponse<CustomerResponse>>;

    if (this.customerId && this.hasExistingProfile) {
      request$ = this._customerService.updateCustomer(
        this.customerId,
        updatePayload,
        this.selectedProfileImage,
      );
    } else {
      const createPayload = this.buildCreatePayload(updatePayload);
      request$ = this._customerService.createCustomer(createPayload, this.selectedProfileImage);
    }

    request$.subscribe({
      next: (response: CustomerResponse | ApiResponse<CustomerResponse>) => {
        this.isSubmitting = false;
        const customer = this.extractCustomerResponse(response);
        if (customer) {
          this.handleSuccessfulSave(customer);
        }
      },
      error: (error: unknown) => {
        this.isSubmitting = false;
        this.submissionMessage = {
          type: 'error',
          text: this.getErrorMessage(error),
        };
      },
    });
  }

  onSkip(): void {
    this._router.navigate(['/home']);
  }

  clearSubmissionMessage(): void {
    this.submissionMessage = null;
  }

  private loadCustomerProfile(): void {
    if (!this.customerId) {
      this.isLoadingProfile = false;
      return;
    }

    this.isLoadingProfile = true;
    this._customerService.getCustomerById(this.customerId).subscribe({
      next: (profile) => {
        if (profile) {
          this.hasExistingProfile = true;
          this.patchForm(profile);
        }
        this.isLoadingProfile = false;
      },
      error: () => {
        this.isLoadingProfile = false;
      },
    });
  }

  private patchForm(profile: CustomerResponse): void {
    const formattedDate = profile.dateOfBirth ? this.formatDateForInput(profile.dateOfBirth) : null;
    this.profileForm.patchValue({
      fullName: profile.fullName,
      address: profile.address ?? '',
      city: profile.city ?? '',
      area: profile.area ?? '',
      phoneNumber: profile.phoneNumber ?? '',
      dateOfBirth: formattedDate ?? '',
    });

    this.existingProfileImageUrl = profile.profileImageUrl ?? null;
    this.profileImagePreview = profile.profileImageUrl ?? null;
    this.profileForm.get('fullName')?.disable();
  }

  private buildUpdatePayload(): CustomerUpdatePayload {
    const formValue = this.profileForm.getRawValue();
    return {
      address: formValue.address?.trim() || null,
      city: formValue.city?.trim() || null,
      area: formValue.area?.trim() || null,
      phoneNumber: formValue.phoneNumber?.trim() || null,
      dateOfBirth: formValue.dateOfBirth ? new Date(formValue.dateOfBirth) : null,
    };
  }

  private buildCreatePayload(updatePayload: CustomerUpdatePayload): CreateCustomerPayload {
    const rawValue = this.profileForm.getRawValue();
    return {
      fullName: rawValue.fullName?.trim() ?? '',
      ...updatePayload,
    };
  }

  private extractCustomerResponse(
    response: CustomerResponse | ApiResponse<CustomerResponse>,
  ): CustomerResponse | null {
    if (!response) {
      return null;
    }

    if ('data' in response) {
      return response.data ?? null;
    }
    return response;
  }

  private handleSuccessfulSave(customer: CustomerResponse): void {
    this.hasExistingProfile = true;
    this.profileForm.get('fullName')?.disable();
    this.customerId = customer.id;
    this.existingProfileImageUrl = customer.profileImageUrl ?? this.existingProfileImageUrl;
    this.profileImagePreview = this.existingProfileImageUrl;

    this.submissionMessage = {
      type: 'success',
      text: this._translate.instant('CustomerProfile.Messages.SubmitSuccess'),
    };

    setTimeout(() => this._router.navigate(['/home']), 1500);
  }

  private formatDateForInput(value: string): string {
    const parsed = new Date(value);
    if (isNaN(parsed.getTime())) {
      return value;
    }
    const year = parsed.getFullYear();
    const month = String(parsed.getMonth() + 1).padStart(2, '0');
    const day = String(parsed.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private getErrorMessage(error: unknown): string {
    if (typeof error === 'string') {
      return error;
    }

    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string') {
        return error.error;
      }

      if (error.error && typeof error.error === 'object' && 'message' in error.error) {
        const message = (error.error as { message?: unknown }).message;
        if (typeof message === 'string') {
          return message;
        }
      }

      return error.message;
    }

    if (error && typeof error === 'object' && 'message' in error) {
      const message = (error as { message?: unknown }).message;
      if (typeof message === 'string') {
        return message;
      }
    }

    return this._translate.instant('CustomerProfile.Messages.GenericError');
  }
}
