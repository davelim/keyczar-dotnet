﻿/*  Copyright 2012 James Tuley (jay+code@tuley.name)
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Keyczar.Compat;


using Keyczar.Crypto;
using Keyczar.Crypto.Streams;
using Keyczar.Util;
using Newtonsoft.Json;
using Org.BouncyCastle.Security;

namespace Keyczar.Pbe
{
    /// <summary>
    /// Stores a key encrypted by password
    /// </summary>
    public class PbeKeyStore
    {

        /// <summary>
        /// Encrypts the key data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="passwordPrompt">The password prompt.</param>
        /// <param name="iterationCount">The interation count.</param>
        /// <returns></returns>
        public static PbeKeyStore EncryptKeyData(byte[] key, Func<string> passwordPrompt, int iterationCount)
        {


            var pks = new PbeKeyStore()
                          {
                              Cipher = PbeKeyType.Aes128,
                              Hmac = PbeHashType.HmacSha1,
                              IterationCount = iterationCount,
                              Salt = new byte[16]
                          };

            Secure.Random.NextBytes(pks.Salt);

            var pbeKey = new PbeAesKey(){Size = 128};
            pbeKey.AesKeyBytes = pks.GetDerivedBytes(pbeKey.Size / 8, passwordPrompt);
            pks.IV = pbeKey.IV;

            using (pbeKey)
            using (var ks = new ImportedKeySet(pbeKey, KeyPurpose.DecryptAndEncrypt, "Pbe key"))
            using (var crypter = new Crypter(ks))
            {
                pks.Key = crypter.Encrypt(key);
            }

            return pks;
        }


        /// <summary>
        /// Gets or sets the cipher.
        /// </summary>
        /// <value>The cipher.</value>
        public PbeKeyType Cipher { get; set; }

        /// <summary>
        /// Gets or sets the hmac.
        /// </summary>
        /// <value>The hmac.</value>
        public PbeHashType Hmac { get; set; }

        /// <summary>
        /// Gets or sets the iteration count.
        /// </summary>
        /// <value>The iteration count.</value>
        public int IterationCount { get; set; }

        /// <summary>
        /// Gets or sets the IV.
        /// </summary>
        /// <value>The IV.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof(WebSafeBase64ByteConverter))]
        public byte[] IV { get; set; }

        /// <summary>
        /// Gets or sets the encrypted key.
        /// </summary>
        /// <value>The key.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof(WebSafeBase64ByteConverter))]
        public byte[] Key { get; set; }

        /// <summary>
        /// Gets or sets the salt.
        /// </summary>
        /// <value>The salt.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof(WebSafeBase64ByteConverter))]
        public byte[] Salt { get; set; }


        /// <summary>
        /// Gets the derived bytes using the store's parameters
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="passwordPrompt">The password prompt.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeySetException">Hmac_Sha256 not supported.</exception>
        protected byte[] GetDerivedBytes(int length, Func<string> passwordPrompt)
        {
            Rfc2898DeriveBytes pdb;
            if (Hmac == PbeHashType.HmacSha1)
            {
                pdb = new Rfc2898DeriveBytes(passwordPrompt(), Salt, IterationCount);
            }
            else if (Hmac == PbeHashType.HmacSha256)
            {
                throw new InvalidKeySetException("Hmac_Sha256 not supported.");
            }
            else
            {
                throw new InvalidKeySetException("Unknown Pbe Cipher");
            }
            using (pdb)
            {
                return pdb.GetBytes(length);
            }
        }

        /// <summary>
        /// Decrypts the key data.
        /// </summary>
        /// <param name="passwordPrompt">The passsword prompt.</param>
        /// <returns></returns>
        public byte[] DecryptKeyData(Func<string> passwordPrompt)
        {
            var key = new PbeAesKey { IV = IV };

            if (Cipher == PbeKeyType.Aes128)
            {
                key.Size = 128;
            }
            else
            {
                throw new InvalidKeySetException("Unknown Pbe Cipher");
            }

            key.AesKeyBytes = GetDerivedBytes(key.Size/8, passwordPrompt);

                using (key)
                using(var ks = new ImportedKeySet(key,KeyPurpose.DecryptAndEncrypt,"Pbe key"))
                using(var crypter = new Crypter(ks))
                {
                     return crypter.Decrypt(Key);
                }
            

        }

		[JsonConverter(typeof(JsonConverter))]
		internal class HardcodedKeyType:KeyType{
			Type _representedType;
			internal HardcodedKeyType(String identifier,Type representedType):base(identifier){
				_representedType =representedType;
			}

			public override Type RepresentedType{
				get{
					return _representedType;
				}set{
					_representedType = value;
				}
			}
		}


        internal class PbeAesKey : AesKey, IPbeKey
        {

            internal PbeAesKey()
            {
                IV = new byte[16];
                Secure.Random.NextBytes(IV);
            }

			KeyType _keyType =new HardcodedKeyType("PBE_AES",typeof(PbeAesKey));

			[JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
			public override KeyType KeyType {
				get {
					return _keyType;
				}set{
					_keyType =value;
				}
			}

			public override byte[] GetKeyHash()
			{
				return Utility.GetBytes(0);
			}

			public override IEnumerable<byte[]> GetFallbackKeyHash ()
			{
				return Enumerable.Empty<byte[]>();
			}

            public byte[] IV { get; set; }

            public CipherTextOnlyFinishingStream GetRawEncryptingStream(Stream output)
            {
                var stream = (CipherTextOnlyFinishingStream)GetEncryptingStream(output);
                stream.CipherTextOnly = true;
                stream.IV = IV;
                return stream;
            }

            public CipherTextOnlyFinishingStream GetRawDecryptingStream(Stream output)
            {
                var stream = (CipherTextOnlyFinishingStream)GetDecryptingStream(output);
                stream.CipherTextOnly = true;
                stream.IV = IV;
                return stream;
            }

        }
    }
}
