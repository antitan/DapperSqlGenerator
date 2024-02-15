
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;
using DapperSqlGenerator.App.Models;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Reflection;
using System.Reflection.Metadata;

namespace DapperSqlGenerator.App.Generator.Repositories
{
    public class DapperRepositoryMethodsGenerator : IRepositoryMethodsGenerator
    { 

        TSqlObject table;
        public DapperRepositoryMethodsGenerator(TSqlObject table)
        {
          this.table = table;
        }

        #region DeleteByExpression

        private string BuildDeleteByExpressionQuery()
        {
            return BuildDeleteQuery(" {criteria.ToMSSqlString()} ");
        }

        public string GenerateDeleteByExpressionMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.DeleteByExpressionAsync]) return string.Empty;
            string query = BuildDeleteByExpressionQuery();
            var entityClassName = table.Name.Parts[1];
            string output = $@"
                        /// <summary>
                        /// Delete {entityClassName}
                        /// </summary>
                        public async Task DeleteByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria)
                        {{
                            using (var connection = new SqlConnection(connectionString))
                            {{  
                                await connection.ExecuteAsync($""{query}"");
                            }}
                        }}" + Environment.NewLine;

            return output;
        }

        #endregion DeleteByExpression

        #region Delete

        private string BuildDeleteQuery(string where)
        {
            string query = $"DELETE FROM {table.Name} WHERE {where}";
            return query;
        }

        private string BuildDeleteQuery()
        {
            string query = string.Empty;
            var pkColumns = table.GetPrimaryKeyColumns();
            var inputParamDeclarations = String.Join(Environment.NewLine + ", ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                var colDataType = col.GetColumnSqlDataType();
                return $"@{colName} {colDataType}";
            }));

            var whereClause_conditions = String.Join(" AND ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}] = @{colName}";
            })); 
            return BuildDeleteQuery(whereClause_conditions);
        }

        public string GenerateDeleteMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.DeleteByPkFieldsNamesAsync]) return string.Empty;
            string query = BuildDeleteQuery();
            var entityClassName = table.Name.Parts[1];
            var pkColumns = table.GetPrimaryKeyColumns();
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            string spParams = String.Join(Environment.NewLine + "            ",
                    pkColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var colVariableName = Common.FirstCharacterToLower(colName.PascalCase());
                        return $@"p.Add(""@{colName}"",{colVariableName});";
                    }));

            string output = $@"
                        /// <summary>
                        /// Delete {entityClassName}
                        /// </summary>
                        public async Task DeleteBy{pkFieldsNames}Async({pkFieldsWithTypes})
                        {{
                            using (var connection = new SqlConnection(connectionString))
                            {{
                                var p = new DynamicParameters();
                                {spParams}

                                await connection.ExecuteScalarAsync<int>(""{query}"", p);
                            }}

                        }}"+Environment.NewLine;

            return output;
        }

        #endregion Delete


       

        private string BuildSelectFromTableWhereAndOrderBy(string selection,string? whereClause = null, string? orderClause = null)
        { 
            string  query = $"SELECT {selection} FROM {table.Name} ";
            if (whereClause != null)
            {
                query += $" WHERE {whereClause} ";
            }
            if (orderClause != null)
            {
                query += $" ORDER BY {orderClause} ";
            }
            return query;
        }

       
        private string BuildSelectTableFileds(string? whereClause = null, string? orderClause = null)
        {
            string query = string.Empty;
            var allColumns = table.GetAllColumns();
            var select_columns = String.Join(", ",
               allColumns.Select(col =>
               {
                   var colName = col.Name.Parts[2];
                   return $"[{colName}]";
               }));

            return BuildSelectFromTableWhereAndOrderBy($" {select_columns} ", whereClause, orderClause);
        }


        private string BuildWhere(string expressionVariableToTest, string queryToConcat)
        {
            string where = $@"if( criteria != null )  {Environment.NewLine}";
            where += "{ "+ Environment.NewLine;
            where += $@"{queryToConcat} += $"" WHERE {{ {expressionVariableToTest}.ToMSSqlString() }} ""; ";
            where += "} " + Environment.NewLine;

            return where;
        }

        private string BuildOrderBy(string expressionVariableToTest, string queryToConcat)
        {
            string orderby = $" var orderComputedExpression = (orderByExpression == null)?\"1\":{expressionVariableToTest}.ToMSSqlString();" + Environment.NewLine;
            orderby += $@"{queryToConcat} += $"" ORDER BY {{orderComputedExpression}}  " +"\";"+ Environment.NewLine;
            return orderby;
        }

        private string BuildOffset( string queryToConcat)
        { 
            //pour le offset le order by est obligatoire !
            return $@"{queryToConcat} += $"" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;"" ;"+Environment.NewLine;
        }

        private string BuildSelectCountFromTable()
        { 
            return BuildSelectColumnsFromTable(" count(*) ");
        }

        private string BuildSelectColumnsFromTable()
        {

            string query = string.Empty;
            var allColumns = table.GetAllColumns();
            var select_columns = String.Join(", ",
               allColumns.Select(col =>
               {
                   var colName = col.Name.Parts[2];
                   return $"[{colName}]";
               }));

            return BuildSelectColumnsFromTable(select_columns);
        }

        private string BuildSelectColumnsFromTable(string selection)
        {
             return $"SELECT {selection} FROM {table.Name} \";  {Environment.NewLine}";
        }


        #region GetByExpression Method

        private string BuildGetByExpressionQuery()
        {
            var query = BuildSelectColumnsFromTable();
            query += BuildWhere("criteria","querySelect");
            return query;
        }

        public string GenerateGetByExpressionMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetByExpressionAsync]) return string.Empty;
            string querySelect = BuildGetByExpressionQuery();
            var entityClassName = table.Name.Parts[1];
            string output = $@"
                /// <summary>
                /// Get {entityClassName} by expression 
                /// </summary>
                public async Task<IEnumerable<{entityClassName}>?> GetByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria)
                {{
                    using (var connection = new SqlConnection(connectionString))
                    {{ 
                        var querySelect = $""{querySelect}
                        var entities = await connection.QueryAsync<{entityClassName}>(querySelect);
                        return entities;
                    }}
                }}";

            return output;
        }

        #endregion GetByExpression Method

        #region GetPaginated Method 


        
        private string BuildCountPaginatedQuery()
        {
            var query = BuildSelectCountFromTable();
            query += BuildWhere("criteria", "queryCount");
            return query;
        }



        private string BuildGetPaginatedQuery()
        {
            var query = BuildSelectColumnsFromTable();
            query += BuildWhere("criteria","querySelect");
            query += BuildOrderBy("orderByExpression", "querySelect");
            query += BuildOffset("querySelect");
           return query;
        }

        public string GenerateGePaginatedMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetPaginatedAsync]) return string.Empty;
            //https://www.davepaquette.com/archive/2019/01/28/paging-large-result-sets-with-dapper-and-sql-server.aspx
            string selectQuery = BuildGetPaginatedQuery();
            string selectCountQuery = BuildCountPaginatedQuery(); 
            var entityClassName = table.Name.Parts[1];
            string output = $@"
                /// <summary>
                /// Get paginated {entityClassName}
                /// </summary>
                public async Task<PagedResults<{entityClassName}>> GetPaginatedAsync( Expression<Func<{entityClassName}, bool>>? criteria, Expression<Func<{entityClassName}, object>>? orderByExpression=null, int page=1, int pageSize=10)
                {{
                    var results = new PagedResults<{entityClassName}>();
                    
                    using (var connection = new SqlConnection(connectionString))
                    {{
                        await connection.OpenAsync();
                        var querySelect = $""{selectQuery}
                        var queryCount = $""{selectCountQuery}
                        var query = $""{{querySelect}} {{queryCount}} "";
                        using (var multi = await connection.QueryMultipleAsync(query,
                            new {{ Offset = (page - 1) * pageSize,
                                  PageSize = pageSize }}))
                        {{
                             results.Items = multi.Read<{entityClassName}>();
                             results.TotalCount = multi.ReadFirst<int>();
                             results.PageIndex = page;
                             results.PageSize = pageSize; 
                        }} 
                    }}
                    return results;
                }}";

            return output;
        }

        #endregion GetAllPaginated Method 

        #region GetAll Method

        private string BuildGetAllQuery()
        {
            return BuildSelectTableFileds();
        }

        public string GenerateGetAllMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetAllAsync]) return string.Empty;
            string query = BuildGetAllQuery();
            var entityClassName = table.Name.Parts[1]; 
            string output = $@"
                /// <summary>
                /// Get all {entityClassName}
                /// </summary>
                public async Task<IEnumerable<{entityClassName}>?> GetAllAsync()
                {{
                    using (var connection = new SqlConnection(connectionString))
                    {{
                        var entities = await connection.QueryAsync<{entityClassName}>(""{query}"");
                        return entities;
                    }}
                }}";

            return output; 
        }

        #endregion GetAll Method

        #region Get by PK

        private string BuildGetByPKQuery()
        {
            string query = string.Empty;    
            var pkColumns = table.GetPrimaryKeyColumns();

            var inputParamDeclarations = String.Join(Environment.NewLine + ", ",
                pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType}";
                })
            );

            var whereClause_conditions = String.Join(" AND ",
                pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"[{colName}] = @{colName}";
                })
            );

            query = BuildSelectTableFileds(whereClause_conditions);
            return query;
        }

        public string GenerateGetByPKMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.GetByPkFieldsNamesAsync]) return string.Empty;
            string query = BuildGetByPKQuery();
            var pkColumns = table.GetPrimaryKeyColumns();
            var entityClassName = table.Name.Parts[1];
            var pkFieldsNames = Common.ConcatPkFieldNames(table);
            var pkFieldsWithTypes = Common.ConcatPkFieldsWithTypes(table);
            string spParams = String.Join(Environment.NewLine + "            ",
                  pkColumns.Select(col =>
                  {
                      var colName = col.Name.Parts[2];
                      var colVariableName = Common.FirstCharacterToLower(colName.PascalCase());
                      return $@"p.Add(""@{colName}"",{colVariableName});";
                  }));

            string output = $@"
                /// <summary>
                /// Get {entityClassName} by PK
                /// </summary>
                public async Task<{entityClassName}?> GetBy{pkFieldsNames}Async({pkFieldsWithTypes})
                {{
                    using (var connection = new SqlConnection(connectionString))
                    {{
                        var p = new DynamicParameters();
                        {spParams}

                        var entity = await connection.QuerySingleOrDefaultAsync<{entityClassName}>(""{query}"", p);
                        return entity;
            
                    }}
                }}"+Environment.NewLine;

            return output;
        }

        #endregion Get by PK

        #region Insert Method
        private string BuildInsertQuery(bool isOnePkColumnIdentity)
        { 
            var allColumns = table.GetAllColumns();
            var nonIdentityColumns = allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));
            var identityColumns = allColumns.Where(col => col.GetProperty<bool>(Column.IsIdentity));
             
            var insertClause_columns = String.Join(", ",
                nonIdentityColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"[{colName}]";
                }));

            var insertClause_values = String.Join(", ",
                nonIdentityColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"@{colName}";
                })
            ); 

            return (isOnePkColumnIdentity) ?
                    $"INSERT INTO {table.Name} ({insertClause_columns})  OUTPUT INSERTED.Id VALUES ({insertClause_values})" :
                    $"INSERT INTO {table.Name} ({insertClause_columns})  VALUES ({insertClause_values})";

        }

        private string BuildInsertDapperOperator(bool isOnePkColumnIdentity,string returnType)
        {
            return (isOnePkColumnIdentity) ? $"QuerySingleAsync<{returnType}>" : "ExecuteAsync";
        }


        public string GenerateInsertMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.InsertAsync]) return string.Empty;
            var pkColumns = table.GetPrimaryKeyColumns();
            var allColumns = table.GetAllColumns();
            //Exclude de PK identity field to put "Direction Output" in Dapper params
            bool isOnePkColumnIdentity = pkColumns.Count() == 1 && pkColumns.ToList()[0].IsColumnIdentity();
            var normalColumns = isOnePkColumnIdentity ? allColumns.Except(pkColumns) : allColumns;

            string query = BuildInsertQuery(isOnePkColumnIdentity);
          
            var entityClassName = table.Name.Parts[1];
            var paramName = Common.FirstCharacterToLower(entityClassName);

            string returnType = isOnePkColumnIdentity
                ? MatchingDataTypeHelper.GetDotNetDataType(pkColumns.ToArray()[0].GetColumnSqlDataType())
                : "bool"; // return bool if insert ok  => we cannot return the new Id generated by Identity

            string spNormalParams = String.Join(Environment.NewLine + "            ",
                   normalColumns.Select(col =>
                   {
                       var colName = col.Name.Parts[2];
                       var entityProp = colName.PascalCase();
                       return $@"p.Add(""@{colName}"", {paramName}.{entityProp});";
                   }));
            string dapperOperator = BuildInsertDapperOperator( isOnePkColumnIdentity,  returnType);
            string retunValue = isOnePkColumnIdentity ? "result" : "true";

            string output = $@"
            /// <summary>
            /// Insert {entityClassName}
            /// </summary>
            public async  Task<{returnType}> InsertAsync({entityClassName} {paramName})
            {{
                using (var connection = new SqlConnection(connectionString))
                {{
                    var p = new DynamicParameters();
                    {spNormalParams}

                    var result = await connection.{dapperOperator}(""{query}"", p);

                    return {retunValue};
                }}
                 
            }}" + Environment.NewLine;

            return output;
        }
        #endregion Insert Method

        #region  Update Method

        private string BuildUpdateQuery()
        { 
            string query = string.Empty;   
            var allColumns = table.GetAllColumns();
            var pkColumns = table.GetPrimaryKeyColumns();
            var nonIdentityColumns = allColumns.Where(col => !col.GetProperty<bool>(Column.IsIdentity));

            var inputParamDeclarations = String.Join(", ",
                allColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType}";
                })
            );

            var updateClause_setStatements = String.Join(", ", nonIdentityColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}] = @{colName}";
            }));

            var whereClause_conditions = String.Join(" AND ", pkColumns.Select(col =>
            {
                var colName = col.Name.Parts[2];
                return $"[{colName}] = @{colName}";
            }));

            query = $"UPDATE {table.Name}  SET {updateClause_setStatements} WHERE {whereClause_conditions}";
            return query;

        }
        public string GenerateUpdateMethod()
        {
            if (!MethodsToGenerate.Check[MethodNameToGenerate.UpdateAsync]) return string.Empty;
            string query = BuildUpdateQuery();
            var entityClassName = table.Name.Parts[1];
            var allColumns = table.GetAllColumns();
            var paramName = Common.FirstCharacterToLower(entityClassName);
             

            string spParams = String.Join(Environment.NewLine + "            ",
                    allColumns.Select(col =>
                    {
                        var colName = col.Name.Parts[2];
                        var entityProp = colName.PascalCase();
                        return $@"p.Add(""@{colName}"", {paramName}.{entityProp});";
                    }));

            string output = $@"
        /// <summary>
        /// Update {entityClassName}
        /// </summary>
        public async Task UpdateAsync({entityClassName} {paramName})
        {{
            using (var connection = new SqlConnection(connectionString))
            {{
                 var p = new DynamicParameters();
                 {spParams}

                await connection.ExecuteScalarAsync<int>(""{query}"", p);
            }}

        }}"+Environment.NewLine;

            return output;
        }
        #endregion Update Method

    }
}
