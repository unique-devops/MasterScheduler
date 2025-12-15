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
                    LastRunTime = reader["LastRunTime"] != DBNull.Value ? DateTime.Parse(reader["LastRunTime"].ToString()!) : null,
                    NextRunTime = reader["NextRunTime"] != DBNull.Value ? DateTime.Parse(reader["NextRunTime"].ToString()!) : null
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
                    LastRunTime = reader["LastRunTime"] != DBNull.Value ? DateTime.Parse(reader["LastRunTime"].ToString()!) : null,
                    NextRunTime = reader["NextRunTime"] != DBNull.Value ? DateTime.Parse(reader["NextRunTime"].ToString()!) : null
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
                    LastRunTime = reader["LastRunTime"] != DBNull.Value ? DateTime.Parse(reader["LastRunTime"].ToString()!) : null,
                    NextRunTime = reader["NextRunTime"] != DBNull.Value ? DateTime.Parse(reader["NextRunTime"].ToString()!) : null
                };
            }
            return job;
        }

        public void Add(JobModel job)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand("INSERT INTO Jobs (JobName, JobType, CronExpression, NextRunTime, IsActive) VALUES (@name, @type, @cron, @nextRunTime, @active)", con);
            cmd.Parameters.AddWithValue("@name", job.JobName);
            cmd.Parameters.AddWithValue("@type", job.JobType);
            cmd.Parameters.AddWithValue("@cron", job.CronExpression);
            cmd.Parameters.AddWithValue("@nextRunTime", job.NextRunTime);
            cmd.Parameters.AddWithValue("@active", job.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public void Update(JobModel job)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand(@"UPDATE Jobs 
                                          SET JobName=@name, CronExpression=@cron, IsActive=@active 
                                          WHERE Id=@id", con);
            cmd.Parameters.AddWithValue("@name", job.JobName);
            cmd.Parameters.AddWithValue("@cron", job.CronExpression);
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
