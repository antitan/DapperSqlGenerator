namespace DapperSqlGenerator.App.Helpers
{
    public static class MatchingDataTypeHelper
    {
        /// <summary>
        /// Translate a SQL data type to the corresponding SQL type class in System.Data.SqlTypes.
        /// This method is needed DataTable generation, for stored procedures accepting table types
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string? GetDotNetDataType_SystemDataSqlTypes(string sqlDataTypeName)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "SqlInt64";
                case "binary":
                case "image":
                case "varbinary":
                    return "SqlBinary";
                case "bit":
                    return "SqlBoolean";
                case "char":
                    return "SqlString";
                case "datetime":
                case "smalldatetime":
                    return "SqlDateTime";
                case "decimal":
                case "money":
                case "numeric":
                    return "SqlDecimal";
                case "float":
                    return "SqlDouble";
                case "int":
                    return "SqlInt32";
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "SqlString";
                case "real":
                    return "SqlSingle";
                case "smallint":
                    return "SqlInt16";
                case "tinyint":
                    return "SqlByte";
                case "uniqueidentifier":
                    return "SqlGuid";
                case "date":
                    return "SqlDateTime";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Translate a SQL data type to the corresponding System.Data.DbType enum value
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string GetDotNetDataType_SystemDataDbTypes(string sqlDataTypeName)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "DbType.Int64";
                case "binary":
                case "image":
                case "varbinary":
                    return "DbType.Binary";
                case "bit":
                    return "DbType.Boolean";
                case "char":
                    return "DbType.String";
                case "datetime":
                case "smalldatetime":
                    return "DbType.DateTime";
                case "decimal":
                case "money":
                case "numeric":
                    return "DbType.Decimal";
                case "float":
                    return "DbType.Double";
                case "int":
                    return "DbType.Int32";
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "DbType.String";
                case "real":
                    return "DbType.Single";
                case "smallint":
                    return "DbType.Int16";
                case "tinyint":
                    return "DbType.Byte";
                case "uniqueidentifier":
                    return "DbType.Guid";
                case "date":
                    return "DbType.DateTime";

                default:
                    return null;
            }
        }


        /// <summary>
        /// Translate a SQL data type to a .NET basic type
        /// </summary>
        /// <param name="sqlDataTypeName"></param>
        /// <returns></returns>
        public static string? GetDotNetDataType(string sqlDataTypeName, bool nullable = false)
        {
            if (sqlDataTypeName == null) throw new ArgumentNullException(nameof(sqlDataTypeName));
            switch (sqlDataTypeName.ToLower())
            {
                case "bigint":
                    return "long" + (nullable ? "?" : "");
                case "binary":
                case "image":
                case "varbinary":
                    return "byte[]";
                case "bit":
                    return "bool" + (nullable ? "?" : "");
                case "char":
                    return "char" + (nullable ? "?" : "");
                case "datetime":
                case "smalldatetime":
                    return "System.DateTime" + (nullable ? "?" : "");
                case "decimal":
                case "money":
                case "numeric":
                    return "decimal" + (nullable ? "?" : "");
                case "float":
                    return "double" + (nullable ? "?" : "");
                case "int":
                    return "int" + (nullable ? "?" : "");
                case "nchar":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return "string";
                case "real":
                    return "single" + (nullable ? "?" : "");
                case "smallint":
                    return "short" + (nullable ? "?" : "");
                case "tinyint":
                    return "byte" + (nullable ? "?" : "");
                case "uniqueidentifier":
                    return "System.Guid" + (nullable ? "?" : "");
                case "date":
                    return "System.DateTime" + (nullable ? "?" : "");

                default:
                    return null;
            }
        }
    }
}
