
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;

namespace DapperSqlGenerator.App.Generator.Repositories
{
    public class StoredProcedureFileGenerator : IGenerate
    {
        private readonly string repositoryNamespace;
        private readonly string storedProcedureName;
        private readonly string projectName;
        private readonly Dictionary<string, string> paramNamesTypes;
        public StoredProcedureFileGenerator(string projectName, string repositoryNamespace, string storedProcedureName, Dictionary<string,string> paramNamesTypes )
        {
            this.repositoryNamespace = repositoryNamespace; 
            this.paramNamesTypes = paramNamesTypes;
            this.storedProcedureName = storedProcedureName;
            this.projectName = projectName;
        }
        public string Generate()
        {
            return $"using Dapper;" + Environment.NewLine +
                    $"using {projectName}.Common.Configuration;" + Environment.NewLine + 
                    $"using Microsoft.Extensions.Options;" + Environment.NewLine +
                    $"using Microsoft.Data.SqlClient;" + Environment.NewLine +
                    $"using System.Data;" + Environment.NewLine +
            $@"namespace {repositoryNamespace} {{

                        {GenerateClass()}

                    }}";
        }

        /// <summary>
        /// Get all the methods for the class repo based on the actual TSql config and entity config
        /// </summary>
        /// <returns></returns>
        private string GenerateStoredProcedureMethod()
        {
            string dapperOperator = "ExecuteAsync";
            if(storedProcedureName.Contains("search", StringComparison.OrdinalIgnoreCase) ||
               storedProcedureName.Contains("select", StringComparison.OrdinalIgnoreCase) ||
               storedProcedureName.Contains("get", StringComparison.OrdinalIgnoreCase)) 
            {
                dapperOperator = "QuerySingleAsync<string>";
            }

            string spNormalParams = String.Join(Environment.NewLine + "            ",
            paramNamesTypes.Keys.Select(col =>
            {
                var varname = Common.FirstCharacterToLower(col.PascalCase());
                return $@"p.Add(""@{col}"",{varname});";
            }));

            string paramList = string.Empty;    
            foreach ( var kvp in paramNamesTypes)
            {
                var varname = Common.FirstCharacterToLower(kvp.Key.PascalCase());
                paramList += MatchingDataTypeHelper.GetDotNetDataType(kvp.Value) + " " + varname + " ,";
            }
            paramList = paramList.TrimEnd(',');    


            string output = $@"
              /// <summary>
              /// {this.storedProcedureName}
              /// </summary>
              public async  Task {this.storedProcedureName}Async({paramList})
              {{
                  using (var connection = new SqlConnection(connectionString))
                  {{
                      var p = new DynamicParameters();
                      {spNormalParams}

                      var result = await connection.{dapperOperator}(""[{this.storedProcedureName}]"", p, commandType: CommandType.StoredProcedure);
                  }}
           
              }}" + Environment.NewLine;

            return output;

        }

        /// <summary>
        /// Get the declaration for the repo class
        /// </summary>
        /// <returns></returns>
        private string GenerateClass()
        {
            var entityClassName = storedProcedureName;
            var repoClassName = entityClassName + "StoredProcedure"; 
            var methodDeclarations = String.Join(Environment.NewLine + "        ", GenerateStoredProcedureMethod());

            string output =
            $@"  
                public class {repoClassName} 
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
    }
}
