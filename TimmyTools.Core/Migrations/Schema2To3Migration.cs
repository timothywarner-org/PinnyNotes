namespace TimmyTools.Core.Migrations;

public class Schema2To3Migration : SchemaMigration
{
    public override int TargetSchemaVersion => 2;
    public override int ResultingSchemaVersion => 3;
    public override string UpdateQuery => $@"
        -- Update ApplicationData
        -- -- Change ThemeColor to ColourScheme
        ALTER TABLE ApplicationData
        ADD COLUMN ColourScheme TEXT DEFAULT NULL;

        ALTER TABLE ApplicationData
        DROP COLUMN ThemeColor;

        -- Update Settings
        -- -- Add UrlToolState
        ALTER TABLE Settings
        ADD COLUMN Tool_GuidState INTEGER DEFAULT 1;

        -- -- Fix column names
        ALTER TABLE Settings 
        RENAME COLUMN Notes_CycleColors TO Notes_CycleColours;

        ALTER TABLE Settings 
        RENAME COLUMN Notes_ColorMode TO Notes_ColourMode;

        ALTER TABLE Settings 
        RENAME COLUMN Tool_ColorState TO Tool_ColourState;

        -- Create Notes Table (schema as of v3, hardcoded for migration stability)
        CREATE TABLE IF NOT EXISTS Notes
        (
            Id                  INTEGER PRIMARY KEY AUTOINCREMENT,

            Content             TEXT,

            X                   REAL,
            Y                   REAL,
            Width               REAL,
            Height              REAL,

            GravityX            INTEGER,
            GravityY            INTEGER,

            ThemeColourScheme   TEXT,

            IsPinned            INTEGER,
            IsOpen              INTEGER
        );

        -- Update schema version
        UPDATE SchemaInfo
        SET Version = {ResultingSchemaVersion}
        WHERE Id = 0;
    ";
}
