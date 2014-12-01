//-----------------------------------------------------------------------
// <copyright file="LZWDecompress.cs" company="FTL">
//     FTL Inc.
// </copyright>
//-----------------------------------------------------------------------

namespace ZIP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for LZW decompression
    /// </summary>
    public class LZWDecompress
    {
        /// <summary>
        /// The primary alphabet
        /// </summary>
        private readonly char[] alphabet;

        /// <summary>
        /// Table of block indices
        /// </summary>
        private readonly IList<string> table;

        /// <summary>
        /// Current size of the block code
        /// </summary>
        private int blockCodeSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="LZWDecompress"/> class.
        /// </summary>
        /// <param name="alphabet">The alphabet.</param>
        public LZWDecompress(string alphabet)
        {
            if (alphabet.Distinct().Count() != alphabet.Count())
            {
                throw new ArgumentException("Dictionary elements are not distinct.");
            }

            this.alphabet = alphabet.ToCharArray();
            Array.Sort(this.alphabet);

            this.table = new List<string>((alphabet.Length * 2) + 1);
        }

        /// <summary>
        /// Decrypts the specified encrypted message.
        /// </summary>
        /// <param name="message">Encrypted message.</param>
        /// <returns>A message decrypted in the specified alphabet</returns>
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

            StringBuilder result = new StringBuilder();
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
                    result.Append(block);

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
                    result.Append(newBlock);
                    this.table.Add(newBlock);
                }

                remaining = remaining.Substring(this.blockCodeSize);

                // Update current block length
                if (this.table.Count.ToString().Length > (this.table.Count - 1).ToString().Length)
                {
                    ++this.blockCodeSize;
                }
            }

            this.Cleanup();

            return result.ToString();
        }

        /// <summary>
        /// Initializes the table.
        /// </summary>
        private void Initialize()
        {
            for (int i = 0; i < this.alphabet.Length; i++)
            {
                this.table.Add(this.alphabet[i].ToString());
            }

            this.blockCodeSize = this.table.Count > 0 ? (this.table.Count - 1).ToString().Length : 0;
        }

        /// <summary>
        /// Deletes reference to the table to allow garbage collector clean it up.
        /// </summary>
        private void Cleanup()
        {
            this.table.Clear();
        }
    }
}