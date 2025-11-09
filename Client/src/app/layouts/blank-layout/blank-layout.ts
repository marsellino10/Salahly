import { RouterOutlet } from '@angular/router';
import { Component } from '@angular/core';
import { NavBlank } from "../../components/shared/nav-blank/nav-blank";
import { Footer } from "../../components/shared/footer/footer";

@Component({
  selector: 'app-blank-layout',
  imports: [RouterOutlet, NavBlank, Footer],
  templateUrl: './blank-layout.html',
  styleUrl: './blank-layout.css',
})
export class BlankLayout {

}
