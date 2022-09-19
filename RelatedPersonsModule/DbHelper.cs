using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace RelatedPersonsModule
{
    public class DbHelper
    {
        public static DateTime CheckDbNullForDate(OracleDataReader reader, string colName)
        {
            return !reader.IsDBNull(colName) ? Convert.ToDateTime(reader[colName]) : DateTime.Now;
        }
        public static DateTime? CheckDbNullForNullDate(OracleDataReader reader, string colName)
        {
            return !reader.IsDBNull(colName) ? Convert.ToDateTime(reader[colName]) : null;
        }

        public static string CheckDbNullForString(OracleDataReader reader, string colName)
        {
            return !reader.IsDBNull(colName) ? reader[colName].ToString() : null;
        }

        public static int CheckDbNullForInteger(OracleDataReader reader, string colName)
        {
            return !reader.IsDBNull(colName) ? Convert.ToInt32(reader[colName]) : 0;
        }
        public static int? CheckDbNullForIntegerNull(OracleDataReader reader, string colName)
        {
            return !reader.IsDBNull(colName) ? Convert.ToInt32(reader[colName]) : null;
        }

        public static decimal CheckDbNullForDecimal(OracleDataReader reader, string colName)
        {
            return !reader.IsDBNull(colName) ? Convert.ToDecimal(reader[colName]) : 0;
        }
    }
}
