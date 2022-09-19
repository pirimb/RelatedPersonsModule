namespace RelatedPersonsModule.Models
{
    public class RelativePersonAndRelType
    {
        public int MainId { get; set; } 
        public int Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Patron { get; set; }
        public string Cif { get; set; }
        public string PinCode { get; set; }
        public int IsShareholder { get; set; }
        public int IsSupervisory { get; set; }
        public int IsAudit { get; set; }
        public int Position { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int Status { get; set; }
        public string RelType { get; set; }
    }
}
