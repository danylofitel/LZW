//-----------------------------------------------------------------------
// <copyright file="ZipUnitTest.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace ZipTest
{
    using System;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ZIP;

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
            var zipper = new LZWCompress(empty);
            var zipperPT = new LZWCompressPT(empty);
            var unzipper = new LZWDecompress(empty);
            Assert.AreEqual(empty, zipper.Encrypt(empty));
            Assert.AreEqual(empty, zipperPT.Encrypt(empty));
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
            var zipper = new LZWCompress(empty);
            zipper.Encrypt("a");
        }

        /// <summary>
        /// ZipEncryptPT, case of empty alphabet and non-empty messages, exceptions expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyAlphabetNonEmptyStringEncryptionPTTest()
        {
            string empty = string.Empty;
            var zipperPT = new LZWCompressPT(empty);
            zipperPT.Encrypt("a");
        }

        /// <summary>
        /// ZipDecrypt, case of empty alphabet and non-empty messages, exceptions expected.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyAlphabetNonEmptyStringDecryptionTest()
        {
            string empty = string.Empty;
            var unzipper = new LZWDecompress(empty);
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
            int maxLength = 6;

            StringBuilder builder = new StringBuilder();

            // For alphabets of different sizes
            for (int alphabetSize = 1; alphabetSize <= maxAlphabetLength; ++alphabetSize)
            {
                string alphabet = fullAlphabet.Substring(0, alphabetSize);
                var zipper = new LZWCompress(alphabet);
                var zipperPT = new LZWCompressPT(alphabet);
                var unzipper = new LZWDecompress(alphabet);

                // For strings in current alphabet of different size
                for (int strlen = 0; strlen <= maxLength; ++strlen)
                {
                    // Number of all possible strings in current alphabet of current size
                    int strings = (int)Math.Pow(alphabet.Length, strlen);

                    for (int i = 0; i < strings; ++i)
                    {
                        builder.Clear();
                        int mask = i;
                        for (int shift = 0; shift < strlen; ++shift)
                        {
                            builder.Append(alphabet[mask % alphabet.Length]);
                            mask >>= 1;
                        }

                        string message = builder.ToString();
                        string encrypted = zipper.Encrypt(message);
                        string encryptedPT = zipperPT.Encrypt(message);
                        Assert.AreEqual(encrypted, encryptedPT);
                        Assert.AreEqual(message, unzipper.Decrypt(encrypted));
                    }
                }
            }
        }
    }
}