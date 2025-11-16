import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-craftman-registration-form',
  imports: [TranslateModule,ReactiveFormsModule,CommonModule],
  templateUrl: './craftman-registration-form.html',
  styleUrl: './craftman-registration-form.css',
})
export class CraftmanRegistrationForm {
 @Input() craftsmanForm!: FormGroup;
 @Input() isSubmitting = false;
 @Output() onSubmit = new EventEmitter<void>();

 onSubmitForm() {
  this.onSubmit.emit();
 }
}
