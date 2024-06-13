using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
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
        //public async Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId)
        //{
        //    try
        //    {
        //        List<int> newIds = [];
        //        foreach (var roleAssignment in request)
        //        {
        //            roleAssignment.Employeeid = EmployeeId;
        //            // Check for duplicates
        //            string checkSql = @"
        //                SELECT COUNT(*) 
        //                FROM tblRoleAssignmentMapping 
        //                WHERE MenuMasterId = @MenuMasterId 
        //                AND Employeeid = @EmployeeId";

        //            bool exists = await _connection.ExecuteScalarAsync<bool>(checkSql, new
        //            {
        //                roleAssignment.MenuMasterId,
        //                EmployeeId = roleAssignment.Employeeid
        //            });

        //            if (!exists)
        //            {
        //                // Insert if no duplicate
        //                string insertSql = @"
        //                    INSERT INTO tblRoleAssignmentMapping (MenuMasterId, Employeeid)
        //                    VALUES (@MenuMasterId, @EmployeeId);
        //                    SELECT CAST(SCOPE_IDENTITY() as int);";

        //                var newId = await _connection.QuerySingleAsync<int>(insertSql, new
        //                {
        //                    roleAssignment.MenuMasterId,
        //                    EmployeeId = roleAssignment.Employeeid
        //                });

        //                newIds.Add(newId);
        //            }
        //        }
        //        if (newIds.Count != 0)
        //        {
        //            return new ServiceResponse<string>(true, "Records inserted successfully.", "Operation successful", 200);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<string>(true, "Operation Failed.", string.Empty, 200);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        //    }
        //}
        public async Task<ServiceResponse<List<RoleAssignmentResponse>>> GetListOfRoleAssignment(GetListOfRoleAssignmentRequest request)
        {
            try
            {
                string query = @"
                SELECT 
                    e.Employeeid
                FROM 
                    tblEmployee e
                WHERE 
                    e.RoleID = @RoleId AND e.DesignationID = @DesignationId";

                var employees = await _connection.QueryAsync<RoleAssignmentResponse>(query, new { request.RoleId, request.DesignationId });

                foreach (var employee in employees)
                {
                    string fetchRoleAssignmentsQuery = @"
                    SELECT 
                        ram.RAMappingId,
                        ram.MenuMasterId,
                        mm.MenuName AS MenuMasterName
                    FROM 
                        tblRoleAssignmentMapping ram
                    INNER JOIN 
                        tblRoleAssiMenuMaster mm ON ram.MenuMasterId = mm.MenuMasterId
                    WHERE 
                        ram.Employeeid = @EmployeeId";

                    var roleAssignments = await _connection.QueryAsync<RoleAssignmentMappingResponse>(fetchRoleAssignmentsQuery, new { EmployeeId = employee.Employeeid });
                    employee.RoleAssignmentMappings = roleAssignments.ToList();
                }

                if (employees.Any())
                {
                    return new ServiceResponse<List<RoleAssignmentResponse>>(true, "Records Found", employees.ToList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<RoleAssignmentResponse>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<RoleAssignmentResponse>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
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
        public async Task<ServiceResponse<string>> RemoveRoleAssignment(int menuMasterId)
        {
            try
            {
                // Fetch the list of MenuMasterIds to delete (including children if parent is provided)
                List<int> menuMasterIdsToDelete = new List<int> { menuMasterId };

                // Query to fetch child MenuMasterIds if the provided ID is a parent
                string fetchChildrenSql = @"
            SELECT MenuMasterId
            FROM tblRoleAssiMenuMaster
            WHERE ParentId = @ParentId";

                var childMenuMasterIds = await _connection.QueryAsync<int>(fetchChildrenSql, new { ParentId = menuMasterId });

                if (childMenuMasterIds != null && childMenuMasterIds.Any())
                {
                    menuMasterIdsToDelete.AddRange(childMenuMasterIds);
                }

                // Check if any role assignment mappings exist for these MenuMasterIds
                string checkSql = @"
            SELECT COUNT(*)
            FROM tblRoleAssignmentMapping
            WHERE MenuMasterId IN @MenuMasterIds";

                bool exists = await _connection.ExecuteScalarAsync<bool>(checkSql, new { MenuMasterIds = menuMasterIdsToDelete });

                if (exists)
                {
                    // Delete the role assignment mappings
                    string deleteSql = @"
                DELETE FROM tblRoleAssignmentMapping
                WHERE MenuMasterId IN @MenuMasterIds";

                    await _connection.ExecuteAsync(deleteSql, new { MenuMasterIds = menuMasterIdsToDelete });

                    return new ServiceResponse<string>(true, "Records deleted successfully.", "Operation Successful", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Records not found.", string.Empty, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<RoleAssignmentResponse>> GetRoleAssignmentById(int EmployeeId)
        {
            try
            {
                // Check if the employee exists
                string checkEmployeeQuery = "SELECT COUNT(1) FROM tblEmployee WHERE Employeeid = @EmployeeId";
                var employeeExists = await _connection.ExecuteScalarAsync<bool>(checkEmployeeQuery, new { EmployeeId });

                if (!employeeExists)
                {
                    return new ServiceResponse<RoleAssignmentResponse>(false, "Employee not found", new RoleAssignmentResponse(), 404);
                }

                // Fetch role assignments
                string fetchRoleAssignmentsQuery = @"
        SELECT 
            ram.RAMappingId,
            ram.MenuMasterId,
            mm.MenuName AS MenuMasterName
        FROM 
            tblRoleAssignmentMapping ram
        INNER JOIN 
            [tblRoleAssiMenuMaster] mm ON ram.MenuMasterId = mm.MenuMasterId
        WHERE 
            ram.Employeeid = @EmployeeId";

                var roleAssignments = await _connection.QueryAsync<RoleAssignmentMappingResponse>(fetchRoleAssignmentsQuery, new { EmployeeId });

                var response = new RoleAssignmentResponse
                {
                    Employeeid = EmployeeId,
                    RoleAssignmentMappings = roleAssignments.ToList()
                };

                return new ServiceResponse<RoleAssignmentResponse>(true, "Records Found", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RoleAssignmentResponse>(false, ex.Message, new RoleAssignmentResponse(), 500);
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateRoleAssignment(List<RoleAssignmentMapping> request, int EmployeeId)
        {
            try
            {
                List<int> newIds = [];
                foreach (var roleAssignment in request)
                {
                    roleAssignment.Employeeid = EmployeeId;

                    // Get all relevant MenuMasterIds (children if parent, otherwise just the ID)
                    var menuMasterIds = await GetMenuMasterIds(roleAssignment.MenuMasterId);

                    foreach (var menuMasterId in menuMasterIds)
                    {
                        // Check for duplicates
                        string checkSql = @"
                    SELECT COUNT(*) 
                    FROM tblRoleAssignmentMapping 
                    WHERE MenuMasterId = @MenuMasterId 
                    AND Employeeid = @EmployeeId";

                        bool exists = await _connection.ExecuteScalarAsync<bool>(checkSql, new
                        {
                            MenuMasterId = menuMasterId,
                            EmployeeId = roleAssignment.Employeeid
                        });

                        if (!exists)
                        {
                            if (menuMasterId == 0)
                            {
                                // Insert if no duplicate
                                string insertSql = @"
                        INSERT INTO tblRoleAssignmentMapping (MenuMasterId, Employeeid)
                        VALUES (@MenuMasterId, @EmployeeId);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                                var newId = await _connection.QuerySingleAsync<int>(insertSql, new
                                {
                                    MenuMasterId = menuMasterId,
                                    EmployeeId = roleAssignment.Employeeid
                                });

                                newIds.Add(newId);
                            }
                            else
                            {
                                string updateSql = @"
                                UPDATE tblRoleAssignmentMapping
                                SET MenuMasterId = @MenuMasterId,
                                    Employeeid = @EmployeeId
                                WHERE RAMappingId = @RAMappingId;";

                                await _connection.ExecuteAsync(updateSql, new
                                {
                                    MenuMasterId = menuMasterId,
                                    EmployeeId = roleAssignment.Employeeid,
                                    roleAssignment.RAMappingId
                                });

                                newIds.Add(roleAssignment.RAMappingId);
                            }
                        }
                    }
                }

                if (newIds.Count > 0)
                {
                    return new ServiceResponse<string>(true, "Records inserted successfully.", "Operation successful", 200);
                }
                else
                {
                    return new ServiceResponse<string>(true, "Operation failed. No new records were inserted.", string.Empty, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        private async Task<List<int>> GetMenuMasterIds(int menuMasterId)
        {
            // Get all children MenuMasterIds if the given ID is a parent
            string query = @"
        SELECT MenuMasterId 
        FROM tblRoleAssiMenuMaster 
        WHERE ParentId = @ParentId";

            var childrenIds = await _connection.QueryAsync<int>(query, new { ParentId = menuMasterId });

            // If there are no children, return the single MenuMasterId
            if (!childrenIds.Any())
            {
                return [menuMasterId];
            }

            // Otherwise, return the list of children MenuMasterIds
            return childrenIds.ToList();
        }
    }
}
