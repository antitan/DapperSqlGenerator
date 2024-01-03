using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DapperSqlGenerator.Console.Helpers
{
    public static class ClassHelper
    {
        public static List<string> GetPublicMethodNamesFromCode(string code)
        {
            // Compiler le code en un assembly
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false
            };

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.HasErrors)
            {
                throw new InvalidOperationException("Compilation error occurred.");
            }

            Assembly compiledAssembly = results.CompiledAssembly;
            List<string> methodNames = new List<string>();

            // Parcourir tous les types dans l'assembly
            foreach (Type type in compiledAssembly.GetTypes())
            {
                // Obtenir les méthodes publiques
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    methodNames.Add(method.Name);
                }
            }

            return methodNames;
        }



        public static int CountOccurrences(string text, string word)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(word))
            {
                return 0;
            }

            // Nettoyer le texte en supprimant les caractères spéciaux, les retours à la ligne, etc.
            string cleanedText = Regex.Replace(text, @"[\r\n\t]", " "); // Remplace les retours à la ligne et les tabulations par des espaces
            cleanedText = Regex.Replace(cleanedText, @"[^\w\s]", ""); // Supprime tout ce qui n'est pas un mot ou un espace

            // Diviser le texte nettoyé en mots
            string[] words = cleanedText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Compter les occurrences du mot cible
            int count = 0;
            foreach (string w in words)
            {
                if (w.Equals(word, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }


    }
}