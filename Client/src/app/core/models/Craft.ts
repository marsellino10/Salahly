export interface Craft {
  id: number;
  name: string;
  description: string;
  iconUrl: string;
  displayOrder: number;
  isActive: boolean;
  createdAt: string;
  craftsmenCount: number;
  activeServiceRequestsCount: number;
}