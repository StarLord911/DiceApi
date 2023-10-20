using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;

namespace DiceApi.Common
{
    public static class HashHelper
    {
        public static string GetSHA256Hash(string input)
        {
            Sha256Digest digest = new Sha256Digest();
            byte[] inputData = Encoding.UTF8.GetBytes(input);
            byte[] output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(inputData, 0, inputData.Length);
            digest.DoFinal(output, 0);

            return Hex.ToHexString(output);
        }
    }
}
