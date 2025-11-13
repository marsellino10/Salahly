import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'index', pathMatch: 'full' },
  { path: 'index', loadComponent: () => import('./pages/shared/landing/landing').then(c => c.Landing) },
  {
    path: '',
    loadComponent: () => import("./layouts/blank-layout/blank-layout").then(c => c.BlankLayout),
    // canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', loadComponent: () => import('./pages/shared/home/home').then(c => c.Home) },
      { path: 'login', loadComponent: () => import('./pages/shared/login/login').then(c => c.Login) },
      { path: 'signup', loadComponent: () => import('./pages/shared/registration/registration').then(c => c.Registration) },
      { path: 'browse', loadComponent: () => import('./pages/shared/browse-technicians/browse-technicians').then(c => c.BrowseTechnicians) },
    ],
  },
];
