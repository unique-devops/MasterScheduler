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
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"])
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
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"])                 
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
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"])
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
                    NextRunTime = string.IsNullOrWhiteSpace(reader["NextRunTime"].ToString()) ? null : Convert.ToDateTime(reader["NextRunTime"])
                };
            }
            return job;
        }

        public void Add(JobModel job)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand("INSERT INTO Jobs (JobName, JobType, CronExpression, NextRunTime, LastRunTime, Status, Message, IsActive) " +
                "VALUES (@name, @type, @cron, @nextRun, @lastRun, @status, @message, @active)", con);
            cmd.Parameters.AddWithValue("@name", job.JobName);
            cmd.Parameters.AddWithValue("@type", job.JobType);
            cmd.Parameters.AddWithValue("@cron", job.CronExpression);
            cmd.Parameters.AddWithValue("@nextRun", job.NextRunTime == null ? "" : job.NextRunTime);
            cmd.Parameters.AddWithValue("@lastRun", job.LastRunTime == null ? "" : job.LastRunTime);
            cmd.Parameters.AddWithValue("@status", job.Status == null ? "" : job.Status);
            cmd.Parameters.AddWithValue("@message", job.Message == null ? "" : job.Message);
            cmd.Parameters.AddWithValue("@active", job.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();
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
    }
}
