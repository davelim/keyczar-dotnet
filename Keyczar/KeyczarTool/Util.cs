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
using System.Linq;
using System.Text;

namespace KeyczarTool
{
    /// <summary>
    /// Internal Utility Methods
    /// </summary>
    internal class Util
    {
        /// <summary>
        /// Prompts for password.
        /// </summary>
        /// <returns></returns>
        public static string PromptForPassword()
        {
            Console.WriteLine(Localized.MsgPleaseEnterPassword);
            return Console.ReadLine();
        }

        /// <summary>
        /// Doubles the prompt for password.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Entered non matching password too many times</exception>
        public static string DoublePromptForPassword()
        {
            int i = 0;
            while (i++ < 4)
            {
                Console.WriteLine(Localized.MsgPleaseEnterPassword);
                var phrase1 = Console.ReadLine();
                Console.WriteLine(Localized.MsgPleaseReenterPassword);
                var phrase2 = Console.ReadLine();

                if (phrase1?.Equals(phrase2) ?? false)
                {
                    return phrase1;
                }
                Console.WriteLine(Localized.MsgPasswordDidNotMatch);
            }
            Console.WriteLine(Localized.MsgPasswordGiveUp);
            throw new Exception("Entered non matching password too many times");
        }
    }
}