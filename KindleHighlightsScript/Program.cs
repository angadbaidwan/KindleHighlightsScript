using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Formats.Tar;

namespace KindleHighlightsScript
{
    public class BookHighlight
    {
        public string quote { get; private set; }
        public string bookTitle { get; private set; }
        public DateTime dateCreated { get; private set; }
        public int pageNum { get; private set; }

        public BookHighlight(string quote, string bookTitle, DateTime dateCreated, int pageNum)
        {
            this.quote = quote;
            this.bookTitle = bookTitle;
            this.dateCreated = dateCreated;
            this.pageNum = pageNum;
        }
    }

    internal class Program
    {
        public static bool ValidateDateRange(DateTime dateIn)
        {
            // NOTE: !Hardcoded Date Validation!
            DateTime startDate = new DateTime(2023, 2, 1);
            DateTime endDate = new DateTime(2023, 3, 1);
            if (dateIn >= startDate && dateIn <= endDate)
                return true;
            else
                return false;
        }

        static void Main(string[] args)
        {
            var bookHighlights = new List<BookHighlight>();

            StreamReader reader = new StreamReader("My Clippings.txt");

            try
            {
                while (reader.EndOfStream == false)
                {
                    // Read highlight/bookmark entry
                    string bookTitleLine = reader.ReadLine();
                    if (String.IsNullOrEmpty(bookTitleLine))
                        break;
                    string pageDateLine = reader.ReadLine();
                    reader.ReadLine();
                    string quoteLine = reader.ReadLine();
                    reader.ReadLine();

                    // Parse Date for validation
                    string[] temp = pageDateLine.Split("Added on ");
                    DateTime dateCreated = DateTime.Parse(temp[1]);

                    // Check if highlight, quote is more than one word, and quote matches date range
                    if (pageDateLine.Contains("Highlight") && quoteLine.Contains(' ') && ValidateDateRange(dateCreated))
                    {
                        string pageNumPattern = @"\d+";
                        int pageNum = Int32.Parse(Regex.Match(pageDateLine, pageNumPattern).ToString());

                        // Update book name to remove illegal file name characters
                        string bookTitlePattern = @"[\\/*?<>|""]";
                        bookTitleLine = Regex.Replace(bookTitleLine.Replace(":", " -"), bookTitlePattern, String.Empty); // using 2 different Regex overloads

                        // Create book highlight and add to list
                        var bookHighlight = new BookHighlight(quoteLine, bookTitleLine, dateCreated, pageNum);
                        bookHighlights.Add(bookHighlight);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                reader.Close();
            }

            // Group highlights by book
            var highlightGroup = bookHighlights.GroupBy(h => h.bookTitle);

            // Create a seperate text file for each book and output highlights in desired format
            foreach (var group in highlightGroup)
            {
                StreamWriter writer = new StreamWriter(group.Key + ".txt");
                try
                {
                    foreach(var item in group)
                    {
                        writer.WriteLine("> " + item.quote);
                        writer.WriteLine(">");
                        writer.WriteLine("\\- page " + item.pageNum);
                        writer.WriteLine();
                        writer.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    writer.Close();
                }
            }
            

        }
    }
}