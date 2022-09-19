using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using RelatedPersonsModule.Interfaces;
using RelatedPersonsModule.Models;
using System.Data;

namespace RelatedPersonsModule.Repository
{
    public class DB : IDB
    {
        private static string connectionString;
        private static string dbName;
        private static DbConfig DBConfig;
        private static ILogger<DB> log;

        public DB(IOptions<DbConfig> options, ILogger<DB> _log)
        {
            DBConfig = options.Value;
            log = _log;
        }

        public void Init(LoginModel user)
        {
            connectionString = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={DBConfig.Host})(PORT={DBConfig.Port})))" +
                                $"(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={DBConfig.ServiceName}))); " +
                                $"User Id ={user.UserName}; Password ={user.Password}; Min Pool Size = 1; Max Pool Size = 10; Pooling = True;" +
                                $" Validate Connection = true; Connection Lifetime = 300; Connection Timeout = 300;";
        }

        public OracleConnection GetConnection()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string not initialized!");
            }
            var connection = new OracleConnection(connectionString);
            connection.Open();
            return connection;
        }

        public int TestConnection()
        {
            int res = -1;
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT 1 FROM DUAL";
                        cmd.CommandType = CommandType.Text;

                        cmd.ExecuteNonQuery();
                        res = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return res;
        }

        public string GetDbName()
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    dbName = DBConfig.ServiceName;
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return dbName;
        }

        public int GetAccessLevel(LoginModel model)
        {
            int res = -1;
            OracleConnection con = null;
            if (model.UserName.IndexOf("bnk_") == 0)
            {
                model.UserName = model.UserName.Remove(0, 4);
            }
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT anti_fraud_admin FROM SBNK_PRL.CAV_ICR WHERE ICR_AD = '{model.UserName}'";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                res = Convert.ToInt32(reader["anti_fraud_admin"]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Test Connection: User - {model.UserName} Error - {ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                }
            }
            return res;
        }

      

        public List<RelatedPerson> GetRelatedPersons(int shareholder, int supervisor, int audit, int position)
        {
            var relatedPersons = new List<RelatedPerson>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_related_persons";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_is_shareholder", OracleDbType.Int32, ParameterDirection.Input).Value = shareholder;
                        cmd.Parameters.Add("p_is_supervisory", OracleDbType.Int32, ParameterDirection.Input).Value = supervisor;
                        cmd.Parameters.Add("p_is_audit", OracleDbType.Int32, ParameterDirection.Input).Value = audit;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Input).Value = position;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RelatedPerson relatedPerson = new RelatedPerson();

                                relatedPerson.Id = Convert.ToInt32(reader["ID"].ToString());
                                relatedPerson.Name = reader["Name"].ToString(); ;
                                relatedPerson.SurName = reader["sur_name"].ToString();
                                relatedPerson.Patron = reader["patron"].ToString();
                                relatedPerson.Cif = reader["cif"].ToString();
                                relatedPerson.PinCode = reader["pin_code"].ToString();
                                relatedPerson.IsShareholder = Convert.ToInt32(reader["is_shareholder"].ToString());
                                relatedPerson.IsSupervisory = Convert.ToInt32(reader["IS_SUPERVISORY"].ToString());
                                relatedPerson.IsAudit = Convert.ToInt32(reader["IS_AUDIT"].ToString());
                                relatedPerson.Position = Convert.ToInt32(reader["POSITION"].ToString());
                                relatedPerson.DeleteDate = DbHelper.CheckDbNullForNullDate(reader, "delete_date");
                                relatedPerson.InsertDate = DbHelper.CheckDbNullForNullDate(reader, "insert_date");
                                relatedPerson.UpdateDate = DbHelper.CheckDbNullForNullDate(reader, "update_date");
                                relatedPerson.Status = Convert.ToInt32(reader["status"].ToString());


                                if (relatedPerson.Position == 1)
                                    relatedPerson.PositionWord = "Şöbə rəisi";
                                else if(relatedPerson.Position == 2)
                                    relatedPerson.PositionWord = "Direktor";
                                else if(relatedPerson.Position == 3)
                                    relatedPerson.PositionWord = "Baş direktor";
                                if (relatedPerson.IsShareholder == 1)
                                    relatedPerson.ShareholderWord = "Bəli";
                                else if (relatedPerson.IsShareholder == 0)
                                    relatedPerson.ShareholderWord = "Xeyr";
                                if (relatedPerson.IsSupervisory == 1)
                                    relatedPerson.SupervisorWord = "Bəli";
                                else if (relatedPerson.IsSupervisory == 0)
                                    relatedPerson.SupervisorWord = "Xeyr";
                                if (relatedPerson.IsAudit == 1)
                                    relatedPerson.AuditWord = "Bəli";
                                else if (relatedPerson.IsAudit == 0)
                                    relatedPerson.AuditWord = "Xeyr";


                                relatedPersons.Add(relatedPerson);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return relatedPersons;
        }

        

        public void AddNewRelatedPerson(RelatedPerson model)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_related_persons";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Name;
                        cmd.Parameters.Add("p_sur_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.SurName;
                        cmd.Parameters.Add("p_patron", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Patron;
                        cmd.Parameters.Add("p_cif", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Cif;
                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.PinCode;
                        cmd.Parameters.Add("p_is_shareholder", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.IsShareholder;
                        cmd.Parameters.Add("p_is_supervisory", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.IsSupervisory;
                        cmd.Parameters.Add("p_is_audit", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.IsAudit;
                        cmd.Parameters.Add("p_position", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Position;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public RelatedPerson GetRelatedPersonsById(int id)
        {
            var relatedPerson = new RelatedPerson();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"select * from sbnk_prl.related_persons t where t.ID = '{id}'";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                relatedPerson.Id = id;
                                relatedPerson.Name = reader["Name"].ToString();
                                relatedPerson.SurName = reader["sur_name"].ToString();
                                relatedPerson.Patron = reader["patron"].ToString();
                                relatedPerson.Cif = reader["cif"].ToString();
                                relatedPerson.PinCode = reader["pin_code"].ToString();
                                relatedPerson.IsShareholder = Convert.ToInt32(reader["is_shareholder"].ToString());
                                relatedPerson.IsSupervisory = Convert.ToInt32(reader["IS_SUPERVISORY"].ToString());
                                relatedPerson.IsAudit = Convert.ToInt32(reader["IS_AUDIT"].ToString());
                                relatedPerson.Position = Convert.ToInt32(reader["POSITION"].ToString());
                                relatedPerson.DeleteDate = DbHelper.CheckDbNullForNullDate(reader, "delete_date");
                                relatedPerson.InsertDate = DbHelper.CheckDbNullForNullDate(reader, "insert_date");
                                relatedPerson.UpdateDate = DbHelper.CheckDbNullForNullDate(reader, "update_date");
                                relatedPerson.Status = Convert.ToInt32(reader["status"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return relatedPerson;
        }

        public void UpdateRelatedPerson(RelatedPerson model)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_related_persons";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_ID", OracleDbType.Int32, ParameterDirection.Input).Value = model.Id;
                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Name;
                        cmd.Parameters.Add("p_sur_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.SurName;
                        cmd.Parameters.Add("p_patron", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Patron;
                        cmd.Parameters.Add("p_cif", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.Cif;
                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = model.PinCode;
                        cmd.Parameters.Add("p_is_sahreholder", OracleDbType.Int32, ParameterDirection.Input).Value = model.IsShareholder;
                        cmd.Parameters.Add("p_is_supervisory", OracleDbType.Int32, ParameterDirection.Input).Value = model.IsSupervisory;
                        cmd.Parameters.Add("p_is_audit", OracleDbType.Int32, ParameterDirection.Input).Value = model.IsAudit;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Input).Value = model.Position;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public RelatedPerson GetRelativesById(int id)
        {
            var relatedPerson = new RelatedPerson();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"select * from sbnk_prl.related_persons t where t.ID = '{id}'";
                        cmd.CommandType = CommandType.Text;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                relatedPerson.Id = id;
                                relatedPerson.Name = reader["Name"].ToString(); ;
                                relatedPerson.SurName = reader["sur_name"].ToString();
                                relatedPerson.Patron = reader["patron"].ToString();
                                relatedPerson.Cif = reader["cif"].ToString();
                                relatedPerson.PinCode = reader["pin_code"].ToString();
                                relatedPerson.IsShareholder = Convert.ToInt32(reader["is_shareholder"].ToString());
                                relatedPerson.IsSupervisory = Convert.ToInt32(reader["IS_SUPERVISORY"].ToString());
                                relatedPerson.IsAudit = Convert.ToInt32(reader["IS_AUDIT"].ToString());
                                relatedPerson.Position = Convert.ToInt32(reader["POSITION"].ToString());
                                relatedPerson.DeleteDate = DbHelper.CheckDbNullForNullDate(reader, "delete_date");
                                relatedPerson.InsertDate = DbHelper.CheckDbNullForNullDate(reader, "insert_date");
                                relatedPerson.UpdateDate = DbHelper.CheckDbNullForNullDate(reader, "update_date");
                                relatedPerson.Status = Convert.ToInt32(reader["status"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return relatedPerson;
        }

        public List<RelativeType> GetRelativeTypes()
        {
            var relativeTypes = new List<RelativeType>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_relationship_types";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("res", OracleDbType.RefCursor, ParameterDirection.ReturnValue);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RelativeType relativeType = new RelativeType();

                                relativeType.Id = Convert.ToInt32(reader["ID"].ToString());
                                relativeType.Type = reader["Name"].ToString();

                                relativeTypes.Add(relativeType);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return relativeTypes;
        }

        public void AddNewRelationship(int mainId, string pinCode, int relType)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_relationship";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = pinCode;
                        cmd.Parameters.Add("p_rel_type", OracleDbType.Int32, ParameterDirection.Input).Value = relType;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }
        public void UpdateRelationship(int mainId, int relId, int relType)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_relationship";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_pin_code", OracleDbType.Int32, ParameterDirection.Input).Value = relId;
                        cmd.Parameters.Add("p_rel_type", OracleDbType.Int32, ParameterDirection.Input).Value = relType;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }


        public List<RelatedPerson> GetRelativeConnectionsById(int id)
        {
            var relatedPersons = new List<RelatedPerson>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_relationships";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = id;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RelatedPerson relatedPerson = new RelatedPerson();

                                relatedPerson.Id = Convert.ToInt32(reader["ID"].ToString());
                                relatedPerson.Name = reader["Name"].ToString(); ;
                                relatedPerson.SurName = reader["sur_name"].ToString();
                                relatedPerson.Patron = reader["patron"].ToString();
                                relatedPerson.Cif = reader["cif"].ToString();
                                relatedPerson.PinCode = reader["pin_code"].ToString();
                                relatedPerson.IsShareholder = Convert.ToInt32(reader["is_shareholder"].ToString());
                                relatedPerson.IsSupervisory = Convert.ToInt32(reader["IS_SUPERVISORY"].ToString());
                                relatedPerson.IsAudit = Convert.ToInt32(reader["IS_AUDIT"].ToString());
                                relatedPerson.Position = Convert.ToInt32(reader["POSITION"].ToString());
                                relatedPerson.DeleteDate = DbHelper.CheckDbNullForNullDate(reader, "delete_date");
                                relatedPerson.InsertDate = DbHelper.CheckDbNullForNullDate(reader, "insert_date");
                                relatedPerson.UpdateDate = DbHelper.CheckDbNullForNullDate(reader, "update_date");
                                relatedPerson.Status = Convert.ToInt32(reader["status"].ToString());

                                relatedPersons.Add(relatedPerson);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return relatedPersons;
        }

        public Relatives GetRelativeTypeById(int mainId, int relId)
        {
            var relatives = new Relatives();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_relative_type";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_rel_id", OracleDbType.Int32, ParameterDirection.Input).Value = relId;
                        cmd.Parameters.Add("p_result", OracleDbType.Int32, ParameterDirection.Output);
                        cmd.ExecuteNonQuery();
                        relatives.RelType = Convert.ToInt32(cmd.Parameters["p_result"].Value.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return relatives;
        }

        public void AddNewDepartment(string name)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_rel_pers_department";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = name;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        

        public Department GetDepartmentById(int depId)
        {
            var department = new Department();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_department_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_dep_id", OracleDbType.Int32, ParameterDirection.Input).Value = depId;
                        cmd.Parameters.Add("p_dep_name", OracleDbType.Varchar2, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        department.Id = depId;
                        department.Name = cmd.Parameters["p_dep_name"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return department;
        }

        public List<Department> GetDepartments()
        {
            var departments = new List<Department>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_rel_pers_departments";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("v_result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Department department = new Department();

                                department.Id = Convert.ToInt32(reader["ID"].ToString());
                                department.Name = reader["Name"].ToString();

                                departments.Add(department);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return departments;
        }
        

        public void UpdateDepartment(int id, string name)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_rel_pers_department";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_id", OracleDbType.Int32, ParameterDirection.Input).Value = id;
                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = name;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }


        public void AddNewDivision(string name)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_rel_pers_division";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = name;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public Department GetDivisionById(int depId)
        {
            var division = new Department();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_division_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_div_id", OracleDbType.Int32, ParameterDirection.Input).Value = depId;
                        cmd.Parameters.Add("p_div_name", OracleDbType.Varchar2, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        division.Id = depId;
                        division.Name = cmd.Parameters["p_div_name"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return division;
        }

        public List<Department> GetDivisions()
        {
            var divisions = new List<Department>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_rel_pers_divisions";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("v_result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Department division = new Department();

                                division.Id = Convert.ToInt32(reader["ID"].ToString());
                                division.Name = reader["Name"].ToString();

                                divisions.Add(division);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return divisions;
        }
        

        public void UpdateDivision(int id, string name)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_rel_pers_division";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_id", OracleDbType.Int32, ParameterDirection.Input).Value = id;
                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = name;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void AddNewDirectorDep(string pinCode, int depId) // for director and gen director(the same table)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_director_department";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = pinCode;
                        cmd.Parameters.Add("p_dep_id", OracleDbType.Int32, ParameterDirection.Input).Value = depId;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void UpdateDirectorDep(int mainId, int depId)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_director_dep";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_dep_id", OracleDbType.Int32, ParameterDirection.Input).Value = depId;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public Department GetDirectorDepById(int mainId)
        {
            var department = new Department();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_related_department_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_dep_id", OracleDbType.Int32, ParameterDirection.Output).Size = 10000;
                        cmd.Parameters.Add("p_dep_name", OracleDbType.Varchar2, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        department.Id = Convert.ToInt32(cmd.Parameters["p_dep_id"].Value.ToString());
                        department.Name = cmd.Parameters["p_dep_name"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return department;
        }
        

        public void AddNewShareholderShare(string pinCode, string shares)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_related_persons_share";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = pinCode;
                        cmd.Parameters.Add("p_shares", OracleDbType.Varchar2, ParameterDirection.Input).Value = shares;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void UpdateShareholdersSharesById(int mainId, string shares)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_related_persons_share";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_share", OracleDbType.Varchar2, ParameterDirection.Input).Value = shares;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public ShareholderShare GetShareholderSharesById(int mainId)
        {
            var shareholderShare = new ShareholderShare();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_share_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_share", OracleDbType.Varchar2, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        shareholderShare.Shares = cmd.Parameters["p_share"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return shareholderShare;
        }

        public void AddNewSupervisorPosition(string pinCode, int position)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_supervisor_position";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = pinCode;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Input).Value = position;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void UpdateSupervisorPositionById(int mainId, int position)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_supervisor_position";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Input).Value = position;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public SupervisorPosition GetSupervisorPositionById(int mainId)
        {
            var supervisorPosition = new SupervisorPosition();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_supervisor_position_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        supervisorPosition.Id = Convert.ToInt32(cmd.Parameters["p_position"].Value.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return supervisorPosition;
        }

        public void DeleteGenDirectorId(int id)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.delete_gen_dir_id_for_update";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = id;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public int[] GetGenDirDepsById(int mainId)
        {
            int[] states = null;
            int depId;
            List<int> dirs = new List<int>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_gen_dir_deps_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_dep_ids", OracleDbType.RefCursor, ParameterDirection.Output).Size = 10000;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                 depId = Convert.ToInt32(reader["ID"].ToString());
                                 dirs.Add(depId);
                            }
                            states = dirs.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return states;
        }


        public List<Department> GetCommittees()
        {
            var committees = new List<Department>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_committees";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("v_result", OracleDbType.RefCursor, ParameterDirection.ReturnValue);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Department commettee = new Department();

                                commettee.Id = Convert.ToInt32(reader["ID"].ToString());
                                commettee.Name = reader["Name"].ToString();

                                committees.Add(commettee);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return committees;
        }


        public void UpdateCommittee(int id, string name)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_committee";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_id", OracleDbType.Int32, ParameterDirection.Input).Value = id;
                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = name;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void AddNewCommittee (string name)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_committee";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_name", OracleDbType.Varchar2, ParameterDirection.Input).Value = name;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public Department GetCommitteeById(int comId)
        {
            var committee = new Department();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_committee_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_com_id", OracleDbType.Int32, ParameterDirection.Input).Value = comId;
                        cmd.Parameters.Add("p_com_name", OracleDbType.Varchar2, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        committee.Id = comId;
                        committee.Name = cmd.Parameters["p_com_name"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return committee;
        }


        public List<PersonCommittee> GetPersonCommitteesById(int id)
        {
            var personCommittees = new List<PersonCommittee>();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_person_committees_info";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = id;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor, ParameterDirection.Output);


                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PersonCommittee personCommittee = new PersonCommittee();

                                personCommittee.Id = Convert.ToInt32(reader["main_id"].ToString());
                                personCommittee.ComId = Convert.ToInt32(reader["committee_id"].ToString()); 
                                personCommittee.ComName = reader["name"].ToString(); 
                                personCommittee.Position = Convert.ToInt32(reader["position"].ToString());

                                personCommittees.Add(personCommittee);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return personCommittees;
        }

        public void AddNewPersonCommittee(int mainId, int comId, int position)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_person_committee_info";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_com_id", OracleDbType.Int32, ParameterDirection.Input).Value = comId;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Input).Value = position;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void UpdatePersonCommittee(int mainId, int comId, int position, int oldComId)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_person_committee_info";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_com_id", OracleDbType.Int32, ParameterDirection.Input).Value = comId;
                        cmd.Parameters.Add("p_position", OracleDbType.Int32, ParameterDirection.Input).Value = position;
                        cmd.Parameters.Add("p_old_com_id", OracleDbType.Int32, ParameterDirection.Input).Value = oldComId;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void DeletePersonCommittee(int mainId, int comId)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.delete_person_committee_info";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_com_id", OracleDbType.Int32, ParameterDirection.Input).Value = comId;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void DeleteRelativeConnection(int mainId, int relId, int relType)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.delete_relative_connection";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_rel_id", OracleDbType.Int32, ParameterDirection.Input).Value = relId;
                        cmd.Parameters.Add("p_rel_type", OracleDbType.Int32, ParameterDirection.Input).Value = relType;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public int CheckExistingByPinCode(string pinCode)
        {
            int result = 0;
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.rel_pers_check_by_pin_code";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("v_result", OracleDbType.Int32, ParameterDirection.ReturnValue);
                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = pinCode;

                        cmd.ExecuteNonQuery();
                        result = Convert.ToInt32(cmd.Parameters["v_result"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return result;
        }


        public void AddNewHeadOfDivById(string pinCode, int divİd)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.insert_head_of_div_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_pin_code", OracleDbType.Varchar2, ParameterDirection.Input).Value = pinCode;
                        cmd.Parameters.Add("p_div_id", OracleDbType.Int32, ParameterDirection.Input).Value = divİd;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public void UpdateHeadOfDivByİd(int mainId, int divİd)
        {
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "sbnk_prl.pkg_related_persons.update_head_of_div_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_div_id", OracleDbType.Int32, ParameterDirection.Input).Value = divİd;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
        }

        public Department GetHeadOfDivDepById(int mainId)
        {
            var division = new Department();
            OracleConnection con = null;
            try
            {
                using (con = GetConnection())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = $"sbnk_prl.pkg_related_persons.get_related_division_by_id";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_main_id", OracleDbType.Int32, ParameterDirection.Input).Value = mainId;
                        cmd.Parameters.Add("p_div_id", OracleDbType.Int32, ParameterDirection.Output).Size = 10000;
                        cmd.Parameters.Add("p_div_name", OracleDbType.Varchar2, ParameterDirection.Output).Size = 10000;
                        cmd.ExecuteNonQuery();
                        division.Id = Convert.ToInt32(cmd.Parameters["p_div_id"].Value.ToString());
                        division.Name = cmd.Parameters["p_div_name"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"{ex.Message}");
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                    con.Dispose();

                }
            }
            return division;
        }

    }
}
