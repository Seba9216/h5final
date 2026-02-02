import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WinnerModal } from './winner-modal';

describe('WinnerModal', () => {
  let component: WinnerModal;
  let fixture: ComponentFixture<WinnerModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WinnerModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WinnerModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
