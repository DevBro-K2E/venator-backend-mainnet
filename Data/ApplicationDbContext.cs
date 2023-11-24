using IsometricShooterWebApp.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IsometricShooterWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WalletTransactionModel> WalletTransactions { get; set; }

        public DbSet<WalletBlockModel> WalletBlocks { get; set; }

        public DbSet<UserSessionModel> UserSessions { get; set; }

        public DbSet<GameModel> Games { get; set; }

        public DbSet<GameMemberModel> GameMembers { get; set; }

        public DbSet<UserGameLogModel> GameLogs { get; set; }

        public DbSet<UserReportModel> UserReports { get; set; }

        public DbSet<SeasonModel> Seasons { get; set; }

        public DbSet<SeasonRewardInfoModel> SeasonRewards { get; set; }

        public DbSet<SeasonUserModel> SeasonUsers { get; set; }

        public DbSet<SeasonUserRewardModel> SeasonUserRewards { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserModel>()
                .Property(x => x.CreationDate)
                .HasDefaultValue(new DateTime(2022, 10, 05));

            //builder.Entity<UserModel>().HasMany<UserGameLogModel>()
            //    .WithOne(x => x.User)
            //    .HasForeignKey(x => x.UserId)
            //    .HasPrincipalKey(x=>x.Id);

            //builder.Entity<GameModel>().HasMany<UserGameLogModel>()
            //    .WithOne(x => x.Game)
            //    .HasForeignKey(x => x.GameId)
            //    .HasPrincipalKey(x => x.Id);

            builder.Entity<UserModel>().HasMany<UserReportModel>()
                .WithOne(x => x.Creator)
                .HasForeignKey(x => x.CreatorId);

            builder.Entity<UserModel>().HasMany<UserReportModel>()
                .WithOne(x => x.Target)
                .HasForeignKey(x => x.TargetId);

            builder.Entity<UserGameLogModel>()
                .HasOne(x => x.Game)
                .WithMany(x => x.GameLogs)
                .HasForeignKey(x => x.GameId)
                .HasPrincipalKey(x => x.Id);

            builder.Entity<UserGameLogModel>()
                .HasOne(x => x.User)
                .WithMany(x => x.GameLogs)
                .HasForeignKey(x => x.UserId)
                .HasPrincipalKey(x => x.Id);



            builder.Entity<UserModel>().Ignore(x => x.Money);

            builder.Entity<GameMemberModel>().HasKey(x => new { x.GameId, x.UserId });

            builder.Entity<UserGameLogModel>().HasKey(x => new { x.GameId, x.UserId });

            builder.Entity<SeasonRewardInfoModel>().HasKey(x => new { x.SeasonId, x.EndOffset });

            builder.Entity<SeasonUserModel>().HasKey(x => new { x.SeasonId, x.UserId });

            builder.Entity<SeasonUserRewardModel>().HasKey(x => new { x.SeasonId, x.UserId, x.Date });
        }
    }
}