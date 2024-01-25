using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Services
{
    public interface IGeneratorService
    {
        Task GenerateFilesAsync(TSqlModel model);

        List<string> Warnings { get; }
    }
}
