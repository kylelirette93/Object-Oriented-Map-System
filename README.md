# Monogame Sprint Log
This is a 2D RPG-like game I'm making in monogame. 
# Sprint 4 Changes and Messages
With this sprint I've added a working Title Screen with the options to play or quit the game as well as a game over screen with the options to restart or quit to the menu. For some visual polish I also added a counter to the top right that shows you what stage your on so you can track how far you've come. There's also a slight fade out of the player character when the player dies to showcase that. The gameflow for it runs with starting at the Title screen, hitting play to start the game and then playing levels until you die.
## Updates/Fixes from last sprint
Player can now die instead of running around after health hits 0, when health reaches zero the game triggers the game-over state and brings up the menu for it. Enemies should no longer be taking multiple turns at once (didn't actually get fixed?)
## Fixes for next sprint - (Ran out of time or couldn't figure them out)
First enemy turn movement is still causing issues occasionally and moving when they shouldn't, Fireball scroll can't kill ghost(potentially because the check for scroll effects is in the FireballScroll script and not the Fireball Script), Game is drawn quite small - need to draw it bigger so it's easier to see. Ran out of time and couldn't get the Boss in a working state so it was left out of project. Wizard/Ranged Enemy can sometimes attack twice if your in range or you throw an item in their direction. Sometime enemies move more than once in a turn.

# Sprint 3 Changes and Messages
With this sprint I've added an inventory system and 4 different items as well as 2 new enemies; The ghost and a ranged enemy. The invenetory system can hold up to 5 items and ignores any items you try to pick up after that and you can use the items in your inventory with the 1-5 num keys. The health potion heals you for 2 health when used but ignores extra health over your max and also can't be used when at full health. The Fireball scroll allows you to summon a fireball and then shoot it off in anything direction dealing damage to the first enemy it hits. The Lightning Scroll deals damage to all enemies in the scene when used. The Bomb item which is my custom item works similarly to the Fireball scroll where you can choose which direction you throw it but when the bomb hits either a wall or enemy it blows up in a 9 tile radius dealing AOE damage to all enemies in the area.  For the enemies I added a Ranged enemy (Evil Wizard) that when the player is in a straight line from it they can launch a ranged attack at the player dealing damage, otherwise they move like normal. The second enemy was the Ghost who can moves through walls and can only be damaged by scroll spells like ther Fireball and Lightning scrolls. Visual effects were also added for the Enemy ranmged attack, fireball scroll attack and the bombs explosion radius as well. A text blurb also pops up in the left hand corner when a directional item is used telling you to choose your direction of attack.
## Updates/Fixes from last sprint
I've added a tracker to the top left to display the players health so you can visually see it.
## Fixes for next sprint - (Ran out of time or couldn't figure them out)
First enemy turn movement is still causing issues and moving when they shouldn't, Player can still move when their health goes below 0(add a game over scenario), Enemies sometimes take more than 1 turn in a row, Fireball scroll can't kill ghost(potentially because the check for scroll effects is in the FireballScroll script and not the Fireball Script), Game is drawn quite small - need to draw it bigger so it's easier to see.

# Sprint 2 Changes and Messages
With this sprint we added a couple new things to the project, being basic enemies, a turn-based movement system, a health and combat system as well as more obstacles generating in our maps.   The enemy class is used for handling basic enemies which we can extend to create variants later on if we want. It initializes the enemies health
as well as methods for taking damage and what happens when their health hits 0. It also has basic tracking to allow the enemies to find the player easily without getting stuck. The turn manager handles everything related to the players/enemies turns during gameplay. It starts by initializing a turnstate as well as starting the game with the players turn first.
It then handles all starting and ending of turns for both players and enemies as well as a method to process enemy turns when more than 1 is in the scene.  The health component is a simple script that both players and enemies can reference to set their health as well as a simple method for taking damage when attacked. We've also updated the map script to handle generating different sized obstacle clusters into the map to make it look better and feel fuller. Thje obstacles are designed to avoid exit doors so ytou never get softlocked as well as making sure you can complete the stage and nothing is blocked off. We had a few new changes to the game manager as well, mostly with handling enemy spawns within the game as well as handling the removal of them from the scene when they die. Other minor changes include damage indicators above the player and enemy heads when they get attacked as well as the enemies get flipped upside down when they are stunned.

# Sprint 1 Changes and Messages

For the project I split everything up into multiple scripts that all handle different things to make it easier to work with as well as following OOP principles.
For the game we have scripts for the Game, Game Manager, Tile, scripts for the 3 current tiles (walkable, non walkable and exit) and then a script for the map.

The main Game1 script is the entry point for the Monogame. It initializes the game, creates an instance of the GameManager and delegates core game loop calls like Update and Draw to the game manager and then handles basic application event like exit on escape.

The Game Manager script handles most of the work with the games logic. It's responsible for loading and storing the player texture and handles their movement on the grid, it manages the Map instance (loading, generating and drawing the maps) and it also looks for available pre-made maps 
    from the content file and then randomly chooses one to start and then lets you run through them, otherwise it loads a randomly generated map. It also detects when a player steps on an Exit tile and will load them into a new map as well as resets their position so that they spawn a safe tile in the map (not a wall).
    It also contains any helper methods I used like IsCellAccessible and ResetPlayerPosition to manage game state.

The Map script contains the tile grid and all map-related functionality. It's in charge of loading the tile textures via the content manager, it also generates a random map by creating a grid of tiles, placing walls and exit tiles on the borders and walkable tiles inside based off of set rules. 
    It also takes care of loading a predefined map from a text file if available, using characters to create specific tile types. It also provides properties for the map's overall pixel dimensions (width and height) and last but not least Drawing the map by iterating over it's tiles and calling the Draw method.

The Tile and derived tile classes define the base tile functionality and each specific type of tile respectively.  The Tile script is the base class that defines common properties like Position, Texture and whether you can walk on the tile or not. It also declares an abstract Draw method that every derived tile class uses.
    The WalkableTile class inherits from Tile.cs and it represents the tile that you can walk on and implements the Draw method to render itself.
    The NonWalkableTile class also inherits from Tile.cs and represents a wall or barrier that the player can't traverse. It also implements the Draw method to render itself.
    The ExitTile class also inherits from Tile.cs and represents tiles, that when a player moves into them, they trigger a map transition. It also implements the Draw method to render itself.

  All Together
  -Game1.cs initializes the game and creates the GameManager. 
  -The GameManager then loads content, sets up the map (using Map.cs), loads available premade maps or a random map, and handles player input and movement.
  -Map.cs (along with the tile classes) is responsible for creating and drawing the game world.
  -When the player reaches an ExitTile, the Gamemanager loads the next premade map or falls back to a randomly generated map, and resets the player's position.
  -The drawing method in GameManager applies any necessary offsets and then draws both the map and the player.

I wanted to keep everything seperate so that each class focused on just one area of functionality so that it would make it easier to maintain and expand upon as the project goes further.
