using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    public abstract class NevodException : Exception
    {
        public NevodException(string message) : base(message)
        {
        }

        public NevodException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    
    public class InternalNevodErrorException : NevodException
    {
        public InternalNevodErrorException(string message) : base(message)
        {
        }
    }
    
    public class PackageGeneratorException : NevodException
    {
        public PackageGeneratorException(string message) : base(message)
        {
        }
    }
    
    public class NevodPackageLoadException : NevodException
    {
        public NevodPackageLoadException(string message) : base(message)
        {
        }
    }
    
    public class InvalidPackageException : NevodException
    {
        public List<PackageErrors> PackageErrorsList { get; }

        public InvalidPackageException(string message, List<PackageErrors> packageErrorsList) : base(message)
        {
            PackageErrorsList = packageErrorsList;
        }
    }
}
