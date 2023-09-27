# Tutorial

A tutorial is something that should be considered for any traditional roguelike, given the genre's inherent complexity.
Roguelikes lend themselves well to interactive tutorials, since a static one risks becoming outdated quickly. 
In this context, an interactive tutorial is a normal run where the player is given gameplay hints and tips on how to navigate the UI.

Tangentially, the game's difficulty curve takes on a shape that eases in new players before ramping up the challenge significantly.
The first dungeon is, fundamentally, a tutorial level of its own - limited in scope to familiarizing the player with the game's many systems.
Any starting character should be able to complete it with relatively little effort, but that said, it is not so easy as to become trivial, nor so repetitive as to become monotonous. 

## Mechanics

### Exploration

- **Traps**: the dungeon is littered with weak traps that the player will learn to avoid and use to their advantage.
- **Features**: the dungeon contains static features such as shrines that the player can interact with.
- **NPCs**: some rooms contain neutral NPCs that the player can talk to, such as merchants.

### Combat

- **Melee combat**: the dungeon is full of weak monsters that will engage predominantly in melee combat, bumping into the player.
- **Ranged combat**: occasionally, an enemy that can shoot projectiles will appear, teaching the player how to find cover and how to retaliate.
- **Healing**:
- **Corridors**: the dungeon is composed of rooms connected by corridor segments where the player can lure enemies to fight them safely 1 on 1.

### Inventory Management

- **Finding loot**: the player will familiarize themselves with the concept of grabbing items from the ground and from containers.
- **Equipping items**: the player will have several chances to find weapons and armor that they can then compare and equip.
- **Using consumables**: the player will find many consumable items and they will face enemies that know how to use these items strategically.
- **Using the quick bar**: the player will learn how the quick bar works by examining and modifying its initial loadout-dependent configuration.

## Progression

Each tutorial floor should have a well-defined reason for existing. 
It should teach the player a cohesive lesson, slowly ramping up the tension as they descend further.

### D1

- Monsters: Rat

This floor introduces the player to the very basics. The layout favors narrow corridors over large open areas.
The monsters here can all be killed easily without a weapon, though they can still surround and overwhelm the player while in the open.
No monsters have ranged or magical attacks, and they all spawn with an empty inventory, but they can still pick items up if they find any.
Item spawns on this floor are restricted mostly to basic consumables (food, rocks, bombs...) and basic weapons (sticks, swords, bows).

## Guaranteed Items

These items are guaranteed to spawn at least once for a given range of floors. This helps avoid situations where the player is extremely unlucky,
and makes runs feel more consistent in the early game. There is some randomness in how these items are spread out, making identification nontrivial.

- **D1~D5**: 2x Potion of Heal
- **D1~D5**: 1x Potion of Poison