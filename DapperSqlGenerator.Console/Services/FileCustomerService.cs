using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Services
{
    public class FileCustomerService : IGeneratorService
    {

        string projectName;
        string[] excludedTables;
        string[] refTables;
        string registerServiceExtensionDir;
        string constantsDir;

        public List<string> Warnings
        {
            get
            {
                return new List<string>();
            }
        }
        public FileCustomerService(string projectName,  string registerServiceExtensionDir, string constantsDir,string[] excludedTables, string[] refTables)
        {
            this.projectName = projectName;
            this.excludedTables = excludedTables;
            this.refTables = refTables; 
            this.constantsDir = constantsDir;   
            this.registerServiceExtensionDir=registerServiceExtensionDir;   
        }
        public async Task GenerateFilesAsync(TSqlModel model)
        {
            //1- register service extensions in .net core
            string file = "./FilesToCopy/ServiceExtensions/ServicesAndRepositoriesExtensions.txt";
            string contentFile = File.ReadAllText(file);
            string path = Path.Combine(registerServiceExtensionDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            var serviceExtension = new ExtensionRegisterReposAndServices(path, contentFile, excludedTables, projectName);
            await serviceExtension.GenerateFilesAsync(model);

            //2- constants 
            file = "./FilesToCopy/Constants/CacheDataConstants.txt";
            contentFile = File.ReadAllText(file);
            path = Path.Combine(constantsDir, Path.GetFileNameWithoutExtension(file) + ".cs");
            var service = new CacheDataConstantsService(path, contentFile, excludedTables,  refTables, projectName);
            await service.GenerateFilesAsync(model);
        }
    }
}
