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
using System.Text;
using Keyczar.Crypto;
using Keyczar.Crypto.Streams;
using Keyczar.Util;
using Newtonsoft.Json;
using Org.BouncyCastle.Security;

namespace Keyczar
{
    /// <summary>
    /// Base class for all crypt/sign Keys
    /// </summary>
    public abstract class Key : IDisposable, IKey
    {
        /// <summary>
        /// Gets the key hash.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetKeyHash();

        /// <summary>
        /// Gets the fallback key hashes. old/buggy hashes from old/other keyczar implementations
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<byte[]> GetFallbackKeyHash() 
            => Enumerable.Empty<byte[]>();

        private KeyType _type;

        /// <summary>
        /// Gets the key type.
        /// </summary>
        /// <value>The key type.</value>
        [JsonIgnore]
        public virtual KeyType KeyType
        {
            get => _type ?? (_type = KeyType.ForType(GetType()));
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; set; }

        /// <summary>
        /// Reads the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="keyData">The key data.</param>
        /// <returns></returns>
        public static Key Read(KeyType type, byte[] keyData, KeyczarConfig config = null)
        {
            config = config ?? KeyczarDefaults.DefaultConfig;
            var json = config.RawStringEncoding.GetString(keyData);
            var key = (Key) JsonConvert.DeserializeObject(json, type.RepresentedType);
            key.Setup(config);
            return key;
        }
        
        /// <summary>
        /// Called after Key is read from keyset (optionally implmented
        /// </summary>
        public virtual void Setup(KeyczarConfig config)
        {
           
        }

        /// <summary>
        /// Generates the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="size">The size.</param>
        /// <returns></returns>
        /// <exception cref="InvalidKeyTypeException"></exception>
        public static Key Generate(KeyType type, int size = 0, KeyczarConfig config =null)
        {
            config = config ?? KeyczarDefaults.DefaultConfig;
            if (size == 0)
            {
                size = type.DefaultSize;
            }
            if (!type.KeySizeOptions.Contains(size))
            {
                throw new InvalidKeyTypeException($"Invalid Size: {size}!");
            }
            var key = (Key) Activator.CreateInstance(type.RepresentedType);
            key.GenerateKey(size, config);
            key.Size = size;
            return key;
        }

        /// <summary>
        /// Generates the key.
        /// </summary>
        /// <param name="size">The size.</param>
        protected abstract void GenerateKey(int size, KeyczarConfig config);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}