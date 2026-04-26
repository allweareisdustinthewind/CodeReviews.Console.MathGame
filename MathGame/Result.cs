namespace MathGame
{
   /// <summary>
   /// Contains information about question with user's answer 
   /// </summary>
   internal class Answer
   {
      // Mathematical question in format "op1 operation op2 = " i.e. "3 x 5 = "
      public string Question { get; set; } = string.Empty;

      // Correct answer to question
      public int CorrectResult { get; set; }

      // User's answer
      public int UserInput { get; set; }

      public Answer (string question, int correctResult, int userInput)
      {
         Question = question;
         CorrectResult = correctResult;
         UserInput = userInput;
      }

      public bool IsCorrect ()
      {
         return UserInput == CorrectResult;
      }

      /// <summary>
      /// Display question and answer on console
      /// </summary>
      /// <param name="indent"></param>
      /// <param name="number"></param>
      /// <param name="maxLength"></param>
      /// <param name="maxQuestionLength"></param>
      public void Display (string indent, int number, int maxLength, int maxQuestionLength)
      {
         string textNumber = $" {number}. ";
         string textQuestion = $"{Question}{UserInput}";
         string padding = string.Empty;
         if (textQuestion.Length < maxQuestionLength)
            padding = new (' ', maxQuestionLength - textQuestion.Length);

         Console.Write ($"{indent}│{textNumber}");
         
         Console.ForegroundColor = ConsoleColor.White;
         Console.Write (textQuestion);

         string result = string.Empty;
         ConsoleColor color = ConsoleColor.White;

         if (IsCorrect ())
         {
            result = $"{padding} - Correct";
            color = ConsoleColor.Green;
         }
         else
         {
            result = $"{padding} - Wrong, must be {CorrectResult}";
            color = ConsoleColor.Red;
         }

         result = result.PadRight (maxLength - (textNumber.Length + textQuestion.Length), ' ');
         Console.ForegroundColor = color;
         Console.Write (result);

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine ("│");
      }
   }

   /// <summary>
   /// Info about one game round (all questions, answers and total statistic)
   /// </summary>
   public class GameResult
   {
      // All answers of round
      List<Answer> _answers = new ();

      // Times of start and end the game
      DateTime _startTime = DateTime.Now;
      DateTime _endTime = DateTime.Now;
      
      // Uniqie number of game
      int _gameNumber;

      // Difficulty level (0 - easy, 1 - normal, 2 - hard)
      Settings.Level _difficulty;

      // Maximal length of text block with information on screen in symbols
      static int _maxLength = 44;

      // Offset of each block from left border of window
      static string _indent = new (' ', 10);

      public GameResult (int gameNumber, Settings.Level difficulty)
      {
         _gameNumber = gameNumber;
         _difficulty = difficulty;
      }

      public void AddResult (string question, int correctAnswer, int userInput)
      {
         _answers.Add (new (question, correctAnswer, userInput));
      }

      public void EndGame ()
      {
         _endTime = DateTime.Now;
      }

      /// <summary>
      /// Display statistic of one game's round with all questions and answers
      /// </summary>
      public void Display ()
      {
         if (_answers.Count <= 0)
            return;

         // Count number of correct answers
         int correctAnswers = 0;
         int maxQuestionLength = 0;
         foreach (var answer in _answers)
         {
            if (answer.IsCorrect ())
               ++correctAnswers;

            string text = $"{answer.Question}{answer.UserInput}";

            maxQuestionLength = Math.Max (maxQuestionLength, text.Length);
         }

         string lineDouble = new ('═', _maxLength);

         var defaultForeground = Console.ForegroundColor;
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine ($"{_indent}╔{lineDouble}╗");

         // Display statistic of game round
         Tuple<string, string> [] info =
         [
            Tuple.Create ($"                   Game {_gameNumber}", ""),
            Tuple.Create (" ", " "),
            Tuple.Create ("   Started:    ", _startTime.ToString ()),
            Tuple.Create ("   Ended:      ", _endTime.ToString ()),
            Tuple.Create ("   Duration:   ", (_endTime - _startTime).Seconds.ToString () + " seconds"),
            Tuple.Create ("   Difficulty: ", _difficulty == Settings.Level.Easy ? "easy" : _difficulty == Settings.Level.Normal ? "normal" : "hard"),
            Tuple.Create (" ", " "),
         ];

         foreach (var (name, data) in info)
            DisplayInfo (name, data);

         // Display info about correct answers
         Console.Write ($"{_indent}║");
         string textCorrectAnswers = $"   Correct answers: {correctAnswers}";
         Console.ForegroundColor = ConsoleColor.Green;
         Console.Write (textCorrectAnswers);

         // Display info about wrong answers
         string textWrongAnswers = $"  Wrong answers: {_answers.Count - correctAnswers}";
         textWrongAnswers = textWrongAnswers.PadRight (_maxLength - textCorrectAnswers.Length, ' ');
         Console.ForegroundColor = ConsoleColor.Red;
         Console.Write (textWrongAnswers);
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine ("║");

         Console.WriteLine ($"{_indent}╚{lineDouble}╝");

         // Display all questions with answers
         string lineSingle = new ('─', _maxLength);
         Console.WriteLine ($"{_indent}┌{lineSingle}┐");
         Console.WriteLine ("{0}│{1}│", _indent, new string (' ', _maxLength));

         for (int i = 0; i < _answers.Count; ++i)
            _answers [i].Display (_indent, i + 1, _maxLength, maxQuestionLength);

         Console.WriteLine ("{0}│{1}│", _indent, new string (' ', _maxLength));
         Console.WriteLine ($"{_indent}└{lineSingle}┘");

         Console.ForegroundColor = defaultForeground;
      }

      /// <summary>
      /// Display one line of game's statistic
      /// </summary>
      /// <param name="name"></param>
      /// <param name="data"></param>
      private void DisplayInfo (string name, string data)
      {
         // Text without value
         if (string.IsNullOrEmpty (data))
         {
            name = name.PadRight (_maxLength, ' ');
            Console.WriteLine ($"{_indent}║{name}║");
            return;
         }

         // Show text in one color and then a value with different color
         Console.Write ($"{_indent}║{name}");
         data = data.PadRight (_maxLength - name.Length, ' ');
         Console.ForegroundColor = ConsoleColor.White;
         Console.Write ($"{data}");

         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine ("║");
      }
   }
}
