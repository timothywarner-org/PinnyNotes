namespace TimmyTools.Core.Migrations;

public class Schema5To6Migration : SchemaMigration
{
    public override int TargetSchemaVersion => 5;
    public override int ResultingSchemaVersion => 6;
    public override string UpdateQuery => $@"
        ALTER TABLE Settings ADD COLUMN Editor_CaretThickness REAL DEFAULT 2.0;
        ALTER TABLE Settings ADD COLUMN Editor_CaretColour INTEGER DEFAULT 0;

        -- Update schema version
        UPDATE SchemaInfo
        SET Version = {ResultingSchemaVersion}
        WHERE Id = 0;
    ";
}
