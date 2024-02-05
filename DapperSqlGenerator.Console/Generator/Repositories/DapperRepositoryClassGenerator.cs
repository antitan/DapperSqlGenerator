﻿
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Generator.Repositories
{
    public class DapperRepositoryClassGenerator : IGenerate
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
                    $"using {projectName}.Common.Configuration;" + Environment.NewLine +
                    $"using {projectName}.Common.Pagination;" + Environment.NewLine +
                    $"using {projectName}.Common.Helpers;" + Environment.NewLine +
                    $"using Microsoft.Extensions.Options;" + Environment.NewLine +
                    $"using Microsoft.Data.SqlClient;" + Environment.NewLine +
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

            var pkColumns = table.GetPrimaryKeyColumns();
            //Exclude de PK identity field to put "Direction Output" in Dapper params
            bool isOneColumnIdentity = pkColumns.Count() == 1 && pkColumns.ToList()[0].IsColumnIdentity();
            string returnType = isOneColumnIdentity
               ? MatchingDataTypeHelper.GetDotNetDataType(pkColumns.ToArray()[0].GetColumnSqlDataType())
               : "bool"; // return bool if insert ok  => we cannot return the new Id generated by Identity

            //Get all
            yield return $"Task<IEnumerable<{entityClassName}>> GetAllAsync();";

            //Get Paginated Entities
            yield return $"Task<PagedResults<{entityClassName}>> GetPaginatedAsync(Expression<Func<{entityClassName}, bool>> whereExpression, Expression<Func<{entityClassName}, object>> orderByExpression, int page=1, int pageSize=10);";

            //Get by Primary key
            yield return $"Task<{entityClassName}> GetBy{pkFieldsNames}Async({pkFieldsWithTypes});";

            //Get by Expression
            yield return $"Task<IEnumerable<{entityClassName}>> GetByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria);";

            //Insert
            yield return $"Task<{returnType}> InsertAsync({entityClassName} {Common.FirstCharacterToLower(entityClassName)});";

            //Update
            yield return $"Task UpdateAsync({entityClassName} {Common.FirstCharacterToLower(entityClassName)});";

            //Delete
            yield return $"Task DeleteBy{pkFieldsNames}Async({pkFieldsWithTypes});";

            //Delete By Expression
            yield return $"Task DeleteByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria);";

        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateClassMethods()
        {
            var repositoryMethodsGenerator = new DapperRepositoryMethodsGenerator(table);

            yield return "#region Generated";
            yield return repositoryMethodsGenerator.GenerateInsertMethod();
            yield return repositoryMethodsGenerator.GenerateUpdateMethod();
            yield return repositoryMethodsGenerator.GenerateDeleteMethod();
            yield return repositoryMethodsGenerator.GenerateDeleteByExpressionMethod();
            yield return repositoryMethodsGenerator.GenerateGetAllMethod();
            yield return repositoryMethodsGenerator.GenerateGetByPKMethod();
            yield return repositoryMethodsGenerator.GenerateGetByExpressionMethod();
            yield return repositoryMethodsGenerator.GenerateGePaginatedMethod();
            yield return "#endregion Generated";
        }



    }
}
