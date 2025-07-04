﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TimeTrackerApi.Data;

#nullable disable

namespace TimeTrackerApi.Migrations
{
    [DbContext(typeof(TimeTrackerContext))]
    [Migration("20250629063139_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.6");

            modelBuilder.Entity("TimeTrackerApi.Models.TimeEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PunchInTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("PunchOutTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("TaskName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TimeEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
