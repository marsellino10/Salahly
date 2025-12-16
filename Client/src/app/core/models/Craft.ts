export interface Craft {
  id: number;
  name: string;
  nameAr?: string | null;
  description: string;
  iconUrl: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;

  craftsmenCount: number;
  activeServiceRequestsCount: number;
}