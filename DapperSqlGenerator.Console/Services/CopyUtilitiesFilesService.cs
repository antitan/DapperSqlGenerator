﻿using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Services
{
    public class CopyUtilitiesFilesService : IGeneratorService
    {
        string projectName; 
        string cacheServiceDir;
        string configurationDir;
        string helpersDir;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }
        public CopyUtilitiesFilesService(string projectName, string cacheServiceDir, string configurationDir,   string helpersDir)
        {
            this.projectName = projectName; 
            this.cacheServiceDir = cacheServiceDir;
            this.configurationDir = configurationDir;
            this.helpersDir = helpersDir;
        }
        public async Task GenerateFilesAsync( TSqlModel model)
        {
            //copy files utilities
            //1- Cache
            string? contentFile = null;
            string file = "./FilesToCopy/Cache/ICacheManager.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            string path = Path.Combine(cacheServiceDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);

            file = "./FilesToCopy/Cache/MemoryCacheManager.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(cacheServiceDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);

            //helpers
            file = "./FilesToCopy/Helpers/JsonHelper.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(helpersDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);

            file = "./FilesToCopy/Helpers/StringHelpers.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(helpersDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);

            file = "./FilesToCopy/Helpers/ReflexionHelper.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(helpersDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);

            file = "./FilesToCopy/Helpers/ExpressionExtensions.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(helpersDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);

            file = "./FilesToCopy/Pagination/PagedResults.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(helpersDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            await File.WriteAllTextAsync(path, contentFile);
        }
    }
}
