import { RouterOutlet } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { NavBlank } from "../../components/shared/nav-blank/nav-blank";
import { Footer } from "../../components/shared/footer/footer";
import { TechnicianNavBar } from '../../components/technician/technician-nav-bar/technician-nav-bar';
import { TechnicianService } from '../../core/services/technician-service';
import { CustomerService } from '../../core/services/customer-service';
import { CustomerNavBar } from '../../components/customer/customer-nav-bar/customer-nav-bar';
import { ChatbotWidget } from '../../pages/shared/chatbot-widget/chatbot-widget';

@Component({
  selector: 'app-blank-layout',
    imports: [RouterOutlet, NavBlank, Footer, TechnicianNavBar, CustomerNavBar, ChatbotWidget],
  templateUrl: './blank-layout.html',
  styleUrl: './blank-layout.css',
})
export class BlankLayout implements OnInit {
  showTechnicianNavBar = false;
  showCustomerNavBar = false;

  constructor(private readonly _technicianService: TechnicianService, private readonly _customerService: CustomerService) {}

  ngOnInit(): void {
    const role = this._technicianService.getTechnicianTokenClaims().role;
    this.showTechnicianNavBar = role?.toLowerCase() === 'craftsman';
    const customerRole = this._customerService.getCustomerTokenClaims().role;
    this.showCustomerNavBar = customerRole?.toLowerCase() === 'customer';
  }
}
