# Magic

Proof of concept project game which allows players to program their own spell logic. With flexibility, game-like constraints and "realistic" physics in mind it aims to create a competitive environment where players can show off and put their spell making skills to the test.

A secondary aim of the project is to be accessible to non-programmers with an interface similar to that of the Scratch programming language, making it more accessible and educational. 

## Energy

Energy is the main focus of each spell. It is an abstract measure and is used up by any action done for a spell. It may be physically manifested, used to push stuff around or apply buffs. 

## Flexibility

The logic allows the player's avatar to do all sorts of actions with their available energy which account for most of the things you'd need to implement your favorite spell from your favorite game. These include buff/debuff effects, summoning portals, spawning creatures or consumables, and operations on energy manifestations like adding/removing energy, changing the element (fire, water, ect), changing the shape (sphere, spike/cone), applying forces and more. 

## Constraints

The game imposes some constraints in the players to avoid making them overpowered. These are things like allowing the player to only "focus" on just a few manifestations, which limits their ability to overwhelm the enemy. Also with limited energy, players are prohibited from manifesting or manipulating powers beyond their experience level. 

## Physics

The game's physics, although fantastical, are still based on realistic physics principles, which lays down a level playing field and makes the learning curve much flatter. Theae are simulations of conservation of energy, pressure, temperature, element interactions, speed limits (a limitation of the physics engine, thus much lower than the speed of light) and more. 
