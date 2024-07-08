using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator.Controllers;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.App.Services
{
    public class MapperGeneratorService : IGeneratorService
    {
        string dirToWrite;
        string[] excludedTables;
        string mapperNamespace;
        string requestsNamespace;
        string responsesNamespace;
        string modelNamespace;
        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public MapperGeneratorService(string modelNamespace, string mapperNamespace, string requestsNamespace, string responsesNamespace, string dirToWrite, string[] excludedTables)
        {
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.mapperNamespace = mapperNamespace;
            this.requestsNamespace = requestsNamespace;
            this.responsesNamespace = responsesNamespace;
            this.modelNamespace = modelNamespace;
        }
        public async Task GenerateFilesAsync(TSqlModel model)
        {
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    string filePath = Path.Combine(dirToWrite, $"{entityName}Mapper.cs");
                    using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            string cs = new MapperGenerator(modelNamespace, mapperNamespace, requestsNamespace, responsesNamespace, table).Generate();
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
