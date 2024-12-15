using Microsoft.EntityFrameworkCore;
using TranscriptionsProcessor.Entities;

namespace TranscriptionsProcessor
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ActionItem> ActionItems { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Summary> Summaries { get; set; }
        public DbSet<Transcription> Transcriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ActionItem configuration
            modelBuilder.Entity<ActionItem>()
                .HasKey(ai => ai.Id);

            modelBuilder.Entity<ActionItem>()
                .HasOne(ai => ai.Meeting)
                .WithMany(m => m.ActionItems)
                .HasForeignKey(ai => ai.MeetingId);

            modelBuilder.Entity<ActionItem>()
                .HasOne(ai => ai.Participant)
                .WithMany()
                .HasForeignKey(ai => ai.ParticipantId);

            // IdempotencyKey configuration
            modelBuilder.Entity<IdempotencyKey>()
                .HasKey(ik => ik.Key);

            modelBuilder.Entity<IdempotencyKey>()
                .HasOne(ik => ik.Meeting)
                .WithMany()
                .HasForeignKey(ik => ik.MeetingId);

            // Meeting configuration
            modelBuilder.Entity<Meeting>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Meeting>()
                .HasOne(m => m.Summary)
                .WithOne(s => s.Meeting)
                .HasForeignKey<Summary>(s => s.MeetingId);

            // Participant configuration
            modelBuilder.Entity<Participant>()
                .HasKey(p => p.ParticipantId);

            // Summary configuration
            modelBuilder.Entity<Summary>()
                .HasKey(s => s.Id);

            // Transcription configuration
            modelBuilder.Entity<Transcription>()
                .HasKey(t => t.MessageId);

            modelBuilder.Entity<Transcription>()
                .HasOne(t => t.Meeting)
                .WithMany(m => m.Transcriptions)
                .HasForeignKey(t => t.MeetingId);

            modelBuilder.Entity<Transcription>()
                .HasOne(t => t.Participant)
                .WithMany()
                .HasForeignKey(t => t.ParticipantId);
        }
    }

}
