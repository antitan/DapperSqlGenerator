﻿using DapperSqlGenerator.Console.Extenions;
using DapperSqlGenerator.Console.Factory;
using DapperSqlGenerator.Console.Generator.Repositories;
using DapperSqlGenerator.Console.Services;
using Microsoft.SqlServer.Dac.Model;
using System.Reflection;

namespace DapperSqlGenerator.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string projectName = "MyProject";
            string connectionString = "Data Source=localhost;Initial Catalog=SmartCv;Integrated Security=True;Persist Security Info=False;Trust Server Certificate=True;";
             
            //directories where files are created
            string dataModelDir = @"C:\Temp\Data";
            if (!Directory.Exists(dataModelDir)) Directory.CreateDirectory(dataModelDir);
            string dataRepositoryDir = @"C:\Temp\Repositories";
            if (!Directory.Exists(dataRepositoryDir)) Directory.CreateDirectory(dataRepositoryDir);
            string dataServiceDir = @"C:\Temp\Services";
            if (!Directory.Exists(dataServiceDir)) Directory.CreateDirectory(dataServiceDir);
            string cacheServiceDir = @"C:\\Temp\\Cache";
            if (!Directory.Exists(cacheServiceDir)) Directory.CreateDirectory(cacheServiceDir);
            string configurationDir = @"C:\\Temp\\Configuration";
            if (!Directory.Exists(configurationDir)) Directory.CreateDirectory(configurationDir);
            string registerServiceExtensionDir = @"C:\\Temp\\Extensions";
            if (!Directory.Exists(registerServiceExtensionDir)) Directory.CreateDirectory(registerServiceExtensionDir);
            string helpersDir = @"C:\\Temp\\Helpers";
            if (!Directory.Exists(helpersDir)) Directory.CreateDirectory(helpersDir);
            string constantsDir = @"C:\\Temp\\Constants";
            if (!Directory.Exists(constantsDir)) Directory.CreateDirectory(constantsDir);

            //excludes table we don't want ot generate
            string[] excludedTables = { "" };
            //references table (static tables)
            string[] refTables = { "" };

           
            string dataModelNamespace = $"{projectName}.Model";
            string dataRepostioryNamespace = $"{projectName}.Repositories";
            string dataServiceNamespace = $"{projectName}.Services";


            var model = TSqlModelFactory.CreateModel(connectionString);

            List<IGeneratorService> generatorServices = new List<IGeneratorService>();
            generatorServices.Add( new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables));
            generatorServices.Add( new RepositoryGeneratorService(dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, excludedTables));
            generatorServices.Add(new ServicesGeneratorService(dataServiceNamespace, dataModelNamespace, dataServiceDir,projectName, excludedTables, refTables));
            generatorServices.Add(new CopyUtilitiesFilesService(projectName,cacheServiceDir,configurationDir,helpersDir));
            generatorServices.Add(new FileCustomerService(projectName,registerServiceExtensionDir,constantsDir, excludedTables));

            await new DataModelGeneratorService(dataModelNamespace, dataModelDir, excludedTables).GenerateFilesAsync(model);
            await new RepositoryGeneratorService(dataModelNamespace, dataRepostioryNamespace, dataRepositoryDir, excludedTables).GenerateFilesAsync(model);
            await new ServicesGeneratorService(dataServiceNamespace, dataModelNamespace, dataServiceDir, projectName, excludedTables, refTables).GenerateFilesAsync(model);
            await new CopyUtilitiesFilesService(projectName, cacheServiceDir, configurationDir, helpersDir).GenerateFilesAsync(model);
            await new FileCustomerService(projectName, registerServiceExtensionDir, constantsDir, excludedTables).GenerateFilesAsync(model);

            List<Task> tasks = new List<Task>();

            generatorServices.ForEach(serv =>
            {
                tasks.Add(Task.Run(() => serv.GenerateFilesAsync(model)));
            });
            await Task.WhenAll(tasks);
             

            //TODO read StoredProcedure
        }
    }
}
