import { Component } from '@angular/core';
import { UrlTableComponent } from './components/url-table/url-table.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [UrlTableComponent],
  template: '<app-url-table></app-url-table>'
})
export class AppComponent {}