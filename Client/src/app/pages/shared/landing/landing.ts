import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MyTranslateService } from '../../../core/services/my-translate-service';
import { TranslateSelect } from "../../../components/shared/translate-select/translate-select";

@Component({
  selector: 'app-landing',
  imports: [RouterLink, TranslateModule, TranslateSelect],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {
  readonly _MyTranslateService:MyTranslateService = inject(MyTranslateService);
  readonly _TranslateService:TranslateService = inject(TranslateService);
  lang = signal("");
  ngOnInit(){
    this.lang.set(this._TranslateService.currentLang);
  }
  change(event:Event):void{
    let lang = (event.target as HTMLSelectElement).value;
    this._MyTranslateService.changeLang(lang);
    this.lang.set(lang);
  }
}
