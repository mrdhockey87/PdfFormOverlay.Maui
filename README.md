# PdfFormOverlay.Maui
Allows for viewing and editing PDF Forms in MAUI


Key Features Added:
Security & Encryption

SQLite Database: Stores form data in encrypted soldiers.db in the OS Documents/databases folder
Database Password: Uses framework-level password for SQLite encryption
AES 128-bit Encryption: User-defined password encrypts/decrypts form data
Security Questions: 3 questions for password recovery
Session Management: Lock/unlock functionality with visual indicators

Database Structure

FormData Table: Stores encrypted form field data with metadata
UserSecurity Table: Stores password hashes, salt, and security questions
AppSettings Table: General application settings
XML Schema: Provided for database creation reference

Security Pages

LoginPage: Main authentication with password entry
SecuritySetupPage: First-time setup with password and security questions
PasswordRecoveryPage: Answer security questions to recover access

Enhanced PDF Form Page

Security status bar with lock/unlock functionality
Session locking prevents unauthorized access
Additional actions: Delete saved data, Email PDF
Visual security indicators throughout the interface

Services & Infrastructure

AesEncryptionService: Handles all encryption/decryption
SecurityService: Password management and validation
DatabaseService: Encrypted SQLite connection management
Enhanced FormDataService: All CRUD operations with encryption

Data Management

Save encrypted form data to database
Load previously saved forms by user
Delete specific saved form entries
Password recovery through security questions
Complete security reset option (deletes all data)

The framework now provides enterprise-level security for sensitive form data while maintaining ease of use. All form data is encrypted at rest and requires user authentication to access. The security questions provide a recovery mechanism while the session locking adds an extra layer of protection during use.