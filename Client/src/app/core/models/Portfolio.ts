import { CraftsmanReview } from './Craftman';

export interface Portfolio {
  id: number;
  craftsmanId: number;
  title: string;
  description: string;
  imageUrl: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  reviews?: CraftsmanReview[];
}

export type PortfolioWithReviews = Portfolio & { reviews: CraftsmanReview[] };
