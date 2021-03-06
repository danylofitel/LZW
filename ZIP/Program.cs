﻿//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace ZIP
{
    using System;
    using System.Linq;
    using System.Text;

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
            Test("benort", string.Concat(Enumerable.Repeat("tobeornottobeor", 100000)) + "tobe");

            Test("0123456789", RandomNumericString(100000));

            do
            {
                Console.WriteLine("Alphabet:");
                var alphabet = Console.ReadLine();
                try
                {
                    var zipper = new LZWCompress(alphabet);
                    var unzipper = new LZWDecompress(alphabet);
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
                                Console.WriteLine("Oops, decrypted message did not match.");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Sorry, this message is wrong!");
                        }

                        Console.WriteLine("Another message?");
                    }
                    while (Console.ReadLine().ToLower() != "n");
                    Console.WriteLine("Another alphabet?");
                }
                catch (Exception)
                {
                    Console.WriteLine("Sorry, this is not a valid alphabet, make sure all characters are distinct!");
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
            var zipper = new LZWCompress(alphabet);
            var zipperPT = new LZWCompressPT(alphabet);
            var unzipper = new LZWDecompress(alphabet);

            var sw = new System.Diagnostics.Stopwatch();

            sw.Start();
            var encrypted = zipper.Encrypt(message);
            Console.WriteLine("Encrypted (HT) in {0} milliseconds", sw.ElapsedMilliseconds);

            sw.Restart();
            var encryptedPT = zipperPT.Encrypt(message);
            Console.WriteLine("Encrypted (PT) in {0} milliseconds", sw.ElapsedMilliseconds);

            if (encrypted != encryptedPT)
            {
                Console.WriteLine("Encryption results do not match!!!");
                Console.WriteLine(encrypted);
                Console.WriteLine(encryptedPT);
                Console.WriteLine();
            }

            sw.Restart();
            var decrypted = unzipper.Decrypt(encrypted);
            Console.WriteLine("Decrypted in       {0} milliseconds", sw.ElapsedMilliseconds);
            if (message != decrypted)
            {
                Console.WriteLine("Decrypted text does not match with original!!!");
                Console.WriteLine(message);
                Console.WriteLine(decrypted);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Returns a random numeric string of specified length.
        /// </summary>
        /// <param name="size">Size of the string.</param>
        /// <returns>A random numeric string.</returns>
        private static string RandomNumericString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = (random.Next() % 10).ToString()[0];
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}