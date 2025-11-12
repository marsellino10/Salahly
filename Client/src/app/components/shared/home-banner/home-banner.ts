import { TechnicianFilter } from './../technician-filter/technician-filter';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-home-banner',
  imports: [TranslateModule,TechnicianFilter],
  templateUrl: './home-banner.html',
  styleUrl: './home-banner.css',
})
export class HomeBanner {
  private readonly router: Router = inject(Router);

  browse() {
    this.router.navigate(['/search']);
  }

  signup() {
    this.router.navigate(['/signup']);
  }
}
