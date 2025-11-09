import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-footer',
  imports: [RouterLink,TranslateModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
quickLinks = [
    { label: 'Footer.Home', path: '/' },
    { label: 'Footer.Browse Technicals', path: '/browse' },
    { label: 'Footer.Login', path: '/login' },
    { label: 'Footer.Sign Up', path: '/signup' },
  ];

  services = [
    'Footer.Electrician',
    'Footer.Plumber',
    'Footer.Carpenter',
    'Footer.Painter',
    'Footer.HVAC Specialist',
    'Footer.Locksmith',
  ];

  contact = [
    { icon: 'bi bi-telephone', text: '+20 11 234 5678' },
    { icon: 'bi bi-envelope', text: 'info@salahly.org' },
    { icon: 'bi bi-geo-alt', text: 'Footer.Cairo, Egypt' },
  ];
}
