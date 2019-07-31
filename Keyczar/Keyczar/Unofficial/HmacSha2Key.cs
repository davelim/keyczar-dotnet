/*  Copyright 2012 James Tuley (jay+code@tuley.name)
 * 
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Keyczar.Crypto;
using Keyczar.Crypto.Streams;
using Keyczar.Util;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Keyczar.Unofficial
{
    /// <summary>
    /// The Hmac 256 Sha1 key
    /// </summary>
    public class HmacSha2Key : Key, ISignerKey, IVerifierKey
    {

        /// <summary>
        /// Gets or sets the hmac key bytes.
        /// </summary>
        /// <value>The hmac key bytes.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof (WebSafeBase64ByteConverter))]
        [JsonProperty("HmacKeyString")]
        public byte[] HmacKeyBytes { get; set; }


        /// <summary>
        /// Gets the key hash.
        /// </summary>
        /// <returns></returns>
        public override byte[] GetKeyHash()
            => Utility.HashKey(KeyczarConst.KeyHashLength, HmacKeyBytes, Utility.GetBytes(HashLength),  Digest.ToBytes());

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            HmacKeyBytes = HmacKeyBytes.Clear();
        }

        /// <summary>
        /// Gets the signing stream.
        /// </summary>
        /// <returns></returns>
        public HashingStream GetSigningStream(KeyczarBase keyczar)
            => GetVerifyingStream(keyczar);

        /// <summary>
        /// Gets the verifying stream.
        /// </summary>
        /// <returns></returns>
        public VerifyingStream GetVerifyingStream(KeyczarBase keyczar)
        {

            IDigest digest;
            if (Digest == DigestAlg.Sha256)
            {
                digest = new Sha256Digest();
            }
            else if (Digest == DigestAlg.Sha384)
            {
                digest = new Sha384Digest();
            }
            else if (Digest == DigestAlg.Sha512)
            {
                digest = new Sha512Digest();
            }
            else
            {
                throw new InvalidKeyTypeException($"Unsupported digest type :{Digest}");
            }

            var hmac = new HMac(digest);
            hmac.Init(new KeyParameter(HmacKeyBytes));
            return new HmacStream(hmac, HashLength);
        }

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <param name="size">The size.</param>
        protected override void GenerateKey(int size, KeyczarConfig config)
        {
            HmacKeyBytes = new byte[size/8];

            Digest = DigestForSize(size);
            HashLength = HashLengthForDigest(Digest);

            Secure.Random.NextBytes(HmacKeyBytes);
        }

        /// <summary>
        /// Gets or sets the digest.
        /// </summary>
        /// <value>
        /// The digest.
        /// </value>
        public DigestAlg Digest { get; set; }

        /// <summary>
        /// Gets or sets the hash length in bytes.
        /// </summary>
        /// <value>
        /// The hash length.
        /// </value>
        public int HashLength { get; set; }

        /// <summary>
        /// Picks the digests based on key size and relative strengths as described in https://tools.ietf.org/html/rfc7518#section-5.2.3
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        protected DigestAlg DigestForSize(int size)
        {
            //Based on convention from
            //https://tools.ietf.org/html/rfc7518#section-5.2.3

            if (size <= 128)
                return DigestAlg.Sha256; 
            if (size <= 192)
                return DigestAlg.Sha384; 
            return DigestAlg.Sha512; 
        }
        
        
        protected int HashLengthForDigest(DigestAlg digestAlg)
        {
            switch (digestAlg)
            {
               case DigestAlg alg when digestAlg == DigestAlg.Sha256:
                   return 256 / 8;
               case DigestAlg alg when alg == DigestAlg.Sha384:
                   return 384 / 8;
               case DigestAlg alg when alg == DigestAlg.Sha512:
                   return 512 / 8;
               default:
                   return 0;
            }
        }
    }
}