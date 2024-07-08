using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator; 
using Microsoft.SqlServer.Dac.Model; 

namespace DapperSqlGenerator.App.Generator.Controllers
{
    public class ResponseGenerator : IGenerate, IGenerateClass
    {
        TSqlObject table;
        string responseNamespace;
        public ResponseGenerator( string responseNamespace, TSqlObject table)
        {
            this.table = table;  
            this.responseNamespace = responseNamespace;  
        }

        public string Generate()
        {
            return 
                    $@"namespace {responseNamespace} {{ 
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

            
            string output =
            $@"  
                public class {entityClassName}ResponseGetByIdDto
                {{ 
                 
	            }}
            ";

            return output;
        }



    }
}
