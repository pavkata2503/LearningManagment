using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class StudyMaterialFilterModel
    {
        public string CreatedName { get; set; }
        public string Search { get; set; }
        public string Category { get; set; }
        public string TypeFile { get; set; }
        public string Subject { get; set; }
        public string ClassFilter { get; set; }
        public string SortOrder { get; set; } = "date_asc";

        public int PageSize { get; set; } = 6;
        public int PageNumber { get; set; } = 1;
    }

}
