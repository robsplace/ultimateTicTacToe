# **Ultimate Tic Tac Toe**

## Introduction

The game of *Ultimate* Tic Tac Toe extends the original, classic game.  
You can read more about the rules [here](https://mathwithbaddrawings.com/2013/06/16/ultimate-tic-tac-toe/).

## Implementation
This version of Ultimate Tic Tac Toe is implemented using .NET WPF, with support for AI plugins that implement the `UltimateTicTacToe.Core.Interfaces.IGameAi` interface.  You can play against another person, against an AI, or pit an AI against an AI.  

## Making Your Own AI
There are two options for making your own AI.  The first option is to implement an instance of 
`UltimateTicTacToe.Core.Interfaces.IGameAi` yourself.  Given a `Game`object, select a board and spot on the board to make your move.  
If you're not as familiar with AI, another option is to extend the `UltimateTicTacToe.Ai.Minimax` class to use standard [Minimax algorithm with alpha/beta pruning](https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning) and implement your own Utility function.