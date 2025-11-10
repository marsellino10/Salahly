import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, FormGroup } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule,RouterLink,TranslateModule,ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  activeTab: 'customer' | 'craftsman' | 'admin' = 'customer';
  private readonly _FormBuilder = inject(FormBuilder);
  loginForm: FormGroup = this._FormBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  handleLogin(role: string) {
    if (this.loginForm.invalid) return;

    const { email, password } = this.loginForm.value;
    console.log('Logging in as:', role, { email, password });

    // TODO: Replace with actual authentication logic
  }
  onSubmit() {
    this.handleLogin(this.activeTab);
  }
}
