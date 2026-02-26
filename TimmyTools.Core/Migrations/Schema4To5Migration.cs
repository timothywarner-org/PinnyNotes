namespace TimmyTools.Core.Migrations;

public class Schema4To5Migration : SchemaMigration
{
    public override int TargetSchemaVersion => 4;
    public override int ResultingSchemaVersion => 5;
    public override string UpdateQuery => $@"
        -- Content column now stores RTF instead of plain text.
        -- Conversion is handled lazily on load (no data migration needed).

        -- Update schema version
        UPDATE SchemaInfo
        SET Version = {ResultingSchemaVersion}
        WHERE Id = 0;
    ";
}
