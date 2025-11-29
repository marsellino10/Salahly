import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CustomerRegistrationForm } from "../../../components/shared/customer-registration-form/customer-registration-form";
import { CraftmanRegistrationForm } from "../../../components/shared/craftman-registration-form/craftman-registration-form";
import { AuthService, RegisterPayload } from '../../../core/services/auth-service';

@Component({
  selector: 'app-registration',
  imports: [CommonModule, RouterLink, TranslateModule, ReactiveFormsModule, CustomerRegistrationForm, CraftmanRegistrationForm],
  templateUrl: './registration.html',
  styleUrl: './registration.css',
})
export class Registration {
  activeTab: 'customer' | 'craftsman' = 'customer';
  private readonly _formBuilder: FormBuilder = inject(FormBuilder);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  isSubmitting = false;
  errorMessage: string | null = null;

  // Customer Registration Form
  RegistrationForm: FormGroup = this._formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    fullName: ['', [Validators.required]],
    userName: ['', [Validators.required]],
    userType: ['customer'],
  });

  // Craftsman Registration Form
  // craftsmanForm: FormGroup = this._formBuilder.group({
  //   email: ['', [Validators.required, Validators.email]],
  //   password: ['', [Validators.required, Validators.minLength(6)]],
  //   fullName: ['', [Validators.required]],
  //   userName: ['', [Validators.required]],
  //   userType: ['craftsman'],
  // });

  setActiveTab(tab: 'customer' | 'craftsman'): void {
    if (this.isSubmitting) return;
    this.activeTab = tab;
    this.RegistrationForm.get('userType')?.setValue(tab);
    this.errorMessage = null;
  }

  onSubmit(): void {
    const currentForm = this.RegistrationForm;
    console.log(currentForm.value);
    if (currentForm.invalid || this.isSubmitting) {
      currentForm.markAllAsTouched();
      return;
    }

    const payload: RegisterPayload = {
      fullName: currentForm.value.fullName?.trim() ?? '',
      userName: currentForm.value.userName?.trim() ?? '',
      email: currentForm.value.email?.trim() ?? '',
      password: currentForm.value.password ?? '',
    };

    this.isSubmitting = true;
    this.errorMessage = null;

    const request$ =
      this.activeTab === 'customer'
        ? this._authService.registerCustomer(payload)
        : this._authService.registerTechnician(payload);

    request$.subscribe({
      next: () => {
        this.isSubmitting = false;
        currentForm.reset();
        this.activeTab = 'customer';
        this._router.navigate(['/login']);
      },
      error: (error) => {
        //console.log(error);
        this.isSubmitting = false;
        this.errorMessage = error?.error?.errors?.join('\n') || error?.error?.data?.errors?.join('\n') || 'An error occurred';
          // typeof error?.errors[0] === 'string'
          //   ? error.errors[0]
          //   : 'Unable to complete registration right now. Please try again.';

      },
    });
  }
}
