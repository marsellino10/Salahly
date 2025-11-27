import { CommonModule, DOCUMENT } from '@angular/common';
import { Component, HostBinding, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { EMPTY, Observable, Subject, forkJoin, of, throwError } from 'rxjs';
import { catchError, map, switchMap, takeUntil } from 'rxjs/operators';
import { Craftsman, CraftsmanReview } from '../../../core/models/Craftman';
import { Portfolio, PortfolioWithReviews } from '../../../core/models/Portfolio';
import { ApiResponse, TechnicianService } from '../../../core/services/technician-service';
import { TechnicianPortfolioGallery } from '../../../components/technician/technician-portfolio-gallery/technician-portfolio-gallery';
import { TechnicianPortfolioModal } from '../../../components/technician/technician-portfolio-modal/technician-portfolio-modal';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

interface TechnicianProfileState {
  craftsman: Craftsman | null;
  portfolio: PortfolioWithReviews[];
  reviews: CraftsmanReview[];
}

@Component({
  selector: 'app-technician-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, TechnicianPortfolioGallery, TechnicianPortfolioModal, TranslateModule],
  templateUrl: './technician-profile.html',
  styleUrls: ['./technician-profile.css'],
})
export class TechnicianProfile implements OnInit, OnDestroy {
  private readonly _route = inject(ActivatedRoute);
  private readonly _technicianService = inject(TechnicianService);
  private readonly _destroy$ = new Subject<void>();
  private readonly _document = inject(DOCUMENT);
  private readonly _translate = inject(TranslateService);

  currentLang: 'ar' | 'en' = this.getInitialLang();
  @HostBinding('attr.dir') direction: 'ltr' | 'rtl' = this.resolveDirection(this.currentLang);

  isLoading = false;
  hasError = false;
  errorMessage = '';

  state: TechnicianProfileState = {
    craftsman: null,
    portfolio: [],
    reviews: [],
  };

  selectedPortfolio: PortfolioWithReviews | null = null;
  selectedPortfolioReviews: CraftsmanReview[] = [];

  get combinedReviews(): CraftsmanReview[] {
    const portfolioReviews = this.state.portfolio.flatMap((item) => item.reviews ?? []);
    const allReviews = [...portfolioReviews, ...this.state.reviews];

    const unique = new Map<string, CraftsmanReview>();
    allReviews.forEach((review) => {
      const key = this.generateReviewKey(review);
      if (!unique.has(key)) {
        unique.set(key, review);
      }
    });

    return Array.from(unique.values()).sort((a, b) => this.sortByDateDesc(a.createdAt, b.createdAt));
  }

  ngOnInit(): void {
    this._translate.onLangChange
      .pipe(takeUntil(this._destroy$))
      .subscribe(({ lang }) => {
        this.currentLang = this.normalizeLang(lang);
        this.direction = this.resolveDirection(this.currentLang);
      });

    this._route.paramMap
      .pipe(
        takeUntil(this._destroy$),
        switchMap((params) => {
          const id = Number(params.get('id'));

          if (!id || Number.isNaN(id)) {
            this.handleError('Invalid technician identifier.');
            return EMPTY;
          }

          return this.loadProfile(id);
        })
      )
      .subscribe({
        next: ({ craftsman, portfolio, reviews }) => {
          const normalizedPortfolio = this.mergePortfolio(craftsman, portfolio.data ?? []);
          const normalizedReviews = this.mergeReviews(craftsman, reviews.data ?? []);

          const portfolioWithReviews = this.attachReviewsToPortfolio(normalizedPortfolio, normalizedReviews);

          this.state = {
            craftsman,
            portfolio: portfolioWithReviews,
            reviews: normalizedReviews,
          };

          this.isLoading = false;
          this.hasError = false;
        },
        error: (error) => this.handleError('Unable to load technician profile.', error),
      });
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  openPortfolioModal(item: PortfolioWithReviews): void {
    this.selectedPortfolio = item;
    this.selectedPortfolioReviews = this.getReviewsForPortfolio(item);
  }

  closePortfolioModal(): void {
    this.selectedPortfolio = null;
    this.selectedPortfolioReviews = [];
  }

  trackArea(index: number, area: { areaId?: number | null; city?: string | null; region?: string | null }): string {
    return `${area.areaId ?? index}-${area.city ?? 'city'}-${area.region ?? 'region'}`;
  }

  trackReview(index: number, review: CraftsmanReview): string | number {
    return review.id ?? this.generateReviewKey(review) ?? index;
  }

  getProfileImage(craftsman: Craftsman | null): string {
    if (craftsman?.profileImageUrl) {
      return craftsman.profileImageUrl;
    }
    return 'https://via.placeholder.com/120x120?text=User';
  }

  getInitials(fullName?: string | null): string {
    if (!fullName?.trim()) {
      return '--';
    }

    return fullName
      .split(' ')
      .filter(Boolean)
      .map((segment) => segment.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  getStarArray(rating: number): boolean[] {
    const normalized = Math.round(rating ?? 0);
    return Array.from({ length: 5 }, (_, index) => index < normalized);
  }

  getRatingArray(rating: number): number[] {
    const normalized = Math.max(0, Math.round(rating ?? 0));
    return Array.from({ length: normalized }, (_, index) => index);
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

  private loadProfile(id: number): Observable<{
    craftsman: Craftsman;
    portfolio: ApiResponse<Portfolio[]>;
    reviews: ApiResponse<CraftsmanReview[]>;
  }> {
    this.resetState();

    const craftsman$ = this._technicianService.getTechnicianById(id).pipe(
      map((response) => response?.data),
      map((craftsman) => {
        if (!craftsman) {
          throw { status: 404, message: 'Technician not found.' };
        }
        return craftsman;
      }),
      catchError((error) => {
        if (error?.status === 404) {
          this.handleError('Technician not found.', error);
        }
        return throwError(() => error);
      })
    );

    return forkJoin({
      craftsman: craftsman$,
      portfolio: this.createPortfolioRequest(id),
      reviews: this.createReviewsRequest(id),
    }).pipe(
      catchError((error) => {
        this.handleError('Unable to load technician profile.', error);
        return EMPTY;
      })
    );
  }

  private createPortfolioRequest(id: number): Observable<ApiResponse<Portfolio[]>> {
    return this._technicianService.getTechnicianPortfolio(id).pipe(
      catchError((error) => {
        if (error?.status === 404) {
          return of({ statusCode: 200, message: 'No portfolio items found.', data: [] } satisfies ApiResponse<Portfolio[]>);
        }
        return throwError(() => error);
      })
    );
  }

  private createReviewsRequest(id: number): Observable<ApiResponse<CraftsmanReview[]>> {
    return this._technicianService.getTechnicianReviews(id).pipe(
      catchError((error) => {
        if (error?.status === 404) {
          return of({ statusCode: 200, message: 'No reviews yet.', data: [] } satisfies ApiResponse<CraftsmanReview[]>);
        }
        return throwError(() => error);
      })
    );
  }

  private mergePortfolio(craftsman: Craftsman, apiPortfolio: Portfolio[]): PortfolioWithReviews[] {
    const craftsmanPortfolio = Array.isArray(craftsman.portfolio) ? craftsman.portfolio : [];

    if (!craftsmanPortfolio.length && !apiPortfolio.length) {
      return [];
    }

    const merged = [...craftsmanPortfolio, ...apiPortfolio];
    const uniqueById = new Map<number, PortfolioWithReviews>();

    merged.forEach((item) => {
      if (!uniqueById.has(item.id)) {
        uniqueById.set(item.id, {
          ...item,
          reviews: Array.isArray(item.reviews) ? item.reviews.map((review) => this.normalizeReview(review)) : [],
        });
      }
    });

    return Array.from(uniqueById.values()).sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  }

  private mergeReviews(craftsman: Craftsman, apiReviews: CraftsmanReview[]): CraftsmanReview[] {
    const craftsmanReviews = Array.isArray(craftsman.reviews) ? craftsman.reviews : [];

    if (craftsmanReviews.length) {
      return craftsmanReviews.map((review) => this.normalizeReview(review));
    }

    return apiReviews.map((review) => this.normalizeReview(review));
  }

  private normalizeReview(review: Partial<CraftsmanReview>): CraftsmanReview {
    return {
      id: review?.id ?? 0,
      reviewerUserId: review?.reviewerUserId ?? 0,
      reviewerName: review?.reviewerName ?? `Reviewer #${review?.reviewerUserId ?? 'N/A'}`,
      reviewerProfileImageUrl: review?.reviewerProfileImageUrl ?? null,
      rating: review?.rating ?? 0,
      comment: review?.comment ?? 'No comment provided.',
      createdAt: review?.createdAt ?? new Date().toISOString(),
      bookingId: review?.bookingId ?? 0,
    };
  }

  private attachReviewsToPortfolio(portfolio: PortfolioWithReviews[], reviews: CraftsmanReview[]): PortfolioWithReviews[] {
    return portfolio.map((item) => ({
      ...item,
      reviews: this.extractReviewsForPortfolioItem(item, reviews),
    }));
  }

  private getReviewsForPortfolio(item: PortfolioWithReviews): CraftsmanReview[] {
    if (Array.isArray(item.reviews) && item.reviews.length) {
      return item.reviews;
    }

    const extracted = this.extractReviewsForPortfolioItem(item, this.state.reviews ?? []);
    return extracted.length ? extracted : [];
  }

  private reviewReferencesPortfolio(review: CraftsmanReview, item: Portfolio): boolean {
    if (!review || !item) {
      return false;
    }

    if (review.bookingId && review.bookingId === item.id) {
      return true;
    }

    const normalizedComment = (review.comment ?? '').toLowerCase();
    const normalizedTitle = (item.title ?? '').toLowerCase();

    return Boolean(normalizedTitle && normalizedComment.includes(normalizedTitle));
  }

  private extractReviewsForPortfolioItem(item: Portfolio, reviewsSource: CraftsmanReview[]): CraftsmanReview[] {
    const inlineReviews = Array.isArray(item.reviews)
      ? item.reviews.map((review) => this.normalizeReview(review))
      : [];

    if (inlineReviews.length) {
      return inlineReviews;
    }

    return reviewsSource.filter((review) => this.reviewReferencesPortfolio(review, item));
  }

  private resetState(): void {
    this.isLoading = true;
    this.hasError = false;
    this.errorMessage = '';
    this.state = {
      craftsman: null,
      portfolio: [],
      reviews: [],
    };
    this.selectedPortfolio = null;
    this.selectedPortfolioReviews = [];
  }

  private handleError(message: string, error?: unknown): void {
    console.error(message, error);
    this.isLoading = false;
    this.hasError = true;
    this.errorMessage = message;
  }

  private generateReviewKey(review: CraftsmanReview): string {
    if (review.id) {
      return review.id.toString();
    }

    return `${review.reviewerUserId}-${review.bookingId}-${review.comment?.trim() ?? ''}`;
  }

  private sortByDateDesc(a: string | Date, b: string | Date): number {
    const dateA = new Date(a).getTime();
    const dateB = new Date(b).getTime();
    return dateB - dateA;
  }

  private getInitialLang(): 'ar' | 'en' {
    const docLang = this._document?.documentElement?.lang ?? this._translate.currentLang ?? 'en';
    return this.normalizeLang(docLang);
  }

  private normalizeLang(lang: string | null | undefined): 'ar' | 'en' {
    return lang?.toLowerCase().startsWith('ar') ? 'ar' : 'en';
  }

  private resolveDirection(lang: string | null | undefined): 'ltr' | 'rtl' {
    return this.normalizeLang(lang) === 'ar' ? 'rtl' : 'ltr';
  }
}
