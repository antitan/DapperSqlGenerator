using DapperSqlGenerator.Console.Extenions; 
using DapperSqlGenerator.Console.Generator.Services;
using DapperSqlGenerator.Console.Helpers;
using Microsoft.SqlServer.Dac.Model; 
using System.Text; 

namespace DapperSqlGenerator.Console.Services
{ 
    public class ServicesGeneratorService : IGeneratorService
    {
        string projectName;
        string dataModelNamespace;
        string serviceModelNamespace;
        string dataRepositoryNamespace;
        string dirToWrite;
        string[] excludedTables;
        string[] refTables;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }

        public ServicesGeneratorService(string serviceModelNamespace, string dataModelNamespace, string dataRepositoryNamespace, string dirToWrite, string projectName, string[] excludedTables, string[] refTables)
        {
            this.serviceModelNamespace = serviceModelNamespace;
            this.dirToWrite = dirToWrite;
            this.excludedTables = excludedTables;
            this.refTables = refTables;
            this.dataModelNamespace = dataModelNamespace;
            this.dataRepositoryNamespace = dataRepositoryNamespace;
            this.projectName = projectName;
        }

        public async Task GenerateFilesAsync(TSqlModel model)
        { 
            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    string filePath = Path.Combine(dirToWrite, $"{entityName}Service.cs");
                    if (!File.Exists(filePath))
                    {
                        using (var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
                            {
                                string cs = new ServicesGenerator(serviceModelNamespace, dataModelNamespace, dataRepositoryNamespace, projectName, refTables, table).Generate();
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
}
