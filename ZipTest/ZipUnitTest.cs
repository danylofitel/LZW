//-----------------------------------------------------------------------
// <copyright file="ZipUnitTest.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace ZipTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Zip;

    /// <summary>
    /// Unit cases for both ZipEncrypt and ZipDecrypt
    /// </summary>
    [TestClass]
    public class ZipUnitTest
    {
        /// <summary>
        /// Case of empty alphabet and empty messages.
        /// </summary>
        [TestMethod]
        public void EmptyStringTest()
        {
            string empty = string.Empty;
            var zipper = new ZipEncrypt(empty);
            var unzipper = new ZipDecrypt(empty);
            Assert.AreEqual(empty, zipper.Encrypt(empty));
            Assert.AreEqual(empty, unzipper.Decrypt(empty));
        }

        /// <summary>
        /// ZipEncrypt, case of empty alphabet and non-empty messages, exceptions expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyAlphabetNonEmptyStringEncryptionTest()
        {
            string empty = string.Empty;
            var zipper = new ZipEncrypt(empty);
            zipper.Encrypt("a");
        }

        /// <summary>
        /// ZipDecrypt, case of empty alphabet and non-empty messages, exceptions expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyAlphabetNonEmptyStringDecryptionTest()
        {
            string empty = string.Empty;
            var unzipper = new ZipDecrypt(empty);
            unzipper.Decrypt("0");
        }

        /// <summary>
        /// Test if decrypt(encrypt(original)) = original for all possible strings of different lengths in specific alphabet.
        /// </summary>
        [TestMethod]
        public void AllStringsTest()
        {
            string fullAlphabet = "abcdefghijklmnopqrstuvwxyz";

            int maxAlphabetLength = 12;
            int maxLength = 5;

            // For alphabets of different sizes
            for (int alphabetSize = 1; alphabetSize <= maxAlphabetLength; ++alphabetSize)
            {
                string alphabet = fullAlphabet.Substring(0, alphabetSize);
                var zipper = new ZipEncrypt(alphabet);
                var unzipper = new ZipDecrypt(alphabet);

                // For strings in current alphabet of different size
                for (int strlen = 0; strlen <= maxLength; ++strlen)
                {
                    // Number of all possible strings in current alphabet of current size
                    int strings = (int)Math.Pow(alphabet.Length, strlen);

                    for (int i = 0; i < strings; ++i)
                    {
                        string message = string.Empty;
                        int mask = i;
                        for (int shift = 0; shift < strlen; ++shift)
                        {
                            message += alphabet[mask % alphabet.Length];
                            mask >>= 1;
                        }

                        string result = unzipper.Decrypt(zipper.Encrypt(message));
                        Assert.AreEqual(message, result);
                    }
                }
            }
        }
    }
}
