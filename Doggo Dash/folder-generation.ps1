# Updated to match:
/Assets/Game
#   /Domain (Entities, ValueObjects, Events, Services)
#   /Application (UseCases, Ports, DTOs)
#   /Infrastructure (Persistence, Ads, IAP, Audio)
#   /Presentation
#     /Runtime (Input, Runner, World/Track|Pickups|Obstacles, UI, Audio, VFX)
#     /Editor (optional, only if approved)
#   /Configs
#   /Art
#   /Prefabs
#   /Scenes


# FOLDERS TO CREATE
$dirs = @(

    "Assets",
    "Assets\Game",

    # Domain
    "Assets\Game\Domain",
    "Assets\Game\Domain\Entities",
    "Assets\Game\Domain\ValueObjects",
    "Assets\Game\Domain\Events",
    "Assets\Game\Domain\Services",

    # Application
    "Assets\Game\Application",
    "Assets\Game\Application\UseCases",
    "Assets\Game\Application\Ports",
    "Assets\Game\Application\DTOs",

    # Infrastructure
    "Assets\Game\Infrastructure",
    "Assets\Game\Infrastructure\Persistence",
    "Assets\Game\Infrastructure\Ads",
    "Assets\Game\Infrastructure\IAP",
    "Assets\Game\Infrastructure\Audio",

    # Presentation
    "Assets\Game\Presentation",
    "Assets\Game\Presentation\Runtime",
    "Assets\Game\Presentation\Runtime\Input",
    "Assets\Game\Presentation\Runtime\Runner",
    "Assets\Game\Presentation\Runtime\World",
    "Assets\Game\Presentation\Runtime\World\Track",
    "Assets\Game\Presentation\Runtime\World\Pickups",
    "Assets\Game\Presentation\Runtime\World\Obstacles",
    "Assets\Game\Presentation\Runtime\UI",
    "Assets\Game\Presentation\Runtime\Audio",
    "Assets\Game\Presentation\Runtime\VFX",

    # Editor (optional - only include if you want it created)
    # "Assets\Game\Presentation\Editor",

    # Non-code Unity content folders
    "Assets\Game\Configs",
    "Assets\Game\Art",
    "Assets\Game\Prefabs",
    "Assets\Game\Scenes"
)



Write-Host "Creating directories..."
foreach ($d in $dirs) {
    if (-not (Test-Path $d)) {
        New-Item -ItemType Directory -Path $d | Out-Null
        Write-Host "  Created dir: $d"
    } else {
        Write-Host "  Exists dir:  $d"
    }
}


Write-Host ""
Write-Host "✅ Done generating Unity script structure."
