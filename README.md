[//]: # (View this readme at https://github.com/thecatisnumber1/icfp2023)

![Squigglizer](imgs/squigglizer.png)

# The Cat is #1!! ICFP 2023

## Project Structure

Most of our code is written in C#, with some support from Python.

### ConsoleRunner

Runs solvers from the command line. Quite hacked up for various purposes.

### Lib

Has most of the interesting code. Solvers and the shared code that handles parsing problems and submitting solutions. Some utility functions too.

### Squigglizer

GUI runner. Render problems, shows meta data and can run solvers. Can be fun to watch.

### How to Build/Run

1. Open ICFP2023.sln in Visual Studio
2. Build all
3. Run Squigglizer
4. Pick Problem
5. Pick solver
6. Click Run
7. ???
8. Concert!

## Major approaches

### Simulated Annealing

This is all we did. However a naive implementation of the scoring function was too slow for annealing to be useful. To get around this we made a major simplification to the problem. Most of our annealers weren't able to move musicians by tiny amounts. Instead they worked off of a fixed set of points, often called slots in our codebase. A musician could be moved from one slot to another by the anealer but the slots themselves could not move. We were able to speed up scoring by precomputing line of sight with the pessimistic assumption that every slot would be filled. In reallity there were many more slots than musicians but since slots closer to audience members were more valuable, they'd tend to always be filled up anyway. The downside with this approach was that if the slots were chosen badly (or even slightly unoptimally) there was nothing the annealer could do to fix it. We tried a number of different way's to intelligently place slots.

#### Let's get crackin

![Let's get crackin](imgs/lets_get_crackin.png)

#### Honeycomber

![Honeycomber](imgs/honeycomber.png)

#### Lens Crafter

![Lens](imgs/lens.png)

## Support stuff

We're kind of a competetive lot and really enjoy seeing how we're doing in real time. To facilitate that, we build our own scoreboard so we could see places below 10:

![scoreboard](imgs/scoreboard.gif)

There's also a page tabling all the problems with a bunch of random metadata about all the problems.

![table](imgs/prob_table.png)

As well as a page per problem so we could see how submissions were doing:

![table](imgs/sol_table.gif)
All of these were sortable, mostly for copy and paste reasons.
