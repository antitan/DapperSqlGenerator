using DapperSqlGenerator.Console.Extenions;
using DapperSqlGenerator.Console.Helpers;
using Microsoft.SqlServer.Dac.Model; 

namespace DapperSqlGenerator.Console.Generator.Repositories
{
    public class DapperRepositoryMethodsGenerator : IRepositoryMethodsGenerator
    { 

        TSqlObject table;
        public DapperRepositoryMethodsGenerator(TSqlObject table)
        {
          this.table = table;
        }

        #region Delete

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
            query = $"DELETE FROM {table.Name} WHERE {whereClause_conditions}";
            return query;
        }

        public string GenerateDeleteMethod()
        {
            string query = BuildDeleteQuery();
            var entityClassName = table.Name.Parts[1];
            var pkColumns = table.GetPrimaryKeyColumns();
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
                        /// Delete {entityClassName}
                        /// </summary>
                        public async Task Delete({pkFieldsWithTypes})
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
                query += "WHERE " + whereClause;
            }
            return query;
        }

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
                public async Task<IEnumerable<{entityClassName}>> GetAll()
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

            var pkFieldNames = String.Join("And",
                pkColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    return $"{colName.PascalCase()}";
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
                public async Task<{entityClassName}> GetBy{pkFieldsNames}({pkFieldsWithTypes})
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
        private string BuildInsertQuery()
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

            return $"INSERT INTO {table.Name} ({insertClause_columns})  OUTPUT INSERTED.Id VALUES ({insertClause_values})";

        }

        public string GenerateInsertMethod()
        {
            string query = BuildInsertQuery();
            var allColumns = table.GetAllColumns();
            var pkColumns = table.GetPrimaryKeyColumns();
            var entityClassName = table.Name.Parts[1];
            var paramName = Common.FirstCharacterToLower(entityClassName);

            //Exclude de PK identity field to put "Direction Output" in Dapper params
            bool isOneColumnIdentity = pkColumns.Count() == 1 && pkColumns.ToList()[0].IsColumnIdentity();
            var normalColumns = isOneColumnIdentity ? allColumns.Except(pkColumns) : allColumns;

            string returnType = isOneColumnIdentity
                ? MatchingDataTypeHelper.GetDotNetDataType(pkColumns.ToArray()[0].GetColumnSqlDataType())
                : "bool"; // return bool if insert ok  => we cannot return the new Id generated by Identity

            string spNormalParams = String.Join(Environment.NewLine + "            ",
                   normalColumns.Select(col =>
                   {
                       var colName = col.Name.Parts[2];
                       var entityProp = colName.PascalCase();
                       return $@"p.Add(""@{colName}"", {paramName}.{entityProp});";
                   }));



            string output = $@"
            /// <summary>
            /// Insert {entityClassName}
            /// </summary>
            public async  Task<{returnType}> Insert({entityClassName} {paramName})
            {{
                using (var connection = new SqlConnection(connectionString))
                {{
                    var p = new DynamicParameters();
                    {spNormalParams}

                    var id = await connection.QuerySingleAsync<int>(""{query}"", p);
                    return id;
                }}
                 
            }}"+ Environment.NewLine;

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

            var inputParamDeclarations = String.Join(Environment.NewLine + ", ",
                allColumns.Select(col =>
                {
                    var colName = col.Name.Parts[2];
                    var colDataType = col.GetColumnSqlDataType();
                    return $"@{colName} {colDataType}";
                })
            );

            var updateClause_setStatements = String.Join(Environment.NewLine + "        , ", nonIdentityColumns.Select(col =>
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
        public async Task Update({entityClassName} {paramName})
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
