import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Craft } from '../../../core/models/Craft';
import { Craftsman } from '../../../core/models/Craftman';
import { TechnicianCard } from "../../../components/technician/technician-card/technician-card";
import { TechnicianFilter } from '../../../components/shared/technician-filter/technician-filter';
import { TechnicianService } from '../../../core/services/technician-service';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
@Component({
  selector: 'app-browse-technicians',
  imports: [CommonModule, FormsModule, TechnicianCard, TechnicianFilter,InfiniteScrollDirective,TranslateModule],
  templateUrl: './browse-technicians.html',
  styleUrl: './browse-technicians.css',
})
export class BrowseTechnicians implements OnInit {

  private readonly _TechnicianService: TechnicianService = inject(TechnicianService);
  private readonly _Router: ActivatedRoute = inject(ActivatedRoute);
  craftsTotalCount: number = 0;
  pageNumber: number = 1;
  pageSize: number = 3;
  totalPages: number = 0;
  hasNextPage!: boolean;
  hasPreviousPage!: boolean;
  isLoading = false;
  isLastPage = false;
  
  craftsmans: Craftsman[] = [];

  ngOnInit(): void {
    this._Router.queryParams.subscribe((params: any) => {
      this.craftsmans = [];
      this.isLastPage = false;
      this.pageNumber = 1;
      this.pageSize = 3;
      this.loadResults(params);
    });
    // this.GetTechnicians();
  }

  GetTechnicians(PageNumber: number = 1,PageSize: number = 3,SearchName: string = '',CraftId: number = 0,Region: string = '',City: string = '',IsAvailable: boolean = true): void {
    this._TechnicianService.getTechnicians(PageNumber,PageSize,SearchName,CraftId,Region,City,IsAvailable).subscribe((data) => {
      this.craftsmans = [...this.craftsmans,...data.data.items];
      this.craftsTotalCount = data.data.totalCount;
      this.totalPages = data.data.totalPages;
      this.hasNextPage = data.data.hasNextPage;
      this.pageNumber = data.data.pageNumber;
      this.hasPreviousPage = data.data.hasPreviousPage;
      this.isLastPage = this.pageNumber === this.totalPages;
      this.isLoading = false;
    });
  }

  loadResults(params: any) {
    if (this.isLoading || this.isLastPage) return;
    this.isLoading = true;
    this.GetTechnicians(this.pageNumber, this.pageSize,params.searchQuery,params.selectedCraftId,params.region,params.city,true);
  }
onScrollDown() {
    this._Router.queryParams.subscribe((params: any) => {
      this.pageNumber++;
      this.loadResults(params);
    });
  }
}
