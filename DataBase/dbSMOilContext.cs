using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Xml;
using System.Windows.Forms;
#nullable disable

namespace SM_Oil
{
    public partial class dbSMOilContext : DbContext
    {
        public dbSMOilContext()
        {
        }

        public dbSMOilContext(DbContextOptions<dbSMOilContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Blending> Blendings { get; set; }
        public virtual DbSet<BlendingRecipe> BlendingRecipes { get; set; }
        public virtual DbSet<Crude> Crudes { get; set; }
        public virtual DbSet<Cut> Cuts { get; set; }
        public virtual DbSet<CutSet> CutSets { get; set; }
        public virtual DbSet<CutSetType> CutSetTypes { get; set; }
        public virtual DbSet<CutType> CutTypes { get; set; }
        public virtual DbSet<Folder> Folders { get; set; }
        public virtual DbSet<Library> Libraries { get; set; }
        public virtual DbSet<Property> Properties { get; set; }
        public virtual DbSet<PropertyGroup> PropertyGroups { get; set; }
        public virtual DbSet<PropertyType> PropertyTypes { get; set; }
        public virtual DbSet<SelectionAnalogue> SelectionAnalogues { get; set; }
        public virtual DbSet<SelectionAnaloguesProperty> SelectionAnaloguesProperties { get; set; }
        public virtual DbSet<SelectionAnaloguesRecipe> SelectionAnaloguesRecipes { get; set; }
        public virtual DbSet<Uom> Uoms { get; set; }
        public virtual DbSet<Uomset> Uomsets { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                XmlDocument xDoc = new XmlDocument();
                string[] xPathes =new string[2];

                //string xPath = AppContext.BaseDirectory + "..\\..\\..\\App.config";
                //xPathes[0] = xPath;
                //xPath = AppContext.BaseDirectory + "SM-Oil.dll.config";
                //xPathes[1] = xPath;
                //foreach(string path in xPathes)
                //{
                //    try
                //    {
                //        xDoc.Load(path);
                //        xPath = path;
                //        break;
                //    }
                //    catch
                //    {
                //        continue;
                //    }
                //}


 ////               string xPath = AppContext.BaseDirectory + "SM-Oil.dll.config";
 ////               xDoc.Load(xPath);

 //               XmlElement xRoot = xDoc.DocumentElement;
 //               if (xRoot == null)
 //               {
 //                   DialogResult result = MessageBox.Show("Отсутствует файл конфигурации бд", "Ошибка", MessageBoxButtons.OK); 
 //                   if (result == DialogResult.OK)
 //                   {
 //                       Environment.Exit(0);

 //                   }
 //               }
 //               XmlNode connectionString = xRoot.SelectSingleNode("connectionStrings").FirstChild;
 //               string conStr = connectionString.SelectSingleNode("Value").InnerText;
 //               optionsBuilder.UseNpgsql(conStr);
                  optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=dbSMOil;Username=postgres;Password=postgres;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blending>(entity =>
            {
                entity.Property(e => e.BlendingId)
                    .HasColumnName("BlendingID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Description).HasColumnType("character varying");

                entity.Property(e => e.ResultInfo).HasColumnType("character varying");
            });

            modelBuilder.Entity<BlendingRecipe>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("BlendingRecipe");

                entity.Property(e => e.BlendingId).HasColumnName("BlendingID");
            });

            modelBuilder.Entity<Crude>(entity =>
            {
                entity.Property(e => e.CrudeId)
                    .HasColumnName("CrudeID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ChangeDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ChangeUser).HasDefaultValueSql("1");

                entity.Property(e => e.CreateDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.FolderId).HasColumnName("FolderID");

                entity.Property(e => e.LibraryId).HasColumnName("LibraryID").HasDefaultValueSql("1"); 

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Owner).HasDefaultValueSql("1");

                entity.Property(e => e.Sourse).HasMaxLength(100);
            });

            modelBuilder.Entity<Cut>(entity =>
            {
                

                entity.Property(e => e.CutId).HasColumnName("CutID").UseIdentityAlwaysColumn();

                entity.Property(e => e.CutSetId).HasColumnName("CutSetID");

                entity.Property(e => e.Description).HasColumnType("character varying");

                entity.Property(e => e.Name).HasColumnType("character varying");
            });

            modelBuilder.Entity<CutSet>(entity =>
            {
                entity.Property(e => e.CutSetId)
                    .HasColumnName("CutSetID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CrudeId).HasColumnName("CrudeID");

                entity.Property(e => e.Description).HasColumnType("character varying");
                entity.Property(e => e.Name).HasColumnType("character varying");

                entity.Property(e => e.isMainCutSet).HasColumnName("isMainCutSet").HasColumnType("boolean");

            });

            modelBuilder.Entity<CutSetType>(entity =>
            {
                entity.ToTable("CutSetType");

                entity.Property(e => e.CutSetTypeId)
                    .HasColumnName("CutSetTypeID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Name).HasColumnType("character varying");

                entity.Property(e => e.Uomtemperature)
                    .HasColumnType("character varying")
                    .HasColumnName("UOMTemperature");

                entity.Property(e => e.UOM)
                    .HasColumnName("UOM");
            });

            modelBuilder.Entity<CutType>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("CutType");

                entity.Property(e => e.CutSetType).HasColumnType("character varying");

                entity.Property(e => e.CutTypeId).HasColumnName("CutTypeID");

                entity.Property(e => e.Fvt).HasColumnName("FVT");

                entity.Property(e => e.Ivt).HasColumnName("IVT");

                entity.Property(e => e.Name).HasColumnType("character varying");
            });

            modelBuilder.Entity<Folder>(entity =>
            {
                entity.Property(e => e.FolderId)
                    .HasColumnName("FolderID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.ChangeDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ChangeUser).HasDefaultValueSql("1");

                entity.Property(e => e.CreateDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Owner).HasDefaultValueSql("1");
            });

            modelBuilder.Entity<Library>(entity =>
            {
                entity.Property(e => e.LibraryId)
                    .ValueGeneratedNever()
                    .HasColumnName("LibraryID");

                entity.Property(e => e.ChangeDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ChangeUser).HasDefaultValueSql("1");

                entity.Property(e => e.CreateDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Owner).HasDefaultValueSql("1");
            });

            modelBuilder.Entity<Property>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.CutId).HasColumnName("CutID");

                entity.Property(e => e.PropertyTypeId).HasColumnName("PropertyTypeID");

                entity.Property(e => e.Uom).HasColumnName("UOM");
                entity.Property(e => e.CutName).HasColumnName("CutName");
            });

            modelBuilder.Entity<PropertyGroup>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("PropertyGroup");

                entity.Property(e => e.Name).HasColumnType("character varying");

                entity.Property(e => e.PropertyGroupId).HasColumnName("PropertyGroupID");

                entity.Property(e => e.TextCode).HasColumnType("character varying");

                entity.Property(e => e.Uomset)
                    .HasColumnType("character varying")
                    .HasColumnName("UOMSet");
            });

            modelBuilder.Entity<PropertyType>(entity =>
            {
                entity.ToTable("PropertyType");

                entity.Property(e => e.PropertyTypeId)
                    .HasColumnName("PropertyTypeID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.IsIndexProperty).HasDefaultValueSql("false");

                entity.Property(e => e.Name).HasColumnType("character varying");

                entity.Property(e => e.PropertyGroupId).HasColumnName("PropertyGroupID");
            });

            modelBuilder.Entity<SelectionAnalogue>(entity =>
            {
                entity.HasKey(e => e.SelectionAnaloguesId)
                    .HasName("SelectionAnalogues_pkey");

                entity.Property(e => e.SelectionAnaloguesId)
                    .HasColumnName("SelectionAnaloguesID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Description).HasColumnType("character varying");

                entity.Property(e => e.ResultInfo).HasColumnType("character varying");
            });

            modelBuilder.Entity<SelectionAnaloguesProperty>(entity =>
            {
                entity.ToTable("SelectionAnaloguesProperty");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Uom).HasColumnName("UOM");
            });

            modelBuilder.Entity<SelectionAnaloguesRecipe>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("SelectionAnaloguesRecipe ");
            });

            modelBuilder.Entity<Uom>(entity =>
            {
                entity.ToTable("UOM");

                entity.Property(e => e.Uomid)
                    .ValueGeneratedNever()
                    .HasColumnName("UOMID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Uomset)
                    .HasColumnType("character varying")
                    .HasColumnName("UOMSet");
            });

            modelBuilder.Entity<Uomset>(entity =>
            {
                entity.ToTable("UOMSet");

                entity.Property(e => e.UomsetId)
                    .ValueGeneratedNever()
                    .HasColumnName("UOMSetID");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.TextCode).HasMaxLength(100);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId)
                    .HasColumnName("UserID")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("character varying");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
