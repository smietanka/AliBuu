using OvhWrapper.Types.Cloud.Storage;
using System;

namespace OvhWrapper.Types
{
    public class ContainerObject
    {
        public long RetrievalDelay { get; set; }
        public DateTime LastModified { get; set; }
        public string Name { get; set; }
        public string RetrievalState { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }
}
