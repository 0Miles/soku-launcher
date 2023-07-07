using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokuLauncher.Models
{
    internal class SelectorNodeModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public bool Selected { get; set; } = false;
    }
}
