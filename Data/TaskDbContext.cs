using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TaskManagerApi.Models;

namespace TaskManagerApi.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
    }
}