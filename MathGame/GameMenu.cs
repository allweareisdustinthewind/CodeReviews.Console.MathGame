namespace MathGame
{
   /// <summary>
   /// Single item of game menu
   /// </summary>
   internal class MenuItem
   {
      // Display name without indention
      public string Name { get; set; }

      // Previous element in menu
      public MenuItem PrevItem { get; set; }

      // Next element in menu
      public MenuItem NextItem { get; set; }

      // X-position of item im console
      public int PosX { get; set; }

      // Y-position of item im console
      public int PosY { get; set; }

      // Action to perform by pressing a button
      public delegate bool CallActionHandler ();
      public CallActionHandler CallAction;

      // Colors to simulate higlights in menu
      static ConsoleColor _defaultForegroundColor   = Console.ForegroundColor;
      static ConsoleColor _defaultBackgroundColor   = Console.BackgroundColor;
      static ConsoleColor _highlightForegroundColor = ConsoleColor.Black;
      static ConsoleColor _highlightBackgroundColor = ConsoleColor.White;

      // States oof item
      public enum State
      { 
         Highlighted,
         Normal
      }

      public MenuItem (string name, int posX = 0, int posY = 0, MenuItem? prevItem = null, MenuItem? nextItem = null, CallActionHandler? handler = null)
      {
         Name = name;
         PrevItem = prevItem!;
         NextItem = nextItem!;
         PosX = posX;
         PosY = posY;
         CallAction = handler!;
      }

      /// <summary>
      /// Connect actual item with a new item 
      /// </summary>
      /// <param name="name">Display name of new item</param>
      /// <param name="posX">X-position</param>
      /// <param name="posY">Y-position</param>
      /// <param name="handler">Action to perform</param>
      /// <returns>New item, which connects with existing item</returns>
      public MenuItem ConnectWith (string name, int posX, int posY, CallActionHandler? handler = null)
      {
         MenuItem item = new (name, posX, posY);
         item.PrevItem = this;
         NextItem = item;
         item.CallAction = handler!;

         return item;
      }

      /// <summary>
      /// Change state of item - highlight / normal
      /// </summary>
      /// <param name="state">State to set</param>
      public void SetState (State state)
      {
         Console.SetCursorPosition (PosX, PosY);

         if (state == State.Highlighted)
         {
            Console.BackgroundColor = _highlightBackgroundColor;
            Console.ForegroundColor = _highlightForegroundColor;
            Console.Write (Name);

            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
         }
         else
         {
            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
            Console.Write (Name);
         }
      }

      /// <summary>
      /// Return next item in hierarchy, which can be selected.
      /// Only items with assigned actions can be selected
      /// </summary>
      /// <returns>Next item to select</returns>
      public MenuItem? GetNextSelectable ()
      {
         MenuItem item = NextItem;
         while (item != null && item.CallAction == null)
            item = item.NextItem;

         return item;
      }

      /// <summary>
      /// Return previous item in hierarchy, which can be selected.
      /// Only items with assigned actions can be selected
      /// </summary>
      /// <returns>Previous item to select</returns>
      public MenuItem? GetPrevSelectable ()
      {
         MenuItem item = PrevItem;
         while (item != null && item.CallAction == null)
            item = item.PrevItem;

         return item;
      }

      /// <summary>
      /// Show item as activated (set "(*)" left from item's name)
      /// </summary>
      /// <param name="selectOnScreen">If true, then item will be also selected with inversed color</param>
      public void Activate (bool selectOnScreen = true)
      {
         // Already activated? Nothing to do.
         if (Name.Contains ("(*)"))
            return;

         Name = Name.Replace ("( )", "(*)");

         Console.SetCursorPosition (PosX, PosY);
         if (selectOnScreen)
         {
            Console.BackgroundColor = _highlightBackgroundColor;
            Console.ForegroundColor = _highlightForegroundColor;
            Console.Write (Name);

            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
         }
         else
         {
            Console.BackgroundColor = _defaultBackgroundColor;
            Console.ForegroundColor = _defaultForegroundColor;
            Console.Write (Name);
         }
      }

      /// <summary>
      /// Show item as deactivated (replace "(*)" with "( )"  left from item's name)
      /// </summary>
      public void Deactivate ()
      {
         // Already deactivated? Nothing to do.
         if (Name.Contains ("( )"))
            return;

         Name = Name.Replace ("(*)", "( )");

         Console.BackgroundColor = _defaultBackgroundColor;
         Console.ForegroundColor = _defaultForegroundColor;

         Console.SetCursorPosition (PosX, PosY);
         Console.Write (Name);
      }
   }

   /// <summary>
   /// Class to display game's menu and process user's choice
   /// </summary>
   public class GameMenu
   {
      // Game's settings
      Settings _settings = new ();

      // Items for actions "Start game", "Settings" and "Show all results"
      MenuItem? _startGameItem;
      MenuItem? _settingsItem;
      MenuItem? _showResultItem;
      
      // Begin and end of settings block
      MenuItem? _settingsBlockBegin;
      MenuItem? _settingsBlockEnd;
      
      // Items of difficulty levels
      MenuItem? _itemEasy;
      MenuItem? _itemNormal;
      MenuItem? _itemHard;

      // Items of game's mode (random / fix)
      MenuItem? _itemModeRandom;
      MenuItem? _itemModeFix;

      // Items of mathematical operations
      MenuItem? _itemOpAdd;
      MenuItem? _itemOpSub;
      MenuItem? _itemOpMul;
      MenuItem? _itemOpDiv;

      // Actual selected menu item
      MenuItem? _curItem;

      // Y-position of item "Show all results"
      int _posShowResult;

      // If true, then block with all settings of the game is visible
      bool _isSettingsShown;

      // All game's logic is hier
      GameLogic? _logic = null;

      public GameMenu ()
      {
         _logic = new (_settings);
      }

      /// <summary>
      /// Show menu on a screen
      /// </summary>
      void Display ()
      {
         DisplayGameDescription ();
         
         // Go through all menu items and show them.
         // Important: menu items form a cyclic list - last item point to the first one
         MenuItem? item = _startGameItem;
         if (item != null)
         {
            do
            {
               Console.SetCursorPosition (item.PosX, item.PosY);
               Console.Write (item.Name);
               item = item.NextItem;
            }
            while (item != null && item != _startGameItem);
         }

         _curItem?.SetState (MenuItem.State.Highlighted);
      }

      /// <summary>
      /// Create title of a game
      /// </summary>
      private void DisplayGameDescription ()
      {
         Console.Clear ();

         string indent = new (' ', 20);
         string line = new ('─', 38);

         ConsoleColor color = Console.ForegroundColor;
         Console.ForegroundColor = ConsoleColor.Yellow;

         Console.WriteLine ();
         Console.WriteLine ("{0}┌{1}┐", indent, line);
         Console.WriteLine ("{0}│{1}MathGame{2}│", indent, new string (' ', 15), new string (' ', 15));
         Console.WriteLine ("{0}│{1}Test your mathematical skills{2}│", indent, new string (' ', 5), new string (' ', 4));
         Console.WriteLine ("{0}└{1}┘\n", indent, line);

         Console.ForegroundColor = color;

         CreateMenuStructure ();
      }

      /// <summary>
      /// Fill internal data with names and positions of all menu items
      /// </summary>
      void CreateMenuStructure ()
      {
         if (_startGameItem != null)
            return;

         // Offset of standard menu items from left border of console window
         const int offsetMainMenu = 8;

         // Offset in symbols from left border of main menu. Will be used to displaoy setting's block
         const int offsetSettingsMenu = 4;

         var (x, y) = Console.GetCursorPosition ();
         x += offsetMainMenu;

         // Create menu with items "Start game", "Settings", "Show all results" and "Exit"
         _startGameItem = new ("Start game", x, y, handler: ActionStartGame);
         _settingsItem   = _startGameItem.ConnectWith ("Settings", x, ++y, ActionSettings);
         _showResultItem = _settingsItem.ConnectWith ("Show all results", x, ++y, ActionShowResults);
         _posShowResult = y;

         var exitItem    = _showResultItem.ConnectWith ("Exit", x, ++y, ActionExit);

         // Connect item "Exit" with "Start game" to make a loop by selecting
         exitItem.NextItem = _startGameItem;
         _startGameItem.PrevItem = exitItem;

         int posX = x + offsetSettingsMenu;
         int posY = _posShowResult;

         // Create menu for setting's block
         _settingsBlockBegin = new ("Difficulty: ", posX, posY);
         _settingsBlockBegin.PrevItem = _settingsItem;

         // Append difficulty levels: easy, normal and hard
         _itemEasy   = _settingsBlockBegin.ConnectWith ("(*) easy ", posX += _settingsBlockBegin.Name.Length, posY, ActionChangeDifficulty);
         _itemNormal = _itemEasy.ConnectWith ("( ) normal ", posX += _itemEasy.Name.Length, posY, ActionChangeDifficulty);
         _itemHard   = _itemNormal.ConnectWith ("( ) hard ",   posX += _itemNormal.Name.Length, posY, ActionChangeDifficulty);

         // Append modes: ramdom select operation and fix operation
         posX = x + offsetSettingsMenu;
         MenuItem itemMode = new ("Operation will be set: ", posX, ++posY);
         itemMode.PrevItem = _itemHard;
         _itemHard.NextItem = itemMode;

         _itemModeRandom = itemMode.ConnectWith ("(*) randomly ", posX += itemMode.Name.Length, posY, ActionChangeMode);
         _itemModeFix = _itemModeRandom.ConnectWith ("( ) as following:", posX += _itemModeRandom.Name.Length, posY);

         // Append mathematical operations to perform
         _itemOpAdd = new ("( ) addition", posX, ++posY, handler: ActionChangeOperation);
         _itemOpAdd.PrevItem = _itemModeFix;
         _itemModeFix.NextItem = _itemOpAdd;

         _itemOpSub = _itemOpAdd.ConnectWith ("( ) subtraction",    posX, ++posY, ActionChangeOperation);
         _itemOpMul = _itemOpSub.ConnectWith ("( ) multiplication", posX, ++posY, ActionChangeOperation);
         _itemOpDiv = _itemOpMul.ConnectWith ("( ) division",       posX, ++posY, ActionChangeOperation);

         _settingsBlockEnd = _itemOpDiv;
         _settingsBlockEnd.NextItem = _showResultItem;

         _curItem = _startGameItem;
      }

      /// <summary>
      /// Show menu item as selected / not seleted
      /// </summary>
      /// <param name="item"></param>
      void SelectItem (MenuItem? item)
      {
         _curItem?.SetState (MenuItem.State.Normal);
         _curItem = item;
         _curItem?.SetState (MenuItem.State.Highlighted);
      }

      /// <summary>
      /// Shift menu's part from item "Show all results" to "Exit" to given number of lines to top / bottom
      /// </summary>
      /// <param name="offset">Count of lines to shift</param>
      void MoveMenu (MenuItem itemToMove, int offset)
      {
         do
         {
            itemToMove.PosY = offset++;
            itemToMove = itemToMove.NextItem;
         }
         while (itemToMove != _startGameItem);
      }

      /// <summary>
      /// Set given menu items in state "inactive"
      /// </summary>
      /// <param name="items"></param>
      void DeactivateItems (MenuItem [] items)
      { 
         foreach (var item in items)
            item.Deactivate ();
      }

      /// <summary>
      /// Start new game round (menu "Start game")
      /// </summary>
      /// <returns></returns>
      bool ActionStartGame ()
      {
         _logic?.PlayRound ();

         // Select "Start game" in end of current game's round
         _curItem = _startGameItem;

         // If block with game's settings open, then collapse it, then show main menu
         if (_isSettingsShown)
            ActionSettings ();
         else
            Display ();

         return false;
      }

      /// <summary>
      /// Show / hide block with game's settings (menu "Settings")
      /// </summary>
      /// <returns></returns>
      bool ActionSettings ()
      {
         _isSettingsShown = !_isSettingsShown;

         // Insert or remove setting's block to main menu and then display menu with changed structure on the screen
         if (_isSettingsShown)
         {
            if (_settingsBlockEnd != null)
               MoveMenu (_showResultItem!, _settingsBlockEnd.PosY + 1);

            if (_settingsItem != null)
               _settingsItem.NextItem = _settingsBlockBegin!;

            if (_showResultItem != null)
               _showResultItem.PrevItem = _settingsBlockEnd!;
         }
         else
         {
            MoveMenu (_showResultItem!, _posShowResult);

            if (_settingsItem != null)
               _settingsItem.NextItem = _showResultItem!;

            if (_showResultItem != null)
               _showResultItem.PrevItem = _settingsItem!;
         }

         _curItem = _settingsItem;

         Display ();

         return false;
      }

      /// <summary>
      /// Set difficulty level (menu "Difficulty" in setting's block)
      /// </summary>
      /// <returns></returns>
      bool ActionChangeDifficulty ()
      {
         if (_curItem == _itemEasy)
         {
            DeactivateItems ([_itemNormal!, _itemHard!]);
            _settings.Difficulty = Settings.Level.Easy;
         }
         else if (_curItem == _itemNormal)
         {
            DeactivateItems ([_itemEasy!, _itemHard!]);
            _settings.Difficulty = Settings.Level.Normal;
         }
         else
         {
            DeactivateItems ([_itemEasy!, _itemNormal!]);
            _settings.Difficulty = Settings.Level.Hard;
         }

         _curItem?.Activate ();

         return false;
      }

      /// <summary>
      /// Set game mode (select operation randomly or use fixed operation)
      /// </summary>
      /// <returns></returns>
      bool ActionChangeMode ()
      {
         DeactivateItems ([_itemModeFix!, _itemOpAdd!, _itemOpSub!, _itemOpMul!, _itemOpDiv!]);
         _curItem?.Activate ();
         _settings.Operation = "@";

         return false;
      }

      /// <summary>
      /// Set fixed arithmmetical operation
      /// </summary>
      /// <returns></returns>
      bool ActionChangeOperation ()
      {
         if (_curItem == _itemOpAdd)
         {
            DeactivateItems ([_itemModeRandom!, _itemOpSub!, _itemOpMul!, _itemOpDiv!]);
            _settings.Operation = "+";
         }
         else if (_curItem == _itemOpSub)
         {
            DeactivateItems ([_itemModeRandom!, _itemOpAdd!, _itemOpMul!, _itemOpDiv!]);
            _settings.Operation = "-";
         }
         else if (_curItem == _itemOpMul)
         {
            DeactivateItems ([_itemModeRandom!, _itemOpAdd!, _itemOpSub!, _itemOpDiv!]);
            _settings.Operation = "x";
         }
         else
         {
            DeactivateItems ([_itemModeRandom!, _itemOpAdd!, _itemOpSub!, _itemOpMul!]);
            _settings.Operation = "/";
         }

         _curItem?.Activate ();
         _itemModeFix?.Activate (false /*selectOnScreen*/);

         return false;
      }

      /// <summary>
      /// Display results of all games (menu "Show all results")
      /// </summary>
      /// <returns></returns>
      bool ActionShowResults ()
      {
         _logic?.ShowResults ();

         // Restore main menu (collapse opened setting's block if needed) and highlight "Start game" item
         _curItem = _startGameItem;

         if (_isSettingsShown)
            ActionSettings ();
         else
            Display ();

         return false;
      }

      /// <summary>
      /// Exit from game (menu "Exit")
      /// </summary>
      /// <returns>True means, that actual game round must be terminated</returns>
      bool ActionExit ()
      {
         return true;
      }

      /// <summary>
      /// Process user's actions in menu
      /// </summary>
      public void Run ()
      {
         Display ();

         Console.CursorVisible = false;

         bool exitGame = false;
         while (!exitGame)
         {
            var key = Console.ReadKey (true).Key;
            switch (key)
            {
               // Select previous item
               case ConsoleKey.UpArrow:
               case ConsoleKey.LeftArrow:
                  SelectItem (_curItem?.GetPrevSelectable ());
                  break;

               // Select next item
               case ConsoleKey.DownArrow:
               case ConsoleKey.RightArrow:
                  SelectItem (_curItem?.GetNextSelectable ());
                  break;

               // Exit game
               case ConsoleKey.Escape:
                  exitGame = true;
                  break;

               // Perform action
               case ConsoleKey.Enter:
               case ConsoleKey.Spacebar:
                  exitGame = _curItem?.CallAction () ?? false;
                  break;
            }
         }
      }
   }
}
