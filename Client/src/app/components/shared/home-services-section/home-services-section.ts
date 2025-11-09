import { Component } from '@angular/core';
import { FeatureCard } from "../feature-card/feature-card";
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-home-services-section',
  imports: [FeatureCard,TranslateModule],
  templateUrl: './home-services-section.html',
  styleUrl: './home-services-section.css',
})
export class HomeServicesSection {
categories = [
    { id: 1, name: 'HomeServicesSection.Electricians', description: 'HomeServicesSection.Certified electricians for all your needs', icon: 'bi bi-lightning' },
    { id: 2, name: 'HomeServicesSection.Plumbers', description: 'HomeServicesSection.Trusted plumbers for home repairs', icon: 'bi bi-droplet-half' },
    { id: 3, name: 'HomeServicesSection.Carpenters', description: 'HomeServicesSection.Skilled carpenters for furniture & repairs', icon: 'bi bi-hammer' },
    // add more categories as needed
  ];
}
