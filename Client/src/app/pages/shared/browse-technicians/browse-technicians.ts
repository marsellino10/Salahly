import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Craft } from '../../../core/models/Craft';
import { Craftsman } from '../../../core/models/Craftman';
import { TechnicianCard } from "../../../components/technician/technician-card/technician-card";
import { TechnicianFilter } from '../../../components/shared/technician-filter/technician-filter';
import { TechnicianService } from '../../../core/services/technician-service';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
@Component({
  selector: 'app-browse-technicians',
  imports: [CommonModule, FormsModule, TechnicianCard, TechnicianFilter,InfiniteScrollDirective],
  templateUrl: './browse-technicians.html',
  styleUrl: './browse-technicians.css',
})
export class BrowseTechnicians implements OnInit {

  private readonly _TechnicianService: TechnicianService = inject(TechnicianService);
  craftsTotalCount: number = 0;
  pageNumber: number = 1;
  pageSize: number = 10;
  totalPages: number = 0;
  hasNextPage!: boolean;
  hasPreviousPage!: boolean;
  isLoading = false;
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
  
  craftsmen: Craftsman[] = [];

  filteredCraftsmen: Craftsman[] = [];

  ngOnInit(): void {
    this.GetTechnicians();
  }

  GetTechnicians(PageNumber: number = 1,PageSize: number = 3): void {
    this._TechnicianService.getTechnicians(PageNumber,PageSize).subscribe((data) => {
      console.log(data);
      this.craftsmen = [...this.craftsmen,...data.data.items];
      this.craftsTotalCount = data.data.totalCount;
      this.pageNumber = data.data.pageNumber;
      this.pageSize = data.data.pageSize;
      this.totalPages = data.data.totalPages;
      this.hasNextPage = data.data.hasNextPage;
      this.hasPreviousPage = data.data.hasPreviousPage;
      this.filteredCraftsmen = [...this.craftsmen];
      this.isLoading = false;
    });
  }

  loadResults() {
    if (this.isLoading || !this.hasNextPage) return;
    this.isLoading = true;
    this.GetTechnicians(this.pageNumber + 1, this.pageSize);
    
  }
onScrollDown() {
    // this._Router.queryParams.subscribe((params: any) => {
    //   this.loadResults(params);
    // });
    this.loadResults();
  }
}
