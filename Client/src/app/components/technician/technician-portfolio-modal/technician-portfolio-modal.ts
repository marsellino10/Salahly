import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CraftsmanReview } from '../../../core/models/Craftman';
import { Portfolio } from '../../../core/models/Portfolio';

@Component({
  selector: 'app-technician-portfolio-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './technician-portfolio-modal.html',
  styleUrls: ['./technician-portfolio-modal.css'],
})
export class TechnicianPortfolioModal {
  @Input() item: Portfolio | null = null;
  @Input() reviews: CraftsmanReview[] = [];
  @Output() close = new EventEmitter<void>();

  trackReview(index: number, review: CraftsmanReview): number {
    return review.id ?? index;
  }

  formatDate(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }
    const date = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(date.getTime())) {
      return '';
    }
    return date.toLocaleDateString();
  }

  getReviewerInitials(name: string | null | undefined): string {
    if (!name?.trim()) {
      return '--';
    }
    return name
      .split(' ')
      .filter(Boolean)
      .map((segment) => segment.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  getRatingArray(rating: number): number[] {
    const normalized = Math.max(0, Math.round(rating ?? 0));
    return Array.from({ length: normalized }, (_, index) => index);
  }

  onClose(): void {
    this.close.emit();
  }
}
