using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoApp.Domain.Entities.Tasks;

namespace ToDoApp.Infrastructure.Data.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<BaseTask>
    {
        public void Configure(EntityTypeBuilder<BaseTask> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            // Discriminator для TPH (Table Per Hierarchy)
            builder.HasDiscriminator<string>("TaskType")
                .HasValue<SimpleTask>("Simple")
                .HasValue<WorkTask>("Work")
                .HasValue<RecurringTask>("Recurring");

            builder.Property(t => t.UserId)
                .IsRequired();

            // ✅ Backing field mapping для Title
            builder.Property(t => t.Title)
                .HasField("_title")
                .UsePropertyAccessMode(PropertyAccessMode.Field) // ✅ Додано!
                .IsRequired()
                .HasMaxLength(200);

            // ✅ Backing field mapping для Description
            builder.Property(t => t.Description)
                .HasField("_description")
                .UsePropertyAccessMode(PropertyAccessMode.Field) // ✅ Додано!
                .HasMaxLength(2000);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.DueDate)
                .IsRequired();

            builder.Property(t => t.IsCompleted)
                .IsRequired();

            builder.Property(t => t.Priority)
                .IsRequired()
                .HasConversion<int>();

            // ✅ WorkTask - ProjectName
            builder.Property<string?>("ProjectName") // ✅ Використовуй ім'я property, не backing field!
                .IsRequired(false)
                .HasColumnName("ProjectName")
                .HasMaxLength(200);

            // ✅ RecurringTask - RepeatInterval  
            builder.Property<TimeSpan?>("RepeatInterval") // ✅ Використовуй ім'я property, не backing field!
                .IsRequired(false)
                .HasColumnName("RepeatInterval");

            // ✅ Relationship: Task -> User
            builder.HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Cascade delete

            // Indexes
            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.DueDate);
            builder.HasIndex(t => t.IsCompleted);
        }
    }
}