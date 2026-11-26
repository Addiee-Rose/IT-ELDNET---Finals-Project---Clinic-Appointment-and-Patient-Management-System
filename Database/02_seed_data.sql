-- =====================================================================
-- 02_seed_data.sql  -  Sample data for the Clinic Appointment and Patient Management System
-- Demo password for EVERY account: Clinic@123
-- (passwords are stored as ASP.NET Core Identity hashes, never as plain text)
-- Run 01_create_database.sql first.
-- =====================================================================
USE clinic_db;

INSERT INTO Users (UserId, Email, PasswordHash, Role) VALUES
  (1, 'staff1@clinic.test', 'AQAAAAIAAYagAAAAEBAdnKIXY+NWQbovcWNcml8ptV5AkS5yETccU6AXdXuo48cenULvUEkNRTzeBYf3dA==', 'Staff'),
  (2, 'staff2@clinic.test', 'AQAAAAIAAYagAAAAEF4ZUlS6ubWQSiSuFg4nZbv3JPjQ4feeauOk2T8knghWIQVdACfXrun+sn81NecGXA==', 'Staff'),
  (3, 'doctor1@clinic.test', 'AQAAAAIAAYagAAAAECv78qcGp7qSYFE7Bzj5No1xs3obZwv9P/+JZUHA2PqDsbd1BtTpndpHafLxQcj+Xw==', 'Doctor'),
  (4, 'doctor2@clinic.test', 'AQAAAAIAAYagAAAAEOVG2IpPJl2uRr6fStuJnpmnoz2iZRswm4ou5qKiqbco2mBXmmpBct6xXjZh1oz7AQ==', 'Doctor'),
  (5, 'doctor3@clinic.test', 'AQAAAAIAAYagAAAAEFz3MfzYUvXyu0J7AZqrydP9qw5N/rq5h4Mvy4pfYQoxeI4Kp0BaknoubRrLT1GBmg==', 'Doctor'),
  (6, 'patient1@clinic.test', 'AQAAAAIAAYagAAAAEG487BGHS9GO89SzKsv6N9qehfXCFVxkM4iCqyfdeDMp+MBI+rHlhmVgNZrIQ+0z2w==', 'Patient'),
  (7, 'patient2@clinic.test', 'AQAAAAIAAYagAAAAEDBiRIvfA7XQPAknmcnxkOcp4RoqJ9TDtZ4i0+9AQWiPsND1xZOJVehBySV8/84mGQ==', 'Patient'),
  (8, 'patient3@clinic.test', 'AQAAAAIAAYagAAAAENZDrhlSJT05HGHnGFQrL9lJlDQdk06qhL9kYDDOW2xryTeKKa85ICffobMAo411Zw==', 'Patient'),
  (9, 'patient4@clinic.test', 'AQAAAAIAAYagAAAAEG4msxNjyEdz4OgRRIpDK1tHbhNN0S4VNpNU1jufoDVbAbZ/J960eUOPI6bwd2IQ7g==', 'Patient'),
  (10, 'patient5@clinic.test', 'AQAAAAIAAYagAAAAECSmKbfJ/qF6DXfeF+G3KT15i7OaXP068Wh6lAx2VCp7ObQgnA8HDJTo3k6CCPRBHg==', 'Patient'),
  (11, 'patient6@clinic.test', 'AQAAAAIAAYagAAAAEJ1Ap3NdKKhpC267QB5NGegGANp0ffTVRuceZdjkkgUjonpUZFtTrOkE8jEUDGzL4Q==', 'Patient');

INSERT INTO ClinicStaff (StaffId, UserId, FullName, Position) VALUES
  (1, 1, 'Rosa Dizon', 'Receptionist'),
  (2, 2, 'Mark Lim', 'Clinic Administrator');

INSERT INTO Doctors (DoctorId, UserId, FullName, Specialization, AvailableDays, StartTime, EndTime) VALUES
  (1, 3, 'Maria Santos', 'General Medicine', 'Mon, Wed, Fri', '09:00:00', '17:00:00'),
  (2, 4, 'Jose Reyes', 'Pediatrics', 'Tue, Thu', '08:00:00', '16:00:00'),
  (3, 5, 'Ana Cruz', 'Dermatology', 'Mon, Tue, Wed, Thu, Fri', '10:00:00', '18:00:00');

INSERT INTO Patients (PatientId, UserId, FullName, BirthDate, Gender, ContactNumber, Address) VALUES
  (1, 6, 'Juan Dela Cruz', '2001-03-14', 'Male', '09171234501', 'Lahug, Cebu City'),
  (2, 7, 'Elena Garcia', '1995-07-22', 'Female', '09171234502', 'Mandaue City, Cebu'),
  (3, 8, 'Pedro Ramos', '1988-11-05', 'Male', '09171234503', 'Talisay City, Cebu'),
  (4, 9, 'Lia Mendoza', '2015-01-30', 'Female', '09171234504', 'Banilad, Cebu City'),
  (5, 10, 'Carlo Bautista', '1979-09-18', 'Male', '09171234505', 'Consolacion, Cebu'),
  (6, 11, 'Sofia Navarro', '2003-05-09', 'Female', '09171234506', 'Mabolo, Cebu City');

INSERT INTO Appointments (AppointmentId, PatientId, DoctorId, AppointmentDate, AppointmentTime, Reason, Status) VALUES
  (1, 1, 1, '2026-11-02', '09:30:00', 'Fever and cough for three days', 'Completed'),
  (2, 2, 3, '2026-11-04', '10:30:00', 'Skin rash on the arms', 'Completed'),
  (3, 4, 2, '2026-11-05', '08:30:00', 'Child wellness check-up', 'Completed'),
  (4, 3, 1, '2026-11-06', '14:00:00', 'High blood pressure follow-up', 'Completed'),
  (5, 5, 3, '2026-11-09', '11:00:00', 'Acne consultation', 'Cancelled'),
  (6, 6, 1, '2026-11-11', '10:00:00', 'Headache and dizziness', 'Cancelled'),
  (7, 1, 3, '2026-11-25', '15:00:00', 'Skin allergy follow-up', 'Approved'),
  (8, 2, 1, '2026-11-27', '09:00:00', 'Annual physical examination', 'Approved'),
  (9, 4, 2, '2026-12-01', '09:00:00', 'Vaccination schedule', 'Approved'),
  (10, 5, 1, '2026-12-02', '13:30:00', 'Back pain', 'Pending'),
  (11, 6, 3, '2026-12-03', '10:00:00', 'Hair loss consultation', 'Pending'),
  (12, 3, 2, '2026-12-03', '14:30:00', 'Cough in my child', 'Pending');

INSERT INTO ConsultationRecords (RecordId, AppointmentId, ConsultationDate, Diagnosis, TreatmentNotes) VALUES
  (1, 1, '2026-11-02', 'Acute upper respiratory infection', 'Rest, increase fluids, paracetamol every 6 hours as needed. Return if fever lasts more than 3 days.'),
  (2, 2, '2026-11-04', 'Contact dermatitis', 'Avoid the suspected soap. Apply prescribed topical cream twice daily for one week.'),
  (3, 3, '2026-11-05', 'Healthy child, growth on track', 'Continue balanced diet. Next check-up in six months.'),
  (4, 4, '2026-11-06', 'Hypertension, controlled', 'Continue current medication. Reduce salt intake. Monitor blood pressure at home.');
