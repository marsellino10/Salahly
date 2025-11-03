import { RouterOutlet } from '@angular/router';
import { Component } from '@angular/core';
import { NavBlank } from "../../components/shared/nav-blank/nav-blank";

@Component({
  selector: 'app-blank-layout',
  imports: [RouterOutlet, NavBlank],
  templateUrl: './blank-layout.html',
  styleUrl: './blank-layout.css',
})
export class BlankLayout {

}
