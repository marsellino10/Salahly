import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService, LoginPayload, LoginResponse } from '../../../core/services/auth-service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, RouterLink, TranslateModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly _formBuilder = inject(FormBuilder);
  private readonly _authService = inject(AuthService);
  private readonly _router = inject(Router);

  loginForm: FormGroup = this._formBuilder.group({
    userName: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  isSubmitting = false;
  errorMessage: string | null = null;

  onSubmit(): void {
    if (this.loginForm.invalid || this.isSubmitting) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const payload: LoginPayload = {
      userName: this.loginForm.value.userName?.trim() ?? '',
      password: this.loginForm.value.password ?? '',
    };

    this.isSubmitting = true;
    this.errorMessage = null;

    this._authService.login(payload).subscribe({
      next: (result: LoginResponse) => {
        this.isSubmitting = false;
        this.loginForm.reset();
        console.log(result.data.userType.toLowerCase());
        const isTechnician = result.data.userType.toLowerCase() == 'craftsman';
        const isCustomer = result.data.userType.toLowerCase() == 'customer';
        const isAdmin = result.data.userType.toLowerCase() == 'admin';
        const isProfileCompleted = result.data.isProfileCompleted ?? true;
        console.log(isTechnician,isCustomer,isAdmin,isProfileCompleted);
        if (isTechnician && !isProfileCompleted) {
          this._router.navigate(['/complete-profile']);
          return;
        }else if(isAdmin){
          this._router.navigate(['/dashboard']);
          return;
        }
        else if(isCustomer && !isProfileCompleted){
          this._router.navigate(['/customer-profile']);
          return;
        }
        else if(isCustomer || isTechnician){
          this._router.navigate(['/home']);
        }
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage =
          typeof error?.error.message === 'string'
            ? error.error.message
            : 'Unable to sign you in right now. Please try again.';
      },
    });
  }
}
