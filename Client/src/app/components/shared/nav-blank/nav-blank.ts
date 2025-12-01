import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateSelect } from "../translate-select/translate-select";
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-nav-blank',
  imports: [CommonModule, RouterLink, TranslateSelect, TranslateModule],
  templateUrl: './nav-blank.html',
  styleUrl: './nav-blank.css',
})
export class NavBlank {
  logoSrc: string = './assets/images/logoEN.jpeg';
  mobileMenuOpen = false;
  user = { name: 'Adel Samy' }; // Example user

  constructor(private router: Router,private translate: TranslateService) {}
  ngOnInit(): void {
    this.setLogo(this.translate.currentLang);
    this.translate.onLangChange.subscribe(event => {
      this.setLogo(event.lang);
    });
  }

  toggleMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu() {
    this.mobileMenuOpen = false;
  }

  navigate(path: string) {
    this.router.navigate([path]);
    this.closeMobileMenu();
  }

  logout() {
    // TODO: add logout logic
    this.closeMobileMenu();
  }
  setLogo(lang: string) {
    this.logoSrc = lang === 'ar' 
      ? './assets/images/logoAR.png' 
      : './assets/images/logoEN.jpeg';
  }
}
