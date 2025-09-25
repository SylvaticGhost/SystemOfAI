using Lab3;
using Lab3.Entities;
using Lab3.ScoringFunctions;
using Lab3.Utils;

const int m = 9;
const int n = 7;

var game = Game.Init(m, n);

const int depth = 5;

var player1 = game.CurrentPlayer;
var player1Alg = new MiniMaxAlgorithm(game, player1, depth, new ScoringMyVarianceCenteredFunc(game.Board));

var player2 = game.GetOpponent(player1);
var player2Alg = new MiniMaxAlgorithm(game, player2, depth, new ScoringFuncAggressive(game.Board));

var host = new GameHost(player1Alg, player2Alg, new GamePrinter(game));
await host.RunAsync(game);

Console.WriteLine("Game over!");