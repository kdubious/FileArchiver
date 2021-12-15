using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileArchiver
{
    public class FileToMove
    {

        public string path { get; set; }
        public PathType pathType { get; set; }
        public long size { get; set; }
        public double modifiedAge { get; set; }
        public Status status { get; set; }

        public FileToMove()
        {

        }

        public FileToMove(string Path, PathType PathType, long Size, double ModifiedAge, Status Status)
        {
            path = Path;
            pathType = PathType;
            size = Size;
            modifiedAge = ModifiedAge;
            status = Status;
        }
    }
}
