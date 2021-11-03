using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    public readonly struct PackageErrors
    {
        public string FilePath { get; }
        public List<Error> Errors { get; }
        
        public PackageErrors(string filePath, List<Error> errors)
        {
            FilePath = filePath;
            Errors = errors;
        }
    }
    
    internal class ErrorsCollector : SyntaxVisitor
    {
        private List<PackageErrors> fErrors;
        private string fCurrentFilePath;
        
        internal static InvalidPackageException AggregateErrorsException(LinkedPackageSyntax linkedTree, string filePath,
            string errorMessage)
        {
            var errorsCollector = new ErrorsCollector();
            List<PackageErrors> packageErrorsList = errorsCollector.CollectErrors(linkedTree, filePath);
            var result = new InvalidPackageException(errorMessage, packageErrorsList);
            return result;
        }

        internal List<PackageErrors> CollectErrors(LinkedPackageSyntax package, string packageFilePath)
        {
            fErrors = new List<PackageErrors>();
            fCurrentFilePath = packageFilePath;
            Visit(package);
            List<PackageErrors> result = fErrors;
            fErrors = null;
            return result;
        }

        protected internal override Syntax VisitLinkedPackage(LinkedPackageSyntax node)
        {
            if (node.Errors.Count != 0)
                fErrors.Add(new PackageErrors(fCurrentFilePath, node.Errors));
            Visit(node.RequiredPackages);
            return node;
        }

        protected internal override Syntax VisitRequiredPackage(RequiredPackageSyntax node)
        {
            string saveCurrentFilePath = fCurrentFilePath;
            fCurrentFilePath = node.FullPath;
            Visit(node.Package);
            fCurrentFilePath = saveCurrentFilePath;
            return node;
        }
    }
}
