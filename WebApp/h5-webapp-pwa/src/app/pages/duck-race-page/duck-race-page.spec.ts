import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DuckRacePage } from './duck-race-page';

describe('DuckRacePage', () => {
  let component: DuckRacePage;
  let fixture: ComponentFixture<DuckRacePage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DuckRacePage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DuckRacePage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
