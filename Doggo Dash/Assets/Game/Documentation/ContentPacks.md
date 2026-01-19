# Content Pack Asset Naming

Use the following naming convention so pack assets can be discovered and audited consistently.

## Format
`Pack_<PackId>_<Category>_<Descriptor>`

- `PackId`: Short biome or theme identifier (`Farm`, `City`, `Spooky`).
- `Category`: `Segment`, `Decor`, `ObstaclePattern`, or `PickupPattern`.
- `Descriptor`: A short, human-friendly label (`Straight01`, `HayBales`, `Easy`, `Dense`).

## Examples
- `Pack_Farm_Segment_Straight01`
- `Pack_Farm_Decor_Windmill`
- `Pack_Farm_ObstaclePattern_Easy`
- `Pack_Farm_PickupPattern_Dense`

## Notes
- Keep `PackId` consistent with `ContentPackSO.packId`.
- Avoid spaces and use PascalCase for the descriptor.
