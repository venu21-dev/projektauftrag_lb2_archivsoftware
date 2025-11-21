using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace archivsoftware.DataAccess
{   
        public class ArchiveContext : DbContext
        {
            // DbSets = Tabellen in der Datenbank
            public DbSet<Folder> Folders { get; set; }
            public DbSet<Document> Documents { get; set; }

            // Connection String konfigurieren
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlServer(
                        "Server=OFFICE\\SQLEXPRESS;" +
                        "Database=DocumentArchive;" +
                        "Trusted_Connection=True;" +
                        "TrustServerCertificate=True;" +
                        "MultipleActiveResultSets=true;"
                    );
                }
            }

            // Relationen und Constraints konfigurieren
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // ===== FOLDER KONFIGURATION =====

                modelBuilder.Entity<Folder>(entity =>
                {
                    // Primärschlüssel
                    entity.HasKey(f => f.Id);

                    // Name ist erforderlich, max 255 Zeichen
                    entity.Property(f => f.Name)
                        .IsRequired()
                        .HasMaxLength(255);

                    // CreatedAt ist erforderlich
                    entity.Property(f => f.CreatedAt)
                        .IsRequired();

                    // Selbstreferenzierende Beziehung: Folder → ParentFolder
                    entity.HasOne(f => f.ParentFolder)
                        .WithMany(f => f.SubFolders)
                        .HasForeignKey(f => f.ParentFolderId)
                        .OnDelete(DeleteBehavior.Restrict); // Verhindert Kaskadenl��schung
                });

                // ===== DOCUMENT KONFIGURATION =====

                modelBuilder.Entity<Document>(entity =>
                {
                    // Primärschlüssel
                    entity.HasKey(d => d.Id);

                    // FileName ist erforderlich
                    entity.Property(d => d.FileName)
                        .IsRequired()
                        .HasMaxLength(255);

                    // FileType ist erforderlich
                    entity.Property(d => d.FileType)
                        .IsRequired()
                        .HasMaxLength(10);

                    // FileData (BLOB) ist erforderlich
                    entity.Property(d => d.FileData)
                        .IsRequired();

                    // PlainText für Volltextsuche (optional, kann leer sein)
                    entity.Property(d => d.PlainText)
                        .HasMaxLength(int.MaxValue); // Unbegrenzte Länge

                    // FileSize ist erforderlich
                    entity.Property(d => d.FileSize)
                        .IsRequired();

                    // ImportedAt ist erforderlich
                    entity.Property(d => d.ImportedAt)
                        .IsRequired();

                    // Beziehung: Document → Folder (1:n)
                    entity.HasOne(d => d.Folder)
                        .WithMany(f => f.Documents)
                        .HasForeignKey(d => d.FolderId)
                        .OnDelete(DeleteBehavior.Cascade); // Dokumente werden gelöscht wenn Folder gelöscht wird
                });
            }
        }
 }