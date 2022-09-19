namespace RelatedPersonsModule.Models
{
    public class MainDataViewModel
    {
        public Shareholder Shareholder { get; set; }
        public List<Shareholder> Shareholders { get; set; }
        public RelatedPerson RelatedPerson { get; set; }
        public RelatedPerson Relative { get; set; }
        public List<RelatedPerson> RelatedPersons { get; set;}
        public Relatives Relatives { get; set; } //for adding new relative(relative.chtml)
        public RelativeType RelativeType { get; set; }
        public List<RelativeType> RelativeTypes { get; set; }
        public RelativePersonAndRelType RelativePersonAndRelType { get; set; }
        public List<RelatedPerson> RelativeConnections { get; set; }
        public int MainId { get; set; }
        public int RelativeId { get; set; }
        public int ComitteeId { get; set; }
        public Department Department { get; set; }
        public Department Division { get; set; }
        public Department Committee { get; set; }
        public Department NewDepartment { get; set; }
        public List<Department> Departments { get; set; }
        public List<Department> Divisions { get; set; }
        public List<Department> Committees { get; set; }
        public ShareholderShare ShareholderShare { get; set; }
        public SupervisorPosition SupervisorPosition { get; set; }
        public int[] states { get; set; }
        public PersonCommittee PersonCommittee { get; set; }
        public List<PersonCommittee> PersonCommittees { get; set; }
        public MainDataViewModel()
        {
            Shareholder = new Shareholder();
            RelatedPerson = new RelatedPerson();
            Relative = new RelatedPerson();
            RelatedPersons = new List<RelatedPerson>();
            Relatives = new Relatives();
            RelativeType = new RelativeType();
            RelativeTypes = new List<RelativeType>();
            RelativePersonAndRelType = new RelativePersonAndRelType();
            RelativeConnections = new List<RelatedPerson>();
            Departments = new List<Department>();
            Divisions = new List<Department>();
            Committees = new List<Department>();
            Department = new Department();
            Division = new Department();
            Committee = new Department();
            NewDepartment = new Department();
            ShareholderShare = new ShareholderShare();
            SupervisorPosition = new SupervisorPosition();
            PersonCommittee = new PersonCommittee();
            PersonCommittees = new List<PersonCommittee>();
    }
    }
}
