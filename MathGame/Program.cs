using MathGame;

// -------------------------------------------------------
//          Global constants and variables   
// -------------------------------------------------------

// Colors to simulate higlights in menu
var defaultForegroundColor  = Console.ForegroundColor;
var defaultBackgroundColor  = Console.BackgroundColor;
var higlightForegroundColor = ConsoleColor.Black;
var higlightBackgroundColor = ConsoleColor.White;

// Position of higlighted menu's element
int menuPosX = 0;
int menuPosY = 0;

// Position of highlighted item im menu "Change difficulty"
int menuDifficultyPosX = 0;

// Items of main menu
string [] menuItems   = ["Start game", "Change difficulty (currently: easy)", "Show all results", "Exit"];

// Itemd of main menu + names of difficulty levels
string [] menuItemsEx = ["Start game", "Change difficulty (currently: easy)", "easy", "normal", "hard", "Show all results", "Exit"];

// Index of currently and previously selected menu items 
int activeMenuItem = 0;
int prevMenuItem   = -1;

// Actual level of difficulty: 0 - easy, 1 - normal, 2 - hard
Levels difficulty = Levels.easy;

// Number of game round
int gameNumber = 0;

// Time spent in actual round
int totalSeconds = 0;
int totalMinutes = 0;
int totalHours   = 0;

// True if menu to change level of difficulty activ, false otherweise
bool inMenuDifficulty = false;

// Offset of menu items from left border of console window
const string indentMenuItem = "        ";

// List of all results of game
List<GameResult> results = new ();

Random random = new ();
System.Threading.Timer ?timer = null;

bool pauseInTimer = false;


// ---------------------------------------------
//        Entry point (game loop)   
// ---------------------------------------------
Run ();


// ---------------------------------------------
//        Supplemented functions
// ---------------------------------------------

//
// Creates title of game
//
void ShowGameDescription ()
{
   Console.ForegroundColor = defaultForegroundColor;
   Console.BackgroundColor = defaultBackgroundColor;

   Console.Clear ();

   string indent = new (' ', 20);
   string line = new ('─', 38);

   Console.ForegroundColor = ConsoleColor.Yellow;

   Console.WriteLine ();
   Console.WriteLine ("{0}┌{1}┐", indent, line);
   Console.WriteLine ("{0}│{1}MathGame{2}│", indent, new string (' ', 15), new string (' ', 15));
   Console.WriteLine ("{0}│{1}Test your mathematical skills{2}│", indent, new string (' ', 5), new string (' ', 4));
   Console.WriteLine ("{0}└{1}┘\n", indent, line);

   Console.ForegroundColor = defaultForegroundColor;
}

//
// Displays menu
//
void ShowGui ()
{
   ShowGameDescription ();

   Console.Write (indentMenuItem);

   // Save start positions of menu (begin of "Start game"-item)
   (menuPosX, menuPosY) = Console.GetCursorPosition ();
   Console.WriteLine (menuItems [0]);

   // Display rest of menu 
   for (int i = 1; i < menuItems.Length; ++i)
      Console.WriteLine ($"{indentMenuItem}{menuItems [i]}");

   Console.WriteLine ();

   activeMenuItem = 0;
   prevMenuItem = -1;
   HighlightActiveMenuItem ();
}

//
// Displays menu of game with names of difficulty levels
//
void ShowGuiEx ()
{
   ShowGameDescription ();

   string indentSubMenu = new (' ', 7);

   foreach (string item in menuItemsEx)
   {
      // Special processing of first difficulty level - we need to know position in console, where this text is
      if (item == "easy")
      {
         Console.Write ($"{indentMenuItem}{indentSubMenu}");
         (menuDifficultyPosX, _) = Console.GetCursorPosition ();
         Console.WriteLine (item);
         continue;
      }

      string menuItem = (item == "normal" || item == "hard") ? $"{indentSubMenu}{item}" : item;
      Console.WriteLine ($"{indentMenuItem}{menuItem}");
   }

   Console.WriteLine ();
}

//
// Show active menu item as selected (invert colors of text and background)
//
void HighlightActiveMenuItem (bool hidePrevSelection = true)
{
   int posX = inMenuDifficulty ? menuDifficultyPosX : menuPosX;
   int posY = menuPosY;
   
   string [] menu = inMenuDifficulty ? menuItemsEx : menuItems;

   // Unmark ppreviously selected item
   if (prevMenuItem >= 0 && hidePrevSelection)
   {
      Console.SetCursorPosition (posX, posY + prevMenuItem);
      Console.ForegroundColor = defaultForegroundColor;
      Console.BackgroundColor = defaultBackgroundColor;
      Console.Write (menu [prevMenuItem]);
   }

   // Higlight menu item, which actual activ is
   Console.SetCursorPosition (posX, posY + activeMenuItem);
   Console.ForegroundColor = higlightForegroundColor;
   Console.BackgroundColor = higlightBackgroundColor;
   Console.Write (menu [activeMenuItem]);

   prevMenuItem = activeMenuItem;
}

//
// Exit from application and say "farewell"
//
void Exit ()
{
   Console.Clear ();
   Console.WriteLine ("\n\n       Good bye!");
   Console.CursorVisible = true;
}

//
// Processing user's choice
//
bool ProcessMenuAction ()
{
   if (inMenuDifficulty)
   {
      difficulty = (Levels) (activeMenuItem - 2);
      inMenuDifficulty = false;

      string level = difficulty switch
      {
         Levels.easy => "easy",
         Levels.normal => "normal",
         _ => "hard"
      };

      string difficultyName = string.Format ("Change difficulty (currently: {0})", level);
      menuItems [1] = difficultyName;
      menuItemsEx [1] = difficultyName;

      ShowGui ();
      return true;
   }

   switch (activeMenuItem)
   {
      // Start game
      case 0:
         StartGame ();
         break;

      // Change difficulty
      case 1:
         ShowGuiEx ();
         inMenuDifficulty = true;
         activeMenuItem = (int) difficulty + 2; // 2 because of two items of menu from above: "Start game" and "Change difficulty"
         HighlightActiveMenuItem (false);
         break;

      // Show all results
      case 2: 
         ShowAllResults ();
         break;

      // Exit
      case 3: 
         return false;

      default:
         break;
   }

   return true;
}

//
// Proocessing pressing "arrow up" in menu
//
void SelectPrevMenuItem ()
{
   int minVal = 0;
   int nextVal = menuItems.Length - 1;

   // Constrain movement in menu "Change difficulty" only on items "easy", "normal" and "hard"
   if (inMenuDifficulty)
   {
      minVal = 2;
      nextVal = 4;
   }

   --activeMenuItem;
   if (activeMenuItem < minVal)
      activeMenuItem = nextVal;

   HighlightActiveMenuItem ();
}

//
// Proocessing pressing "arrow down" in menu
//
void SelectNextMenuItem ()
{
   int maxVal = menuItems.Length;
   int nextVal = 0;

   // Constrain movement in menu "Change difficulty" only on items "easy", "normal" and "hard"
   if (inMenuDifficulty)
   {
      maxVal = 5;
      nextVal = 2;
   }

   ++activeMenuItem;
   if (activeMenuItem >= maxVal)
      activeMenuItem = nextVal;

   HighlightActiveMenuItem ();
}

//
// Update count of questions with correct and wrong answers after receiving antwort from user
//
void UpdateStatistic (int curQuestion, int maxQuestions, int correctAnswers = 0, int wrongAnswers = 0)
{
   var (x, y) = Console.GetCursorPosition ();
   Console.SetCursorPosition (0, 0);

   string indent = new (' ', 5);

   Console.ForegroundColor = ConsoleColor.Yellow;
   Console.WriteLine ($"\n               Question {curQuestion} of {maxQuestions}\n");

   Console.ForegroundColor = ConsoleColor.Green;
   Console.Write ($"{indent}Correct answers: {correctAnswers}   ");

   Console.ForegroundColor = ConsoleColor.Red;
   Console.WriteLine ($"Wrong answers: {wrongAnswers}");

   Console.ForegroundColor = ConsoleColor.Yellow;
   string line = new ('─', 37);
   Console.WriteLine ($"{indent}{line}\n");

   Console.ForegroundColor = defaultForegroundColor;

   // For the first question we don't restore previous position in console, because after updating statistic
   // will be launched timer. Position of timer will be dealed separat, therefore actual position in console must not be changed 
   if (curQuestion > 1)
      Console.SetCursorPosition (x, y);
}

//
// Create two operands for operation '+'
//
void GenerateOperandsForAddition (out int op1, out int op2)
{
   op1 = 0;
   op2 = 0;

   switch (difficulty)
   {
      case Levels.easy:
         op1 = random.Next (1, 10);
         op2 = random.Next (1, 10);
         break;

      case Levels.normal:
         op1 = random.Next (10, 30);
         op2 = random.Next (10, 30);
         break;

      case Levels.hard:
         op1 = random.Next (50, 100);
         op2 = random.Next (50, 100);
         break;
   }
}

//
// Create two operands for operation '-'
//
void GenerateOperandsForSubtraction (out int op1, out int op2)
{
   op1 = 0;
   op2 = 0;

   int from = 0;
   int to = 0;
   int minDifference = 0;

   switch (difficulty)
   {
      case Levels.easy:
         from = 1;
         to = 10;
         minDifference = 1;
         break;

      case Levels.normal:
         from = 10;
         to = 30;
         minDifference = 7;
         break;

      case Levels.hard:
         from = 50;
         to = 100;
         minDifference = 30;
         break;
   }

   // Ensure, that op2 < op1 (prevent from getting negative results).
   // To make the game a bit more challlenged ensure, that two operands are not too near
   for (; ; )
   {
      op1 = random.Next (from, to + 1);
      op2 = random.Next (from, to + 1);
      if (op2 < op1 && op1 - op2 >= minDifference)
         break;
   }
}

//
// Create two operands for operation 'x'
//
void GenerateOperandsForMultiplication (out int op1, out int op2)
{
   op1 = 0;
   op2 = 0;

   int from = 0;
   int to = 0;

   switch (difficulty)
   {
      case Levels.easy:
         from = 2;
         to = 10;
         break;

      case Levels.normal:
         from = 10;
         to = 21;
         break;

      case Levels.hard:
         from = 30;
         to = 51;
         break;
   }

   op1 = random.Next (from, to);
   op2 = random.Next (from, to);
}

//
// Create two operands for operation '/'
//
void GenerateOperandsForDivision (out int op1, out int op2)
{
   op1 = 0;
   op2 = 0;

   int from = 0;
   int to = 0;

   switch (difficulty)
   {
      case Levels.easy:
         from = 4;
         to = 101;
         break;

      case Levels.normal:
         from = 51;
         to = 401;
         break;

      case Levels.hard:
         from = 101;
         to = 1001;
         break;
   }

   // Ensure, that opt1 is not a prime number, otherwise there is not a sufficient solution
   op1 = 3;
   while (IsPrimeNumber (op1))
   {
      op1 = random.Next (from, to);
   }

   // Search for op2 suuch that op1 / op2 without rest
   for (; ; )
   {
      op2 = random.Next (2, op1);
      if ((op1 % op2) == 0)
         break;
   }
}

//
// True if value is a prime number
//
bool IsPrimeNumber (int val)
{
   if (val <= 1)
      return false;

   if (val == 2)
      return true;

   int maxVal = (int) Math.Sqrt (val);
   for (int i = 2; i <= maxVal; ++i)
   {
      if (val % i == 0)
         return false;
   }

   return true;
}

//
// Creates random mathematical question 
//
string GenerateQuestion (out int result)
{
   result = 0;

   int operation = random.Next (0, 4);
   int op1 = 0;
   int op2 = 0;

   string operationSymbol = "";

   switch (operation)
   {
      case 0: // Operation '+'
         GenerateOperandsForAddition (out op1, out op2);
         operationSymbol = "+";
         result = op1 + op2;
         break;

      case 1: // Operation '-'
         GenerateOperandsForSubtraction (out op1, out op2);
         operationSymbol = "-";
         result = op1 - op2;
         break;

      case 2: // Operation 'x'
         GenerateOperandsForMultiplication (out op1, out op2);
         operationSymbol = "\u00d7";
         result = op1 * op2;
         break;

      case 3: // Operation '/'
         GenerateOperandsForDivision (out op1, out op2);
         operationSymbol = "/";
         result = op1 / op2;
         break;
   }

   return $"{op1} {operationSymbol} {op2} = ";
}

//
// Received input from user and analyzes it for correctness
//
int GetAnswer ()
{
   int val = 0;
   string error = string.Empty;

   var (posX, posY) = Console.GetCursorPosition ();

   for (; ; )
   {
      Console.CursorVisible = true;
      string ?answer = Console.ReadLine ();

      // Not a number or negative number
      if (answer == null || !int.TryParse (answer, out val) || val < 0)
      {
         pauseInTimer = true;

         if (val < 0)
            error = " Number must be greater or equal zero ";
         else
            error = " Incorrect format of input ";

         Console.ForegroundColor = ConsoleColor.White;
         Console.BackgroundColor = ConsoleColor.Red;
         Console.Write (error);
         Console.CursorVisible = false;

         System.Threading.Thread.Sleep (2000);
         var (x, y) = Console.GetCursorPosition ();
         Console.SetCursorPosition (0, y);

         Console.ForegroundColor = defaultForegroundColor;
         Console.BackgroundColor = defaultBackgroundColor;
         Console.Write (new string (' ', error.Length));

         Console.SetCursorPosition (posX, posY);
         if (answer != null)
         {
            Console.Write (new string (' ', answer.Length));
            Console.SetCursorPosition (posX, posY);
         }

         pauseInTimer = false;

         continue;
      }

      break;
   }

   Console.CursorVisible = false;
   return val;
}

//
// Report, that answer is wrong
//
void ReportWrongAnswer (int correctAnswer)
{
   Console.ForegroundColor = ConsoleColor.Red;
   Console.WriteLine ($"      Answer is wrong. Correct answer: {correctAnswer}\n");
   Console.ForegroundColor = defaultForegroundColor;
}

//
// Report, that answer is correct
//
void ReportCorrectAnswer ()
{
   Console.ForegroundColor = ConsoleColor.Green;
   Console.WriteLine ("      Answer is correct\n");
   Console.ForegroundColor = defaultForegroundColor;
}

//
// Ask matematical questions and process answers
//
void StartGame ()
{
   // Start with blank window
   Console.ForegroundColor = defaultForegroundColor;
   Console.BackgroundColor = defaultBackgroundColor;
   Console.Clear ();

   // Total count of questions in quiz
   const int numQuestions = 5;

   int correctAnswers = 0;
   int wrongAnswers = 0;

   // Contains complete state of game round
   GameResult info = new (++gameNumber, (int) difficulty);

   totalSeconds = 0;
   totalMinutes = 0;
   totalHours = 0;

   for (int i = 0; i < numQuestions; ++i)
   {
      UpdateStatistic (i + 1, numQuestions, correctAnswers, wrongAnswers);

      if (timer == null)
         timer = new Timer (TimerCallback, null, 0, 1000);

      string question = GenerateQuestion (out int result);
      string text = $"  {i + 1}.  {question}";
      Console.Write (text);

      int answer = GetAnswer ();
      if (answer != result)
      {
         ++wrongAnswers;
         ReportWrongAnswer (result);
      }
      else
      {
         ++correctAnswers;
         ReportCorrectAnswer ();
      }

      info.AddResult (question, result, answer);
   }

   timer?.Dispose ();
   timer = null;

   info.EndGame ();
   results.Add (info);

   UpdateStatistic (numQuestions, numQuestions, correctAnswers, wrongAnswers);

   Console.WriteLine ("\nPress any key to go to menu...");
   Console.ReadKey ();

   ShowGui ();
}

//
// Updates timer in console. Will be launched by starting a new game in 1-second taсt
//
void TimerCallback (Object ?obj)
{
   if (pauseInTimer)
      return;

   ++totalSeconds;

   // After each 60 seconds uppdate minutes and hours
   if (totalSeconds > 59)
   {
      totalSeconds = 0;
      ++totalMinutes;

      if (totalMinutes > 59)
         ++totalHours;
   }

   string time = $"Time: {totalHours:D2}:{totalMinutes:D2}:{totalSeconds:D2}";
   var (x, y) = Console.GetCursorPosition ();

   Console.CursorVisible = false;
   Console.SetCursorPosition (50, 1);

   var colorForeground = Console.ForegroundColor;
   var colorBackground = Console.BackgroundColor;

   Console.ForegroundColor = defaultForegroundColor;
   Console.BackgroundColor = defaultBackgroundColor;

   Console.Write (time);

   Console.ForegroundColor = colorForeground;
   Console.BackgroundColor = colorBackground;

   Console.SetCursorPosition (x, y);
   Console.CursorVisible = true;
}

//
// Display results all game rounds
//
void ShowAllResults ()
{
   Console.ForegroundColor = defaultForegroundColor;
   Console.BackgroundColor = defaultBackgroundColor;
   Console.Clear ();

   if (results.Count <= 0)
      Console.WriteLine ("\n Actual there is no results");
   else
   {
      foreach (var res in results)
         res.Display ();
   }

   Console.WriteLine ("\n Press any key to go to menu...");
   Console.ReadKey ();

   ShowGui ();
}

//
// Game loop
//
void Run ()
{
   ShowGui ();

   Console.CursorVisible = false;
   for (; ; )
   { 
      var key = Console.ReadKey (true).Key;
      switch (key)
      {
         case ConsoleKey.UpArrow:
            SelectPrevMenuItem ();
            break;

         case ConsoleKey.DownArrow:
            SelectNextMenuItem ();
            break;

         case ConsoleKey.Enter:
            {
               if (!ProcessMenuAction ())
               {
                  Exit ();
                  return;
               }
            }
            break;

         case ConsoleKey.Escape:
            {
               Exit ();
               return;
            }
      }
   }
}

//
// Difficulty levels
//
enum Levels
{
   easy,
   normal,
   hard
};