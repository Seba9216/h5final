import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConnectionAreaPoker } from './connection-area-poker';

describe('ConnectionAreaPoker', () => {
  let component: ConnectionAreaPoker;
  let fixture: ComponentFixture<ConnectionAreaPoker>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConnectionAreaPoker]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConnectionAreaPoker);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
