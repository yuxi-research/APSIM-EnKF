﻿using System;
using System.Reflection;

namespace SWIMFrame
{
    /// <summary>
    /// Various utilities and extensions used by SWIM.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start and end indexes.
        /// From http://www.dotnetperls.com/array-slice. Provides a simpler way of slicing arrays
        /// when converting from FORTRAN.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            int len = end - start + 1;

            // Return new array.
            T[] res = new T[len+1];
            for (int i = 0; i < len; i++)
                res[i + 1] = source[i + start];
            return res;
        }

        /// <summary>
        /// General method to allow calling of private members for unit testing.
        /// http://www.codeproject.com/Articles/19911/Dynamically-Invoke-A-Method-Given-Strings-with-Met
        /// </summary>
        /// <param name="typeName" type="string">the class type that the method belongs to</param>
        /// <param name="methodName" type="string">name of the method</param>
        /// <param name="parameters" type="params object[]">one or more parameters for the method you wish to call</param>
        /// <returns type="object">returns an object if it has anything to return</returns>
        public static object TestMethod(string typeName, string methodName, params object[] parameters)
        {
            //get the type of the class
            Type theType = Type.GetType("SWIMFrame." + typeName + ", SWIMFrame");

            //invoke the method from the string. if there is anything to return, it returns to obj.
            object obj = theType.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, parameters);

            //return the object if it exists
            return obj;
        }

        /// <summary>
        /// Populate an array with a value.
        /// </summary>
        /// <typeparam name="T">the class type that the method belongs to</typeparam>
        /// <param name="arr">The array to fill.</param>
        /// <param name="value">The value to fill with.</param>
        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = value;
            return arr;
        }

        /// <summary>
        /// Populate a 2D array.
        /// </summary>
        /// <typeparam name="T">the class type that the method belongs to</typeparam>
        /// <param name="arr">The 2D array to populate.</param>
        /// <param name="value">The value to fill with.</param>
        /// <returns></returns>
        public static T[,] Populate2D<T>(this T[,] arr, T value)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
                for (int y = 0; y < arr.GetLength(1); y++)
                    arr[x, y] = value;
            return arr;
        }

        /// <summary>
        /// Returns a row or column from the given 2D array.
        /// </summary>
        /// <param name="m">The array.</param>
        /// <param name="index">The index to return.</param>
        /// <param name="column">True for column, false for row</param>
        /// <returns></returns>
        public static double[] GetRowCol(double[,] m, int index, bool column)
        {
            double[] retVal;
            if(column)
            {
                retVal = new double[m.GetLength(0)];
                for (int i = 0; i < m.GetLength(0); i++)
                    retVal[i] = m[i, index];
            }
            else
            {
                retVal = new double[m.GetLength(1)];
                for (int i = 0; i < m.GetLength(1); i++)
                    retVal[i] = m[index,i];
            }
            return retVal;
        }
    }
}
