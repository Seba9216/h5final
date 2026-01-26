import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConnectionArea } from './connection-area';

describe('ConnectionArea', () => {
  let component: ConnectionArea;
  let fixture: ComponentFixture<ConnectionArea>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConnectionArea]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConnectionArea);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
