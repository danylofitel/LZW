//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace Zip
{
    using System;
    using System.Linq;

    /// <summary>
    /// Demo class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            /*
            Test("benort", string.Concat(Enumerable.Repeat("tobeornottobeor", 10000)) + "tobe");
             * */

            do
            {
                Console.WriteLine("Alphabet:");
                var alphabet = Console.ReadLine();
                try
                {
                    var zipper = new ZipEncrypt(alphabet);
                    var unzipper = new ZipDecrypt(alphabet);
                    do
                    {
                        Console.WriteLine("Message:");
                        var message = Console.ReadLine();
                        try
                        {
                            var encrypted = zipper.Encrypt(message);
                            Console.WriteLine(encrypted);
                            if (unzipper.Decrypt(encrypted) != message)
                            {
                                Console.WriteLine("Oops, I fucked up a bit.");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("You idiot, this message is fucking wrong!");
                        }

                        Console.WriteLine("Another message?");
                    }
                    while (Console.ReadLine().ToLower() != "n");
                    Console.WriteLine("Another alphabet?");
                }
                catch (Exception)
                {
                    Console.WriteLine("You idiot, this is not a fucking alphabet, go back to school!");
                }
            }
            while (Console.ReadLine().ToLower() != "n");
        }

        /// <summary>
        /// Tests the specified alphabet and message.
        /// </summary>
        /// <param name="alphabet">The alphabet.</param>
        /// <param name="message">The message.</param>
        private static void Test(string alphabet, string message)
        {
            var zipper = new ZipEncrypt(alphabet);
            var unzipper = new ZipDecrypt(alphabet);
            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            var encrypted = zipper.Encrypt(message);
            Console.WriteLine("Encrypted in {0} milliseconds", sw.ElapsedMilliseconds);

            sw.Restart();
            var decrypted = unzipper.Decrypt(encrypted);
            Console.WriteLine("Decrypted in {0} milliseconds", sw.ElapsedMilliseconds);

            Console.WriteLine("Original size: {0}; encrypted size: {1}", message.Length, encrypted.Length);
            Console.WriteLine(message == decrypted ? "Success!!!" : "Fail...");
        }
    }
}
