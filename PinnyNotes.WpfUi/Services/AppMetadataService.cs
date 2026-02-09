using PinnyNotes.Core.DataTransferObjects;
using PinnyNotes.Core.Repositories;
using PinnyNotes.WpfUi.Models;

namespace PinnyNotes.WpfUi.Services;

public class AppMetadataService(AppMetadataRepository appMetadataRepository)
{
    private readonly AppMetadataRepository _appMetadataRepository = appMetadataRepository;

    public AppMetadataModel Metadata { get; private set; } = null!;

    public async Task Load()
    {
        AppMetadataDataDto appMetadata = await _appMetadataRepository.GetById(1);

        Metadata = new()
        {
            LastUpdateCheck = appMetadata.LastUpdateCheck,
            ColourScheme = appMetadata.ColourScheme
        };
    }

    public async Task Save()
    {
        _ = await _appMetadataRepository.Update(
            new AppMetadataDataDto(
                Id: 1,

                LastUpdateCheck: Metadata.LastUpdateCheck,
                ColourScheme: Metadata.ColourScheme
            )
        );
    }
}
