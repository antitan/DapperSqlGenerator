using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator;
using DapperSqlGenerator.App.Helpers; 
using Microsoft.SqlServer.Dac.Model; 

namespace DapperSqlGenerator.App.Generator.Controllers
{
    public class RequestsGenerator : IGenerate, IGenerateClass
    {
        TSqlObject table;
        string requestNamespace;
        string modelNamespace;
        public RequestsGenerator(string modelNamespace,string requestNamespace, TSqlObject table)
        {
            this.table = table;  
            this.requestNamespace = requestNamespace;  
            this.modelNamespace = modelNamespace;   
        }

        public string Generate()
        {
            return $"using {modelNamespace};" + Environment.NewLine + Environment.NewLine +
                  $@"namespace {requestNamespace} {{ 
                       {GenerateClass()}
                    }}";
        }
       

        public string GenerateClassPart()
        {
            return Generate();
        }

        /// <summary>
        /// Get the declaration for the repo class
        /// </summary>
        /// <returns></returns>
        private string GenerateClass()
        {
            var entityClassName = table.Name.Parts[1];
            var entityClassNamePascal = entityClassName.PascalCase();
            var controllerName = entityClassNamePascal + "Controller";
            var serviceInterface = "I" + entityClassName+ "Service";
            var serviceVariable = entityClassNamePascal + "Service";
            var pkFieldsNames = Common.ConcatPkFieldTypes(table);

            var allColumns = table.GetAllColumns();
            var pkColumns = table.GetPrimaryKeyColumns();

            var memberDeclarationsWihtoutPk = string.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = colName.PascalCase();
                var colDataType = col.GetColumnSqlDataType();//false
                var isNullable = col.IsColumnNullable();
                bool isPk = pkColumns.SingleOrDefault(c => c.Name.Parts[2] == colName) != null ? true : false;

                if(isPk)return string.Empty;

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

                if ("IsDeleted" == memberName) return string.Empty;

                return $"{decorators}public {memberType} {memberName} {{ get; set; }}";
            }));

            var memberDeclarationsPkOnly = string.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = colName.PascalCase();
                var colDataType = col.GetColumnSqlDataType();//false
                var isNullable = col.IsColumnNullable();
                bool isPk = pkColumns.SingleOrDefault(c => c.Name.Parts[2] == colName) != null ? true : false;

                if (!isPk) return string.Empty;

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

            string output =
            $@"  
                 public class {entityClassName}RequestAddDto
                {{ 
                    {memberDeclarationsWihtoutPk}
                }}
	            public class {entityClassName}RequestUpdateDto : {entityClassName}RequestAddDto
	            {{
		             {memberDeclarationsPkOnly}
	            }}
            ";

            return output;
        }



    }
}
