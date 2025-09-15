using Lab2.Algorithms;
using Lab2.Containers;
using Spectre.Console;

const int missionaries = 3;
const int cannibals = 4;
const int boatSize = 2;

const int ramLimitMb = 512;
const int timeoutSec = 30;

var algorithm = new Algorithm(new BfsNodeContainer(ramLimitMb));
var bfsExecutor = new AlgorithmExecutor(algorithm);
await bfsExecutor.Run(missionaries, cannibals, boatSize, timeoutSec, ramLimitMb);

var aStarAlgorithm = new Algorithm(new AStarNodeContainer(ramLimitMb));
var aStarExecutor = new AlgorithmExecutor(aStarAlgorithm);
await aStarExecutor.Run(missionaries, cannibals, boatSize, timeoutSec, ramLimitMb);
