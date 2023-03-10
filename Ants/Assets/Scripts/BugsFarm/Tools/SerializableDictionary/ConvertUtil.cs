using UnityEngine;


/*
	https://forum.unity.com/threads/finally-a-serializable-dictionary-for-unity-extracted-from-system-collections-generic.335797/#post-2282920
*/


namespace com.spacepuppy.Utils
{
    public static class ConvertUtil
    {

        #region RegEx Constants

        private const string RX_ISHEX = @"(?<sign>[-+]?)(?<flag>0x|#|&H)(?<num>[\dA-F]+)(?<fractional>(\.[\dA-F]+)?)$";

        #endregion

        #region Color

        public static int ToInt(Color color)
        {
            return (Mathf.RoundToInt(color.a * 255) << 24) +
                   (Mathf.RoundToInt(color.r * 255) << 16) +
                   (Mathf.RoundToInt(color.g * 255) << 8) +
                   Mathf.RoundToInt(color.b * 255);
        }

        public static Color ToColor(int value)
        {
            var a = (float)(value >> 24 & 0xFF) / 255f;
            var r = (float)(value >> 16 & 0xFF) / 255f;
            var g = (float)(value >> 8 & 0xFF) / 255f;
            var b = (float)(value & 0xFF) / 255f;
            return new Color(r, g, b, a);
        }

        public static Color ToColor(string value)
        {
            return ToColor(ToInt(value));
        }

        public static Color ToColor(Color32 value)
        {
            return new Color((float)value.r / 255f,
                             (float)value.g / 255f,
                             (float)value.b / 255f,
                             (float)value.a / 255f);
        }

        public static Color ToColor(Vector3 value)
        {

            return new Color((float)value.x,
                             (float)value.y,
                             (float)value.z);
        }

        public static Color ToColor(Vector4 value)
        {
            return new Color((float)value.x,
                             (float)value.y,
                             (float)value.z,
                             (float)value.w);
        }

        public static Color ToColor(object value)
        {
            if (value is Color) return (Color)value;
            if (value is Color32) return ToColor((Color32)value);
            if (value is Vector3) return ToColor((Vector3)value);
            if (value is Vector4) return ToColor((Vector4)value);
            return ToColor(ToInt(value));
        }

        public static int ToInt(Color32 color)
        {
            return (color.a << 24) +
                   (color.r << 16) +
                   (color.g << 8) +
                   color.b;
        }

        public static Color32 ToColor32(int value)
        {
            byte a = (byte)(value >> 24 & 0xFF);
            byte r = (byte)(value >> 16 & 0xFF);
            byte g = (byte)(value >> 8 & 0xFF);
            byte b = (byte)(value & 0xFF);
            return new Color32(r, g, b, a);
        }
        
        public static Color32 ToColor32(string value)
        {
            return ToColor32(ToInt(value));
        }

        public static Color32 ToColor32(Color value)
        {
            return new Color32((byte)(value.r * 255f),
                               (byte)(value.g * 255f),
                               (byte)(value.b * 255f),
                               (byte)(value.a * 255f));
        }

        public static Color32 ToColor32(Vector3 value)
        {

            return new Color32((byte)(value.x * 255f),
                               (byte)(value.y * 255f),
                               (byte)(value.z * 255f), 255);
        }

        public static Color32 ToColor32(Vector4 value)
        {
            return new Color32((byte)(value.x * 255f),
                               (byte)(value.y * 255f),
                               (byte)(value.z * 255f),
                               (byte)(value.w * 255f));
        }

        public static Color32 ToColor32(object value)
        {
            if (value is Color32) return (Color32)value;
            if (value is Color) return ToColor32((Color)value);
            if (value is Vector3) return ToColor32((Vector3)value);
            if (value is Vector4) return ToColor32((Vector4)value);
            return ToColor32(ToInt(value));
        }

        #endregion

        #region ConvertToUInt

        /// <summary>
        /// This will convert an integer to a uinteger. The negative integer value is treated as what the memory representation of that negative 
        /// value would be as a uinteger.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static uint ToUInt(sbyte value)
        {
            return System.Convert.ToUInt32(value);
        }

        public static uint ToUInt(byte value)
        {
            return System.Convert.ToUInt32(value);
        }

        public static uint ToUInt(short value)
        {
            return System.Convert.ToUInt32(value);
        }

        public static uint ToUInt(ushort value)
        {
            return System.Convert.ToUInt32(value);
        }

        public static uint ToUInt(int value)
        {
            return System.Convert.ToUInt32(value & 0xffffffffu);
        }

        public static uint ToUInt(uint value)
        {
            return value;
        }

        public static uint ToUInt(long value)
        {
            return System.Convert.ToUInt32(value & 0xffffffffu);
        }

        public static uint ToUInt(ulong value)
        {
            return System.Convert.ToUInt32(value & 0xffffffffu);
        }

        ////public static uint ToUInt(float value)
        ////{
        ////    return System.Convert.ToUInt32(value & 0xffffffffu);
        ////}

        ////public static uint ToUInt(double value)
        ////{
        ////    return System.Convert.ToUInt32(value & 0xffffffffu);
        ////}

        ////public static uint ToUInt(decimal value)
        ////{
        ////    return System.Convert.ToUInt32(value & 0xffffffffu);
        ////}

        public static uint ToUInt(bool value)
        {
            return (value) ? 1u : 0u;
        }

        public static uint ToUInt(char value)
        {
            return System.Convert.ToUInt32(value);
        }

        public static uint ToUInt(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToUInt32(value);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return ToUInt(value.ToString());
            }
        }

        public static uint ToUInt(string value, System.Globalization.NumberStyles style)
        {
            return ToUInt(ToDouble(value, style));
        }

        public static uint ToUInt(string value)
        {
            return ToUInt(ToDouble(value, System.Globalization.NumberStyles.Any));
        }

        #endregion

        #region ConvertToInt

        public static int ToInt(sbyte value)
        {
            return System.Convert.ToInt32(value);
        }

        public static int ToInt(byte value)
        {
            return System.Convert.ToInt32(value);
        }

        public static int ToInt(short value)
        {
            return System.Convert.ToInt32(value);
        }

        public static int ToInt(ushort value)
        {
            return System.Convert.ToInt32(value);
        }

        public static int ToInt(int value)
        {
            return value;
        }

        public static int ToInt(uint value)
        {
            if (value > int.MaxValue)
            {
                return int.MinValue + System.Convert.ToInt32(value & 0x7fffffff);
            }
            else
            {
                return System.Convert.ToInt32(value & 0xffffffff);
            }
        }

        public static int ToInt(long value)
        {
            if (value > int.MaxValue)
            {
                return int.MinValue + System.Convert.ToInt32(value & 0x7fffffff);
            }
            else
            {
                return System.Convert.ToInt32(value & 0xffffffff);
            }
        }

        public static int ToInt(ulong value)
        {
            if (value > int.MaxValue)
            {
                return int.MinValue + System.Convert.ToInt32(value & 0x7fffffff);
            }
            else
            {
                return System.Convert.ToInt32(value & 0xffffffff);
            }
        }

        public static int ToInt(float value)
        {
            return System.Convert.ToInt32(value);
            //if (value > int.MaxValue)
            //{
            //    return int.MinValue + System.Convert.ToInt32(value & 0x7fffffff);
            //}
            //else
            //{
            //    return System.Convert.ToInt32(value & 0xffffffff);
            //}
        }

        public static int ToInt(double value)
        {
            return System.Convert.ToInt32(value);
            //if (value > int.MaxValue)
            //{
            //    return int.MinValue + System.Convert.ToInt32(value & 0x7fffffff);
            //}
            //else
            //{
            //    return System.Convert.ToInt32(value & 0xffffffff);
            //}
        }

        public static int ToInt(decimal value)
        {
            return System.Convert.ToInt32(value);
            //if (value > int.MaxValue)
            //{
            //    return int.MinValue + System.Convert.ToInt32(value & 0x7fffffff);
            //}
            //else
            //{
            //    return System.Convert.ToInt32(value & 0xffffffff);
            //}
        }

        public static int ToInt(bool value)
        {
            return value ? 1 : 0;
        }

        public static int ToInt(char value)
        {
            return System.Convert.ToInt32(value);
        }

        public static int ToInt(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is Color)
            {
                return ToInt((Color)value);
            }
            else if(value is Color32)
            {
                return ToInt((Color32)value);
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToInt32(value);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return ToInt(value.ToString());
            }
        }

        public static int ToInt(string value, System.Globalization.NumberStyles style)
        {
            return ToInt(ToDouble(value, style));
        }
        public static int ToInt(string value)
        {
            return ToInt(ToDouble(value, System.Globalization.NumberStyles.Any));
        }

        #endregion

        #region "ConvertToULong"
        /// <summary>
        /// This will System.Convert an integer to a uinteger. The negative integer value is treated as what the memory representation of that negative 
        /// value would be as a uinteger.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ulong ToULong(sbyte value)
        {
            return System.Convert.ToUInt64(value);
        }

        public static ulong ToULong(byte value)
        {
            return System.Convert.ToUInt64(value);
        }

        public static ulong ToULong(short value)
        {
            return System.Convert.ToUInt64(value);
        }

        public static ulong ToULong(ushort value)
        {
            return System.Convert.ToUInt64(value);
        }

        public static ulong ToULong(int value)
        {
            return System.Convert.ToUInt64(value & long.MaxValue);
        }

        public static ulong ToULong(uint value)
        {
            return System.Convert.ToUInt64(value);
        }

        public static ulong ToULong(long value)
        {
            return System.Convert.ToUInt64(value & long.MaxValue);
        }

        public static ulong ToULong(ulong value)
        {
            return value;
        }

        ////public static ulong ToULong(float value)
        ////{
        ////    return System.Convert.ToUInt64(value & long.MaxValue);
        ////}

        ////public static ulong ToULong(double value)
        ////{
        ////    return System.Convert.ToUInt64(value & long.MaxValue);
        ////}

        ////public static ulong ToULong(decimal value)
        ////{
        ////    return System.Convert.ToUInt64(value & long.MaxValue);
        ////}

        public static ulong ToULong(bool value)
        {
            return (value) ? 1ul : 0ul;
        }

        public static ulong ToULong(char value)
        {
            return System.Convert.ToUInt64(value);
        }

        public static ulong ToULong(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToUInt64(value);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return ToULong(value.ToString());
            }
        }

        public static ulong ToULong(string value, System.Globalization.NumberStyles style)
        {
            return ToULong(ToDouble(value, style));
        }
        public static ulong ToULong(string value)
        {
            return ToULong(ToDouble(value, System.Globalization.NumberStyles.Any));
        }
        #endregion

        #region "ConvertToLong"

        public static long ToLong(sbyte value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(byte value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(short value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(ushort value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(int value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(uint value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(long value)
        {
            return value;
        }

        public static long ToLong(ulong value)
        {
            if (value > long.MaxValue)
            {
                return int.MinValue + System.Convert.ToInt32(value & long.MaxValue);
            }
            else
            {
                return System.Convert.ToInt64(value & long.MaxValue);
            }
        }

        ////public static long ToLong(float value)
        ////{
        ////    return System.Convert.ToInt64(value & long.MaxValue);
        ////}

        ////public static long ToLong(double value)
        ////{
        ////    return System.Convert.ToInt64(value & long.MaxValue);
        ////}

        ////public static long ToLong(decimal value)
        ////{
        ////    return System.Convert.ToInt64(value & long.MaxValue);
        ////}

        public static long ToLong(bool value)
        {
            return value ? 1 : 0;
        }

        public static long ToLong(char value)
        {
            return System.Convert.ToInt64(value);
        }

        public static long ToLong(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToInt64(value);
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return ToLong(value.ToString());
            }
        }

        public static long ToLong(string value, System.Globalization.NumberStyles style)
        {
            return ToLong(ToDouble(value, style));
        }

        public static long ToLong(string value)
        {
            return ToLong(ToDouble(value, System.Globalization.NumberStyles.Any));
        }
        #endregion

        #region "ToSingle"

        public static float ToSingle(sbyte value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(byte value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(short value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(ushort value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(int value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(uint value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(long value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(ulong value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(float value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(double value)
        {
            return (float)value;
        }

        public static float ToSingle(decimal value)
        {
            return System.Convert.ToSingle(value);
        }

        public static float ToSingle(bool value)
        {
            return value ? 1 : 0;
        }

        public static float ToSingle(char value)
        {
            return ToSingle(System.Convert.ToInt32(value));
        }

        public static float ToSingle(Vector2 value)
        {
            return value.x;
        }

        public static float ToSingle(Vector3 value)
        {
            return value.x;
        }

        public static float ToSingle(Vector4 value)
        {
            return value.x;
        }
        
        public static float ToSingle(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToSingle(value);
                }
                catch
                {
                    return 0;
                }
            }
            else if(value is Vector2)
            {
                return ToSingle((Vector2)value);
            }
            else if(value is Vector3)
            {
                return ToSingle((Vector3)value);
            }
            else if (value is Vector4)
            {
                return ToSingle((Vector3)value);
            }
            else
            {
                return ToSingle(value.ToString());
            }
        }

        public static float ToSingle(string value, System.Globalization.NumberStyles style)
        {
            return System.Convert.ToSingle(ToDouble(value, style));
        }
        public static float ToSingle(string value)
        {
            return System.Convert.ToSingle(ToDouble(value, System.Globalization.NumberStyles.Any));
        }
        #endregion

        #region "ToDouble"

        public static double ToDouble(sbyte value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(byte value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(short value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(ushort value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(int value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(uint value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(long value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(ulong value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(float value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(double value)
        {
            return value;
        }

        public static double ToDouble(decimal value)
        {
            return System.Convert.ToDouble(value);
        }

        public static double ToDouble(bool value)
        {
            return value ? 1 : 0;
        }

        public static double ToDouble(char value)
        {
            return ToDouble(System.Convert.ToInt32(value));
        }

        public static double ToDouble(Vector2 value)
        {
            return value.x;
        }

        public static double ToDouble(Vector3 value)
        {
            return value.x;
        }

        public static double ToDouble(Vector4 value)
        {
            return value.x;
        }
        
        public static double ToDouble(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToDouble(value);
                }
                catch
                {
                    return 0;
                }
            }
            else if (value is Vector2)
            {
                return ToDouble((Vector2)value);
            }
            else if (value is Vector3)
            {
                return ToDouble((Vector3)value);
            }
            else if (value is Vector4)
            {
                return ToDouble((Vector3)value);
            }
            else
            {
                return ToDouble(value.ToString(), System.Globalization.NumberStyles.Any, null);
            }
        }

        /// <summary>
        /// System.Converts any string to a number with no errors.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="style"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: I would also like to possibly include support for other number system bases. At least binary and octal.
        /// </remarks>
        public static double ToDouble(string value, System.Globalization.NumberStyles style, System.IFormatProvider provider)
        {
            if (string.IsNullOrEmpty(value)) return 0d;

            style = style & System.Globalization.NumberStyles.Any;
            double dbl = 0;
            if(double.TryParse(value, style, provider, out dbl))
            {
                return dbl;
            }
            else
            {
                //test hex
                int i;
                bool isNeg = false;
                for (i = 0; i < value.Length; i++)
                {
                    if (value[i] == ' ' || value[i] == '+') continue;
                    if (value[i] == '-')
                    {
                        isNeg = !isNeg;
                        continue;
                    }
                    break;
                }

                if (i < value.Length - 1 &&
                        (
                        (value[i] == '#') ||
                        (value[i] == '0' && (value[i + 1] == 'x' || value[i + 1] == 'X')) ||
                        (value[i] == '&' && (value[i + 1] == 'h' || value[i + 1] == 'H'))
                        ))
                {
                    //is hex
                    style = (style & System.Globalization.NumberStyles.HexNumber) | System.Globalization.NumberStyles.AllowHexSpecifier;

                    if (value[i] == '#') i++;
                    else i += 2;
                    int j = value.IndexOf('.', i);

                    if (j >= 0)
                    {
                        long lng = 0;
                        long.TryParse(value.Substring(i, j - i), style, provider, out lng);

                        if (isNeg)
                            lng = -lng;

                        long flng = 0;
                        string sfract = value.Substring(j + 1).Trim();
                        long.TryParse(sfract, style, provider, out flng);
                        return System.Convert.ToDouble(lng) + System.Convert.ToDouble(flng) / System.Math.Pow(16d, sfract.Length);
                    }
                    else
                    {
                        string num = value.Substring(i);
                        long l;
                        if (long.TryParse(num, style, provider, out l))
                            return System.Convert.ToDouble(l);
                        else
                            return 0d;
                    }
                }
                else
                {
                    return 0d;
                }
            }
            

            ////################
            ////OLD garbage heavy version

            //if (value == null) return 0d;
            //value = value.Trim();
            //if (string.IsNullOrEmpty(value)) return 0d;

            //#if UNITY_WEBPLAYER
            //			Match m = Regex.Match(value, RX_ISHEX, RegexOptions.IgnoreCase);
            //#else
            //            Match m = Regex.Match(value, RX_ISHEX, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //#endif

            //if (m.Success)
            //{
            //    long lng = 0;
            //    style = (style & System.Globalization.NumberStyles.HexNumber) | System.Globalization.NumberStyles.AllowHexSpecifier;
            //    long.TryParse(m.Groups["num"].Value, style, provider, out lng);

            //    if (m.Groups["sign"].Value == "-")
            //        lng = -lng;

            //    if (m.Groups["fractional"].Success)
            //    {
            //        long flng = 0;
            //        string sfract = m.Groups["fractional"].Value.Substring(1);
            //        long.TryParse(sfract, style, provider, out flng);
            //        return System.Convert.ToDouble(lng) + System.Convert.ToDouble(flng) / System.Math.Pow(16d, sfract.Length);
            //    }
            //    else
            //    {
            //        return System.Convert.ToDouble(lng);
            //    }

            //}
            //else
            //{
            //    style = style & System.Globalization.NumberStyles.Any;
            //    double dbl = 0;
            //    double.TryParse(value, style, provider, out dbl);
            //    return dbl;

            //}
        }

        public static double ToDouble(string value, System.Globalization.NumberStyles style)
        {
            return ToDouble(value, style, null);
        }

        public static double ToDouble(string value)
        {
            return ToDouble(value, System.Globalization.NumberStyles.Any, null);
        }
        #endregion

        #region "ToDecimal"
        public static decimal ToDecimal(sbyte value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(byte value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(short value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(ushort value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(int value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(uint value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(long value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(ulong value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(float value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(double value)
        {
            return System.Convert.ToDecimal(value);
        }

        public static decimal ToDecimal(decimal value)
        {
            return value;
        }

        public static decimal ToDecimal(bool value)
        {
            return value ? 1 : 0;
        }

        public static decimal ToDecimal(char value)
        {
            return ToDecimal(System.Convert.ToInt32(value));
        }

        public static decimal ToDecimal(Vector2 value)
        {
            return (decimal)value.x;
        }

        public static decimal ToDecimal(Vector3 value)
        {
            return (decimal)value.x;
        }

        public static decimal ToDecimal(Vector4 value)
        {
            return (decimal)value.x;
        }

        public static decimal ToDecimal(object value)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToDecimal(value);
                }
                catch
                {
                    return 0;
                }
            }
            else if (value is Vector2)
            {
                return ToDecimal((Vector2)value);
            }
            else if (value is Vector3)
            {
                return ToDecimal((Vector3)value);
            }
            else if (value is Vector4)
            {
                return ToDecimal((Vector3)value);
            }
            else
            {
                return ToDecimal(value.ToString());
            }
        }

        public static decimal ToDecimal(string value, System.Globalization.NumberStyles style)
        {
            return System.Convert.ToDecimal(ToDouble(value, style));
        }
        public static decimal ToDecimal(string value)
        {
            return System.Convert.ToDecimal(ToDouble(value, System.Globalization.NumberStyles.Any));
        }
        #endregion

        #region "ToBool"
        public static bool ToBool(sbyte value)
        {
            return value != 0;
        }

        public static bool ToBool(byte value)
        {
            return value != 0;
        }

        public static bool ToBool(short value)
        {
            return value != 0;
        }

        public static bool ToBool(ushort value)
        {
            return value != 0;
        }

        public static bool ToBool(int value)
        {
            return value != 0;
        }

        public static bool ToBool(uint value)
        {
            return value != 0;
        }

        public static bool ToBool(long value)
        {
            return value != 0;
        }

        public static bool ToBool(ulong value)
        {
            return value != 0;
        }

        public static bool ToBool(float value)
        {
            return value != 0;
        }

        public static bool ToBool(double value)
        {
            return value != 0;
        }

        public static bool ToBool(decimal value)
        {
            return value != 0;
        }

        public static bool ToBool(bool value)
        {
            return value;
        }

        public static bool ToBool(char value)
        {
            return System.Convert.ToInt32(value) != 0;
        }

        public static bool ToBool(object value)
        {
            if (value == null)
            {
                return false;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToBoolean(value);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return ToBool(value.ToString());
            }
        }

        /// <summary>
        /// Converts a string to boolean. Is FALSE greedy.
        /// A string is considered TRUE if it DOES meet one of the following criteria:
        /// 
        /// doesn't read blank: ""
        /// doesn't read false (not case-sensitive)
        /// doesn't read 0
        /// doesn't read off (not case-sensitive)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool ToBool(string str)
        {
            //str = (str + "").Trim().ToLower();
            //return !System.Convert.ToBoolean(string.IsNullOrEmpty(str) || str == "false" || str == "0" || str == "off");

            return !(string.IsNullOrEmpty(str) || str.Equals("false", System.StringComparison.OrdinalIgnoreCase) || str.Equals("0", System.StringComparison.OrdinalIgnoreCase) || str.Equals("off", System.StringComparison.OrdinalIgnoreCase));
        }


        public static bool ToBoolInverse(sbyte value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(byte value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(short value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(ushort value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(int value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(uint value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(long value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(ulong value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(float value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(double value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(decimal value)
        {
            return value != 0;
        }

        public static bool ToBoolInverse(bool value)
        {
            return value;
        }

        public static bool ToBoolInverse(char value)
        {
            return System.Convert.ToInt32(value) != 0;
        }

        public static bool ToBoolInverse(object value)
        {
            if (value == null)
            {
                return false;
            }
            else if (value is System.IConvertible)
            {
                try
                {
                    return System.Convert.ToBoolean(value);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return ToBoolInverse(value.ToString());
            }
        }

        /// <summary>
        /// Converts a string to boolean. Is TRUE greedy (inverse of ToBool)
        /// A string is considered TRUE if it DOESN'T meet any of the following criteria:
        /// 
        /// reads blank: ""
        /// reads false (not case-sensitive)
        /// reads 0
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool ToBoolInverse(string str)
        {
            //str = (str + "").Trim().ToLower();
            //return (!string.IsNullOrEmpty(str) && str != "false" && str != "0");

            return !string.IsNullOrEmpty(str) && 
                   !str.Equals("false", System.StringComparison.OrdinalIgnoreCase) && 
                   !str.Equals("0", System.StringComparison.OrdinalIgnoreCase) && 
                   !str.Equals("off", System.StringComparison.OrdinalIgnoreCase);
        }
        #endregion



    }
}

