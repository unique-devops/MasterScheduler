using MasterScheduler.Shared.DataModels;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Data
{
    public class JobRepository
    {
        public List<JobModel> GetPendingTask()
        {
            var list = new List<JobModel>();
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            string sql = @"
                    SELECT *
                    FROM Jobs
                    WHERE IsActive = 1                     
                      AND NextRunTime IS NOT NULL
                      AND datetime(NextRunTime) <= datetime('now','localtime')
                    ORDER BY datetime(NextRunTime) ASC;
                    ";
            using var cmd = new SqliteCommand(sql, con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new JobModel
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    JobName = reader["JobName"].ToString()!,
                    JobType = reader["JobType"].ToString()!,
                    CronExpression = reader["CronExpression"].ToString()!,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    Status = reader["Status"].ToString(),
                    Message = reader["Message"].ToString(),
                    LastRunTime = string.IsNullOrWhiteSpace(reader["LastRunTime"].ToString()) ? null : Convert.ToDateTime(reader["LastRunTime"]),
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"]),
                    CreatedAt = string.IsNullOrWhiteSpace(reader["CreatedAt"].ToString()) ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = string.IsNullOrWhiteSpace(reader["UpdatedAt"].ToString()) ? null : Convert.ToDateTime(reader["UpdatedAt"])
                });
            }
            return list;
        }
        public List<JobModel> GetAll()
        {
            var list = new List<JobModel>();
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Jobs", con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new JobModel
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    JobName = reader["JobName"].ToString()!,
                    JobType = reader["JobType"].ToString()!,
                    CronExpression = reader["CronExpression"].ToString()!,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    Status = reader["Status"].ToString(),
                    Message = reader["Message"].ToString(),
                    LastRunTime = string.IsNullOrWhiteSpace(reader["LastRunTime"].ToString()) ? null : Convert.ToDateTime(reader["LastRunTime"]),
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"]),
                    CreatedAt = string.IsNullOrWhiteSpace(reader["CreatedAt"].ToString()) ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = string.IsNullOrWhiteSpace(reader["UpdatedAt"].ToString()) ? null : Convert.ToDateTime(reader["UpdatedAt"])
                });
            }
            return list;
        }
        public List<JobModel> GetOrderById(int LastId = 0)
        {
            var list = new List<JobModel>();
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Jobs WHERE Id > " + LastId + " ORDER BY Id ASC", con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new JobModel
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    JobName = reader["JobName"].ToString()!,
                    JobType = reader["JobType"].ToString()!,
                    CronExpression = reader["CronExpression"].ToString()!,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    Status = reader["Status"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    LastRunTime = string.IsNullOrWhiteSpace(reader["LastRunTime"].ToString()) ? null : Convert.ToDateTime(reader["LastRunTime"]),
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"]),
                    CreatedAt = string.IsNullOrWhiteSpace(reader["CreatedAt"].ToString()) ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = string.IsNullOrWhiteSpace(reader["UpdatedAt"].ToString()) ? null : Convert.ToDateTime(reader["UpdatedAt"])
                });
            }
            return list;
        }

        public JobModel? GetById(int JobId)
        {
            JobModel job = null;
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            using var cmd = new SqliteCommand("SELECT * FROM Jobs where id = @JobId", con);
            cmd.Parameters.AddWithValue("@JobId", JobId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                job = new JobModel
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    JobName = reader["JobName"].ToString()!,
                    JobType = reader["JobType"].ToString()!,
                    CronExpression = reader["CronExpression"].ToString()!,
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    Status = reader["Status"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    LastRunTime = string.IsNullOrWhiteSpace(reader["LastRunTime"].ToString()) ? null : Convert.ToDateTime(reader["LastRunTime"]),
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"]),
                    CreatedAt = string.IsNullOrWhiteSpace(reader["CreatedAt"].ToString()) ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = string.IsNullOrWhiteSpace(reader["UpdatedAt"].ToString()) ? null : Convert.ToDateTime(reader["UpdatedAt"])
                };
            }
            return job;
        }

        public int Add(JobModel job)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand("INSERT INTO Jobs (JobName, JobType, CronExpression, NextRunTime, LastRunTime, Status, Message, IsActive) " +
                " VALUES (@name, @type, @cron, @nextRun, @lastRun, @status, @message, @active);" +
                " SELECT last_insert_rowid();", con);
            cmd.Parameters.AddWithValue("@name", job.JobName);
            cmd.Parameters.AddWithValue("@type", job.JobType);
            cmd.Parameters.AddWithValue("@cron", job.CronExpression);
            cmd.Parameters.AddWithValue("@nextRun", job.NextRunTime == null ? "" : job.NextRunTime);
            cmd.Parameters.AddWithValue("@lastRun", job.LastRunTime == null ? "" : job.LastRunTime);
            cmd.Parameters.AddWithValue("@status", job.Status == null ? "" : job.Status);
            cmd.Parameters.AddWithValue("@message", job.Message == null ? "" : job.Message);
            cmd.Parameters.AddWithValue("@active", job.IsActive ? 1 : 0);
            job.Id = Convert.ToInt32(cmd.ExecuteScalar());
            //cmd.ExecuteNonQuery();
            return job.Id;
        }

        public void Update(JobModel job)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand(@"UPDATE Jobs SET JobName=@name, CronExpression=@cron,
                      NextRunTime =@nextRun, LastRunTime =@lastRun, Status =@status, Message =@message,
                      IsActive=@active WHERE Id=@id", con);
            cmd.Parameters.AddWithValue("@name", job.JobName);
            cmd.Parameters.AddWithValue("@cron", job.CronExpression);
            cmd.Parameters.AddWithValue("@nextRun", job.NextRunTime == null ? "" : job.NextRunTime);
            cmd.Parameters.AddWithValue("@lastRun", job.LastRunTime == null ? "" : job.LastRunTime);
            cmd.Parameters.AddWithValue("@status", job.Status == null ? "" : job.Status);
            cmd.Parameters.AddWithValue("@message", job.Message == null ? "" : job.Message);
            cmd.Parameters.AddWithValue("@active", job.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", job.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand("DELETE FROM Jobs WHERE Id=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }


        //----------------jobDetail-----------------

        public JobDetailModel? GetDetailById(int JobId)
        {
            JobDetailModel jobDetails = null;
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            using var cmd = new SqliteCommand("SELECT * FROM JobDetails where JobId = @JobId", con);
            cmd.Parameters.AddWithValue("@JobId", JobId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                jobDetails = new JobDetailModel
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    JobId = Convert.ToInt32(reader["JobId"]),
                    Details = reader["Details"].ToString()!,
                    CreatedAt = string.IsNullOrWhiteSpace(reader["CreatedAt"].ToString()) ? null : Convert.ToDateTime(reader["CreatedAt"]),
                    UpdatedAt = string.IsNullOrWhiteSpace(reader["UpdatedAt"].ToString()) ? null : Convert.ToDateTime(reader["UpdatedAt"])
                };
            }
            return jobDetails;
        }

        public void AddUpdateJobDetail(JobDetailModel jobDetail)
        {
            var exist = GetDetailById(jobDetail.Id);
            if (exist == null)
            {
                using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
                con.Open();
                var cmd = new SqliteCommand("INSERT INTO JobDetails (JobId, Details) " +
                    "VALUES (@jobId, @details)", con);
                cmd.Parameters.AddWithValue("@jobId", jobDetail.JobId);
                cmd.Parameters.AddWithValue("@details", jobDetail.Details ?? "");
                cmd.ExecuteNonQuery();
            }
            else {

                using var con1 = new SqliteConnection(DatabaseHelper.ConnectionString);
                con1.Open();
                var cmd1 = new SqliteCommand(@"UPDATE JobDetails SET Details=@details, UpdatedAt=@updatedAt WHERE JobId=@jobId", con1);
                cmd1.Parameters.AddWithValue("@details", jobDetail.Details ?? "");
                cmd1.Parameters.AddWithValue("@updatedAt", DateTime.Now);
                cmd1.Parameters.AddWithValue("@jobId", jobDetail.JobId);                
                cmd1.ExecuteNonQuery();
            }                
        }

       
    }
}
