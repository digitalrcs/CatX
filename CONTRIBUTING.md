# Contributing to CatX

Thank you for helping make keyboards safer from paws.

## Development setup

1. Use Windows 11 with the .NET 8 SDK.
2. Fork and clone the repository.
3. Run `dotnet build CatX.sln --configuration Release`.
4. Run `dotnet test CatX.sln --configuration Release`.
5. Start the app with `dotnet run --project src/CatX/CatX.csproj`.

## Testing keyboard changes safely

- Keep a mouse connected and the CatX window visible.
- Select a recovery shortcut and test it immediately after enabling the guard.
- Confirm normal keyboard input returns after unlocking.
- Confirm closing CatX while guarded restores input.
- Do not attempt to suppress `Ctrl + Alt + Delete` or other Windows secure-attention behavior.

## Pull requests

Keep changes focused and explain how they were tested. New cat artwork must be original or have a license compatible with MIT; include its attribution and license when applicable. Never commit certificates, signing keys, personal settings, or compiled output.
