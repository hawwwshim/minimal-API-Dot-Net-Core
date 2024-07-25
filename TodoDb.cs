using Microsoft.EntityFrameworkCore;
using TodoApi;

public class TodoDb : DbContext
{
    public DbSet<Customer> Todos { get; set; }

    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options)
    {
    }
}