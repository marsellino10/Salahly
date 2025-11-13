import { Portfolio } from "./Portfolio";
import { ServiceArea } from "./ServiceArea";

export interface Craftsman {
  id: number;
  fullName: string;
  craftId: number;
  ratingAverage: number;
  totalCompletedBookings: number;
  isAvailable?: boolean;
  hourlyRate?: number;
  bio?: string;
  yearsOfExperience?: number;
  verifiedAt?: string | null;
  profileImageUrl?: string | null;
  portfolio: Portfolio[];
  serviceAreas: ServiceArea[];
}