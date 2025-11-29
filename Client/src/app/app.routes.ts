import { Routes } from '@angular/router';
import { technicianAuthGuard } from './core/guards/technician-auth.guard';
import { customerAuthGuard } from './core/guards/customer-auth.guard';

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
      { path: 'technicians/:id/profile', loadComponent: () => import('./pages/technician/technician-profile/technician-profile').then(c => c.TechnicianProfile) },
    ],
  },
  {
    path: '',
    loadComponent: () => import('./layouts/technician-layout/technician-layout').then(c => c.TechnicianLayout),
    canActivate: [technicianAuthGuard],
    children: [
      {
        path: 'complete-profile',
        loadComponent: () => import('./pages/technician/complete-profile/complete-profile').then(c => c.CompleteProfile),
      },
      {
        path: 'browse-opportunities',
        loadComponent: () => import('./pages/technician/browse-opportunities/browse-opportunities').then(c => c.BrowseOpportunities),
      },
      {
        path: 'history',
        loadComponent: () => import('./pages/technician/history/history').then(c => c.History),
      },
    ],
  },
  {
    path: '',
    loadComponent: () => import('./layouts/auth-layout/auth-layout').then(c => c.AuthLayout),
    canActivate: [customerAuthGuard],
    children: [
      {
        path: 'customer-profile',
        loadComponent: () => import('./pages/customer/customer-profile/customer-profile').then(c => c.CustomerProfile),
      },
      {
        path: 'show-services-requested',
        loadComponent: () => import('./pages/customer/show-services-requested/show-services-requested').then(c => c.ShowServicesRequested),
      },
      {
        path: 'service-request-details/:id',
        loadComponent: () => import('./pages/customer/service-request-details/service-request-details').then(c => c.ServiceRequestDetails),
      },
      {
        path: 'service-request-form',
        loadComponent: () => import('./pages/customer/service-request-form/service-request-form').then(c => c.ServiceRequestForm),
      }
    ],
  },
  {
    path:'',
    loadComponent: () => import('./layouts/admin-layout/admin-layout').then(c => c.AdminLayout),
    //canActivate: [AdminAuthGuard],
    children:[
      {
        path:'dashboard',
        loadComponent: () => import('./pages/admin/dashboard/dashboard').then(c => c.Dashboard),
      },
      {
        path:'crafts',
        loadComponent: () => import('./pages/admin/craft/craft').then(c => c.Craft),
      },
      {
        path:'areas',
        loadComponent: () => import('./pages/admin/area/area').then(c => c.Area),
      },
    ]
  }
];
