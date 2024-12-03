using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HatLoader
{
    public static class SpapiDataReader
    {
        public static bool HandleLines(string[] lines, Dictionary<string, Func<List<string>, bool>> propertyHandler, Action<string> errorHandler)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var l_ = lines[i];

                if (l_.IsNullOrWhiteSpace())
                    continue;

                var l = l_.Trim();

                if (l.StartsWith("#") && l.Length > 1)
                {
                    var readmode = l.Substring(1).Trim().ToLowerInvariant();

                    if (readmode.IsNullOrWhiteSpace())
                        continue;

                    var res = TryReadDataProperty(lines, ref i, out var prop);

                    if (propertyHandler.TryGetValue(readmode.ToLowerInvariant(), out var handle))
                    {
                        if (!res)
                            continue;

                        if (handle == null)
                            continue;

                        if (!handle(prop))
                            return false;
                    }

                    else
                    {
                        errorHandler?.Invoke($"unknown property {readmode}.");
                        return false;
                    }
                }

                else
                {
                    errorHandler?.Invoke($"unexpected data at line {i + 1}.");
                    return false;
                }
            }

            return true;
        }

        private static bool TryReadDataProperty(string[] lines, ref int index, out List<string> property)
        {
            property = [];

            for (index++; index < lines.Length; index++)
            {
                var l_ = lines[index];

                if (l_.IsNullOrWhiteSpace())
                    continue;

                var l = l_.Trim();

                if (l.StartsWith("#") && l.Length > 1)
                {
                    // Move index back so the data reader can process the next read mode
                    index--;
                    break;
                }

                property.Add(l);
            }

            // Reached end of file
            return property.Count > 0;
        }
    }
}
