import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AreaService } from '../../../core/services/area-service';
import { Area } from '../../../core/models/Area';
import { Craft } from '../../../core/models/Craft';
import { CraftService } from '../../../core/services/craft-service';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-technician-filter',
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './technician-filter.html',
  styleUrl: './technician-filter.css',
})
export class TechnicianFilter implements OnInit {
  private readonly _areaService: AreaService = inject(AreaService);
  private readonly _craftService: CraftService = inject(CraftService);
  private readonly _router: Router = inject(Router);
  searchQuery: string = '';
  selectedCraftId: number = 0;
  selectedRegion: string = '';
  selectedCity: string = '';
  crafts: Craft[] = [];
  areas: Area[] = [];
  regions: string[] = [];
  cities: string[] = [];

  ngOnInit(): void {
    this._craftService.GetAllCrafts().subscribe({
      next: (res) => {
        console.log(res);
        this.crafts = res.data;
      },
      error: (err) => {
        console.log(err);
      }
    });
    this._areaService.GetAllAreas().subscribe({
      next: (res) => {
        this.areas = res.data;
        this.regions = Object.keys(this.GroupAreaByRegion());
        this.cities = res.data.map((area: Area) => area.city);
      },
      error: (err) => {
        console.log(err);
      }
    });
  }
  GroupAreaByRegion() {
    return this.areas.reduce((acc, area) => {
      if (!acc[area.region]) {
        acc[area.region] = [];
      }
      acc[area.region].push(area);
      return acc;
    }, {} as { [region: string]: Area[] });
  }
  
  OnRegionChange(region: string) {
    if(region === '') {
      this.selectedRegion = '';
      this.selectedCity = '';
      this.cities = this.areas.map((area: Area) => area.city);
      return;
    }
    this.selectedRegion = region;
    this.cities = this.GroupAreaByRegion()[region].map((area: Area) => area.city);
    this.selectedCity = this.cities[0];
  }
  
  OnCityChange(city: string) {
    if(city === '') {
      this.selectedCity = '';
      this.selectedRegion = '';
      return;
    }
    this.selectedCity = city;
    this.selectedRegion = this.areas.find((area: Area) => area.city === city)!.region;
  }
  GetAreaIdFromCityAndRegion(city: string, region: string) {
    return this.areas.find((area: Area) => area.city === city && area.region === region)?.id ?? 0;
  }
  // You can watch/filter technicians using these properties
  onSubmit() {
    let areaId = this.GetAreaIdFromCityAndRegion(this.selectedCity, this.selectedRegion);
    this._router.navigate(['/browse'], {
      queryParams: {
        searchQuery: this.searchQuery,
        selectedCraftId: this.selectedCraftId,
        areaId: areaId,
        isAvailable: true,
      },
    });
  }
}
