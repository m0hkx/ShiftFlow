import { Routes } from '@angular/router';

import { authGuard } from './auth/auth.guard';
import { Home } from './home/home';
import { Login } from './login/login';
import { Layout } from './layout/layout';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: '', 
    component: Layout, 
    canActivate: [authGuard], 
    children: [
    { path: '', component: Home, canActivate: [authGuard] },
  ]},
  { path: '**', redirectTo: '' },
];
