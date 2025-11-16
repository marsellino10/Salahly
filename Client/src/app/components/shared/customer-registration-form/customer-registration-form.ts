import { Component, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { EventEmitter } from '@angular/core';

@Component({
  selector: 'app-customer-registration-form',
  imports: [TranslateModule,ReactiveFormsModule],
  templateUrl: './customer-registration-form.html',
  styleUrl: './customer-registration-form.css',
})
export class CustomerRegistrationForm {
 @Input() customerForm!: FormGroup;
 @Input() isSubmitting = false;
 @Output() onSubmit = new EventEmitter<void>();

 onSubmitForm() {
  this.onSubmit.emit();
 }
}
