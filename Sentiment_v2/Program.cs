using System;
using System.Collections; // feh
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Sentiment_v2
{
    class Program
    {
        static void Main(string[] args)
        {

            // Set up some words to ignore
            Dictionary<string, int> IgnoreWords = new Dictionary<string, int>();
            IgnoreWords.Add("the", 1);
            IgnoreWords.Add("and", 1);
            IgnoreWords.Add("that", 1);
            IgnoreWords.Add("you", 1);
            IgnoreWords.Add("for", 1);
            IgnoreWords.Add("with", 1);
            IgnoreWords.Add("are", 1);
            IgnoreWords.Add("this", 1);
            IgnoreWords.Add("have", 1);
            IgnoreWords.Add("was", 1);
            IgnoreWords.Add("from", 1);
            IgnoreWords.Add("they", 1);
            IgnoreWords.Add("url", 1);
            IgnoreWords.Add("will", 1);
            IgnoreWords.Add("all", 1);
            IgnoreWords.Add("your", 1);
            IgnoreWords.Add("other", 1);
            IgnoreWords.Add("than", 1);
            IgnoreWords.Add("who", 1);
            IgnoreWords.Add("his", 1);
            IgnoreWords.Add("what", 1);
            IgnoreWords.Add("one", 1);
            IgnoreWords.Add("their", 1);
            IgnoreWords.Add("can", 1);
            IgnoreWords.Add("has", 1);
            IgnoreWords.Add("would", 1);
            IgnoreWords.Add("there", 1);
            IgnoreWords.Add("about", 1);


            string FileName = System.Environment.GetEnvironmentVariable("map_input_file");
            string FileChunk = System.Environment.GetEnvironmentVariable("map_input_start");
            string YYYYMMDD; // String to hold stripped out date elements from file name
            DateTime FileDate; // DateTime to hold above converted to date
            DateTime StartDate = new DateTime(2005, 1, 1); // Start point of date differentiator
            System.TimeSpan DiffSinceStart; // Tiemspan to hold gap between StartDate and FileDate 
            int DaysSinceStart; // int to hold gap between StartDate and FileDate in days
            
            // Specific debug code
            //if (FileName == null)
            //{   FileName = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXnews.for.20110324040502.txt";
            //    FileChunk = "1";
            //}

            string ExecutionGUID = Guid.NewGuid().ToString();   // Guid to represent execution
            int MessageSequence = 1; // Int to hold message sequence
            string MessageId = ""; // Initialised bucket to hold a artificial message id
            string AuthorId = "Unknown"; // Initialised bucket to hold Author Name set to "Unknown"
            int LinesOutput = 0; // Counter to test number of lines output
            string line; // Variable to hold current line
            char[] delimiters = new char[] { ',', ' ', '"' };    // Set of delimiters to split strings by
            Regex regx = new Regex("[^a-zA-Z]"); // Regex pattern to exclude any non alpha character

            // Complex Regex pattern to exclude date stamps such as "on sat oct bob wrote"        
            Regex regxDate = new Regex(@"(\bon\b[\s]{1,})(\b(sat|sun|mon|tue|wed|thu|fri)\b[\s]{1,})(\b(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)\b)");

            int value; // Dummy variable to support Dictionary lookup of Ignored words

            // Manage date, convert to days from YYYYMMDD format (e.g. 20050101)
            int DateStart = FileName.IndexOf("news.for.") + 9;
            YYYYMMDD = FileName.Substring(DateStart,8); // Stripped out date elements from file name
            FileDate = DateTime.ParseExact(YYYYMMDD,"yyyyMMdd",null);
            DiffSinceStart = FileDate - StartDate;
            DaysSinceStart = DiffSinceStart.Days;

            // Initialise unique Message Id
            MessageId = ExecutionGUID + "-" + MessageSequence.ToString();

            // Read line by line from STDIN
            while ((line = Console.ReadLine()) != null)
            {
                // Remove any quoted posts as identified by starting with ">"
                if (line.StartsWith(">") == true)
                {   // Abort further line processing
                    continue;
                }

                // Ignore any zero length strings
                if (line.Length == 0)
                {   // Abort further line processing
                    continue;
                }

                // Set new empty string
                string line_clean = string.Empty;

                // Strip out non alpha characters and replace with spaces
                line_clean = regx.Replace(line, " ");

                // Put to lowercase for matching reasons
                line_clean = line_clean.ToLower();

                // Trim the line of whitespace
                line_clean = line_clean.Trim();

                // First assess if this line is record end or a continuation

                // Check for end of message 
                if (line_clean == "end of document") // Note actual text content is "---END.OF.DOCUMENT---" but cleanup above alters it
                {   // End of message

                    // Increment Message Sequence
                    MessageSequence = MessageSequence + 1;
                    MessageId = ExecutionGUID + "-" + MessageSequence.ToString();
                    AuthorId = "Unknown"; // Clear AuthorId
                    LinesOutput = 0; // Reset Lines Output counter

                }
                else if (line_clean.Length == 0)
                {   // Empty line
                    // Do nothing
                }
                else
                {   // Message continues

                    // If no lines output, put in a test for post author
                    // Assumptions are that there is no content in the post prior to "Author wrote:" line, and that the "Author wrote:" content does not span multiple lines
                    // Also only works if Post author has stuck to default "wrote" setting
                    if (LinesOutput == 0 && line_clean.Contains("wrote"))
                    {
                        // Strip out any date context preceding Authors name
                        line_clean = regxDate.Replace(line_clean, "");

                        // Re-trim the line of whitespace
                        line_clean = line_clean.Trim();

                        // Get everything to the left of "wrote"
                        AuthorId = line_clean.Substring(0, line_clean.IndexOf("wrote"));

                        // replace "emailaddress" - dummy data entry to anonymise data
                        AuthorId = AuthorId.Replace("emailaddress", "");

                        // Get rid of duplicated whitespaces
                        AuthorId = Regex.Replace(AuthorId, @"\s{2,}", " ");

                        // trim it
                        AuthorId = AuthorId.Trim();

                        // switch back to unknown if empty again
                        if (AuthorId.Length == 0)
                        { AuthorId = "Unknown"; }

                    }

                    LinesOutput++; // Increment Lines output counter

                    string[] words = line_clean.Split(' ');

                    foreach (string descword in words)
                    {
                        // Only write to STDOUT if there is content, ignoring words of 2 characters or less
                        if (descword.Length > 2)
                        {
                            // Check if in list of ignore words, only write if not
                            if (!IgnoreWords.TryGetValue(descword, out value))
                            {
                                Console.WriteLine("{0}", DaysSinceStart.ToString() + "." + FileChunk + "|" + MessageSequence.ToString() + "|" + AuthorId + "|" + descword);
                            }
                        }

                    }

                }

            }

        }
    }
}


