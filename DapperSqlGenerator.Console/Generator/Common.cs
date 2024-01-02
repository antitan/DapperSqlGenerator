using DapperSqlGenerator.Console.Extenions;
using DapperSqlGenerator.Console.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Generator
{
    public static class Common
    {
        /// <summary>
        /// Helper to convert first char to lower
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FirstCharacterToLower(string str)
        {
            return String.IsNullOrEmpty(str) || Char.IsLower(str, 0)
                ? str
                : Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Concat all the actual pk fields names in a string with "&&" as a separator
        /// </summary>
        /// <returns></returns>
        public static string ConcatPkFieldNamesForLinq(TSqlObject table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            return String.Join(" && ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"f.{colName} == {Common.FirstCharacterToLower(colName.PascalCase())}";
            }));
        }

        /// <summary>
        /// Concat all the actual pk fields names in a string with "And" as a separator
        /// </summary>
        /// <returns></returns>
        public static string ConcatPkFieldNames(TSqlObject table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            return String.Join("And",
                      pkColumns.Select(col =>
                      {
                          var colName = col.Name.Parts[2];
                          return $"{colName.PascalCase()}";
                      })
                    );
        }

        /// <summary>
        /// Concat all the pk fields with their types (for method signature)
        /// </summary>
        /// <returns></returns>
        public static string ConcatPkFieldsWithTypes(TSqlObject table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            return String.Join(", ",
                      pkColumns.Select(col =>
                      {
                          var colName = col.Name.Parts[2];
                          var colDataType = col.GetColumnSqlDataType(false);

                          //Search for custom member type or use the conversion from Sql Types
                          var memberType = MatchingDataTypeHelper.GetDotNetDataType(colDataType, false);

                          return $"{memberType} {Common.FirstCharacterToLower(colName.PascalCase())}";
                      })
                    );
        }

        /// <summary>
        /// Concat pk fields for call : PkIdPart1,PkIdPart2
        /// </summary>
        /// <returns></returns>
        public static string ConcatPkFieldsWithComma(TSqlObject table)
        {
            var pkColumns = table.GetPrimaryKeyColumns();
            return String.Join(", ",
                      pkColumns.Select(col =>
                      {
                          var colName = col.Name.Parts[2]; 
                          return $"{Common.FirstCharacterToLower(colName.PascalCase())}";
                      })
                    );
        }

    }
}
