
using DapperSqlGenerator.App.Extenions;
using DapperSqlGenerator.App.Generator.Entities;
using DapperSqlGenerator.App.Helpers;
using Microsoft.SqlServer.Dac.Model;
using System.Text;

namespace DapperSqlGenerator.App.Services
{
    public class DataModelGeneratorService : IGeneratorService
    {
        string dataModelNamespace;
        string dirToWrite;
        string[] excludedTables;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public DataModelGeneratorService(string dataModelNamespace, string dirToWrite,string [] excludedTables)
        {
            this.dataModelNamespace = dataModelNamespace;
            this.dirToWrite = dirToWrite;  
            this.excludedTables = excludedTables;   
        }
        public async Task GenerateFilesAsync( TSqlModel model)
        { 
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    string filePath = Path.Combine(dirToWrite, $"{entityName}.cs");
                    using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            string cs = new EntityClassGenerator(dataModelNamespace, table).Generate();
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
