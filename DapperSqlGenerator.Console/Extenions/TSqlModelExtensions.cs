using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Extenions
{
    public static class TSqlModelExtensions
    { 
        /// <summary>
        /// Get all tables from a model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="excludeSysSchema"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllTables(this TSqlModel model, bool excludeSysSchema = true)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var tables = model.GetObjects(DacQueryScopes.All, ModelSchema.Table);
            if (tables != null)
            {
                if (excludeSysSchema)
                    tables = tables.Where(currTable => currTable.Name.Parts[0].ToLower() != "sys");
            }
            return tables;
        }

        /// <summary>
        /// Get all roles from a model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="excludeSysSchema"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllRoles(this TSqlModel model, bool excludeSysSchema = true)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var roles = model.GetObjects(DacQueryScopes.All, ModelSchema.Role);
            if (roles != null)
            {
                if (excludeSysSchema)
                    roles = roles.Where(curRole => !curRole.Name.Parts[0].ToLower().Contains("db_") && curRole.Name.Parts[0].ToLower() != "public");
            }
            return roles;
        }


        /// <summary>
        /// Get all columns from a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetAllColumns(this TSqlObject table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            var columns = table.GetReferenced(Table.Columns);
            return columns;
        }




        /// <summary>
        /// Get the data type and its size (if applicable) from a table column.
        /// </summary>
        /// <param name="column">The column to analyze.</param>
        /// <returns>The SQL data type as a string, with details if applicable.</returns>
        public static string GetColumnSqlDataType(this TSqlObject column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));

            // Retrieve the SQL data type.
            SqlDataType sdt = column.GetReferenced(Column.DataType).First().GetProperty<SqlDataType>(DataType.SqlDataType);

            // Determine if additional details like length, precision, or scale are needed.
            switch (sdt)
            {
                case SqlDataType.Xml:
                case SqlDataType.NText:
                case SqlDataType.Text:
                case SqlDataType.Char:
                case SqlDataType.NChar:
                case SqlDataType.VarChar:
                case SqlDataType.NVarChar:
                case SqlDataType.VarBinary:
                    int length = column.GetProperty<int>(Column.Length);
                    bool isMax = column.GetProperty<bool>(Column.IsMax);
                    return $"{sdt.ToString().ToLower()}";

                case SqlDataType.Decimal:
                case SqlDataType.Numeric:
                    int precision = column.GetProperty<int>(Column.Precision);
                    int scale = column.GetProperty<int>(Column.Scale);
                    return $"{sdt.ToString().ToLower()}";
                //return $"{sdt.ToString().ToLower()}({precision},{scale})";

                default:
                    return sdt.ToString().ToLower();
            }
        }


        /// <summary>
        /// Get primary key column(s) from a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<TSqlObject> GetPrimaryKeyColumns(this TSqlObject table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            TSqlObject pk = table.GetReferencing(PrimaryKeyConstraint.Host, DacQueryScopes.UserDefined).FirstOrDefault();
            if (pk != null)
            {
                var columns = pk.GetReferenced(PrimaryKeyConstraint.Columns);
                if (columns != null)
                {
                    return columns;
                }
            }
            return new TSqlObject[0];
        }

        /// <summary>
        /// Get uk(s) with attached column(s) from a table. Based on unique constrains finding
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<TSqlObject>> GetUniqueKeysWithColumns(this TSqlObject table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            IEnumerable<TSqlObject> uks = table.GetReferencing(UniqueConstraint.Host, DacQueryScopes.UserDefined);

            if (uks != null)
            {
                foreach (var uk in uks)
                {
                    var columns = uk.GetReferenced(UniqueConstraint.Columns);

                    if (columns != null)
                    {
                        yield return columns;
                    }
                }
            }

        }


        /// <summary>
        /// Check if a column is nullable
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnNullable(this TSqlObject column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            bool result = column.GetProperty<bool>(Column.Nullable);
            return result;
        }

        /// <summary>
        /// Check if a column is identity
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnIdentity(this TSqlObject column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            bool result = column.GetProperty<bool>(Column.IsIdentity);
            return result;
        }


        /// <summary>
        /// Transforms a string in the form 'foo_bar' into a string in the form 'FooBar'
        /// </summary>
        /// <param name="the_string"></param>
        /// <returns></returns>
        public static string PascalCase(this string the_string)
        {
            // If there are 0 or 1 characters, just return the string.
            if (the_string == null) return the_string;
            if (the_string.Length < 2) return the_string.ToUpper();

            // Split the string into words.
            string[] words = the_string.Split(
                new char[] { '_' },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }
    }
}
