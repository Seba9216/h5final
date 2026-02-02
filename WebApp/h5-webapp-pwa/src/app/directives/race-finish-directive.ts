import { Directive, EventEmitter, HostListener, Input, Output } from '@angular/core';


@Directive({
  selector: '[appRaceFinish]'
})
export class RaceFinishDirective {
  @Input() duckData: any;
  @Output() finishLine = new EventEmitter<any>();

  @HostListener('animationend', ['$event'])
  onAnimationEnd(event: AnimationEvent) {
    if (event.animationName === 'race') {
      this.finishLine.emit(this.duckData);
    }
  }
}