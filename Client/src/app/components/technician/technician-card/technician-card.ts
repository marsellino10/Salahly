import { Component, Input } from '@angular/core';
import { Craftsman } from '../../../core/models/Craftman';

@Component({
  selector: 'app-technician-card',
  imports: [],
  templateUrl: './technician-card.html',
  styleUrl: './technician-card.css',
})
export class TechnicianCard {
  @Input() craftsman!: Craftsman;


  getInitials(fullName: string): string {
    return fullName.split(' ').map(name => name.charAt(0)).join('').toUpperCase();
  }

  viewProfile(craftsmanId: number): void {
    console.log('View profile for craftsman:', craftsmanId);
    // Implement navigation to craftsman profile
  }
  
  getStarArray(rating: number): boolean[] {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
      stars.push(i <= Math.floor(rating));
    }
    return stars;
  }
}
