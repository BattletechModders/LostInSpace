# LostInSpace
Provides access and visual control over star systems.

Control over starsystem access is done by modifying the TravelRequirements section of the starsystem def at runtime, and system can optionally be visually hidden from players. Control over systems can be done through settings.json, through in-game events, and by calling public API. All three methods work by adding/removing tags to the starsystems, so any tags added/removed in this way will persist when the save is loaded.

Travel Requirements section of starsystemdef:

```
"TravelRequirements": [
    {
      "Scope": "Company",
      "RequirementTags": {
        "items": [
          "map_travel_2a"
        ],
        "tagSetSourceFile": ""
      },
      "ExclusionTags": {
        "items": [],
        "tagSetSourceFile": ""
      },
      "RequirementComparisons": []
    }
  ],
```
## Control via settings.json
Example settings.json:

```
{
	"debugLog": true,
	"traceLog": true,
	"hiddenSystems": {
		"starsystemdef_St.Ives": [
			"LiS__NavReq__StIvesHider__starsystemdef_St.Ives__HIDDEN"
		],
		"starsystemdef_Detroit": [
			"LiS__NavReq__DetroitRestrict__starsystemdef_Detroit__RESTRICT"
		],
		"starsystemdef_Brockway": [
			"LiS__NavExc__BrockwayHider__starsystemdef_Brockway__HIDDEN", "LiS__NavExc__BrockwayHider2__BrockwayTravelBan__HIDDEN"
		],
		"starsystemdef_Lyreton": [
			"LiS__NavExc__LyretonRestrictor__starsystemdef_Lyreton__RESTRICT"
		]
	}
}
```

`hiddenSystems` - dictionary<string, list<string>> - with key = starsystemdef ID (e.g. `starsystemdef_Algol`), and value = list of tags to be added to the starystem. These tags are parsed at runtime to add travel requirements and hide system.

These tags must follow the following format: `LiS__{type}__{yourcustomtagID}__{system ID)__{function}`: e.g. `"LiS__NavReq__StIvesHider__starsystemdef_St.Ives__HIDDEN"`

- {type}: must be either `NavReq` or `NavExc`. Tags with NavReq will be parsed into TravelRequirements RequirementTags. Any company <i>without</i> these tags will not be allowed to travel to the system. Tags with NavExc will be parsed into TravelRequirements ExclusionTags. Any company that <i>has</i> any of these tags will not be allowed to travel to the system.
- {yourcustomtagID} is just a unique identifier string for the tag; can be anything.
- {system ID}: ID field from starsystemdef (e.g. starsystemdef_Algol)
- {function}: if HIDDEN, system will be hidden from player view and travel restricted. if RESTRICT, system will be visible but travel will be restricted.

## Control via in-game events
In-game events can add company tags, which will then be parsed into system tags (these company tags will not actually be added to the company TagSet. These added tags must be in the following format:

`ADD_LiS__{type}__{yourcustomtagID}__{system ID)__{function}`: e.g. `"LiS__NavReq__StIvesHider__starsystemdef_St.Ives__HIDDEN"` - to ADD this tag to the system

`REMOVE_LiS__{type}__{yourcustomtagID}__{system ID)__{function}`: e.g. `"LiS__NavReq__StIvesHider__starsystemdef_St.Ives__HIDDEN"` - to REMOVE this tag from the systema

- the prefix `ADD_` or `REMOVE_` is used to control whether the remainder of the tag will be added or removed from the starsystem and travel requirements. Prefix itself is not added to the starsystem tags.
- {type}: must be either `NavReq` or `NavExc`. Tags with NavReq will be parsed into TravelRequirements RequirementTags. Any company <i>without</i> these tags will not be allowed                     to travel to the system. Tags with NavExc will be parsed into TravelRequirements ExclusionTags. Any company that <i>has</i> any of these tags will not be allowed to travel to the system.
- {yourcustomtagID} is just a unique identifier string for the tag; can be anything.
- {system ID}: ID field from starsystemdef (e.g. starsystemdef_Algol)
- {function}: if HIDDEN, system will be hidden from player view and travel restricted. if RESTRICT, system will be visible but travel will be restricted.

## Control via API
To add tags and travel requirements to a system:

call `LostInSpace.Framework.Util.AddSystemRestrictions(string systemID, List<string> tags)` to add tags.

  `systemID` - string, ID field from starsystemdef (e.g. starsystemdef_Algol)
  `tags` - List<string>, list of tags to add to system. These tags follow same format as tags added via settings.json or via events (minus the ADD_ or REMOVE_ prefix).

call `LostInSpace.Framework.Util.RemoveSystemRestrictions(string systemID, List<string> tags = null)` to remove tags.

  `systemID` - string, ID field from starsystemdef (e.g. starsystemdef_Algol)
  `tags` - List<string>, list of tags to remove from system. These tags follow same format as tags added via settings.json or via events (minus the ADD_ or REMOVE_ prefix). if `null`, <i>all</i> LostInSpace tags will be removed from the system.
