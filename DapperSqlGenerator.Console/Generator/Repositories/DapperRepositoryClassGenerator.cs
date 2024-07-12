
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;
using DapperSqlGenerator.App.Models;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Generator.Repositories
{
    public class DapperRepositoryClassGenerator : IGenerate , IGenerateInterface, IGenerateClass
    {
        TSqlObject table;
        string repositoryNamespace;
        string dataModelNamespace;
        string projectName;
        public DapperRepositoryClassGenerator(string projectName,string dataModelNamespace, string repositoryNamespace, TSqlObject table)
        {
            this.table = table;
            this.repositoryNamespace = repositoryNamespace;
            this.dataModelNamespace = dataModelNamespace;
            this.projectName = projectName; 
        }
        

        public  string Generate()
        {
            return  $"using Dapper;" + Environment.NewLine +
                    $"using System.Linq.Expressions;" + Environment.NewLine +
                    $"using {dataModelNamespace};"+Environment.NewLine+
                    $"using {projectName}.Common.Pagination;" + Environment.NewLine +
                    $"using {projectName}.Common.Helpers;" + Environment.NewLine +
                    $"using Microsoft.Extensions.Configuration;" + Environment.NewLine +
                    $"using Microsoft.Data.SqlClient;" + Environment.NewLine + Environment.NewLine +
            $@"namespace {repositoryNamespace} 
            {{
                        {GenerateInterface()}
                        {GenerateClass()}
            }}";
        }

        public string  GenerateInterfacePart()
        {
            return $"using System.Linq.Expressions;" + Environment.NewLine +
                   $"using {dataModelNamespace};" + Environment.NewLine +
                   $"using {projectName}.Common.Pagination;" + Environment.NewLine + Environment.NewLine +
           $@"namespace {repositoryNamespace} 
            {{
                        {GenerateInterface()}
            }}";
        }

        public string GenerateClassPart()
        {
            return $"using Dapper;" + Environment.NewLine +
                   $"using System.Linq.Expressions;" + Environment.NewLine +
                   $"using {dataModelNamespace};" + Environment.NewLine +
                   $"using {projectName}.Common.Pagination;" + Environment.NewLine +
                   $"using {projectName}.Common.Helpers;" + Environment.NewLine +
                   $"using Microsoft.Extensions.Configuration;" + Environment.NewLine +
                   $"using Microsoft.Data.SqlClient;" + Environment.NewLine + Environment.NewLine +
           $@"namespace {repositoryNamespace} 
            {{
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

            string methodDeclarationGeneratedOutput = string.Empty;
            var methodDeclarationGenerated = GenerateClassMethods();
            foreach (var methodDeclaration in methodDeclarationGenerated)
            {
                if (methodDeclaration != String.Empty)
                    methodDeclarationGeneratedOutput += methodDeclaration + Environment.NewLine;
            }

            string output =
            $@"  
                public class {repoClassName} : {repoInterfaceName}
                {{

                    private readonly string? connectionString;

                    public {repoClassName}(IConfiguration configuration)
                    {{
                        connectionString = configuration.GetConnectionString(""DefaultConnection"");
                    }}
        
                    {methodDeclarationGeneratedOutput}

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

            string methodDeclarationGeneratedOutput = string.Empty;
            var methodDeclarationGenerated = GenerateInterfaceMethods();
            foreach (var methodDeclaration in methodDeclarationGenerated)
            {
                if (methodDeclaration != String.Empty)
                    methodDeclarationGeneratedOutput += methodDeclaration + Environment.NewLine;
            }

            string output =
            $@" public partial interface {repoInterfaceName}
                {{ 
                    {methodDeclarationGeneratedOutput}
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

            var pkColumns = table.GetPrimaryKeyColumns();
            //Exclude de PK identity field to put "Direction Output" in Dapper params
            bool isOneColumnIdentity = pkColumns.Count() == 1 && pkColumns.ToList()[0].IsColumnIdentity();
            string? returnType = isOneColumnIdentity
               ? MatchingDataTypeHelper.GetDotNetDataType(pkColumns.ToArray()[0].GetColumnSqlDataType())
               : "bool"; // return bool if insert ok  => we cannot return the new Id generated by Identity

            //Get all
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetAllAsync]) ? $"Task<IEnumerable<{entityClassName}>?> GetAllAsync();" : string.Empty;

            //Get Paginated Entities
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetPaginatedAsync]) ? $"Task<PagedResults<{entityClassName}>> GetPaginatedAsync(Expression<Func<{entityClassName}, bool>>? criteria=null, Expression<Func<{entityClassName}, object>>? orderByExpression=null, int page=1, int pageSize=10);" : string.Empty;

            //Get by Primary key
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetByPkFieldsNamesAsync]) ? $"Task<{entityClassName}?> GetBy{pkFieldsNames}Async({pkFieldsWithTypes});" : string.Empty;

            //Get by Expression
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetByExpressionAsync]) ? $"Task<IEnumerable<{entityClassName}>?> GetByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria);" : string.Empty;

            //Insert
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.InsertAsync]) ? $"Task<{returnType}> InsertAsync({entityClassName} {Common.FirstCharacterToLower(entityClassName)});" : string.Empty;

            //Update
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.UpdateAsync]) ? $"Task UpdateAsync({entityClassName} {Common.FirstCharacterToLower(entityClassName)});" : string.Empty;

            //Delete
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.DeleteByPkFieldsNamesAsync]) ? $"Task DeleteBy{pkFieldsNames}Async({pkFieldsWithTypes});" : string.Empty;

            //Delete By Expression
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.DeleteByExpressionAsync])? $"Task DeleteByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria);":string.Empty;
          
            //InsertAsyncTransaction
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.InsertAsyncTransaction]) ? $"Task<{returnType}> InsertAsyncTransaction({entityClassName} {Common.FirstCharacterToLower(entityClassName)}, SqlTransaction sqlTransaction);" : string.Empty;

            //UpdateAsyncTransaction
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.UpdateAsyncTransaction]) ? $"Task UpdateAsyncTransaction({entityClassName} {Common.FirstCharacterToLower(entityClassName)}, SqlTransaction sqlTransaction);" : string.Empty;

            //DeleteByPkFieldsNamesAsyncTransaction
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.DeleteByPkFieldsNamesAsyncTransaction]) ? $"Task DeleteBy{pkFieldsNames}AsyncTransaction({pkFieldsWithTypes}, SqlTransaction sqlTransaction);" : string.Empty;

        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateClassMethods()
        {
            var repositoryMethodsGenerator = new DapperRepositoryMethodsGenerator(table);

            yield return "#region Generated";
            yield return repositoryMethodsGenerator.GenerateGetAllMethod();
            yield return repositoryMethodsGenerator.GenerateGetByPKMethod();
            yield return repositoryMethodsGenerator.GenerateGetByExpressionMethod();
            yield return repositoryMethodsGenerator.GenerateGePaginatedMethod();
            yield return repositoryMethodsGenerator.GenerateInsertMethod();
            yield return repositoryMethodsGenerator.GenerateUpdateMethod();
            yield return repositoryMethodsGenerator.GenerateDeleteMethod();
            yield return repositoryMethodsGenerator.GenerateDeleteByExpressionMethod();

            yield return repositoryMethodsGenerator.GenerateInsertTransactionMethod();
            yield return repositoryMethodsGenerator.GenerateUpdateTransactionMethod();
            yield return repositoryMethodsGenerator.GenerateDeleteTransactionMethod(); 


            yield return "#endregion Generated";
        }

       
    }
}
