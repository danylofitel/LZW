//-----------------------------------------------------------------------
// <copyright file="ZipDecrypt.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace Zip
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class for Zip decryption
    /// </summary>
    public class ZipDecrypt
    {
        /// <summary>
        /// The primary dictionary
        /// </summary>
        private readonly char[] dictionary;

        /// <summary>
        /// Table of block indices
        /// </summary>
        private IList<string> table;

        /// <summary>
        /// Current size of the block code
        /// </summary>
        private int blockCodeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipDecrypt"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public ZipDecrypt(string dictionary)
        {
            if (dictionary.Distinct().Count() != dictionary.Count())
            {
                throw new ArgumentException("Dictionary elements are not distinct.");
            }

            this.dictionary = dictionary.ToCharArray();
            Array.Sort(this.dictionary);
        }

        /// <summary>
        /// Decrypts the specified encrypted message.
        /// </summary>
        /// <param name="message">Encrypted message.</param>
        /// <returns>A message decrypted in the specified dictionary</returns>
        public string Decrypt(string message)
        {
            foreach (char c in message)
            {
                if (!char.IsDigit(c))
                {
                    throw new ArgumentException(string.Format("{0} is not a digit.", c));
                }
            }

            this.Initialize();

            string result = string.Empty;
            string remaining = message;
            string newBlock = null;

            while (remaining.Length > 0)
            {
                // Extract number of current block
                int blockNumber;
                if (!int.TryParse(remaining.Substring(0, this.blockCodeSize), out blockNumber))
                {
                    throw new ArgumentException("Invalid block index.");
                }

                // Add the new block to encrypted message
                if (blockNumber < this.table.Count)
                {
                    // Get current block from the table
                    string block = this.table[blockNumber];

                    // Add it to encrypted message
                    result += block;

                    // Recreate and add to the table a new block from the previous step
                    if (newBlock != null)
                    {
                        // Ending of the previous new block is beginning of current block
                        newBlock += block[0];

                        // Add previous new block to the table
                        this.table.Add(newBlock);
                    }

                    // Prepare the next new block
                    newBlock = block;
                }
                else
                {
                    if (newBlock == null)
                    {
                        throw new ArgumentException("Invalid block index.");
                    }

                    // Current block is the same as previous new one
                    newBlock += newBlock[0];
                    result += newBlock;
                    this.table.Add(newBlock);
                }

                remaining = remaining.Substring(this.blockCodeSize);

                // Update current block length
                if (this.table.Count.ToString().Length > (this.table.Count - 1).ToString().Length)
                {
                    ++this.blockCodeSize;
                }
            }

            return result;
        }

        /// <summary>
        /// Initializes the table.
        /// </summary>
        private void Initialize()
        {
            this.table = new List<string>();
            for (int i = 0; i < this.dictionary.Length; i++)
            {
                this.table.Add(this.dictionary[i].ToString());
            }

            this.blockCodeSize = this.table.Count > 0 ? (this.table.Count - 1).ToString().Length : 0;
        }
    }
}