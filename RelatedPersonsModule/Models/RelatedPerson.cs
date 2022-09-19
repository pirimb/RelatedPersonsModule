using DocumentFormat.OpenXml.Drawing.Charts;
using Fingers10.ExcelExport.Attributes;
using System.ComponentModel.DataAnnotations;

namespace RelatedPersonsModule.Models
{
    public class RelatedPerson
    {
        public int Id { get; set; }
        [Display(Name = "Ad")]
        [IncludeInReport(Order = 2)]
        public string Name { get; set; }
        [Display(Name = "Soyad")]
        [IncludeInReport(Order = 3)]
        public string SurName { get; set; }
        [Display(Name = "Ata adı")]
        [IncludeInReport(Order = 4)]
        public string Patron { get; set; }
        [IncludeInReport(Order = 5)]
        [Display(Name = "Qeydiyyat nömrəsi")]
        public string Cif { get; set; }
        [IncludeInReport(Order = 6)]
        [Display(Name = "Pin kod")]
        public string PinCode { get; set; }
        public int IsShareholder { get; set; }
        public int IsSupervisory { get; set; }
        public int IsAudit { get; set; }
        public int Position { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int Status { get; set; }

        [Display(Name = "Tutduğu vəzifə")]
        [IncludeInReport(Order = 7)]
        public string PositionWord { get; set; }
        [Display(Name = "Səhmdar")]
        [IncludeInReport(Order = 8)]
        public string ShareholderWord { get; set; }
        [Display(Name = "Müşahidə şurası")]
        [IncludeInReport(Order = 9)]
        public string SupervisorWord { get; set; }
        [Display(Name = "Audit")]
        [IncludeInReport(Order = 10)]
        public string AuditWord { get; set; }    
    }
}
