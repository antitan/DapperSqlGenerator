using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;
using DapperSqlGenerator.App.Models;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Generator.Services
{
    public class ServicesGenerator : IGenerate
    {
        TSqlObject table;
        string dataModelNamespace;
        string serviceNamespace;
        string dataRepositoryNamespace;
        string projectName;
        string[] refTables;

        public ServicesGenerator(string serviceNamespace, string dataModelNamespace, string dataRepositoryNamespace, string projectName, string[] refTables, TSqlObject table)
        {
            this.table = table;
            this.dataModelNamespace = dataModelNamespace;
            this.dataRepositoryNamespace = dataRepositoryNamespace;
            this.serviceNamespace = serviceNamespace;
            this.refTables = refTables; 
            this.projectName = projectName; 
        }

        public string Generate()
        {
            return  "using Microsoft.Extensions.Logging;" + Environment.NewLine +
                    $"using Dapper;" + Environment.NewLine +
                    $"using System.Text.Json;" + Environment.NewLine +
                    $"using System.Linq.Expressions;" + Environment.NewLine +
                    $"using {projectName}.Common.Helpers;" + Environment.NewLine +
                    $"using {projectName}.Common.Pagination;" + Environment.NewLine +
                    $"using {projectName}.Common.Constants;" + Environment.NewLine +
                    $"using {projectName}.Common.Cache;" + Environment.NewLine +
                    $"using {dataModelNamespace};" + Environment.NewLine +
                    $"using {dataRepositoryNamespace};" + Environment.NewLine +
                    $@"namespace {serviceNamespace} {{

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
            var repoClassName = entityClassName + "Service";
            var repoInterfaceName = "I" + repoClassName;
            var methodDeclarations = String.Join(Environment.NewLine + "        ", GenerateClassMethods());

            string output =
            $@"  
                public class {repoClassName} : {repoInterfaceName}
                {{
                    private readonly I{entityClassName}Repository {Common.FirstCharacterToLower(entityClassName)}Repository;
                    private readonly ICacheManager cacheManager;
                    private readonly ILogger<{repoClassName}> logger;

                    public {repoClassName}(I{entityClassName}Repository {Common.FirstCharacterToLower(entityClassName)}Repository, ICacheManager cacheManager,ILogger<{repoClassName}> logger )
                    {{
                         this.{Common.FirstCharacterToLower(entityClassName)}Repository = {Common.FirstCharacterToLower(entityClassName)}Repository;
                         this.cacheManager = cacheManager;
                         this.logger = logger;
                    }}
        
                    {methodDeclarations}

                }}
            ";

            return output;
        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GenerateClassMethods()
        {
            var entityClassName = table.Name.Parts[1];
            bool isRefTable = refTables.Contains(entityClassName);  

            yield return GenerateInsertDelegate(entityClassName);
            yield return GenerateUpdateDelegate(entityClassName);
            yield return GenerateDeleteDelegate(entityClassName);
            if (isRefTable) yield return GenerateGetAllRefDelegate(entityClassName);
            yield return GenerateGetAllPaginatedDelegate(entityClassName);
            yield return GenerateGetByPkDelegate(entityClassName);
            yield return GenerateGetByExpressionDelegate(entityClassName);
            yield return GenerateDeleteByExpressionDelegate(entityClassName);
        }

        private string GenerateDeleteByExpressionDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.DeleteByExpressionAsync]) return string.Empty;
            string output = $@"
                /// <summary>
                /// Delete {entityClassName} by Expression
                /// </summary>
                public async Task DeleteByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria)
                {{
                    try
                    {{
                        await {Common.FirstCharacterToLower(entityClassName)}Repository.DeleteByExpressionAsync(criteria);
                    }}
                    catch(Exception ex)
                    {{
                        logger.LogError($"" Problem to DeleteByExpressionAsync for {entityClassName} Criter=[criteria.ToString() - criteria.ToMSSqlString()]   error : {{ex}}"");
                    }}  
                }}" + Environment.NewLine;

            return output;
        }

        private string GenerateGetByExpressionDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetByExpressionAsync]) return string.Empty;
            string output = $@"
                /// <summary>
                /// Get {entityClassName} by Expression
                /// </summary>
                public async Task<IEnumerable<{entityClassName}>?> GetByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria)
                {{
                    IEnumerable<{entityClassName}>? result=null;
                    try
                    {{
                        result = await {Common.FirstCharacterToLower(entityClassName)}Repository.GetByExpressionAsync(criteria);
                    }}
                    catch(Exception ex)
                    {{
                        logger.LogError($"" Problem to GetByExpressionAsync for {entityClassName} Criter=[criteria.ToString() - criteria.ToMSSqlString()] error : {{ex}}"");
                    }}  
                    return result;
                }}" + Environment.NewLine;

            return output;
        }

        private string GenerateGetByPkDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetByPkFieldsNamesAsync]) return string.Empty;
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table); 
            var pkFieldsWithComma = Common.ConcatPkFieldsWithComma(table);

            string output = $@"
                /// <summary>
                /// Get {entityClassName} by PK
                /// </summary>
                public async Task<{entityClassName}?> GetBy{pkFieldsNames}Async({pkFieldsWithTypes})
                {{
                    {entityClassName}? result = null;
                    try
                    {{
                        result = await {Common.FirstCharacterToLower(entityClassName)}Repository.GetBy{pkFieldsNames}Async({pkFieldsWithComma});
                    }}
                    catch(Exception ex)
                    {{
                        logger.LogError($"" Problem to GetBy{pkFieldsNames} {entityClassName}  error : {{ex}}"");
                    }} 
                    return result;
                }}" + Environment.NewLine;

            return output;
        }
        
        private string GenerateGetAllPaginatedDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetPaginatedAsync]) return string.Empty;
            string output = $@"
                 /// <summary>
                 /// Get paginated {entityClassName}
                 /// </summary>
                 public async Task<PagedResults<{entityClassName}>> GetPaginatedAsync(Expression<Func<{entityClassName}, bool>>? criteria=null, Expression<Func<{entityClassName}, object>>? orderByExpression=null, int page=1, int pageSize=10)
                 {{ 
                      PagedResults<{entityClassName}> result = null;
                      try
                      {{
                         result = await {Common.FirstCharacterToLower(entityClassName)}Repository.GetPaginatedAsync(criteria,orderByExpression,page,pageSize);
                      }}
                    catch (Exception ex)
                    {{
                        logger.LogError($"" Problem to GetAllPaginatedAsync {entityClassName}  error : {{ex}}"");
                    }}
                    return result;
                 }}";

            return output;
        }

        private string GenerateGetAllRefDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetAllAsync]) return string.Empty;
            string output = $@"
                 /// <summary>
                 /// Get all {entityClassName}
                 /// </summary>
                 public async Task<IEnumerable<{entityClassName}>?> GetAllAsync()
                 {{ 
                      IEnumerable<{entityClassName}>? result = null;
                      try
                      {{
                         if (!cacheManager.IsSet(CacheDataConstants.{entityClassName}AllCacheKey))
                         {{
                             result = await {Common.FirstCharacterToLower(entityClassName)}Repository.GetAllAsync();
                             cacheManager.Add(CacheDataConstants.{entityClassName}AllCacheKey, result);
                         }}
                         else 
                            return cacheManager.Get<IEnumerable<{entityClassName}>>(CacheDataConstants.{entityClassName}AllCacheKey);
                     }}
                    catch (Exception ex)
                    {{
                        logger.LogError($"" Problem to get all {entityClassName}  error : {{ex}}"");
                    }}
                    return result;
                 }}";

            return output;
        }

        private string GenerateInsertDelegate(string entityClassName) 
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.InsertAsync]) return string.Empty;
            var pkColumns = table.GetPrimaryKeyColumns();
             //Exclude de PK identity field to put "Direction Output" in Dapper params
            bool isOneColumnIdentity = pkColumns.Count() == 1 && pkColumns.ToList()[0].IsColumnIdentity();
             string returnType = isOneColumnIdentity
                ? MatchingDataTypeHelper.GetDotNetDataType(pkColumns.ToArray()[0].GetColumnSqlDataType())
                : "bool"; // return bool if insert ok  => we cannot return the new Id generated by Identity

            var paramName = Common.FirstCharacterToLower(entityClassName);

            return $@"
            /// <summary>
            /// Insert {entityClassName}
            /// </summary>
            public async  Task<{returnType}> InsertAsync({entityClassName} {paramName})
            {{
                {returnType} result = ({returnType})ReflexionHelper.GetDefaultValue(typeof({returnType}));
                try
                {{
                    result = await {Common.FirstCharacterToLower(entityClassName)}Repository.InsertAsync({paramName}); 
                }}
                catch(Exception ex)
                {{
                    logger.LogError($"" Problem to insert {entityClassName} {{JsonSerializer.Serialize({paramName}, JsonHelper.ConfigureDefaultSerialization())}}   error : {{ex}}"");
                }}
                return result;
            }}";
        }

        private string GenerateUpdateDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.UpdateAsync]) return string.Empty;
            var paramName = Common.FirstCharacterToLower(entityClassName);
            string output = $@"
            /// <summary>
            /// Update {entityClassName}
            /// </summary>
            public async Task UpdateAsync({entityClassName} {paramName})
            {{
                try
                {{
                   await {Common.FirstCharacterToLower(entityClassName)}Repository.UpdateAsync({paramName});  
                }}
                catch(Exception ex)
                {{
                    logger.LogError($"" Problem to update {entityClassName} {{ JsonSerializer.Serialize({paramName}, JsonHelper.ConfigureDefaultSerialization()) }}  error : {{ex}}"");
                }}
            }}" + Environment.NewLine;
            return output;
        }

        private string GenerateDeleteDelegate(string entityClassName)
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.DeleteByPkFieldsNamesAsync]) return string.Empty;
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table); 
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithComma = Common.ConcatPkFieldsWithComma(table);

            string output = $@"
                        /// <summary>
                        /// Delete {entityClassName}
                        /// </summary>
                        public async Task DeleteBy{pkFieldsNames}Async({pkFieldsWithTypes})
                        {{
                            try
                            {{
                                 await {Common.FirstCharacterToLower(entityClassName)}Repository.DeleteBy{pkFieldsNames}Async({pkFieldsWithComma});
                            }}
                            catch(Exception ex)
                            {{
                                logger.LogError($"" Problem to delete {entityClassName} {pkFieldsWithTypes}  error : {{ex}}"");
                            }}
                        }}" + Environment.NewLine;
            return output;
        }


        /// <summary>
        /// Get the declaration for the repo interface
        /// </summary>
        /// <returns></returns>
        private string GenerateInterface()
        {
            var entityClassName = table.Name.Parts[1];
            var repoClassName = entityClassName + "Service";
            var repoInterfaceName = "I" + repoClassName;

            var methodDeclarations = String.Join(Environment.NewLine + "        ", GenerateInterfaceMethods());

            string output =
            $@" public interface {repoInterfaceName}
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

            bool isRefTable = refTables.Contains(entityClassName);
            //Get all
            if(isRefTable)
                yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetAllAsync]) ? $"Task<IEnumerable<{entityClassName}>?> GetAllAsync();":string.Empty;

            //Get Paginated Entities
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetPaginatedAsync]) ? $"Task<PagedResults<{entityClassName}>> GetPaginatedAsync(Expression<Func<{entityClassName}, bool>>? criteria=null, Expression<Func<{entityClassName}, object>>? orderByExpression=null, int page=1, int pageSize=10);" : string.Empty;

            //Get by Primary key
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetByPkFieldsNamesAsync]) ? $"Task<{entityClassName}?> GetBy{pkFieldsNames}Async({pkFieldsWithTypes});" : string.Empty;

            //Get by Expression
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.GetByExpressionAsync]) ? $"Task<IEnumerable<{entityClassName}>?> GetByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria);" : string.Empty;

            //Insert
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.InsertAsync]) ? $"Task<{returnType}> InsertAsync({entityClassName} {Common.FirstCharacterToLower(entityClassName)});" : string.Empty;

            //Update
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.UpdateAsync]) ? $"Task UpdateAsync({entityClassName} {Common.FirstCharacterToLower(entityClassName)});":string.Empty;

            //Delete
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.DeleteByPkFieldsNamesAsync]) ? $"Task DeleteBy{pkFieldsNames}Async({pkFieldsWithTypes});":string.Empty;

            //DeleteAsync
            yield return (MethodsToGenerate.Check[MethodNameToGenerate.DeleteByExpressionAsync])? $"Task DeleteByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria);":string.Empty;
        }
    }
}
