﻿using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class TicketRepository : ITicketRepository
    {
        private readonly IDbConnection _connection;

        public TicketRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddTicket(Ticket request)
        {
            try
            {
                int rowsAffected = await _connection.ExecuteAsync(
                    @"INSERT INTO tblTicket (TicketID, Boardname, ClassName, CourseName, DateAndTime, MobileNumber, QueryInfo, QueryType, Status, SubjectName, TicketNo) 
              VALUES (@TicketID, @Boardname, @ClassName, @CourseName, @DateAndTime, @MobileNumber, @QueryInfo, @QueryType, @Status, @SubjectName, @TicketNo)",
                    request);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Ticket Added Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Ticket>>> GetAllTicketsList(GeAllTicketsRequest request)
        {
            try
            {
                var query = @"SELECT TicketID, Boardname, ClassName, CourseName, DateAndTime, MobileNumber, QueryInfo, QueryType, Status, SubjectName, TicketNo
                      FROM tblTicket
                      WHERE (Boardname = @Boardname OR @Boardname IS NULL)
                            AND (CourseName = @CourseName OR @CourseName IS NULL)
                            AND (ClassName = @ClassName OR @ClassName IS NULL)
                            AND (DateAndTime = @Today OR @Today IS NULL)
                            AND (DateAndTime >= @StartDate AND DateAndTime <= @EndDate)
                      ORDER BY DateAndTime DESC";

                var tickets = await _connection.QueryAsync<Ticket>(query, request);
                if (tickets != null)
                {
                    return new ServiceResponse<List<Ticket>>(true, "Records Found", tickets.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Ticket>>(false, "Records Not Found", new List<Ticket>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Ticket>>(false, ex.Message, new List<Ticket>(), 500);
            }
        }
    }
}