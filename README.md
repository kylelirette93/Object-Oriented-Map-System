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
