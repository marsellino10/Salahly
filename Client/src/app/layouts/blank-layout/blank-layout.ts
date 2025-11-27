import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { NavBlank } from "../../components/shared/nav-blank/nav-blank";
import { Footer } from "../../components/shared/footer/footer";
import { TechnicianNavBar } from '../../components/technician/technician-nav-bar/technician-nav-bar';
import { TechnicianService } from '../../core/services/technician-service';
import { CustomerService } from '../../core/services/customer-service';
import { CustomerNavBar } from '../../components/customer/customer-nav-bar/customer-nav-bar';
import { ChatbotWidget } from '../../pages/shared/chatbot-widget/chatbot-widget';
import { filter } from 'rxjs';
import { AdminService } from '../../core/services/admin-service';
import { AdminNavBar } from "../../components/admin/admin-nav-bar/admin-nav-bar";

@Component({
  selector: 'app-blank-layout',
    imports: [RouterOutlet, NavBlank, Footer, TechnicianNavBar, CustomerNavBar, ChatbotWidget, AdminNavBar],
  templateUrl: './blank-layout.html',
  styleUrl: './blank-layout.css',
})
export class BlankLayout implements OnInit {
  showTechnicianNavBar = false;
  showCustomerNavBar = false;
  showAdminNavBar = false;

  constructor(private readonly _technicianService: TechnicianService, 
    private readonly _customerService: CustomerService,
    private readonly _adminService: AdminService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.checkRole();
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.checkRole();
      });
  }
  private checkRole(): void {
    const techRole = this._technicianService.getTechnicianTokenClaims()?.role;
    const customerRole = this._customerService.getCustomerTokenClaims()?.role;
    const adminRole = this._adminService.getAdminTokenClaims()?.role
    this.showTechnicianNavBar = techRole?.toLowerCase() === 'craftsman';
    this.showCustomerNavBar = customerRole?.toLowerCase() === 'customer';
    this.showAdminNavBar = adminRole?.toLowerCase() === 'admin';
  }
}
