import { Component, Input } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { HoverEffect } from "../../../core/directives/hover-effect";

@Component({
  selector: 'app-feature-card',
  imports: [TranslateModule, HoverEffect],
  templateUrl: './feature-card.html',
  styleUrl: './feature-card.css',
})
export class FeatureCard {
  @Input() Icon: string = '';
  @Input() Title: string = '';
  @Input() Description: string = '';
}
