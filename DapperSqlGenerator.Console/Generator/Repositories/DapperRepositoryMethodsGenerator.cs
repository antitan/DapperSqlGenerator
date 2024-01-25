
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model; 

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

        private string BuildSelectTableFileds(string whereClause = null)
        {
            string query = string.Empty;
            var allColumns = table.GetAllColumns();
            var select_columns = String.Join(", ",
               allColumns.Select(col =>
               {
                   var colName = col.Name.Parts[2];
                   return $"[{colName}]";
               }));

            query = $"SELECT {select_columns} FROM {table.Name} ";
            if(whereClause != null)
            {
                query += " WHERE " + whereClause;
            }
            return query;
        }


        #region GetByExpression Method

        private string BuildGetByExpressioQuery()
        {
            return BuildSelectTableFileds(" {criteria.ToMSSqlString()} ");
        }

        public string GenerateGetByExpressionMethod()
        {
            string query = BuildGetByExpressioQuery();
            var entityClassName = table.Name.Parts[1];
            string output = $@"
                /// <summary>
                /// Get {entityClassName} by expression 
                /// </summary>
                public async Task<IEnumerable<{entityClassName}>> GetByExpressionAsync(Expression<Func<{entityClassName}, bool>> criteria)
                {{
                    using (var connection = new SqlConnection(connectionString))
                    {{ 
                        var entities = await connection.QueryAsync<{entityClassName}>($""{query}"");
                        return entities;
                    }}
                }}";

            return output;
        }

        #endregion GetByExpression Method


        #region GetAll Method

        private string BuildGetAllQuery()
        {
            return BuildSelectTableFileds();
        }

        public string GenerateGetAllMethod()
        {
            string query = BuildGetAllQuery();
            var entityClassName = table.Name.Parts[1]; 
            string output = $@"
                /// <summary>
                /// Get all {entityClassName}
                /// </summary>
                public async Task<IEnumerable<{entityClassName}>> GetAllAsync()
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
                public async Task<{entityClassName}> GetBy{pkFieldsNames}Async({pkFieldsWithTypes})
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
