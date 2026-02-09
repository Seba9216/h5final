import { Routes } from '@angular/router';
import { HomePage } from './pages/home-page/home-page';
import { PlaningPokerPage } from './pages/planing-poker-page/planing-poker-page';
import { DuckRacePage } from './pages/duck-race-page/duck-race-page';
import { authGuard } from './guards/auth.guard';

console.log('Routes file loaded - authGuard imported:', authGuard);

export const routes: Routes = [
    {
        path: 'login',
        loadComponent: () => import('./pages/login-page/login-page').then(m => m.LoginPage)
    },
    {
        path : '',
        component: HomePage,
        canActivate: [authGuard]
    },
    {
        path: 'poker',
        component: PlaningPokerPage,
        canActivate: [authGuard]
    },
    {
        path: 'duckrace',
        component: DuckRacePage,
        canActivate: [authGuard]
    }
];

console.log('🔥 Routes configured:', routes);