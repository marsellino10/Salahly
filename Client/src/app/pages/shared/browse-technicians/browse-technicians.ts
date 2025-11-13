import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Craft } from '../../../core/models/Craft';
import { Craftsman } from '../../../core/models/Craftman';
import { TechnicianCard } from "../../../components/technician/technician-card/technician-card";
import { TechnicianFilter } from '../../../components/shared/technician-filter/technician-filter';

@Component({
  selector: 'app-browse-technicians',
  imports: [CommonModule, FormsModule, TechnicianCard, TechnicianFilter],
  templateUrl: './browse-technicians.html',
  styleUrl: './browse-technicians.css',
})
export class BrowseTechnicians implements OnInit {
  
  crafts: Craft[] = [
    {
      id: 0,
      name: 'All Categories',
      description: 'All available crafts',
      iconUrl: '',
      displayOrder: 0,
      isActive: true,
      createdAt: '',
      craftsmenCount: 0,
      activeServiceRequestsCount: 0
    },
    {
      id: 1,
      name: 'Plumber',
      description: 'Plumbing services',
      iconUrl: '',
      displayOrder: 1,
      isActive: true,
      createdAt: '',
      craftsmenCount: 15,
      activeServiceRequestsCount: 5
    },
    {
      id: 2,
      name: 'Electrician',
      description: 'Electrical services',
      iconUrl: '',
      displayOrder: 2,
      isActive: true,
      createdAt: '',
      craftsmenCount: 12,
      activeServiceRequestsCount: 3
    },
    {
      id: 3,
      name: 'HVAC',
      description: 'Heating, ventilation, and air conditioning',
      iconUrl: '',
      displayOrder: 3,
      isActive: true,
      createdAt: '',
      craftsmenCount: 8,
      activeServiceRequestsCount: 2
    }
  ];
  
  craftsmen: Craftsman[] = [
    {
      id: 1,
      fullName: 'Khalid Abdullah',
      craftId: 1,
      ratingAverage: 4.9,
      totalCompletedBookings: 68,
      isAvailable: true,
      hourlyRate: 140,
      bio: 'Expert plumber with over 12 years of experience. Specialized in pipe repairs, water heater installation, and emergency plumbing services.',
      yearsOfExperience: 12,
      verifiedAt: '2023-01-15T10:00:00Z',
      profileImageUrl: null,
      portfolio: [],
      serviceAreas: [
        {
          areaId: 1,
          region: 'Western Region',
          city: 'Jeddah',
          serviceRadiusKm: 25,
          isActive: true
        }
      ]
    },
    {
      id: 2,
      fullName: 'Faisal Al-Ghamdi',
      craftId: 3,
      ratingAverage: 4.9,
      totalCompletedBookings: 51,
      isAvailable: true,
      hourlyRate: 160,
      bio: 'HVAC specialist with 9 years of experience in air conditioning installation, maintenance, and repair services.',
      yearsOfExperience: 9,
      verifiedAt: '2023-02-20T10:00:00Z',
      profileImageUrl: null,
      portfolio: [],
      serviceAreas: [
        {
          areaId: 2,
          region: 'Central Region',
          city: 'Riyadh',
          serviceRadiusKm: 30,
          isActive: true
        }
      ]
    },
    {
      id: 3,
      fullName: 'Ahmed Hassan',
      craftId: 2,
      ratingAverage: 4.8,
      totalCompletedBookings: 42,
      isAvailable: true,
      hourlyRate: 150,
      bio: 'Certified electrician with 8 years of experience in residential and commercial electrical work.',
      yearsOfExperience: 8,
      verifiedAt: '2023-03-10T10:00:00Z',
      profileImageUrl: null,
      portfolio: [],
      serviceAreas: [
        {
          areaId: 2,
          region: 'Central Region',
          city: 'Riyadh',
          serviceRadiusKm: 20,
          isActive: true
        }
      ]
    }
  ];

  filteredCraftsmen: Craftsman[] = [];

  ngOnInit(): void {
    this.filteredCraftsmen = [...this.craftsmen];
  }


}
