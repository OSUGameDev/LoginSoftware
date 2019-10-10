using System.Linq;
using System.Text;

namespace GameDevLogin
{
    static class Extensions
    {
        /// <summary>
        /// Convert an array of objects to a string by calling ToString on each element
        /// </summary>
        /// <typeparam name="T">Object to stringify</typeparam>
        /// <param name="values">Array of values</param>
        /// <returns>String</returns>
        public static string ArrayToString<T>(this T[] values)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                builder.Append(values[i]);
                if (i != values.Length - 1)
                    builder.Append(',');
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
