import { RouterOutlet } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { NavBlank } from "../../components/shared/nav-blank/nav-blank";
import { Footer } from "../../components/shared/footer/footer";
import { TechnicianNavBar } from '../../components/technician/technician-nav-bar/technician-nav-bar';
import { TechnicianService } from '../../core/services/technician-service';

@Component({
  selector: 'app-blank-layout',
  imports: [RouterOutlet, NavBlank, Footer, TechnicianNavBar],
  templateUrl: './blank-layout.html',
  styleUrl: './blank-layout.css',
})
export class BlankLayout implements OnInit {
  showTechnicianNavBar = false;

  constructor(private readonly _technicianService: TechnicianService) {}

  ngOnInit(): void {
    const role = this._technicianService.getTechnicianTokenClaims().role;
    this.showTechnicianNavBar = role?.toLowerCase() === 'craftsman';
  }
}
