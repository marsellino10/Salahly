import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AdminService, AreaStatsDto, CraftAverageReviewDto, CraftsmanShortDto, OffersStatsDto } from '../../../core/services/admin-service';
import { ServiceRequestDto } from '../../../core/services/services-requests.service';
import { TranslateModule } from '@ngx-translate/core';

interface DashboardData {
  serviceRequestCount: number;
  craftsmenCount: number;
  craftsCount: number;
  totalExperience: number;
  offersStats: OffersStatsDto;
  mostActiveArea: AreaStatsDto | null;
  craftAverageReviews: CraftAverageReviewDto[];
  topCraftsmen: CraftsmanShortDto[];
  recentRequests: ServiceRequestDto[];
}

interface DashboardCard {
  title: string;
  subtitle: string;
  value: string;
  accent: 'primary' | 'success' | 'warning' | 'info';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private readonly _adminService = inject(AdminService);

  loading = true;
  error: string | null = null;
  data: DashboardData | null = null;
  summaryCards: DashboardCard[] = [];
  craftAveragePreview: CraftAverageReviewDto[] = [];

  ngOnInit(): void {
    this.loadDashboard();
  }

  refresh(): void {
    this.loadDashboard();
  }

  trackCard(_: number, card: DashboardCard): string {
    return card.title;
  }

  trackCraftsman(_: number, craftsman: CraftsmanShortDto): number {
    return craftsman.id;
  }

  trackCraftAverage(_: number, craft: CraftAverageReviewDto): number {
    return craft.craftId;
  }

  trackRequest(_: number, request: ServiceRequestDto): number {
    return request.serviceRequestId;
  }

  formatDate(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '--' : date.toLocaleDateString();
  }

  getCraftAverageWidth(value?: number | null): number {
    const normalized = ((value ?? 0) / 5) * 100;
    return Math.min(100, Math.max(0, Number(normalized.toFixed(2))));
  }

  statusClass(status?: string | null): string {
    const normalized = (status ?? '').toLowerCase();
    const map: Record<string, string> = {
      completed: 'success',
      inprogress: 'info',
      in_progress: 'info',
      pending: 'warning',
      hasoffers: 'info',
      offeraccepted: 'info',
      cancelled: 'danger',
      expired: 'muted',
    };
    return `status-pill--${map[normalized] ?? 'muted'}`;
  }

  private loadDashboard(): void {
    this.loading = true;
    this.error = null;

    forkJoin({
      serviceRequestCount: this._adminService.countServiceRequests(),
      offersStats: this._adminService.getOffersStats(),
      craftsmenCount: this._adminService.countCraftsmen(),
      totalExperience: this._adminService.getTotalCraftsmenExperience(),
      craftsCount: this._adminService.countCrafts(),
      mostActiveArea: this._adminService.getMostActiveArea(),
      craftAverageReviews: this._adminService.getCraftsAverageReviews(),
      topCraftsmen: this._adminService.getTopCraftsmenByReviews(5),
      recentRequests: this._adminService.getServiceRequests({ orderBy: 'date', asc: false }),
    }).subscribe({
      next: (stats) => {
        const data: DashboardData = {
          serviceRequestCount: stats.serviceRequestCount ?? 0,
          craftsmenCount: stats.craftsmenCount ?? 0,
          craftsCount: stats.craftsCount ?? 0,
          totalExperience: stats.totalExperience ?? 0,
          offersStats: stats.offersStats ?? { totalOffers: 0, averageOffersPerServiceRequest: 0 },
          mostActiveArea: stats.mostActiveArea ?? null,
          craftAverageReviews: stats.craftAverageReviews ?? [],
          topCraftsmen: stats.topCraftsmen ?? [],
          recentRequests: (stats.recentRequests ?? []).slice(0, 6),
        };

        this.data = data;
        this.summaryCards = this.buildSummaryCards(data);
        this.craftAveragePreview = data.craftAverageReviews.slice(0, 6);
        this.loading = false;
      },
      error: (err) => {
        this.error = this.extractErrorMessage(err);
        this.loading = false;
      },
    });
  }

  private buildSummaryCards(data: DashboardData): DashboardCard[] {
    return [
      {
        title: 'AdminDashboard.Summary.ServiceRequests.Title',
        subtitle: 'AdminDashboard.Summary.ServiceRequests.Subtitle',
        value: this.formatNumber(data.serviceRequestCount),
        accent: 'primary',
      },
      {
        title: 'AdminDashboard.Summary.Craftsmen.Title',
        subtitle: 'AdminDashboard.Summary.Craftsmen.Subtitle',
        value: this.formatNumber(data.craftsmenCount),
        accent: 'success',
      },
      {
        title: 'AdminDashboard.Summary.Crafts.Title',
        subtitle: 'AdminDashboard.Summary.Crafts.Subtitle',
        value: this.formatNumber(data.craftsCount),
        accent: 'info',
      },
      {
        title: 'AdminDashboard.Summary.TotalExperience.Title',
        subtitle: 'AdminDashboard.Summary.TotalExperience.Subtitle',
        value: `${this.formatNumber(data.totalExperience)} yrs`,
        accent: 'warning',
      },
      {
        title: 'AdminDashboard.Summary.TotalOffers.Title',
        subtitle: 'AdminDashboard.Summary.TotalOffers.Subtitle',
        value: this.formatNumber(data.offersStats.totalOffers),
        accent: 'info',
      },
      {
        title: 'AdminDashboard.Summary.AverageOffers.Title',
        subtitle: 'AdminDashboard.Summary.AverageOffers.Subtitle',
        value: this.formatNumber(data.offersStats.averageOffersPerServiceRequest ?? 0, 2),
        accent: 'success',
      },
    ];
  }

  private formatNumber(value: number, fractionDigits: number = 0): string {
    return Number(value || 0).toLocaleString('en-US', {
      minimumFractionDigits: fractionDigits,
      maximumFractionDigits: fractionDigits,
    });
  }

  private extractErrorMessage(error: unknown): string {
    if (!error) {
      return 'AdminDashboard.Errors.LoadFailed';
    }

    if (typeof error === 'string') {
      return error;
    }

    if (error instanceof Error) {
      return error.message;
    }

    if (typeof error === 'object' && error !== null && 'message' in error) {
      const message = (error as Record<string, unknown>)['message'];
      if (typeof message === 'string') {
        return message;
      }
    }

    return 'AdminDashboard.Errors.LoadFailed';
  }
}
