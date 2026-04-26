namespace MathGame
{
   //
   // All settings of the game
   //
   public class Settings
   {
      // Count of all questions in the game
      public static readonly int MaxQuestions = 5; // Count of all questions in the game

      // Arithmetical operation. If '@', then operation will ge generated randomly
      public string Operation { get; set; } = "@";

      // Level of difficulty
      public Level Difficulty { get; set; } = Level.Easy;

      public enum Level
      { 
         Easy,
         Normal,
         Hard
      }
   }
}
