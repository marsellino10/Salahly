import { Component, Input } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-feature-card',
  imports: [TranslateModule],
  templateUrl: './feature-card.html',
  styleUrl: './feature-card.css',
})
export class FeatureCard {
  @Input() Icon: string = '';
  @Input() Title: string = '';
  @Input() Description: string = '';
}
