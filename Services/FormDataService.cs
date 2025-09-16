using Newtonsoft.Json;
using PdfFormOverlay.Maui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // Enhanced Form Data Service with SQLite and Encryption
    public class FormDataService
    {
        private readonly DatabaseService _databaseService;

        public FormDataService()
        {
            _databaseService = new DatabaseService();
        }

        public async Task<bool> SaveFormDataAsync(string formId, string formName, Dictionary<string, object> fieldValues)
        {
            try
            {
                var userPassword = SecurityService.GetUserPassword();
                if (string.IsNullOrEmpty(userPassword))
                {
                    throw new InvalidOperationException("User password not set. Please set up security first.");
                }

                var database = await _databaseService.GetDatabaseAsync();

                // Serialize and encrypt field data
                var jsonData = JsonConvert.SerializeObject(fieldValues);
                var encryptedData = AesEncryptionService.Encrypt(jsonData, userPassword);

                var record = new FormDataRecord
                {
                    FormId = formId,
                    FormName = formName,
                    EncryptedFieldData = encryptedData,
                    SavedDate = DateTime.Now,
                    LastModified = DateTime.Now
                };

                await database.InsertAsync(record);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving form data: {ex.Message}");
                return false;
            }
        }

        public async Task<List<SavedFormData>> GetSavedFormsAsync(string formId = null)
        {
            try
            {
                var userPassword = SecurityService.GetUserPassword();
                if (string.IsNullOrEmpty(userPassword))
                    return new List<SavedFormData>();

                var database = await _databaseService.GetDatabaseAsync();

                List<FormDataRecord> records;
                if (string.IsNullOrEmpty(formId))
                {
                    records = await database.Table<FormDataRecord>()
                        .OrderByDescending(r => r.SavedDate)
                        .ToListAsync();
                }
                else
                {
                    records = await database.Table<FormDataRecord>()
                        .Where(r => r.FormId == formId)
                        .OrderByDescending(r => r.SavedDate)
                        .ToListAsync();
                }

                var savedForms = new List<SavedFormData>();

                foreach (var record in records)
                {
                    try
                    {
                        var decryptedJson = AesEncryptionService.Decrypt(record.EncryptedFieldData, userPassword);
                        if (!string.IsNullOrEmpty(decryptedJson))
                        {
                            var fieldValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(decryptedJson);

                            savedForms.Add(new SavedFormData
                            {
                                FormId = record.FormId,
                                FormName = record.FormName,
                                FieldValues = fieldValues,
                                SavedDate = record.SavedDate
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error decrypting record {record.Id}: {ex.Message}");
                    }
                }

                return savedForms;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving saved forms: {ex.Message}");
                return new List<SavedFormData>();
            }
        }

        public async Task<bool> DeleteSavedFormAsync(string formId, DateTime savedDate)
        {
            try
            {
                var database = await _databaseService.GetDatabaseAsync();

                var record = await database.Table<FormDataRecord>()
                    .Where(r => r.FormId == formId && r.SavedDate == savedDate)
                    .FirstOrDefaultAsync();

                if (record != null)
                {
                    await database.DeleteAsync(record);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting saved form: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetupUserSecurityAsync(string password, List<SecurityQuestion> securityQuestions)
        {
            try
            {
                if (securityQuestions.Count != 3)
                    throw new ArgumentException("Exactly 3 security questions are required");

                var database = await _databaseService.GetDatabaseAsync();
                var salt = SecurityService.GenerateSalt();

                var securityRecord = new UserSecurityRecord
                {
                    Id = 1,
                    PasswordHash = SecurityService.HashPassword(password, salt),
                    Salt = salt,
                    SecurityQuestion1 = securityQuestions[0].Question,
                    SecurityQuestion2 = securityQuestions[1].Question,
                    SecurityQuestion3 = securityQuestions[2].Question,
                    SecurityAnswer1Hash = SecurityService.HashPassword(securityQuestions[0].Answer.ToLower().Trim(), salt),
                    SecurityAnswer2Hash = SecurityService.HashPassword(securityQuestions[1].Answer.ToLower().Trim(), salt),
                    SecurityAnswer3Hash = SecurityService.HashPassword(securityQuestions[2].Answer.ToLower().Trim(), salt),
                    CreatedDate = DateTime.Now,
                    LastPasswordChange = DateTime.Now
                };

                await database.InsertOrReplaceAsync(securityRecord);
                SecurityService.SaveUserPassword(password);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up user security: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateUserPasswordAsync(string password)
        {
            try
            {
                var database = await _databaseService.GetDatabaseAsync();
                var securityRecord = await database.GetAsync<UserSecurityRecord>(1);

                var hashedPassword = SecurityService.HashPassword(password, securityRecord.Salt);
                return hashedPassword == securityRecord.PasswordHash;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating password: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetSecurityQuestionsAsync()
        {
            try
            {
                var database = await _databaseService.GetDatabaseAsync();
                var securityRecord = await database.GetAsync<UserSecurityRecord>(1);

                return new List<string>
            {
                securityRecord.SecurityQuestion1,
                securityRecord.SecurityQuestion2,
                securityRecord.SecurityQuestion3
            };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting security questions: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<string> RecoverPasswordAsync(List<string> answers)
        {
            try
            {
                if (answers.Count != 3)
                    return null;

                var database = await _databaseService.GetDatabaseAsync();
                var securityRecord = await database.GetAsync<UserSecurityRecord>(1);

                var answer1Hash = SecurityService.HashPassword(answers[0].ToLower().Trim(), securityRecord.Salt);
                var answer2Hash = SecurityService.HashPassword(answers[1].ToLower().Trim(), securityRecord.Salt);
                var answer3Hash = SecurityService.HashPassword(answers[2].ToLower().Trim(), securityRecord.Salt);

                if (answer1Hash == securityRecord.SecurityAnswer1Hash &&
                    answer2Hash == securityRecord.SecurityAnswer2Hash &&
                    answer3Hash == securityRecord.SecurityAnswer3Hash)
                {
                    // Generate a temporary password or prompt for new password
                    return "TempPassword123!"; // In real implementation, should prompt for new password
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recovering password: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsSecuritySetupAsync()
        {
            try
            {
                var database = await _databaseService.GetDatabaseAsync();
                var securityRecord = await database.Table<UserSecurityRecord>()
                    .Where(r => r.Id == 1)
                    .FirstOrDefaultAsync();

                return securityRecord != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking security setup: {ex.Message}");
                return false;
            }
        }
    }

}
