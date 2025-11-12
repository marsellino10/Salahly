import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-technician-filter',
  imports: [CommonModule, FormsModule],
  templateUrl: './technician-filter.html',
  styleUrl: './technician-filter.css',
})
export class TechnicianFilter {
searchQuery: string = '';
  selectedCategory: string = '';

  categories = [
    { id: 'electrician', name: 'Electrician' },
    { id: 'plumber', name: 'Plumber' },
    { id: 'carpenter', name: 'Carpenter' },
    { id: 'painter', name: 'Painter' },
  ];

  // You can watch/filter technicians using these properties
  onFiltersChange() {
    console.log({
      search: this.searchQuery,
      category: this.selectedCategory,
    });
  }
}
