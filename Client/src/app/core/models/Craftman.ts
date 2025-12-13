import { Portfolio } from "./Portfolio";
import { ServiceArea } from "./ServiceArea";

export interface CraftsmanReview {
  id: number;
  reviewerUserId: number;
  reviewerName: string;
  reviewerProfileImageUrl?: string | null;
  rating: number;
  comment: string;
  createdAt: string;
  bookingId: number;
}

export interface Craftsman {
  id: number;
  fullName: string;
  craftId: number;
  craftName?: string | null;
  jobTitle?: string | null;
  ratingAverage: number;
  totalCompletedBookings: number;
  isAvailable?: boolean;
  hourlyRate?: number;
  bio?: string;
  yearsOfExperience?: number;
  verifiedAt?: string | null;
  profileImageUrl?: string | null;
  portfolio: Portfolio[];
  isVerified?: boolean;
  serviceAreas: ServiceArea[];
  reviews?: CraftsmanReview[];
  balance?: number;
}