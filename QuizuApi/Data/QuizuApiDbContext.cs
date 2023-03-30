﻿using Azure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using QuizuApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace QuizuApi.Data
{
    public class QuizuApiDbContext : IdentityDbContext<User>
    {
        public QuizuApiDbContext(DbContextOptions<QuizuApiDbContext> options) : base(options) { }

        public override DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        private void TrackChanges()
        {
            var tracker = ChangeTracker;

            foreach (var entry in tracker.Entries())
            {
                if (entry.Entity is AuditModel)
                {
                    var referenceEntity = entry.Entity as AuditModel;
                    if (referenceEntity is not null)
                    {
                        switch (entry.State)
                        {
                            case EntityState.Added:
                                referenceEntity.CreatedAt = DateTime.Now;
                                break;
                            case EntityState.Modified:
                                referenceEntity.LastModifiedAt = DateTime.Now;
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TrackChanges();

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            TrackChanges();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            TrackChanges();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            TrackChanges();

            return base.SaveChanges();
        }
    }
}