using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Testing_Github
{
    public class Issue
    {
        public Issue()
        {

        }

        public int id { get; set; }

        public int  number { get; set; }

        public string title { get; set; }

        public string body { get; set; }
    }
}
