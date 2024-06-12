using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class RoleAssignmentRepository : IRoleAssignmentRepository
    {
        private readonly IDbConnection _connection;

        public RoleAssignmentRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId)
        {
            try
            {
                List<int> newIds = [];
                foreach (var roleAssignment in request)
                {
                    roleAssignment.Employeeid = EmployeeId;
                    // Check for duplicates
                    string checkSql = @"
                        SELECT COUNT(*) 
                        FROM tblRoleAssignmentMapping 
                        WHERE MenuMasterId = @MenuMasterId 
                        AND Employeeid = @EmployeeId";

                    bool exists = await _connection.ExecuteScalarAsync<bool>(checkSql, new
                    {
                        roleAssignment.MenuMasterId,
                        EmployeeId = roleAssignment.Employeeid
                    });

                    if (!exists)
                    {
                        // Insert if no duplicate
                        string insertSql = @"
                            INSERT INTO tblRoleAssignmentMapping (MenuMasterId, Employeeid)
                            VALUES (@MenuMasterId, @EmployeeId);
                            SELECT CAST(SCOPE_IDENTITY() as int);";

                        var newId = await _connection.QuerySingleAsync<int>(insertSql, new
                        {
                            roleAssignment.MenuMasterId,
                            EmployeeId = roleAssignment.Employeeid
                        });

                        newIds.Add(newId);
                    }
                }
                if (newIds.Count != 0)
                {
                    return new ServiceResponse<string>(true, "Records inserted successfully.", "Operation successful", 200);
                }
                else
                {
                    return new ServiceResponse<string>(true, "Operation Failed.", string.Empty, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public Task<ServiceResponse<List<RoleAssignmentMapping>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request)
        {
            throw new NotImplementedException();
        }
        public async Task<ServiceResponse<List<MenuMasterDTOResponse>>> GetMasterMenu()
        {
            try
            {

                string sql = @"
                SELECT MenuMasterId, MenuName, ParentId
                FROM tblRoleAssiMenuMaster";

                var menuData = await _connection.QueryAsync<RoleAssiMenuMaster>(sql);

                var parentMenus = menuData
                    .Where(m => m.ParentId == 0)
                    .Select(p => new MenuMasterDTOResponse
                    {
                        ParentId = p.MenuMasterId,
                        ParentName = p.MenuName,
                        MenuMasterChildren = menuData
                            .Where(c => c.ParentId == p.MenuMasterId)
                            .Select(c => new MenuMasterChild
                            {
                                ChildId = c.MenuMasterId,
                                ChildName = c.MenuName
                            })
                            .ToList()
                    })
                    .ToList();

                return new ServiceResponse<List<MenuMasterDTOResponse>>(true, "Records Found", parentMenus, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MenuMasterDTOResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<string>> RemoveRoleAssignment(int RAMappingId)
        {
            try
            {
                string checkSql = @"SELECT COUNT(*) FROM tblRoleAssignmentMapping WHERE RAMappingId = @RAMappingId";
                bool exists = await _connection.ExecuteScalarAsync<bool>(checkSql, new { RAMappingId });
                if (exists)
                {
                    string deleteSql = @"DELETE FROM tblRoleAssignmentMapping WHERE RAMappingId = @RAMappingId";

                    await _connection.ExecuteAsync(deleteSql, new { RAMappingId });

                    return new ServiceResponse<string>(true, "Record deleted successfully.", "Operation Successful", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Record not found.", string.Empty, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
