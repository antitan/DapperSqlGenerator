using Microsoft.SqlServer.Dac.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Services
{
    public class CopyUtilitiesFilesService : IGeneratorService
    {
        string projectName; 
        string cacheServiceDir;
        string configurationDir;
        string helpersDir;
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
            string contentFile = null;
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


            //2-configuration connection string
            file = "./FilesToCopy/Configuration/ConnectionStrings.txt";
            contentFile = File.ReadAllText(file);
            contentFile = contentFile.Replace("{projectNamespace}", projectName);
            path = Path.Combine(configurationDir, Path.GetFileNameWithoutExtension(file) + ".cs");
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

        }
    }
}
