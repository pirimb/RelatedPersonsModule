using Oracle.ManagedDataAccess.Client;
using RelatedPersonsModule.Models;

namespace RelatedPersonsModule.Interfaces
{
    public interface IDB
    {
        public void Init(LoginModel user);
        public OracleConnection GetConnection();
        public int TestConnection();
        public int GetAccessLevel(LoginModel model);
        public RelatedPerson GetRelatedPersonsById(int id);
        public void UpdateRelatedPerson(RelatedPerson model);
        public List<RelatedPerson> GetRelatedPersons(int shareholder, int supervisor, int audit, int position);
        public void AddNewRelatedPerson(RelatedPerson model);
        public List<RelativeType> GetRelativeTypes();
        public void AddNewRelationship(int mainId, string pinCode, int relType);
        public List<RelatedPerson> GetRelativeConnectionsById(int id);
        public void UpdateRelationship(int mainId, int relId, int relType);
        public Relatives GetRelativeTypeById(int mainId, int relId);
        public void AddNewDepartment(string name);
        public void AddNewDivision(string name);
        public Department GetDivisionById(int depId);
        public List<Department> GetDivisions();
        public void UpdateDivision(int id, string name);
        public List<Department> GetDepartments();
        public Department GetDepartmentById(int depId);
        public void UpdateDepartment(int id, string name);
        public void AddNewDirectorDep(string pinCode, int depId);
        public void UpdateDirectorDep(int mainId, int depId);
        public Department GetDirectorDepById(int mainId);
        public void AddNewHeadOfDivById(string pinCode, int divİd);
        public void UpdateHeadOfDivByİd(int mainId, int divİd);
        public Department GetHeadOfDivDepById(int mainId);
        public void AddNewShareholderShare(string pinCode, string shares);
        public void UpdateShareholdersSharesById(int mainId, string shares);
        public ShareholderShare GetShareholderSharesById(int mainId);
        public void AddNewSupervisorPosition(string pinCode, int position);
        public void UpdateSupervisorPositionById(int mainId, int position);
        public SupervisorPosition GetSupervisorPositionById(int mainId);
        public void DeleteGenDirectorId(int id);
        public int[] GetGenDirDepsById(int mainId);
        public void AddNewCommittee(string name);
        public void UpdateCommittee(int id, string name);
        public List<Department> GetCommittees();
        public Department GetCommitteeById(int comId);
        public List<PersonCommittee> GetPersonCommitteesById(int id);
        public void AddNewPersonCommittee(int mainId, int comId, int position);
        public void UpdatePersonCommittee(int mainId, int comId, int position, int oldComId);
        public void DeletePersonCommittee(int mainId, int comId);
        public void DeleteRelativeConnection(int mainId, int relId, int relType);
        public int CheckExistingByPinCode(string pinCode);
        public string GetDbName();

    }
}
