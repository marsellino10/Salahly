import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import {
  AdminService,
  PortfolioItemResponseDto,
} from '../../../core/services/admin-service';
import { RouterLink } from "@angular/router";

type SortMode = 'newest' | 'oldest' | 'title';

@Component({
  selector: 'app-approve-protfolio-item',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './approve-protfolio-item.html',
  styleUrl: './approve-protfolio-item.css',
})
export class ApproveProtfolioItem implements OnInit {
  private readonly adminService = inject(AdminService);

  items: PortfolioItemResponseDto[] = [];
  filteredItems: PortfolioItemResponseDto[] = [];
  loading = false;
  approvingId: number | null = null;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  searchTerm = '';
  selectedCraftsmanId: string = 'all';
  sortMode: SortMode = 'newest';

  readonly skeletonPlaceholders = Array.from({ length: 6 }, (_, index) => index);

  summary = {
    total: 0,
    craftsmanCount: 0,
    longestWaitingDays: 0,
  };

  ngOnInit(): void {
    this.loadInactivePortfolios();
  }

  get craftsmanOptions(): number[] {
    return Array.from(new Set(this.items.map((item) => item.craftsmanId))).sort((a, b) => a - b);
  }

  loadInactivePortfolios(): void {
    this.loading = true;
    this.errorMessage = null;

    this.adminService
      .getInactivePortfolio()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (items) => {
          console.log(items);
          this.items = items ?? [];
          this.computeSummary();
          this.applyFilters();
        },
        error: (error) => {
          this.errorMessage = error?.error?.message ?? error?.message ?? 'Failed to load portfolio items.';
        },
      });
  }

  applyFilters(): void {
    const search = this.searchTerm.trim().toLowerCase();
    const craftsmanFilter = this.selectedCraftsmanId === 'all' ? null : Number(this.selectedCraftsmanId);

    let nextItems = this.items.filter((item) => {
      const matchesCraftsman = craftsmanFilter === null || item.craftsmanId === craftsmanFilter;
      const matchesSearch =
        !search ||
        item.title?.toLowerCase().includes(search) ||
        item.description?.toLowerCase().includes(search) ||
        item.id.toString().includes(search);
      return matchesCraftsman && matchesSearch;
    });

    nextItems = this.sortItems(nextItems);
    this.filteredItems = nextItems;
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCraftsmanId = 'all';
    this.sortMode = 'newest';
    this.applyFilters();
  }

  approveItem(item: PortfolioItemResponseDto): void {
    if (this.approvingId) {
      return;
    }

    this.approvingId = item.id;
    this.errorMessage = null;
    this.successMessage = null;

    this.adminService
      .approvePortfolioItem(item.id)
      .pipe(finalize(() => (this.approvingId = null)))
      .subscribe({
        next: () => {
          this.items = this.items.filter((portfolioItem) => portfolioItem.id !== item.id);
          this.computeSummary();
          this.applyFilters();
          this.successMessage = `Portfolio item "${item.title}" has been approved.`;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message ?? error?.message ?? 'Failed to approve portfolio item.';
        },
      });
  }

  trackById(_: number, item: PortfolioItemResponseDto): number {
    return item.id;
  }

  formatFullDate(value: Date | string): string {
    const date = new Date(value);
    return new Intl.DateTimeFormat('en-US', { dateStyle: 'medium', timeStyle: 'short' }).format(date);
  }

  formatRelativeDate(value: Date | string): string {
    const created = new Date(value).getTime();
    const now = Date.now();
    const diffMs = Math.max(now - created, 0);
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));

    if (diffHours < 24) {
      return `${diffHours || 1}h waiting`;
    }

    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 30) {
      return `${diffDays}d waiting`;
    }

    const diffMonths = Math.floor(diffDays / 30);
    return `${diffMonths}mo waiting`;
  }

  private sortItems(items: PortfolioItemResponseDto[]): PortfolioItemResponseDto[] {
    return [...items].sort((a, b) => {
      if (this.sortMode === 'title') {
        return a.title.localeCompare(b.title);
      }

      const dateA = new Date(a.createdAt).getTime();
      const dateB = new Date(b.createdAt).getTime();
      return this.sortMode === 'newest' ? dateB - dateA : dateA - dateB;
    });
  }

  private computeSummary(): void {
    const uniqueCraftsmen = new Set(this.items.map((item) => item.craftsmanId));
    const now = Date.now();
    const longestWaitingDays = this.items.reduce((max, item) => {
      const created = new Date(item.createdAt).getTime();
      const diffDays = Math.floor((now - created) / (1000 * 60 * 60 * 24));
      return Math.max(max, diffDays);
    }, 0);

    this.summary = {
      total: this.items.length,
      craftsmanCount: uniqueCraftsmen.size,
      longestWaitingDays,
    };
  }
}
