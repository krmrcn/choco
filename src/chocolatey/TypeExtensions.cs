﻿// Copyright © 2017 - 2025 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//
// You may obtain a copy of the License at
//
// 	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace chocolatey
{
    /// <summary>
    ///   Extensions for Type
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///   Determines whether a type is a built in system type (ValueType, string, primitive or is in the System Namespace)
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if meets criteria for system type</returns>
        public static bool IsBuiltinType(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            // if all else fails, check to see if the namespace is at system.
            return type.IsPrimitive
                   || type.IsValueType
                   || (type == typeof(string))
                   || type.Namespace.Equals("System", StringComparison.Ordinal);
        }

        /// <summary>
        ///   Determines whether a type is a collection, like a list, array or dictionary.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if enumerable</returns>
        public static bool IsCollectionType(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            // Surely we can do a check around "is ICollection" / "is ICollection<>" here?
            return type.IsArray
                   || type.Namespace.Contains("System.Collections");
        }

#pragma warning disable IDE0022, IDE1006
        [Obsolete("This overload is deprecated and will be removed in v3.")]
        public static bool is_built_in_system_type(this Type type)
            => IsBuiltinType(type);

        [Obsolete("This overload is deprecated and will be removed in v3.")]
        public static bool is_collections_type(this Type type)
            => IsCollectionType(type);
#pragma warning restore IDE0022, IDE1006
    }
}
