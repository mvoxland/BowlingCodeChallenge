# BowlingCodeChallenge



The Challenge:
Create a program in your choice of language that can calculate the score of a full round of bowling, based on user inputs and the following rules:

Strike:
If you knock down all 10 pins in the first shot of a frame, you get a strike.
How to score: A strike earns 10 points plus the sum of your next two shots.

Spare:
If you knock down all 10 pins using both shots of a frame, you get a spare.
How to score: A spare earns 10 points plus the sum of your next one shot.

Open Frame:
If you do not knock down all 10 pins using both shots of your frame (9 or fewer pins knocked down), you have an open frame.
How to score: An open frame only earns the number of pins knocked down.

The 10th Frame:
If you roll a strike in the first shot of the 10th frame, you get 2 more shots.
If you roll a spare in the first two shots of the 10th frame, you get 1 more shot.
If you leave the 10th frame open after two shots, the game is over and you do not get an additional shot.

How to Score: The score for the 10th frame is the total number of pins knocked down in the 10th frame.


BowlingLogic
This is where all the logic for scoring a bowling game is held. To start scoring a game, construct an instance of ScoreCard and begin using Roll() on it. Scoring can be accessed through GetScore() or GetFrameScore() on the same ScoreCard object. This class library also includes some helpful extension methods for your convenience in dealing with the scorecard.

I made some use of AI to quickly refactor things as I was creating this portion, but it was very light and more a time saving measure than anything. All the architecture was designed by me, and all the implementation was written by me (or autofilled and then edited by me).

BowlingLogic.Tests
Ai generated tests for the BowlingLogic. From scratch in agentic mode with Claude Opus 4.6.

