﻿// <auto-generated />
using System;
using InternetAppProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InternetAppProject.Migrations
{
    [DbContext(typeof(InternetAppProjectContext))]
    [Migration("20210908193716_Mig06")]
    partial class Mig06
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.8")
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

                    b.Property<int>("Level")
                        .HasColumnType("int");

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

                    b.Property<byte[]>("Data")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DriveId")
                        .HasColumnType("int");

                    b.Property<DateTime>("EditTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<DateTime>("UploadTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DriveId");

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
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

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
                    b.HasOne("InternetAppProject.Models.Drive", null)
                        .WithMany("ImageId")
                        .HasForeignKey("DriveId");
                });

            modelBuilder.Entity("InternetAppProject.Models.PurchaseEvent", b =>
                {
                    b.HasOne("InternetAppProject.Models.User", "UserID")
                        .WithMany("Purchases")
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
                    b.Navigation("ImageId");

                    b.Navigation("UserId");
                });

            modelBuilder.Entity("InternetAppProject.Models.User", b =>
                {
                    b.Navigation("Purchases");
                });
#pragma warning restore 612, 618
        }
    }
}
