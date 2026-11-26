-- =====================================================================
-- 01_create_database.sql
-- Creates the database and the 6 tables of the Clinic Appointment and
-- Patient Management System (works on MySQL 8 and MariaDB).
-- Table and column names match the C# models exactly, so Entity Framework
-- Core can read and write them without any extra setup.
-- =====================================================================
DROP DATABASE IF EXISTS clinic_db;
CREATE DATABASE clinic_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE clinic_db;

-- 1) Users: login accounts (one account per patient, doctor, or staff member)
CREATE TABLE Users (
    UserId        INT          NOT NULL AUTO_INCREMENT,
    Email         VARCHAR(100) NOT NULL,
    PasswordHash  VARCHAR(255) NOT NULL,
    Role          VARCHAR(20)  NOT NULL,          -- Patient, Doctor, or Staff
    PRIMARY KEY (UserId),
    UNIQUE KEY UQ_Users_Email (Email)             -- BR-9: one account per email
) ENGINE=InnoDB;

-- 2) Patients
CREATE TABLE Patients (
    PatientId      INT          NOT NULL AUTO_INCREMENT,
    UserId         INT          NOT NULL,
    FullName       VARCHAR(100) NOT NULL,
    BirthDate      DATE         NOT NULL,
    Gender         VARCHAR(20)  NOT NULL,
    ContactNumber  VARCHAR(20)  NOT NULL,
    Address        VARCHAR(200) NOT NULL,
    PRIMARY KEY (PatientId),
    UNIQUE KEY UQ_Patients_UserId (UserId),
    CONSTRAINT FK_Patients_Users FOREIGN KEY (UserId) REFERENCES Users (UserId)
) ENGINE=InnoDB;

-- 3) Doctors (with their availability schedule)
CREATE TABLE Doctors (
    DoctorId        INT          NOT NULL AUTO_INCREMENT,
    UserId          INT          NOT NULL,
    FullName        VARCHAR(100) NOT NULL,
    Specialization  VARCHAR(100) NOT NULL,
    AvailableDays   VARCHAR(100) NOT NULL,        -- e.g. 'Mon, Wed, Fri'
    StartTime       TIME         NOT NULL,
    EndTime         TIME         NOT NULL,
    PRIMARY KEY (DoctorId),
    UNIQUE KEY UQ_Doctors_UserId (UserId),
    CONSTRAINT FK_Doctors_Users FOREIGN KEY (UserId) REFERENCES Users (UserId)
) ENGINE=InnoDB;

-- 4) ClinicStaff
CREATE TABLE ClinicStaff (
    StaffId   INT          NOT NULL AUTO_INCREMENT,
    UserId    INT          NOT NULL,
    FullName  VARCHAR(100) NOT NULL,
    Position  VARCHAR(100) NOT NULL,
    PRIMARY KEY (StaffId),
    UNIQUE KEY UQ_ClinicStaff_UserId (UserId),
    CONSTRAINT FK_ClinicStaff_Users FOREIGN KEY (UserId) REFERENCES Users (UserId)
) ENGINE=InnoDB;

-- 5) Appointments
CREATE TABLE Appointments (
    AppointmentId    INT          NOT NULL AUTO_INCREMENT,
    PatientId        INT          NOT NULL,
    DoctorId         INT          NOT NULL,
    AppointmentDate  DATE         NOT NULL,
    AppointmentTime  TIME         NOT NULL,
    Reason           VARCHAR(300) NOT NULL,
    Status           VARCHAR(20)  NOT NULL DEFAULT 'Pending',   -- Pending, Approved, Completed, Cancelled
    PRIMARY KEY (AppointmentId),
    CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients (PatientId),
    CONSTRAINT FK_Appointments_Doctors  FOREIGN KEY (DoctorId)  REFERENCES Doctors (DoctorId)
) ENGINE=InnoDB;

-- 6) ConsultationRecords (at most one record per appointment)
CREATE TABLE ConsultationRecords (
    RecordId          INT           NOT NULL AUTO_INCREMENT,
    AppointmentId     INT           NOT NULL,
    ConsultationDate  DATE          NOT NULL,
    Diagnosis         VARCHAR(500)  NOT NULL,
    TreatmentNotes    VARCHAR(1000) NOT NULL,
    PRIMARY KEY (RecordId),
    UNIQUE KEY UQ_ConsultationRecords_Appointment (AppointmentId),
    CONSTRAINT FK_ConsultationRecords_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments (AppointmentId)
) ENGINE=InnoDB;
