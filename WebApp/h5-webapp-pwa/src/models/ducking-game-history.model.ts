import { DuckingGameModel } from "./ducking-game.model";

export interface DuckingGameHistoryModel {
  id: number;
  duckingGames: DuckingGameModel[];
}
