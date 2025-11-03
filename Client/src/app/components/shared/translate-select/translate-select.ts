import { Component, inject, signal } from '@angular/core';
import { MyTranslateService } from '../../../core/services/my-translate-service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-translate-select',
  imports: [TranslateModule],
  templateUrl: './translate-select.html',
  styleUrl: './translate-select.css',
})
export class TranslateSelect {
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
