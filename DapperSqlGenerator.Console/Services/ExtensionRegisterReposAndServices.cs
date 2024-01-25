
using DapperSqlGenerator.App.Extenions;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Services
{
    public class ExtensionRegisterReposAndServices : IGeneratorService
    {
        string filePathToWrite;
        string[] excludedTables;
        string projectName;
        string contentFile;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }
        public ExtensionRegisterReposAndServices(string filePathToWrite, string contentFile, string[] excludedTables, string projectName)
        { 
            this.filePathToWrite = filePathToWrite;
            this.excludedTables = excludedTables;
            this.projectName = projectName; 
            this.contentFile = contentFile; 
        }
        public async Task GenerateFilesAsync(TSqlModel model)
        { 
            var listRepositories = new List<string>();
            var listService = new List<string>();  

            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName))
                {
                    listRepositories.Add($"services.AddScoped<I{entityName}Repository,{entityName}Repository>();");
                    listService.Add($"services.AddScoped<I{entityName}Service,{entityName}Service>();");
                }
            }

            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            contentFile = contentFile.Replace("{RegisterDataRepositories}", string.Join(Environment.NewLine,listRepositories));
            contentFile = contentFile.Replace("{RegisterDataServices}", string.Join(Environment.NewLine, listService));
            await File.WriteAllTextAsync(filePathToWrite, contentFile);

        }
    }
}
