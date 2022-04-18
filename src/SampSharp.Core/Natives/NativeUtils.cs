﻿// SampSharp
// Copyright 2022 Tim Potze
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
using System.Globalization;
using System.Text;
using SampSharp.Core.Hosting;

namespace SampSharp.Core.Natives;

/// <summary>
/// Provides functions used by native proxies generated by native object proxy generator.
/// </summary>
public static class NativeUtils
{
    /// <summary>
    /// Casts an integer pointer to an integer.
    /// </summary>
    /// <param name="ptr">The integer pointer to cast.</param>
    /// <returns>The cast integer.</returns>
    public static unsafe int IntPointerToInt(int* ptr)
    {
        return (int) (IntPtr) ptr;
    }
        
    /// <summary>
    /// Casts a byte pointer to an integer.
    /// </summary>
    /// <param name="ptr">The byte pointer to cast.</param>
    /// <returns>The cast integer.</returns>
    public static unsafe int BytePointerToInt(byte* ptr)
    {
        return (int) (IntPtr) ptr;
    }

    /// <summary>
    /// Gets the byte count of the specified input string based on the currently active encoding.
    /// </summary>
    /// <param name="input">The input string to get the byte count of.</param>
    /// <returns>The number of bytes of the input string including a nul terminator.</returns>
    public static int GetByteCount(string input)
    {
        var enc = InternalStorage.RunningClient.Encoding ?? Encoding.ASCII;
        return enc.GetByteCount(input) + 1;
    }

    /// <summary>
    /// Gets the bytes of the specified input string based on the currently active encoding.
    /// </summary>
    /// <param name="input">The input string to get the bytes of.</param>
    /// <param name="output">The buffer to store the bytes in.</param>
    public static void GetBytes(string input, Span<byte> output)
    {
        var enc = InternalStorage.RunningClient.Encoding ?? Encoding.ASCII;
        enc.GetBytes(input.AsSpan(), output);
        output[^1] = 0;
    }

    /// <summary>
    /// Gets the string from the specified bytes based on the currently active encoding.
    /// </summary>
    /// <param name="bytes">The bytes to get the string from.</param>
    /// <returns>The converted string excluding nul terminators.</returns>
    public static string GetString(Span<byte> bytes)
    {
        var enc = InternalStorage.RunningClient.Encoding ?? Encoding.ASCII;

        return enc.GetString(bytes).TrimEnd('\0');
    }

    /// <summary>
    /// Synchronizes an invocation to a native pointer.
    /// </summary>
    /// <param name="synchronizationProvider">The synchronization provider.</param>
    /// <param name="native">The native pointer.</param>
    /// <param name="format">The format of the native arguments.</param>
    /// <param name="data">The native arguments.</param>
    /// <returns>The return value of the native.</returns>
    public static unsafe int SynchronizeInvoke(ISynchronizationProvider synchronizationProvider, IntPtr native,
        string format, int* data)
    {
        int result = default;
        synchronizationProvider.Invoke(() =>
            result = Interop.FastNativeInvoke(native, format, data));
        return result;
    }


    /// <summary>
    /// Converts an array to on span of integers.
    /// </summary>
    /// <param name="array">The array to convert.</param>
    /// <param name="length">The length of the data to convert.</param>
    /// <returns>The span of integers.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1871:Two branches in a conditional structure should not have exactly the same implementation", Justification = "False message")]
    public static Span<int> ArrayToIntSpan(Array array, int length)
    {
        if (array == null)
            return new int[length];

        if (array.Length < length)
        {
            throw new InvalidOperationException("Array length does not match length specified in length argument");
        }

        Span<int> result;
        switch (array)
        {
            case int[] a:
                return new Span<int>(a, 0, length);
            case float[] a:
                result = new int[length];
                for (var i = 0; i < length; i++)
                    result[i] = ValueConverter.ToInt32(a[i]);
                return result;
            case bool[] a:
                result = new int[length];
                for (var i = 0; i < length; i++)
                    result[i] = ValueConverter.ToInt32(a[i]);
                return result;
            default:
                throw new InvalidOperationException("Unsupported array type");
        }
    }

    /// <summary>
    /// Converts a span of integers to an array of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to store the result in or <c>null</c> if a new array should be allocated.</param>
    /// <param name="span">The span of integers to convert.</param>
    /// <returns>The converted array.</returns>
    public static T[] IntSpanToArray<T>(Array array, Span<int> span)
    {
        array ??= new T[span.Length];

        if (array is not T[] result)
        {
            throw new InvalidOperationException("Array is not of specified type");
        }
            
        if (array.Length < span.Length)
        {
            throw new InvalidOperationException("Array length does not match length of native result");
        }
            
        if(typeof(T) == typeof(int))
            CopySpan(span, (int[])(object)result);
        else if(typeof(T) == typeof(float))
            CopySpan(span, (float[])(object)result);
        else if(typeof(T) == typeof(bool))
            CopySpan(span, (bool[])(object)result);
        else
            throw new InvalidOperationException("Unsupported parameter type");

        return result;
    }

    /// <summary>
    /// Assigns variable arguments values to the specified buffers.
    /// </summary>
    /// <param name="args">The arguments buffer.</param>
    /// <param name="values">The values buffer.</param>
    /// <param name="varArgs">The variable arguments.</param>
    /// <param name="argOffset">The offset in the arguments buffer at which the varargs start.</param>
    /// <param name="valueOffset">The offset in the values buffer at which the varargs start.</param>
    /// <param name="state">The state of the variable arguments.</param>
    public static unsafe void SetVarArgsValues(int* args, int* values, object[] varArgs, int argOffset, int valueOffset, VarArgsState state)
    {
        for (var i = 0; i < varArgs.Length; i++)
        {
            void SetArgPointerToValue()
            {
                args[argOffset + i] = IntPointerToInt((int*)((byte*)values + ((valueOffset + i) * AmxCell.Size)));
            }

            var value = varArgs[i];

            if (value is int intValue)
            {
                SetArgPointerToValue();
                values[valueOffset + i] = intValue;
            }
            else if (value is bool boolValue)
            {
                SetArgPointerToValue();
                values[valueOffset + i] = ValueConverter.ToInt32(boolValue);
            }
            else if (value is float floatValue)
            {
                SetArgPointerToValue();
                values[valueOffset + i] = ValueConverter.ToInt32(floatValue);
            }
            else if (value is string strValue)
            {
                var byteCount = GetByteCount(strValue);
                var strBuffer = new byte[byteCount];
                GetBytes(strValue, strBuffer);
                var address = state.PinBuffer(strBuffer);

                args[argOffset + i] = address;
            }
            else if (value is int[] intArray)
            {
                var address = state.PinBuffer(intArray);
                args[argOffset + i] = address;
            }
            else if (value is float[] floatArray)
            {
                var address = state.PinBuffer(floatArray);
                args[argOffset + i] = address;
            }
            else if (value is bool[] boolArray)
            {
                var boolInts = new int[boolArray.Length];
                for (var j = 0; j < boolArray.Length; j++)
                {
                    boolInts[j] = boolArray[j] ? 1 : 0;
                }

                var address = state.PinBuffer(boolInts);
                args[argOffset + i] = address;
            }
            else
            {
                // unknown type handle like an integer with value 0.
                SetArgPointerToValue();
                values[valueOffset + i] = 0;
            }
        }
    }
        
    /// <summary>
    /// Appends the format for specified variable arguments to the specified format.
    /// </summary>
    /// <param name="format">The format to append to.</param>
    /// <param name="varArgs">The variable arguments for which to append formatting.</param>
    /// <returns>The complete arguments format.</returns>
    public static string AppendVarArgsFormat(string format, object[] varArgs)
    {
        if (varArgs.Length == 0)
        {
            return format;
        }

        var sb = new StringBuilder(format);

        foreach (var value in varArgs)
        {
            if (value is Array array)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "a[{0}]", array.Length);
            }
            else if (value is string)
            {
                sb.Append('s');
            }
            else
            {
                // "In Pawn variadic functions always take their variable arguments (those represented by "...") by reference. This
                // means that for such functions you have to use the 'r' specifier where you would normally use 'b', 'i' 'd' or 'f'."
                sb.Append('r');
            }
        }

        return sb.ToString();
    }

    private static void CopySpan(Span<int> span, int[] arr)
    {
        span.CopyTo(new Span<int>(arr));
    }

    private static void CopySpan(Span<int> span, float[] arr)
    {
        for (var i = 0; i < span.Length; i++)
        {
            arr[i] = ValueConverter.ToSingle(span[i]);
        }
    }

    private static void CopySpan(Span<int> span, bool[] arr)
    {
        for (var i = 0; i < span.Length; i++)
        {
            arr[i] = ValueConverter.ToBoolean(span[i]);
        }
    }
}