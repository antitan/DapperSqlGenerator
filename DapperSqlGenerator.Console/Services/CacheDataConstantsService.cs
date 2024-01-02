using DapperSqlGenerator.Console.Extenions;
using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Services
{
   
    public class CacheDataConstantsService : IGeneratorService
    {
        string filePathToWrite;
        string[] excludedTables;
        string[] refTables;
        string projectName;
        string contentFile;
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
