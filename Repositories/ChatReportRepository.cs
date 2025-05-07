namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using Src.Data;
    using Src.Model;
    using System.Threading.Tasks;

    public class ChatReportRepository : IChatReportRepository
    {
        private readonly DatabaseConnection dbConnection;
        private readonly IActivityRepository _activityRepository;

        public ChatReportRepository(DatabaseConnection dbConnection, IActivityRepository activityRepository)
        {
            this.dbConnection = dbConnection;
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
        }

        public int GetNumberOfGivenTipsForUser(string reportedUserCnp)
        {
            SqlParameter[] tipsParameters = new SqlParameter[]
            {
                 new SqlParameter("@UserCnp", reportedUserCnp)
            };
            const string GetQuery = "SET NOCOUNT ON; SELECT COUNT(*) AS NumberOfTips FROM GivenTips WHERE UserCnp = @UserCnp;";
            int countTips = this.dbConnection.ExecuteScalar<int>(GetQuery, tipsParameters, CommandType.Text);
            return countTips;
        }

        public async Task UpdateActivityLog(string reportedUserCnp, int amount)
        {
            await _activityRepository.AddActivityAsync(
                reportedUserCnp,
                "Chat",
                amount,
                "Chat abuse");
        }

        public List<ChatReport> GetChatReports()
        {
            try
            {
                const string SelectQuery = @"
                    SELECT Id, ReportedUserCnp, ReportedMessage 
                    FROM ChatReports";

                DataTable? chatReportsDataTable = this.dbConnection.ExecuteReader(SelectQuery, null, CommandType.Text);

                if (chatReportsDataTable == null)
                {
                    return new List<ChatReport>();
                }

                List<ChatReport> chatReports = new List<ChatReport>();

                foreach (DataRow row in chatReportsDataTable.Rows)
                {
                    chatReports.Add(new ChatReport
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        ReportedUserCnp = row["ReportedUserCnp"].ToString() ?? string.Empty,
                        ReportedMessage = row["ReportedMessage"].ToString() ?? string.Empty,
                    });
                }

                return chatReports;
            }
            catch (Exception exception)
            {
                throw new Exception("Error retrieving chat reports", exception);
            }
        }

        public void DeleteChatReport(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid report ID");
            }

            try
            {
                const string DeleteQuery = "DELETE FROM ChatReports WHERE Id = @Id";

                SqlParameter[] deleteParameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", id)
                };

                int rowsAffected = this.dbConnection.ExecuteNonQuery(DeleteQuery, deleteParameters, CommandType.Text);

                if (rowsAffected == 0)
                {
                    throw new Exception($"No chat report found with Id {id}");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error deleting chat report: {exception.Message}", exception);
            }
        }

        public void UpdateScoreHistoryForUser(string userCNP, int newScore)
        {
            try
            {
                const string UpdateScoreHistoryQuery = @"
            IF EXISTS (SELECT 1 FROM CreditScoreHistory WHERE UserCNP = @UserCnp AND Date = CAST(GETDATE() AS DATE))
            BEGIN
                UPDATE CreditScoreHistory
                SET Score = @NewScore
                WHERE UserCnp = @UserCnp AND Date = CAST(GETDATE() AS DATE);
            END
            ELSE
            BEGIN
                INSERT INTO CreditScoreHistory (UserCnp, Date, Score)
                VALUES (@UserCnp, CAST(GETDATE() AS DATE), @NewScore);
            END";

                SqlParameter[] scoreHistoryParameters = new SqlParameter[]
                {
            new SqlParameter("@UserCnp", SqlDbType.VarChar, 16) { Value = userCNP },
            new SqlParameter("@NewScore", SqlDbType.Int) { Value = newScore }
                };

                int rowsAffected = this.dbConnection.ExecuteNonQuery(UpdateScoreHistoryQuery, scoreHistoryParameters, CommandType.Text);

                if (rowsAffected == 0)
                {
                    throw new Exception("No changes were made to the CreditScoreHistory.");
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Error updating credit score history: {exception.Message}", exception);
            }
        }
    }
}