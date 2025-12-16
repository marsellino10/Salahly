import { Component, Input, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Craftsman } from '../../../core/models/Craftman';

@Component({
  selector: 'app-technician-card',
  imports: [],
  templateUrl: './technician-card.html',
  styleUrl: './technician-card.css',
})
export class TechnicianCard {
  @Input() craftsman!: Craftsman;

  private readonly _router = inject(Router);


  getInitials(fullName: string): string {
    console.log(this.craftsman);
    return fullName.split(' ').map(name => name.charAt(0)).join('').toUpperCase();
  }

  viewProfile(craftsmanId: number): void {
    this._router.navigate(['/technicians', craftsmanId, 'profile']);
  }
  
  getStarArray(rating: number): boolean[] {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
      stars.push(i <= Math.floor(rating));
    }
    return stars;
  }
}
