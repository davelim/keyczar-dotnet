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
using Keyczar.Compat;
using Keyczar.Crypto;
using Keyczar.Crypto.Streams;
using Keyczar.Util;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

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

            var pbeKey = new PbeAesKey() {Size = 128};
            pbeKey.AesKeyBytes = pks.GetDerivedBytes(pbeKey.Size/8, passwordPrompt);
            pks.IV = pbeKey.IV;

            using (pbeKey)
            using (var ks = new ImportedKeySet(pbeKey, KeyPurpose.DecryptAndEncrypt, "Pbe key"))
            using (var crypter = new Crypter(ks))
            {
                var data = crypter.Encrypt(key);
                byte[] justciphertext = new byte[data.Length - KeyczarConst.HeaderLength];
                Array.Copy(data, KeyczarConst.HeaderLength, justciphertext, 0, justciphertext.Length);
                pks.Key = justciphertext;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof (WebSafeBase64ByteConverter))]
        public byte[] IV { get; set; }

        /// <summary>
        /// Gets or sets the encrypted key.
        /// </summary>
        /// <value>The key.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof (WebSafeBase64ByteConverter))]
        public byte[] Key { get; set; }

        /// <summary>
        /// Gets or sets the salt.
        /// </summary>
        /// <value>The salt.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1819:PropertiesShouldNotReturnArrays"), JsonConverter(typeof (WebSafeBase64ByteConverter))]
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
            PbeParametersGenerator pdb;
            if (Hmac == PbeHashType.HmacSha1)
            {
                pdb = new Pkcs5S2ParametersGenerator();
                pdb.Init(PbeParametersGenerator.Pkcs5PasswordToBytes(passwordPrompt().ToCharArray()), Salt,
                         IterationCount);
            }
            else if (Hmac == PbeHashType.HmacSha256)
            {
                throw new InvalidKeySetException("Hmac_Sha256 not supported.");
            }
            else
            {
                throw new InvalidKeySetException("Unknown Pbe Cipher");
            }

            var key = (KeyParameter) pdb.GenerateDerivedMacParameters(length*8);
            return key.GetKey();
        }

        /// <summary>
        /// Decrypts the key data.
        /// </summary>
        /// <param name="passwordPrompt">The passsword prompt.</param>
        /// <returns></returns>
        public byte[] DecryptKeyData(Func<string> passwordPrompt)
        {
            var key = new PbeAesKey {IV = IV};

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
            using (var ks = new ImportedKeySet(key, KeyPurpose.DecryptAndEncrypt, "Pbe key"))
            using (var crypter = new Crypter(ks))
            using (var memstream = new MemoryStream())
            {
                memstream.Write(KeyczarConst.FormatBytes, 0, KeyczarConst.FormatBytes.Length);
                memstream.Write(new byte[KeyczarConst.KeyHashLength], 0, KeyczarConst.KeyHashLength);
                memstream.Write(Key, 0, Key.Length);
                return crypter.Decrypt(memstream.ToArray());
            }
        }


		   [JsonConverter(typeof(JsonConverter))]
		   internal class HardcodedKeyType:KeyType{
			     Type _representedType;
		       private KeyKind _kind;
			     internal HardcodedKeyType(String identifier,Type representedType, KeyKind kind):base(identifier){
			      	_representedType =representedType;
			        _kind = kind;
			     }

           public override KeyKind Kind => _kind;

		       public override Type RepresentedType
            {
                get => _representedType;
                set => _representedType = value;
            }
        }



        internal class PbeAesKey : AesKey, IPbeKey
        {
            internal PbeAesKey()
            {
                IV = new byte[16];
                Secure.Random.NextBytes(IV);
            }

            private KeyType _keyType = new HardcodedKeyType("PBE_AES", typeof (PbeAesKey), KeyKind.Symmetric);

            [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
            public override KeyType KeyType
            {
                get => _keyType;
                set => _keyType = value;
            }

            public override byte[] GetKeyHash() => Utility.GetBytes(0);

            public override IEnumerable<byte[]> GetFallbackKeyHash() => Enumerable.Empty<byte[]>();

            public byte[] IV { get; set; }

            public override FinishingStream GetEncryptingStream(Stream output, KeyczarBase keyczar)
            {
                var stream = (CipherTextOnlyFinishingStream) base.GetEncryptingStream(output, keyczar);
                stream.CipherTextOnly = true;
                stream.IV = IV;
                return stream;
            }

            public override FinishingStream GetDecryptingStream(Stream output, KeyczarBase keyczar)
            {
                var stream = (CipherTextOnlyFinishingStream) base.GetDecryptingStream(output, keyczar);
                stream.CipherTextOnly = true;
                stream.IV = IV;
                return stream;
            }
        }
    }
}