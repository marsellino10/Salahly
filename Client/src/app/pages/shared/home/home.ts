import { Component } from '@angular/core';
import { HomeBanner } from '../../../components/shared/home-banner/home-banner';
import { Features } from '../../../components/shared/features/features';

@Component({
  selector: 'app-home',
  imports: [HomeBanner,Features],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {

}
