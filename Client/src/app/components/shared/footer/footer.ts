import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-footer',
  imports: [RouterLink,TranslateModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  private readonly translate: TranslateService = inject(TranslateService);
logoSrc: string = './assets/images/logoEN.jpeg';
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
ngOnInit(): void {
    this.setLogo(this.translate.currentLang);
    this.translate.onLangChange.subscribe(event => {
      this.setLogo(event.lang);
    });
  }
  setLogo(lang: string) {
    this.logoSrc = lang === 'ar' 
      ? './assets/images/logoAR.png' 
      : './assets/images/logoEN.jpeg';
  }
}
