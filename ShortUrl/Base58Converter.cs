using System;

namespace ShortUrl
{
    /// <summary>
    /// Original post: [Tiny Url’s in C#](http://www.faygate.net/post/133462295/tinyurlcode)
    /// </summary>
    public static class Base58Converter
    {
        // Base62 alphabet
        // abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789
        // Base58 alphabet
        // 123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ
        private const String sBase58Alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

        public static String Encode(UInt32 numberToShorten)
        {
            String sConverted = "";
            Int32 iAlphabetLength = sBase58Alphabet.Length;

            while (numberToShorten > 0)
            {
                long lNumberRemainder = (numberToShorten % iAlphabetLength);
                numberToShorten = Convert.ToUInt32(numberToShorten / iAlphabetLength);
                sConverted = sBase58Alphabet[Convert.ToInt32(lNumberRemainder)] + sConverted;
            }

            return sConverted;
        }

        public static long Decode(String base58StringToExpand)
        {
            long lConverted = 0;
            long lTemporaryNumberConverter = 1;

            while (base58StringToExpand.Length > 0)
            {
                String sCurrentCharacter = base58StringToExpand.Substring(base58StringToExpand.Length - 1);
                lConverted = lConverted + (lTemporaryNumberConverter * sBase58Alphabet.IndexOf(sCurrentCharacter));
                lTemporaryNumberConverter = lTemporaryNumberConverter * sBase58Alphabet.Length;
                base58StringToExpand = base58StringToExpand.Substring(0, base58StringToExpand.Length - 1);
            }

            return lConverted;
        }
    }
}
