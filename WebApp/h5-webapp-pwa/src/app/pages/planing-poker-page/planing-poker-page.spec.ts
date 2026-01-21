import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaningPokerPage } from './planing-poker-page';

describe('PlaningPokerPage', () => {
  let component: PlaningPokerPage;
  let fixture: ComponentFixture<PlaningPokerPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaningPokerPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlaningPokerPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
