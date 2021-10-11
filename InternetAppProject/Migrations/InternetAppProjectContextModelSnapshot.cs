﻿// <auto-generated />
using System;
using InternetAppProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InternetAppProject.Migrations
{
    [DbContext(typeof(InternetAppProjectContext))]
    partial class InternetAppProjectContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ImageTag", b =>
                {
                    b.Property<int>("ImagesId")
                        .HasColumnType("int");

                    b.Property<int>("TagsId")
                        .HasColumnType("int");

                    b.HasKey("ImagesId", "TagsId");

                    b.HasIndex("TagsId");

                    b.ToTable("ImageTag");
                });

            modelBuilder.Entity("InternetAppProject.Models.Drive", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Current_usage")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("TypeIdId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TypeIdId");

                    b.ToTable("Drive");
                });

            modelBuilder.Entity("InternetAppProject.Models.DriveType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Last_change")
                        .HasColumnType("datetime2");

                    b.Property<int>("Max_Capacity")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("DriveType");
                });

            modelBuilder.Entity("InternetAppProject.Models.Image", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("DIdId")
                        .HasColumnType("int");

                    b.Property<byte[]>("Data")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EditTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<DateTime>("UploadTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DIdId");

                    b.ToTable("Image");
                });

            modelBuilder.Entity("InternetAppProject.Models.PurchaseEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserIDId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserIDId");

                    b.ToTable("PurchaseEvent");
                });

            modelBuilder.Entity("InternetAppProject.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Tag");
                });

            modelBuilder.Entity("InternetAppProject.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Create_time")
                        .HasColumnType("datetime2");

                    b.Property<string>("Credit_card")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<bool>("Visual_mode")
                        .HasColumnType("bit");

                    b.Property<int>("Zip")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DId")
                        .IsUnique()
                        .HasFilter("[DId] IS NOT NULL");

                    b.ToTable("User");
                });

            modelBuilder.Entity("InternetAppProject.Models.Workplace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("P_lat")
                        .HasColumnType("real");

                    b.Property<float>("P_long")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("Workplace");
                });

            modelBuilder.Entity("ImageTag", b =>
                {
                    b.HasOne("InternetAppProject.Models.Image", null)
                        .WithMany()
                        .HasForeignKey("ImagesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("InternetAppProject.Models.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("InternetAppProject.Models.Drive", b =>
                {
                    b.HasOne("InternetAppProject.Models.DriveType", "TypeId")
                        .WithMany()
                        .HasForeignKey("TypeIdId");

                    b.Navigation("TypeId");
                });

            modelBuilder.Entity("InternetAppProject.Models.Image", b =>
                {
                    b.HasOne("InternetAppProject.Models.Drive", "DId")
                        .WithMany("Images")
                        .HasForeignKey("DIdId");

                    b.Navigation("DId");
                });

            modelBuilder.Entity("InternetAppProject.Models.PurchaseEvent", b =>
                {
                    b.HasOne("InternetAppProject.Models.User", "UserID")
                        .WithMany("PurchaseEvents")
                        .HasForeignKey("UserIDId");

                    b.Navigation("UserID");
                });

            modelBuilder.Entity("InternetAppProject.Models.User", b =>
                {
                    b.HasOne("InternetAppProject.Models.Drive", "D")
                        .WithOne("UserId")
                        .HasForeignKey("InternetAppProject.Models.User", "DId");

                    b.Navigation("D");
                });

            modelBuilder.Entity("InternetAppProject.Models.Drive", b =>
                {
                    b.Navigation("Images");

                    b.Navigation("UserId");
                });

            modelBuilder.Entity("InternetAppProject.Models.User", b =>
                {
                    b.Navigation("PurchaseEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
