using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class ViolationReport
    {
        [NotMapped]
        public ReportType ReportTypeEnum
        {
            get => (ReportType)Reporttype;  // int → enum
            set => Reporttype = (int)value; // enum → int
        }

        [NotMapped]
        public ReportStatus StatusEnum
        {
            get => (ReportStatus)Status;
            set => Status = (int)value;
        }
    }
}
