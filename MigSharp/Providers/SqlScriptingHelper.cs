﻿using System;
using System.Data;
using System.Globalization;
using JetBrains.Annotations;

namespace MigSharp.Providers
{
    internal static class SqlScriptingHelper
    {
        [Pure]
        public static string ToSql(object value, DbType dbType, bool prefixUnicodeLiterals)
        {
            if (value == null) throw new ArgumentNullException("value");

            if (DBNull.Value.Equals(value))
            {
                return "NULL";
            }

            Func<object, string> script = GetScriptingFunction(dbType, prefixUnicodeLiterals);
            if (script == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Values of type {0} cannot be scripted.", dbType));
            }
            try
            {
                return script(value);
            }
            catch (FormatException x)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not convert '{0}' of type {1} to a SQL expression of type {2}.", value, value.GetType(), dbType), x);
            }
        }

        private static Func<object, string> GetScriptingFunction(DbType dbType, bool prefixUnicodeLiterals)
        {
            string unicodePrefix = ((dbType == DbType.String || dbType == DbType.StringFixedLength) && prefixUnicodeLiterals) ? "N" : string.Empty;
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return value => unicodePrefix + "'" + Convert.ToString(value, CultureInfo.InvariantCulture).Replace("'", "''") + "'";
                case DbType.Byte:
                    return value => Convert.ToByte(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Boolean:
                    return value => Convert.ToBoolean(value, CultureInfo.InvariantCulture) ? "1" : "0";
                case DbType.Decimal:
                case DbType.VarNumeric:
                    return value => Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Date:
                    return value => "'" + ((DateTime)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'"; // ISO 8601                    
                case DbType.DateTime:
                case DbType.DateTime2:
                    return value =>
                        {
                            DateTime dateTime = (DateTime)value;
                            if (dateTime.Millisecond > 0)
                            {
                                return "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "'"; // ISO 8601          
                            }
                            else
                            {
                                return "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) + "'"; // ISO 8601
                            }
                        };
                case DbType.Double:
                    return value => Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture);
                case DbType.Int16:
                    return value => Convert.ToInt16(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Int32:
                    return value => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Int64:
                    return value => Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.SByte:
                    return value => Convert.ToSByte(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Single:
                    return value => Convert.ToSingle(value, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture);
                case DbType.UInt16:
                    return value => Convert.ToUInt16(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.UInt32:
                    return value => Convert.ToUInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.UInt64:
                    return value => Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                default:
                    return null;
            }
        }

        internal static bool IsScriptable(DbType dbType)
        {
            return GetScriptingFunction(dbType, true) != null;
        }
    }
}