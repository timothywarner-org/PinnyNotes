namespace TimmyTools.Core.Migrations;

public class Schema6To7Migration : SchemaMigration
{
    public override int TargetSchemaVersion => 6;
    public override int ResultingSchemaVersion => 7;
    public override string UpdateQuery => $@"
        ALTER TABLE Settings ADD COLUMN BreakTimer_ClassTitle TEXT DEFAULT '';
        ALTER TABLE Settings ADD COLUMN BreakTimer_NextUp TEXT DEFAULT '';

        -- Update schema version
        UPDATE SchemaInfo
        SET Version = {ResultingSchemaVersion}
        WHERE Id = 0;
    ";
}
