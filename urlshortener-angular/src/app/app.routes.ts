import { Routes } from '@angular/router';
import { UrlTableComponent } from './components/url-table/url-table.component';
import { LoginComponent } from './components/login/login.component';

export const routes: Routes = [
  { path: '', component: UrlTableComponent },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' }
];