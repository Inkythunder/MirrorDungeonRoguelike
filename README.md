# Mirror Dungeon

A 2D, top-down, grid-based roguelike game made in Unity and using CC0 artwork for sprites and animations.
Travel to and from the mirror dungeon to unlock the exit and descend deeper while evading or battling enemies.

<p>
  <img src=repo_images/Screenshot_2.png width="400" alt="Gameplay start">
  <img src=repo_images/Screenshot_1.png width="400" alt="Gameplay 2">
</p>

## Features

- Fully procedurally generated dungeons using Binary Space Partitioning to divide the level into regions 
then an agent based carve algorithm to generate unique, cave-like structures for each room. 
Rooms are then connected with corridors using a nearest-neighbour algorithm.

- Two identical world exist, one a perfect copy of the other. In the mirror world, enemies are petrified into
statues and can be moved or evaded. Upon returning to the real world, they return to life. The player must
use the mirror world to unlock the exit of each level and escape but spending too long in there will awaken
more powerful enemies.

- Combat operates in turns and enemies will chase the player using a flow-field starting from the player.
This is an efficient solution that allows for a high number of enemies to chase the player at once without
the need for recomputation per enemy but rather only once per turn.

- There are three main enemy types. A small grunt that will wander around aimlessly but hurt the player if
they make contact. A larger thug that will actively chase the player once spotted. And a key carrier that cannot
die which the player must use to unlock the exit. The mirror world contains a fourth enemy type that punishes
the player for spending too long in there.

- Items spawn throughout the levels gaining power the deeper the player gets. Potions will heal the player for 
10 hit poins and weapons increase the player's attack damage.

- A HUD displays information to the player about their current status in game. A message log informs the 
player of events such as damage taken, or enemy killed. Damage numbers are displayed above any entity that 
takes damage. Enemies display health bars to allow the player to keep track of who's taken damage so far.

## How to Play

**Objective**: The aim of the game is to descend as far as possible in the dungeon and see how far you can make it.
The exit starts locked and must be unlocked by placing the key carrier on the glyph. The key carrier can be
found by entering the mirror world where an arrow will guide the player to a glowing enemy. This is the key carrier.

### Controls:
| Key             | Action        |
|-----------------|---------------|
| `↑` `←` `↓` `→` | Move          |
| `Q`             | Switch Worlds |
| `1`             | Potion        |
| `Space`         | Wait          |
| `F3 + G`        | Debug Mode    |

**Mirror world**: Switching to the mirror world causes all enemies from the real world to turn to stone. They will not move unless
pushed by the player and they cannot harm the player while they are statues. In the mirror world, the glyph
becomes active and by placing the key carrier on it, the exit will be unlocked and the player can descend to the
next level.

**Game over**: The runs ends when the player is killed.

## Debug Mode

Enter debug mode by holding F3 and tapping G.

This will display data about the game including:
- Level seed
- Turn number
- X and Y coordinates of the player
- Current world
- Number of turns in the mirror world
- Current active guardian count

## Attributions

| Asset | Author | Source | License | Obtained |
|---|---|---|---|---|
| DungeonTileset II | 0x72 | https://0x72.itch.io/dungeontileset-ii | CC0 | 2026-05-02 |
