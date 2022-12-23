using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Johnson
{

    public class APIReponse
    {
        public int count { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string quote { get; set; }
        public string episode { get; set; }
        public string person { get; set; }
        public string image { get; set; }
    }

}
