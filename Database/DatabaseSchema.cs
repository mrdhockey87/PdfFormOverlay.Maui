using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Database
{
    // Database Creation XML Schema
    public static class DatabaseSchema
    {
        public const string SchemaXml = @"<?xml version='1.0' encoding='utf-8'?>
<database name='soldiers.db' version='1.0'>
    <tables>
        <table name='FormData'>
            <columns>
                <column name='Id' type='INTEGER' primaryKey='true' autoIncrement='true' />
                <column name='FormId' type='TEXT' notNull='true' />
                <column name='FormName' type='TEXT' notNull='true' />
                <column name='EncryptedFieldData' type='TEXT' notNull='true' />
                <column name='SavedDate' type='DATETIME' notNull='true' />
                <column name='LastModified' type='DATETIME' notNull='true' />
            </columns>
            <indexes>
                <index name='IX_FormData_FormId' columns='FormId' />
                <index name='IX_FormData_SavedDate' columns='SavedDate' />
            </indexes>
        </table>
        
        <table name='UserSecurity'>
            <columns>
                <column name='Id' type='INTEGER' primaryKey='true' />
                <column name='PasswordHash' type='TEXT' notNull='true' />
                <column name='Salt' type='TEXT' notNull='true' />
                <column name='SecurityQuestion1' type='TEXT' notNull='true' />
                <column name='SecurityQuestion2' type='TEXT' notNull='true' />
                <column name='SecurityQuestion3' type='TEXT' notNull='true' />
                <column name='SecurityAnswer1Hash' type='TEXT' notNull='true' />
                <column name='SecurityAnswer2Hash' type='TEXT' notNull='true' />
                <column name='SecurityAnswer3Hash' type='TEXT' notNull='true' />
                <column name='CreatedDate' type='DATETIME' notNull='true' />
                <column name='LastPasswordChange' type='DATETIME' notNull='true' />
            </columns>
        </table>
        
        <table name='AppSettings'>
            <columns>
                <column name='Key' type='TEXT' primaryKey='true' />
                <column name='Value' type='TEXT' />
                <column name='LastUpdated' type='DATETIME' notNull='true' />
            </columns>
        </table>
    </tables>
    
    <constraints>
        <constraint table='FormData' type='unique' columns='FormId,SavedDate' />
    </constraints>
    
    <triggers>
        <trigger name='UpdateLastModified' table='FormData' event='UPDATE'>
            UPDATE FormData SET LastModified = datetime('now') WHERE Id = NEW.Id;
        </trigger>
    </triggers>
</database>";
    }
}
