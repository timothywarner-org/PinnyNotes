namespace PinnyNotes.Core.Migrations;

public class Schema3To4Migration : SchemaMigration
{
    public override int TargetSchemaVersion => 3;
    public override int ResultingSchemaVersion => 4;
    public override string UpdateQuery => $@"
        -- Add Title column to Notes
        ALTER TABLE Notes
        ADD COLUMN Title TEXT DEFAULT NULL;

        -- Remove IsPinned column from Notes
        ALTER TABLE Notes
        DROP COLUMN IsPinned;

        -- Migrate MinimizeMode: PreventIfPinned (2) -> Allow (0)
        UPDATE Settings
        SET Notes_MinimizeMode = 0
        WHERE Notes_MinimizeMode = 2;

        -- Migrate TransparencyMode: WhenPinned (2) -> Enabled (1)
        UPDATE Settings
        SET Notes_TransparencyMode = 1
        WHERE Notes_TransparencyMode = 2;

        -- Update schema version
        UPDATE SchemaInfo
        SET Version = {ResultingSchemaVersion}
        WHERE Id = 0;
    ";
}
