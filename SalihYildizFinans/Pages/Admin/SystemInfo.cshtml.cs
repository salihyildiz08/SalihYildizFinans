using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SalihYildizFinans.Data;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SalihYildizFinans.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SystemInfoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SystemInfoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public DatabaseStats Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                await _context.Database.OpenConnectionAsync();
                var connection = _context.Database.GetDbConnection();

                // 1. Get Numerical Space Info
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            SUM(size * 8.0 / 1024) AS TotalAllocatedMB,
                            SUM(FILEPROPERTY(name, 'SpaceUsed') * 8.0 / 1024) AS UsedMB
                        FROM sys.database_files 
                        WHERE type_desc = 'ROWS'";
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Stats.TotalAllocatedMB = Convert.ToDouble(reader["TotalAllocatedMB"]);
                            Stats.UsedMB = Convert.ToDouble(reader["UsedMB"]);
                            Stats.LimitMB = 1024.0; // User specified limit
                            Stats.RemainingMB = Stats.LimitMB - Stats.UsedMB;
                            Stats.UsagePercentage = (Stats.UsedMB / Stats.LimitMB) * 100.0;
                        }
                    }
                }

                // 2. Get sp_spaceused for traditional labels (Optional but kept for detailed break-down)
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "EXEC sp_spaceused";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Stats.DatabaseName = reader["database_name"]?.ToString();
                        }

                        if (await reader.NextResultAsync() && await reader.ReadAsync())
                        {
                            Stats.Reserved = reader["reserved"]?.ToString();
                            Stats.Data = reader["data"]?.ToString();
                            Stats.IndexSize = reader["index_size"]?.ToString();
                            Stats.Unused = reader["unused"]?.ToString();
                        }
                    }
                }

                // 3. Get Server Info
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT @@VERSION as Version, SERVERPROPERTY('MachineName') as MachineName";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Stats.ServerVersion = reader["Version"]?.ToString();
                            Stats.MachineName = reader["MachineName"]?.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Sistem bilgileri alınırken bir hata oluştu.");
                Console.WriteLine(ex.Message); 
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        public class DatabaseStats
        {
            public string? DatabaseName { get; set; }
            public double TotalAllocatedMB { get; set; }
            public double UsedMB { get; set; }
            public double LimitMB { get; set; }
            public double RemainingMB { get; set; }
            public double UsagePercentage { get; set; }
            
            public string? Reserved { get; set; }
            public string? Data { get; set; }
            public string? IndexSize { get; set; }
            public string? Unused { get; set; }
            public string? ServerVersion { get; set; }
            public string? MachineName { get; set; }
        }
    }
}
