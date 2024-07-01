using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Generator.Entities
{
    public class EntityClassGenerator : IGenerate
    {
        TSqlObject table;
        string projectNamespace;
        public EntityClassGenerator(string projectNamespace, TSqlObject table)
        {
            this.table = table;
            this.projectNamespace = projectNamespace;
        }

        public string Generate()
        {
            var allColumns = table.GetAllColumns();
            var pkColumns = table.GetPrimaryKeyColumns();

            var memberDeclarations = string.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = colName.PascalCase();
                var colDataType = col.GetColumnSqlDataType();
                var isNullable = col.IsColumnNullable();
                bool isPk = pkColumns.SingleOrDefault(c => c.Name.Parts[2] == colName) != null ? true : false;

                var memberType = MatchingDataTypeHelper.GetDotNetDataType(colDataType, isNullable);

                //Decorators
                var decorators = string.Empty;


                if (memberType == "string")
                {
                    var colLen = col.GetProperty<int>(Column.Length);
                    if (colLen > 0)
                    {
                        decorators += $"[System.ComponentModel.DataAnnotations.StringLength({colLen})]" + Environment.NewLine + "        ";
                    }
                }

                return $"{decorators}public {memberType} {memberName} {{ get; set; }}" + Environment.NewLine;
            }));

            string output = $@"using System;" + Environment.NewLine +
                            $"namespace {projectNamespace}" + Environment.NewLine +
                            "{" + Environment.NewLine +
                            $"  public class {table.Name.Parts[1]}" + Environment.NewLine +
                            "   { " + Environment.NewLine +
                            $"      {memberDeclarations}" + Environment.NewLine +
                            "   }" + Environment.NewLine +
                            "}" + Environment.NewLine;

            return output;
        }

    }
}