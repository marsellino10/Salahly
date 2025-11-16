import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from "../../components/shared/footer/footer";
import { TechnicianNavBar } from '../../components/technician/technician-nav-bar/technician-nav-bar';

@Component({
  selector: 'app-technician-layout',
  imports: [RouterOutlet, TechnicianNavBar, Footer],
  templateUrl: './technician-layout.html',
  styleUrl: './technician-layout.css',
})
export class TechnicianLayout {

}
