
using DapperSqlGenerator.App.Extenions;
using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Services
{

    public class CacheDataConstantsService : IGeneratorService
    {
        string filePathToWrite;
        string[] excludedTables;
        string[] refTables;
        string projectName;
        string contentFile;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();  
            }
        }

        public CacheDataConstantsService(string filePathToWrite, string contentFile, string[] excludedTables, string[] refTables, string projectName)
        {
            this.filePathToWrite = filePathToWrite;
            this.excludedTables = excludedTables;
            this.refTables = refTables;
            this.projectName = projectName;
            this.contentFile = contentFile;
        }

        public async Task GenerateFilesAsync(TSqlModel model)
        {
            var listCacheDataKeys = new List<string>(); 

            foreach (var table in model.GetAllTables())
            {
                var entityName = table.Name.Parts[1].PascalCase();
                if (!excludedTables.Contains(entityName) && refTables.Contains(entityName))
                {
                    listCacheDataKeys.Add($"public static string {entityName}AllCacheKey = \"{entityName}.all\";"); 
                }
            }
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            contentFile = contentFile.Replace("{CacheDataKeys}", string.Join(Environment.NewLine, listCacheDataKeys));
            await File.WriteAllTextAsync(filePathToWrite, contentFile);
        }
    }
}
