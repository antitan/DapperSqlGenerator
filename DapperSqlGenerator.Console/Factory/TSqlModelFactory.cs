using Microsoft.SqlServer.Dac.Model;

namespace DapperSqlGenerator.App.Factory
{
    public class TSqlModelFactory
    {
        public static TSqlModel CreateModel(string connexionString)
        {
            ModelExtractOptions options = new ModelExtractOptions()
            {
                LoadAsScriptBackedModel = false,
            };
            return TSqlModel.LoadFromDatabase(connexionString, options);
            //ModelLoadOptions options = new ModelLoadOptions()
            //{
            //    LoadAsScriptBackedModel = false,
            //};
            //return TSqlModel.LoadFromDacpac(dacpacFileName, options);
        }
    }
}
