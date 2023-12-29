using DapperSqlGenerator.Console.Extenions;
using DapperSqlGenerator.Console.Helpers;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Generator.Repositories
{
    public class DapperRepositoryClassGenerator : IGenerate
    {
        TSqlObject table;
        string repositoryNamespace;
        string dataModelNamespace;
        
        public DapperRepositoryClassGenerator(string dataModelNamespace, string repositoryNamespace, TSqlObject table)
        {
            this.table = table;
            this.repositoryNamespace = repositoryNamespace;
            this.dataModelNamespace = dataModelNamespace;
        }
        

        public  string Generate()
        {
            return  $"using Dapper;" + Environment.NewLine +
                    $"using {dataModelNamespace};"+Environment.NewLine+
                    $@"namespace {repositoryNamespace} {{

                        {GenerateInterface()}

                        {GenerateClass()}

                    }}";
        }

        

      

        /// <summary>
        /// Get the declaration for the repo class
        /// </summary>
        /// <returns></returns>
        private string GenerateClass()
        {
            var entityClassName = table.Name.Parts[1];
            var repoClassName = entityClassName + "Repository";
            var repoInterfaceName = "I" + repoClassName;
            var methodDeclarations = String.Join(Environment.NewLine + "        ", GenerateClassMethods());

            string output =
            $@"  
                public class {repoClassName} : {repoInterfaceName}
                {{

                    private readonly string connectionString;

                    public {repoClassName}(IOptions<ConnectionStrings> connectionsStringsOptions)
                    {{
                        connectionString = connectionsStringsOptions.Value.DefaultConnection;
                    }}
        
                    {methodDeclarations}

                }}
            ";

            return output;
        }

        /// <summary>
        /// Get the declaration for the repo interface
        /// </summary>
        /// <returns></returns>
        private string GenerateInterface()
        {
            var entityClassName = table.Name.Parts[1];
            var repoClassName = entityClassName + "Repository";
            var repoInterfaceName = "I" + repoClassName;

            var methodDeclarations = String.Join(Environment.NewLine + "        ",GenerateInterfaceMethods());

            string output =
            $@" public partial interface {repoInterfaceName}
                {{ 
                    {methodDeclarations}
                }}";

            return output;
        }

        /// <summary>
        /// Get all methods signatures for the interface based on the actual config
        /// yc SqlStored proc config and Entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateInterfaceMethods()

        {
            var entityClassName = table.Name.Parts[1];
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            //Get all
            yield return $"Task<IEnumerable<{entityClassName}>> GetAll();";

            //Get by Primary key
            yield return $"Task<{entityClassName}> GetBy{pkFieldsNames}({pkFieldsWithTypes});";
             
            //Insert
            yield return $"Task<int> Insert({entityClassName} {Common.FirstCharacterToLower(entityClassName)});";

            //Update
            yield return $"Task Update({entityClassName} {Common.FirstCharacterToLower(entityClassName)});";

            //Delete
            yield return $"Task Delete(int id);"; // TODO: only work with and int id as pk, hard coded need to be changed
             
        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateClassMethods()
        {
            var repositoryMethodsGenerator = new DapperRepositoryMethodsGenerator(table);

            yield return repositoryMethodsGenerator.GenerateInsertMethod();
            yield return repositoryMethodsGenerator.GenerateUpdateMethod();
            yield return repositoryMethodsGenerator.GenerateDeleteMethod();
            yield return repositoryMethodsGenerator.GenerateGetAllMethod();
            yield return repositoryMethodsGenerator.GenerateGetByPKMethod();
        }



    }
}
