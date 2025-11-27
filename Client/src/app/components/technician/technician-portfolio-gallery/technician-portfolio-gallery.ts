import { CommonModule } from '@angular/common';
import { Component, EventEmitter, HostBinding, Input, Output } from '@angular/core';
import { Portfolio } from '../../../core/models/Portfolio';
import { CraftsmanReview } from '../../../core/models/Craftman';

export type PortfolioGalleryItem = Portfolio & { reviews: CraftsmanReview[] };

@Component({
  selector: 'app-technician-portfolio-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './technician-portfolio-gallery.html',
  styleUrls: ['./technician-portfolio-gallery.css'],
})
export class TechnicianPortfolioGallery {
  @Input() items: PortfolioGalleryItem[] = [];
  @Output() select = new EventEmitter<PortfolioGalleryItem>();
  @Input() direction: 'ltr' | 'rtl' = 'ltr';
  @HostBinding('attr.dir') get dir(): 'ltr' | 'rtl' {
    return this.direction;
  }

  trackById(index: number, item: PortfolioGalleryItem): number {
    return item.id ?? index;
  }

  onSelect(item: PortfolioGalleryItem): void {
    this.select.emit(item);
  }

  getImageUrl(item: PortfolioGalleryItem): string {
    return item.imageUrl || 'https://via.placeholder.com/600x400?text=Portfolio+Item';
  }
}
