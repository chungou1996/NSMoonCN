using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSMoonPak.PakException
{
    public class NSMoonException : Exception
    {
        public NSMoonException() : base()
        {

        }

        public NSMoonException(string message) : base(message)
        {

        }
    }

    public class MagicMissMatching : NSMoonException
    {
        public MagicMissMatching():base()
        {

        }
        public MagicMissMatching(string message) : base(message)
        {
        }
    }
}
