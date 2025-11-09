import { Component } from '@angular/core';
import { HomeBanner } from '../../../components/shared/home-banner/home-banner';
import { Features } from '../../../components/shared/features/features';
import { HomeServicesSection } from "../../../components/shared/home-services-section/home-services-section";

@Component({
  selector: 'app-home',
  imports: [HomeBanner, Features, HomeServicesSection],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {

}
