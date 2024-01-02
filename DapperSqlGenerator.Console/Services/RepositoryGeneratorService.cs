using DapperSqlGenerator.Console.Extenions;
using DapperSqlGenerator.Console.Generator;
using DapperSqlGenerator.Console.Generator.Repositories;
using DapperSqlGenerator.Console.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Services
{ 
    public class RepositoryGeneratorService : IGeneratorService
    {
        string datamodelNamespace;
        string dataRepostioryNamespace;
        string dirToWrite;
        string[] excludedTables;
        string projectName;
        public RepositoryGeneratorService(string datamodelNamespace,  string dataRepostioryNamespace, string dirToWrite, string projectName,string[] excludedTables)
        { 
            this.datamodelNamespace= datamodelNamespace;
            this.dataRepostioryNamespace = dataRepostioryNamespace;
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.projectName = projectName;
        }

        public async Task GenerateFilesAsync(TSqlModel model)
        { 
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    string filePath = Path.Combine(dirToWrite, $"{entityName}Repository.cs");
                    using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            string cs = new DapperRepositoryClassGenerator(projectName, datamodelNamespace, dataRepostioryNamespace, table).Generate();
                            if (cs != string.Empty)
                            {
                                string formatedCode = CodeFormatterHelper.ReformatCode(cs);
                                await writer.WriteLineAsync(formatedCode);
                            }
                        }
                    }
                }
            }
        }
    }
}
