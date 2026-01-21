import { Routes } from '@angular/router';
import { HomePage } from './pages/home-page/home-page';
import { PlaningPokerPage } from './pages/planing-poker-page/planing-poker-page';
import { DuckRacePage } from './pages/duck-race-page/duck-race-page';

export const routes: Routes = [
    {
        path : '',
        component: HomePage
    },
    {
        path: 'poker',
        component: PlaningPokerPage
    },
    {
        path: 'duckrace',
        component: DuckRacePage
    }
];
