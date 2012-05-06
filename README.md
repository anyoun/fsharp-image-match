F# Image Match
====================

F# Image Match uses a greedy algorithm with simulated annealing to reproduce an image using overlapping semi-transparent rectangles. It's inspired by Roger Alsing's [Evolisa](http://rogeralsing.com/2008/12/07/genetic-programming-evolution-of-mona-lisa/).

The project uses a combination of F# and C#, with most of the logic and display in F# and the parts that deal with pointers in C#.

When running, the program shows a realtime display of the image as it evolves. This helps really understand what's happening behind the scenes.

-------------

Copyright 2012 Will Thomas

F# Image Match is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

F# Image Match is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with F# Image Match. If not, see <http://www.gnu.org/licenses/>.
