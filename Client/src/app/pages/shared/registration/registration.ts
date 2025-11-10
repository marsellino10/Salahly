import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { inject } from '@angular/core';

@Component({
  selector: 'app-registration',
  imports: [CommonModule, FormsModule,RouterLink,TranslateModule,ReactiveFormsModule],
  templateUrl: './registration.html',
  styleUrl: './registration.css',
})
export class Registration {
  activeTab: 'customer' | 'craftsman' = 'customer';
  private readonly _FormBuilder: FormBuilder = inject(FormBuilder);
  
  // Customer Registration Form
  customerForm: FormGroup = this._FormBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    fullName: ['', [Validators.required]],
    profileImageUrl: [''],
    userType: ['customer'],
    address: ['', [Validators.required]],
    city: ['', [Validators.required]],
    area: ['', [Validators.required]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^(010|011|012|015)[0-9]{8}$/)]],
    dateOfBirth: ['', [Validators.required]],
  });

  // Craftsman Registration Form
  craftsmanForm: FormGroup = this._FormBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    fullName: ['', [Validators.required]],
    profileImageUrl: [''],
    userType: ['craftsman'],
    bio: ['', [Validators.required]],
    yearsOfExperience: ['', [Validators.required, Validators.min(0)]],
    serveAreas: this._FormBuilder.array([
      this._FormBuilder.group({
        city: ['', [Validators.required]],
        area: ['', [Validators.required]]
      })
    ])
  });

  get serveAreas() {
    return this.craftsmanForm.get('serveAreas') as any;
  }

  addServeArea() {
    this.serveAreas.push(
      this._FormBuilder.group({
        city: ['', [Validators.required]],
        area: ['', [Validators.required]]
      })
    );
  }

  removeServeArea(index: number) {
    if (this.serveAreas.length > 1) {
      this.serveAreas.removeAt(index);
    }
  }

  onSubmit() {
    const currentForm = this.activeTab === 'customer' ? this.customerForm : this.craftsmanForm;
    
    if (currentForm.invalid) {
      currentForm.markAllAsTouched();
      return;
    }

    const formData = currentForm.value;
    console.log(`Registering ${this.activeTab}:`, formData);

    // TODO: Replace with actual registration logic
    // For customer: send customerForm.value to API
    // For craftsman: send craftsmanForm.value to API (will be reviewed before activation)
  }
}
