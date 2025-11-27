import { Component } from '@angular/core';
import { AdminNavBar } from "../../components/admin/admin-nav-bar/admin-nav-bar";
import { RouterModule } from "@angular/router";
import { Footer } from "../../components/shared/footer/footer";

@Component({
  selector: 'app-admin-layout',
  imports: [AdminNavBar, RouterModule, Footer],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout {

}
