﻿using System.IO;
using System.Text;

namespace OfficeSoft.Data.Crud
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.OleDb;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Xml;

    public static class ValueConvert
    {




        public static T TryGetDataValue<T>(IDataReader reader, string name)
        {
            try
            {
                var objectValue = reader[name];
                if (objectValue != DBNull.Value)
                {
                    return (T)objectValue;
                }
            }
            catch (Exception exception)
            {
                LoggingService.SetMessage("TryGetDataValue cant convert", name + " " + exception.Message + " " + exception.StackTrace, ErrorType.Error);
            }
            return default(T);
        }



        public static OleDbType GetOleDbType(this SqlDbType sqlDbType)
        {
            //http://msdn.microsoft.com/en-us/library/yy6y35y8%28v=vs.110%29.aspx
            switch (sqlDbType)
            {
                case SqlDbType.BigInt:
                    return OleDbType.BigInt;
                case SqlDbType.Binary:
                    return OleDbType.Binary;
                case SqlDbType.Bit:
                    return OleDbType.Boolean;
                    break;
                case SqlDbType.Char:
                    return OleDbType.Char;
                    break;
                case SqlDbType.DateTime:
                    return OleDbType.DBTimeStamp;
                    break;
                case SqlDbType.Decimal:
                    return OleDbType.Decimal;
                    break;
                case SqlDbType.Float:
                    return OleDbType.Double;
                    break;
                case SqlDbType.Image:
                    return OleDbType.Variant;
                    break;
                case SqlDbType.Int:
                    return OleDbType.Integer;
                    break;
                case SqlDbType.Money:
                    return OleDbType.Currency;
                    break;
                case SqlDbType.NChar:
                    return OleDbType.WChar;
                    break;
                case SqlDbType.NText:
                    return OleDbType.VarWChar;
                    break;
                case SqlDbType.NVarChar:
                    return OleDbType.VarWChar;
                    break;
                case SqlDbType.Real:
                    return OleDbType.Single;
                    break;
                case SqlDbType.UniqueIdentifier:
                    return OleDbType.Guid;
                    break;
                case SqlDbType.SmallDateTime:
                    return OleDbType.DBDate;
                    break;
                case SqlDbType.SmallInt:
                    return OleDbType.SmallInt;
                    break;
                case SqlDbType.SmallMoney:
                    return OleDbType.Currency;
                    break;
                case SqlDbType.Text:
                    return OleDbType.VarWChar;
                    break;
                case SqlDbType.Timestamp:
                    return OleDbType.DBTimeStamp;
                    break;
                case SqlDbType.TinyInt:
                    return OleDbType.TinyInt;
                    break;
                case SqlDbType.VarBinary:
                    return OleDbType.VarBinary;
                    break;
                case SqlDbType.VarChar:
                    return OleDbType.VarChar;
                    break;
                case SqlDbType.Variant:
                    return OleDbType.Variant;
                    break;
                case SqlDbType.Xml:
                    return OleDbType.VarWChar;
                    break;
                case SqlDbType.Udt:
                    return OleDbType.Variant;
                    break;
                case SqlDbType.Structured:
                    return OleDbType.Variant;
                    break;
                case SqlDbType.Date:
                    return OleDbType.Date;
                    break;
                case SqlDbType.Time:
                    return OleDbType.DBTime;
                    break;
                case SqlDbType.DateTime2:
                    return OleDbType.DBTimeStamp;
                    break;
                case SqlDbType.DateTimeOffset:
                    return OleDbType.DBTimeStamp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sqlDbType");
            }

        }





        public static int ToInt(this object value)
        {

            if (value == null || value == DBNull.Value)
            {
                return 0;
            }
            if (value.ToString() != string.Empty)
            {

                int tempValue = 0;
                if (int.TryParse(value.ToStringValue(), out tempValue))
                {
                    return tempValue;
                }

                else
                {

                }

            }

            return 0;
        }

        public static int? ToIntNullable(this object value)
        {

            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            if (value.ToString() != string.Empty)
                return int.Parse(value.ToString());
            return 0;
        }

        public static string ToAnsiStringFixedLength(this object value)
        {
            return ToStringValue(value);
        }

        public static int ToInt32(this object value)
        {
            return ToInt(value);
        }
        public static string ToString(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;
            return value.ToString();
        }

        public static string ToStringValue(this object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;
            return value.ToString();
        }


        public static bool IsNullOrEmpty(this object value)
        {
            if (value == null)
                return true;


            if (string.IsNullOrEmpty(value.ToStringValue()))
                return true;


            return false;
        }

        public static bool IsNotNullOrEmpty(this object value)
        {
            if (value == null)
                return false;


            if (string.IsNullOrEmpty(value.ToStringValue()))
                return false;


            return true;
        }


        public  static bool IsFileLocked(this FileInfo file)
        {
            FileStream fileStream = (FileStream)null;
            try
            {
                if (!file.IsReadOnly)
                    fileStream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                return true;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
            }
            return false;
        }

        public static string ToStringValue(this object value, string defaultValue)
        {
            if (value == null || value == DBNull.Value) return defaultValue;

            string tempString = Convert.ToString(value);
            if (string.IsNullOrEmpty(tempString)) return defaultValue;

            return value.ToString();
        }

        public static string ToAnsiString(this object value)
        {
            return ToStringValue(value);
        }
        public static string ToStringFixedLength(this object value)
        {
            return ToStringValue(value);
        }

        public static XmlDocument ToXml(this object value)
        {
            return (XmlDocument)value;
        }

        public static DateTime? ToDateTimeNullable(this object value)
        {
            if (value == null || value == DBNull.Value) return null;
            try
            {
                DateTime myD = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture,
                              DateTimeStyles.AssumeLocal);
                return myD;

            }
            catch (Exception)
            {
                try
                {
                    DateTime dateTime = Convert.ToDateTime(value);
                    return dateTime;
                }
                catch (Exception)
                {
                    try
                    {
                        DateTime dateTime = DateTime.FromOADate(value.ToDouble());
                        return dateTime;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            DateTime dateTime = DateTime.FromBinary(value.ToLong());
                            return dateTime;
                        }
                        catch (Exception exception)
                        {

                        }

                    }

                }

            }
            return null;
        }

        public static DateTime ToDateTime(this object value)
        {
            if (value == null || value == DBNull.Value) return

                new DateTime(1753, 1, 1);


            DateTime myD = DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture,
                                          DateTimeStyles.AssumeLocal);


            return myD;
        }

        public static DateTime? ToDateTimeNull(this object value, int addDays)
        {
            if (value == null || value == DBNull.Value)
                return null;

            DateTime myD;
            if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture,
                DateTimeStyles.AssumeLocal, out myD))
            {
                if (addDays != 0)
                {
                    return myD.AddDays(addDays);
                }
            }
            return myD;
        }

        public static string ToDateString(this object value)
        {
            var date = value.ToDateTime();

            return date.ToShortDateString();
        }

        public static Guid ToGuid(this object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return Guid.NewGuid();
            }
            return (Guid)value;
        }


        public static Guid? ToGuidNullable(this object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }
            return (Guid)value;
        }


        public static short ToShort(this object value)
        {
            short myT;
            if (value == null || value == DBNull.Value)
                return 0;
            short.TryParse(value.ToString(), out myT);
            return myT;
        }

        public static double? ToDoubleNullable(this object value)
        {
            double myT;
            if (value == null || value == DBNull.Value)
                return null;
            if (value.ToString().Contains(","))
            {
                value = value.ToString().Replace(".", "");
            }
            double.TryParse(value.ToString(), out myT);
            return myT;
        }

        public static long ToLong(this object value)
        {
            long myT;
            if (value == null || value == DBNull.Value)
                return 0;
            long.TryParse(value.ToString(), out myT);
            return myT;
        }

        public static double ToDouble(this object value)
        {
            double myT;
            if (value == null || value == DBNull.Value)
                return 0;
            if (value.ToString().Contains(","))
            {
                value = value.ToString().Replace(".", "");
            }
            double.TryParse(value.ToString(), out myT);
            return myT;
        }



        public static bool ToBool(this object value)
        {
            if (value == null || value == DBNull.Value)
                return false;
            if (value is string)
            {

                var stringValue = value.ToStringValue().ToLower();

                if (stringValue == "yes" || stringValue == "1" || stringValue == "ja" || stringValue == "y" || stringValue == "j")
                {
                    return true;
                }
                return false;
            }

            bool myb = false;
            try
            {
                myb = Convert.ToBoolean(value);
            }
            catch (Exception exception)
            {


            }


            return myb;

            //return (bool)value;
        }

        public static decimal ToDecimal(this object value)
        {
            decimal myD;
            if (value == null || value == DBNull.Value)
                return 0;
            if (value.ToString().Contains(","))
            {
                value = value.ToString().Replace(".", "");
            }
            decimal.TryParse(value.ToString(), out myD);
            return myD;
        }

        public static byte ToByte(this object value)
        {
            if (value == null || value == DBNull.Value)
                return new byte();
            return (byte)value;
        }

        public static byte[] ToByteArray(this object value)
        {
            if (value == null || value == DBNull.Value)
                return null;
            return (byte[])value;
        }
        public static Byte[] ToBinary(object value)
        {
            return ToByteArray(value);
        }
        public static bool ToBoolean(this object value)
        {
            return ToBool(value);
        }

        public static decimal ToCurrency(this object value)
        {
            return ToDecimal(value);
        }

        public static object ToObject(this object value)
        {
            return value;
        }



        public static T ToEnumValue<T>(this object value, object defaultValue)
        {

            try
            {
                return (T)Enum.Parse(typeof(T), value.ToStringValue(), true);
            }
            catch (Exception exception)
            {

            }

            return (T)defaultValue;

        }

        public static T BaseConvert<T>(this object Source)
        {
            var result = Source;
            Type DestType = typeof(T);

            if ((!(Source.GetType() == DestType)) && (DestType != typeof(object)))
            {
                var converter = TypeDescriptor.GetConverter(DestType);
                result = converter.ConvertFrom(Source);
            }
            return (T)result;
        }

        public static string ToSqlDateTimeString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static DateTime EndOfTheDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }
        public static DateTime BeginningOfTheDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }
        public static DateTime GetNextWorkingDay(this DateTime Current)
        {
            if (Current.DayOfWeek == DayOfWeek.Friday)
            {
                Current = Current.AddDays(3);
            }
            else
            {
                Current = Current.AddDays(1);
            }
            return Current;
        }
        public static DateTime GetLastWorkingDay(this DateTime Current)
        {
            if (Current.DayOfWeek == DayOfWeek.Monday)
            {
                Current = Current.AddDays(-3);
            }
            else
            {
                Current = Current.AddDays(-1);
            }
            return Current;
        }
        public static DateTime GetFirstDayOfWeek(this DateTime date)
        {
            var candidateDate = date;
            while (candidateDate.DayOfWeek != DayOfWeek.Monday)
            {
                candidateDate = candidateDate.AddDays(-1);
            }
            return candidateDate;
        }
        public static int GetWeekNumber(this DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
        public static double MetersToMiles(double? meters)
        {
            if (meters == null)
                return 0F;

            return meters.Value * 0.000621371192;
        }
        public static double MilesToMeters(double? miles)
        {
            if (miles == null)
                return 0;

            return miles.Value * 1609.344;
        }
        public static int GetAge(this DateTime Birthday)
        {
            DateTime now = DateTime.Now;
            int Years = now.Year - Birthday.Year;

            if (Years > 0)
            {
                if (now.Month < Birthday.Month)
                {
                    Years = Years - 1;
                }
                else if (now.Month == Birthday.Month)
                {
                    if (now.Day < Birthday.Day)
                    {
                        Years = Years - 1;
                    }
                }
                return Years;
            }
            else if (Years < 0)
            {
                throw new Exception("error ");
            }
            return 0;
        }
        public static DateTime SetHours(this DateTime MyDate, string strHour)
        {
            MyDate = DateTime.Parse((String.Format("{0} {1}", MyDate.ToShortDateString(), strHour)));
            return MyDate;
        }
        public static int ToInt(this bool value)
        {
            return (value == true) ? 1 : 0;
        }
        public static string ToHex(this string text)
        {
            long input;
            bool isNumeric;
            string hexValue;
            Console.WriteLine("Enter a numeric : ");
            isNumeric = long.TryParse(text.Trim(), out input);
            if (isNumeric)
            {
                hexValue = string.Format("0*{0:X}", input);
                return hexValue;
            }
            else
            {
                Console.WriteLine("invalid input ! ");
                return null;
            }
        }
        public static string makeValidFileName(this string text)
        {
            string pattern = @"[&?:\/*""<>|]";
            string replacestring = "_";
            return Regex.Replace(text, pattern, replacestring);
        }
        public static string ChangePathToInternetURL(this string strFilePath)
        {
            string RetVal = "";
            RetVal = Regex.Replace(strFilePath, @"\\", "/");

            return RetVal = "file:///" + RetVal;
        }
        public static string AddZeroPrefix(this string s, int normaleLengte)
        {
            while (s.Length < normaleLengte)
            {
                s = "0" + s;
            }
            return s;
        }
        public static string ToCSVstring(this string[] SingleArray)
        {
            return String.Join(", ", SingleArray);
        }
        static double RoundToSignificantDigits(this double d, int digits)
        {
            double scale = Math.Pow(10, Math.Floor(Math.Log10(d)) + 1);
            return scale * Math.Round(d / scale, digits);
        }
        public static double GetPercent(this double value, double max)
        {
            return ((value * 100) / max);
        }
        public static int GetPercent(this int value, int max)
        {
            return ((value * 100) / max);
        }

        public static List<T> ToList<T>(this object value)
        {
            var list = new List<T>();
            list.Add((T)value);
            return list;
        }


    }
}