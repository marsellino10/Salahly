import { Component } from '@angular/core';
import { FeatureCard } from '../feature-card/feature-card';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-features',
  imports: [FeatureCard,TranslateModule],
  templateUrl: './features.html',
  styleUrl: './features.css',
})
export class Features {
  features = [
    {
      icon: 'bi bi-check-circle',
      title: 'HomeFeatures.Verified Professionals',
      description: 'HomeFeatures.All service providers are verified and rated by customers'
    },
    {
      icon: 'bi bi-lightning-charge',
      title: 'HomeFeatures.Instant Booking',
      description: 'HomeFeatures.Book services quickly and easily with just a few clicks'
    },
    {
      icon: 'bi bi-chat-dots',
      title: 'HomeFeatures.Direct Communication',
      description: 'HomeFeatures.Chat or call technicians directly to discuss your service needs'
    }
  ];
}
