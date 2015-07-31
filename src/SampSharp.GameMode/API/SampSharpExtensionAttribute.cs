﻿// SampSharp
// Copyright 2015 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace SampSharp.GameMode.API
{
    /// <summary>
    ///     Specifies the extension to load from this assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class SampSharpExtensionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SampSharpExtensionAttribute" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public SampSharpExtensionAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        ///     Gets the type of the extension.
        /// </summary>
        public Type Type { get; private set; }
    }
}