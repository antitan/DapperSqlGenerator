using DapperSqlGenerator.Console.Extenions;
using DapperSqlGenerator.Console.Generator.Repositories;
using DapperSqlGenerator.Console.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Generator.Services
{
    public class ServicesGenerator : IGenerate
    {
        TSqlObject table;
        string dataModelNamespace;
        string serviceNamespace;
        string projectName;
        string[] refTables;

        public ServicesGenerator(string serviceNamespace, string dataModelNamespace, string projectName, string[] refTables, TSqlObject table)
        {
            this.table = table;
            this.dataModelNamespace = dataModelNamespace;
            this.serviceNamespace = serviceNamespace;
            this.refTables = refTables; 
            this.projectName = projectName; 
        }

        public string Generate()
        {
            return  "using Microsoft.Extensions.Logging;" + Environment.NewLine +
                    $"using Dapper;" + Environment.NewLine +
                    $"using {projectName}.Common.Helpers;" + Environment.NewLine +
                    $"using {projectName}.Common.Constants;" + Environment.NewLine +
                    $"using {dataModelNamespace};" + Environment.NewLine +
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
                    private readonly I{entityClassName}Repository {entityClassName.ToLower()}Repository;
                    private readonly ICacheManager cacheManager;
                    private readonly ILogger<{entityClassName}> logger;

                    public {repoClassName}(I{repoClassName}Repository {entityClassName.ToLower()}Repository, ICacheManager cacheManager,ILogger<{entityClassName}> logger )
                    {{
                         this.{entityClassName.ToLower()}Repository = {entityClassName.ToLower()}Repository;
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
            yield return (isRefTable)? GenerateGetAllDelegateRef(entityClassName) : GenerateGetAllDelegate(entityClassName);
            yield return (isRefTable) ? GenerateGetAllGetByPkDelegateRef(entityClassName) : GenerateGetAllGetByPkDelegate(entityClassName);
        }

        private string GenerateGetAllGetByPkDelegate(string entityClassName)
        {  
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table); 

            string output = $@"
                /// <summary>
                /// Get {entityClassName} by PK
                /// </summary>
                public async Task<{entityClassName}> GetBy{pkFieldsNames}({pkFieldsWithTypes})
                {{
                    using (var connection = new SqlConnection(connectionString))
                    {{
                        await return {entityClassName.ToLower()}.GetBy{pkFieldsNames}({pkFieldsWithTypes});
                    }}
                }}" + Environment.NewLine;

            return output;
        }
        private string GenerateGetAllGetByPkDelegateRef(string entityClassName)
        { 
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);

            string output = $@"
                /// <summary>
                /// Get {entityClassName} by PK
                /// </summary>
                public async Task<{entityClassName}> GetBy{pkFieldsNames}({pkFieldsWithTypes})
                {{
                    using (var connection = new SqlConnection(connectionString))
                    {{
                        await return {entityClassName.ToLower()}.GetBy{pkFieldsNames}({pkFieldsNames});
                    }}
                }}" + Environment.NewLine;

            return output;
        }

        private string GenerateGetAllDelegate(string entityClassName)
        {
            string output = $@"
                 /// <summary>
                 /// Get all {entityClassName}
                 /// </summary>
                 public async Task<IEnumerable<{entityClassName}>> GetAll()
                 {{
                     return await {entityClassName.ToLower()}GetAll();
                 }}";

            return output;
        }
        private string GenerateGetAllDelegateRef(string entityClassName)
        {
            string output = $@"
                 /// <summary>
                 /// Get all {entityClassName}
                 /// </summary>
                 public async Task<IEnumerable<{entityClassName}>> GetAll()
                 {{
                     return await {entityClassName.ToLower()}.GetAll();
                 }}";

            return output;
        }

        private string GenerateInsertDelegate(string entityClassName) 
        {
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
            public async  Task<{returnType}> Insert({entityClassName} {paramName})
            {{
                try
                {{
                    return await {entityClassName.ToLower()}.Insert({paramName}); 
                }}
                catch(Exception ex)
                {{
                    logger.LogError($"" Problem to insert {entityClassName} JsonSerializer.Serialize({paramName}, JsonHelper.ConfigureDefaultSerialization())   error : {{ex}}"");
                }}
            }}";
        }

        private string GenerateUpdateDelegate(string entityClassName)
        {
            var paramName = Common.FirstCharacterToLower(entityClassName);
            string output = $@"
            /// <summary>
            /// Update {entityClassName}
            /// </summary>
            public async Task Update({entityClassName} {paramName})
            {{
                try
                {{
                  return await {entityClassName.ToLower()}Update({paramName});  
                }}
                catch(Exception ex)
                {{
                    logger.LogError($"" Problem to update {entityClassName} JsonSerializer.Serialize({paramName}, JsonHelper.ConfigureDefaultSerialization())   error : {{ex}}"");
                }}
            }}" + Environment.NewLine;
            return output;
        }

        private string GenerateDeleteDelegate(string entityClassName)
        {
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            var pkFieldName = Common.ConcatPkFieldNames(table);

            string output = $@"
                        /// <summary>
                        /// Delete {entityClassName}
                        /// </summary>
                        public async Task Delete({pkFieldsWithTypes})
                        {{
                            try
                            {{
                                return await {entityClassName.ToLower()}Repository.Delete({pkFieldName});
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
            var repoClassName = entityClassName + "Repository";
            var repoInterfaceName = "I" + repoClassName;

            var methodDeclarations = String.Join(Environment.NewLine + "        ", GenerateInterfaceMethods());

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
    }
}
