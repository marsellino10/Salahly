import { Component } from '@angular/core';
import { Footer } from "../../components/shared/footer/footer";
import { RouterOutlet } from '@angular/router';
import { CustomerNavBar } from "../../components/customer/customer-nav-bar/customer-nav-bar";

@Component({
  selector: 'app-auth-layout',
  imports: [RouterOutlet, Footer, CustomerNavBar],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.css',
})
export class AuthLayout {

}
