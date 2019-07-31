﻿/*  Copyright 2013 James Tuley (jay+code@tuley.name)
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

namespace Keyczar.Unofficial
{
    /// <summary>
    /// Place to find Unofficial KeyType identifiers
    /// </summary>
    public static class UnofficialKeyType
    {
        /// <summary>
        /// Unofficial type AES Authenticated Encryption with Associated Data
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly KeyType AesAead = "C#_AES_AEAD";


        /// <summary>
        /// Unofficial type Hmac Sha2
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly KeyType HmacSha2 = "C#_HMAC_SHA2";

        /// <summary>
        /// Unofficial type AES Hmac Sha2
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly KeyType AesHmacSha2 = "C#_AES_HMAC_SHA2";

        /// <summary>
        /// The Unofficial RSA priv sign
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly KeyType RSAPrivSign =
                "C#_RSA_SIGN_PRIV";

        /// <summary>
        /// The Unofficial RSA pub sign
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly KeyType RSAPubSign =
                "C#_RSA_SIGN_PUB";
        
        
        /// <summary>
        /// The Unofficial RSA priv sign
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly KeyType RSAPrivPkcs15Sign =
            "C#_RSA_SIGN_PKCS15_PRIV";

        /// <summary>
        /// The Unofficial RSA pub sign
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")] public static readonly KeyType RSAPubPkcs15Sign =
            "C#_RSA_SIGN_PCKS15_PUB";


        static UnofficialKeyType()
        {
            //Unofficial
            AesAead.KeySizes<AesAeadKey>(256, 192, 128).IsUnofficial().DefineSpec();
            HmacSha2.KeySizes<HmacSha2Key>(128, 192, 256).IsUnofficial().DefineSpec();
            AesHmacSha2.KeySizes<AesHmacSha2Key>(128, 192, 256).IsUnofficial().DefineSpec();
            RSAPrivSign.KeySizes<RsaPrivateSignPssKey>(3072, 4096, 8192).WeakSizes(2048, 1024).IsAsymmetric().IsUnofficial().DefineSpec();
            RSAPubSign.KeySizes<RsaPublicSignPssKey>(3072, 4096, 8192).WeakSizes(2048, 1024).IsAsymmetric().IsUnofficial().IsPublic().DefineSpec();
            
            RSAPrivPkcs15Sign.KeySizes<RsaPrivateSignPkcs15Key>(3072, 4096, 8192).WeakSizes(2048, 1024).IsAsymmetric().IsUnofficial().DefineSpec();
            RSAPubPkcs15Sign.KeySizes<RsaPublicSignPkcs15Key>(3072, 4096, 8192).WeakSizes(2048, 1024).IsAsymmetric().IsUnofficial().IsPublic().DefineSpec();
        }
    }
}