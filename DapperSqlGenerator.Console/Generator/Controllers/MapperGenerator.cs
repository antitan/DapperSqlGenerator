using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator;
using Microsoft.SqlServer.Dac.Model; 

namespace DapperSqlGenerator.App.Generator.Controllers
{
    public class MapperGenerator : IGenerate, IGenerateClass
    {
        TSqlObject table;
        string mapperNamespace;
        string requestsNamespace;
        string responsesNamespace;
        string modelNamespace;
        public MapperGenerator(string modelNamespace,  string mapperNamespace, string requestsNamespace, string responsesNamespace, TSqlObject table)
        {
            this.table = table;  
            this.mapperNamespace = mapperNamespace;  
            this.requestsNamespace = requestsNamespace;
            this.responsesNamespace = responsesNamespace;  
            this.modelNamespace = modelNamespace;
        }

        public string Generate()
        {
            return $"using {requestsNamespace};" + Environment.NewLine +
                    $"using {modelNamespace};" + Environment.NewLine +
                    $"using {responsesNamespace};" + Environment.NewLine + Environment.NewLine +
                    $@"namespace {mapperNamespace} {{
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
            var serviceInterface = "I" + entityClassName + "Service";
            var serviceVariable = entityClassNamePascal + "Service";
            var pkFieldsNames = Common.ConcatPkFieldTypes(table);

            var allColumns = table.GetAllColumns();
            var pkColumns = table.GetPrimaryKeyColumns();
            var memberDeclarationsWihtoutPk = string.Join(Environment.NewLine + "        ", allColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var memberName = colName.PascalCase();

                bool isPk = pkColumns.SingleOrDefault(c => c.Name.Parts[2] == colName) != null ? true : false;

                if (isPk || "IsDeleted" == memberName) return string.Empty;

                return $"{memberName}=dto.{memberName},";
            }));

            string output =
            $@"  
                public static class {entityClassName}Mapper
                {{ 
                    public static {entityClassName} ToEntity(this {entityClassName}RequestAddDto dto)
                    {{
                        return new {entityClassName}
                        {{
                            {memberDeclarationsWihtoutPk}
                        }};
                    }}

                    public static {entityClassName}ResponseGetByIdDto ToGetByIdDto(this {entityClassName} entity)
                    {{
                        return new {entityClassName}ResponseGetByIdDto
                        {{
                 
                        }};
                    }} 
                  
	            }}
            ";

            return output;
        }



    }
}
