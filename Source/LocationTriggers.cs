﻿using HarmonyLib;
using Mono.Cecil.Cil;
using NineSolsAPI;
using RCGFSM.Items;
using RCGFSM.PlayerAction;
using System.Collections.Generic;
using UnityEngine;
using static RCGFSM.Items.PickItemAction;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class LocationTriggers {
    private static void CheckLocation(Location location) {
        // TODO: talk to AP of course
        ToastManager.Toast($"CheckLocation() called with Archipelago location: {location}");
    }

    // A "full path" is a slash-delimited sequence of GameObject names, e.g. RootGameObject/NextObject/AnotherObject/LeafObject.
    // But a GameObject can have multiple child GOs with identical names, so full path is not enough to uniquely identify an object.
    // Nine Sols does this *a lot* unfortunately. It mostly affects us with GOs representing Jin chests, especially same-sized chests.
    // So for us a "disambiguated" path adds child indices with a ### where necessary: RootGameObject/NextObject###1/AnotherObject/LeafObject.
    // I chose multiple #s because I found a single # in a long path was too easy to overlook.
    public static string GetFullDisambiguatedPath(GameObject go) {
        var transform = go.transform;
        List<string> pathParts = new List<string>();

        while (transform != null) {
            var currentGOName = transform.name;

            var parent = transform.parent;
            if (parent != null && parent.childCount > 1) {
                bool hasSiblingWithIdenticalName = false;
                for (var i = 0; i < parent.childCount; ++i) {
                    var sibling = parent.GetChild(i);
                    if (sibling.name == currentGOName && sibling != transform) {
                        hasSiblingWithIdenticalName = true;
                        break;
                    }
                }
                if (hasSiblingWithIdenticalName)
                    currentGOName += "###" + transform.GetSiblingIndex();
            }

            pathParts.Add(currentGOName);
            transform = transform.parent;
        }

        pathParts.Reverse();
        return string.Join("/", pathParts);
    }

    private static Dictionary<string, Location> goPathToLocation = new Dictionary<string, Location> {
        {
            "A0_S4 gameLevel/Room/Prefab/1_DropPickable 遺書 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.UC_NOTE
        },
        {
            "A0_S4 gameLevel/Room/Prefab/1_DropPickable 竹簡 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.UC_SCROLL
        },

        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/QCmachine  染血的大紅槿/LootProvider/A1_S1_DropPickable_Flower/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFM_BREAK_CORPSE
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_M_中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFM_CHEST_UPPER_RIGHT
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/BR_TreasureDing_S_小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFM_CHEST_LOWER_VENT
        },
        {
            "A1_S1_GameLevel/Room/Prefab/Treasure 寶箱 Chests/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFM_DB_SURVEILLANCE
        },

        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_S 小寶箱_小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_UPPER_PAGODA
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_MOVING_BOXES
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFE_CHEST_ELEVATOR
        },
        {
            "A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/3_DropPickable 定身玉  FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFE_DROP_BAICHANG
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFE_CHEST_OVER_HAZARD
        },
        {
            "A1_S2_GameLevel/Room/Prefab/寶箱 Treasure Chests/EventBinder 小錢袋/LootProvider/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFE_CHEST_STATUE
        },

        {
            // save flag 37b820ea-4a5a-40a2-a581-767d5362ed5f_19ef97be8cb7b4fca9d79b754bb6c81cScriptableDataBool -> A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/MoneyCrateFlag
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFD_CHEST_LOWER_LEVEL
        },
        {
            // save flag 537b2e71-c903-4d2b-9525-1f501c333287_19ef97be8cb7b4fca9d79b754bb6c81cScriptableDataBool -> A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/BR_TreasureDing_M/MoneyCrateFlag
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_UNDER_LOWER_LEFT_EXIT
        },
        {
            "A1_S3_GameLevel/Room/Prefab/EventBinder/LootProvider 藥草催化器/0_DropPickable 斷魂刃 FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_DROP_1_SHANGUI
        },
        {
            "A1_S3_GameLevel/Room/Prefab/EventBinder/LootProvider 藥草催化器/0_DropPickable 藥草催化器 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_DROP_2_SHANGUI
        },
        {
            // save flag 7aae1bfb-7415-4e03-b8c8-0e593375aff5_19ef97be8cb7b4fca9d79b754bb6c81cScriptableDataBool -> A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金 (1)/MoneyCrateFlag
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金 (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.AFD_CHEST_BELOW_NODE
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 扶桑牌/0_DropPickable 文物 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_CRYSTAL_CAVES
        },
        {
            // save flag "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[Variables]/[Variable] Picked",
            //"A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] [Variable] Picked = true",
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體 小道果/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.AFD_FLOWER_UNDER_ELEVATOR
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 中錢袋、藥材-雄橙/0_DropPickable 藥材-雄橙 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_UPPER_RIGHT_1
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/LootProvider 中錢袋、藥材-雄橙/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_UPPER_RIGHT_2
        },
        {
            "A1_S3_GameLevel/Room/Prefab/寶箱 Chests/EventBinder 卸力玉/LootProvider/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AFD_CHEST_STATUES
        },

        {
            "AG_S1/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking Errors/[Action] GetItem",
            Location.CH_LAUNCH_MEMORIAL
        },
        {
            "AG_S1/Room/Prefab/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_COUNCIL_SIGN
        },
        {
            "AG_S1/Room/Prefab/1_DropPickable SceneObserve FSM (2)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_COUNCIL_TENETS
        },
        {
            "AG_S1/Room/Prefab/寶箱/LootProvider  中錢袋/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_CHEST_VENTS
        },
        {
            "AG_SG1/Room/LootProvider 藥斗擴充瓶/0_DropPickable 藥斗擴充瓶  FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CH_CHEST_AXEBOT_AND_TURRETS
        },

        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S  小量金###2/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_BELOW_NODE
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S  小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_RIGHT_EXIT
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_CHEST_GUARDED_BY_TURRET
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRW_CHEST_VENTS
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.PRW_FLOWER
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/(ViewTest)Pickable_DIESunDeadbody/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_DGRD
        },
        {
            "A2_S3/Room/Prefab/寶箱 Chests/EventBinder 中錢袋/LootProvider/0_DropPickable 中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRW_CHEST_STATUE
        },

        {
            "A2_S1/Room/Prefab/寶箱 Chests 左/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_LEFT_EXIT
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 左/1_DropPickable SceneObserve FSM 元能柱調查報告/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_RHIZOMATIC_ENERGY_METER
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 左/LootProvider 多功能工具組/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_ABOVE_NODE
        },
        {
            "A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.PRC_SHANHAI_CHIP // removal by force
        },
        {
            "A2_S1/Room/Prefab/GuideFish_Acting/NPC_GuideFish A2Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M61_A2_S1_索取A2記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.PRC_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/LootProvider 無懼玉/0_DropPickable 無懼玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_RIGHT_OF_PAGODA
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_GUARDED_BY_BEETLE
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_S 小量金###1/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_NEAR_MOVING_BOX
        },
        {
            "A2_S1/Room/Prefab/寶箱 Chests 右/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRC_CHEST_BREAKABLE_WALL_RIGHT
        },
        {
            // breaking chest itself triggers "A2_S1/Room/Prefab/寶箱 Chests 右/EventBinder 中錢袋/LootProvider /Step Unlock FSM One Step Floor Secret Variant Variant/FSM Animator/View/Sealed Treasure/View/Platform/BR_TreasureDing_M/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            "A2_S1/Room/Prefab/寶箱 Chests 右/EventBinder 中錢袋/LootProvider /3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRC_CHEST_STATUE
        },

        {
            "A2_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRE_CHEST_UPPER_LEFT
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/EventBinder_開啟橋後觸發Boss Fight 算力元件/LootProvider 算力元件/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_DROP_JIAODUAN
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRE_CHEST_AFTER_LASERS
        },
        {
            "BR_JumperPrinter(Clone)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.PRE_CHEST_UNDER_BOX
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/EventBinder 小錢袋/LootProvider/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_STATUE
        },
        {
            "A2_S2/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/3_DropPickable 中錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.PRE_CHEST_UPPER_RIGHT
        },

        {
            "A2_S5_ BossHorseman_GameLevel/Room/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.RP_CONTROL_PANEL
        },
        {
            "A2_S5_ BossHorseman_GameLevel/Room/Simple Binding Tool/Boss_SpearHorse_Logic/LootProvider/0_DropPickable 藥斗功率 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.RP_DROP_YINGZHAO
        },
        {
            "A2_S5_ BossHorseman_GameLevel/Room/Sleeppod  FSM/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.RP_KUAFU_SANCTUM
        },

        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###6/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_MIDDLE_1
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_MIDDLE_2
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_RIGHT
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋###1/BR_TreasureDing_M (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_LEFT_POOL_ABOVE
        },
        // TODO: force Ji to exist even later in the game
        // A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/--[Variables]/[Variable] 羿殺死A4_S3刑天 being false makes Ji appear
        {
            "A3_S1/Room/Prefab/Gameplay_BellTower/General FSM Object_On And Off Switch Variant/FSM Animator/LogicRoot/[Off]Node/SimpleCutSceneFSM_初次遇見姬/--[States]/FSM/[State] PlayCutScene/[Action] 取得樂譜",
            Location.LYR_JI_MUSIC
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_TOWER
        },
        {
            "A3_SG4/Room/Prefab/LootProvider 菸斗擴充/[Mech]SealedBoxTreasure FSM Hack Variant (1)/FSM Animator/View/SealedBox_view/LogicRoot/Loot Spawner_1",
            Location.LYR_CHEST_TOWER_ROOM
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/LootProvider 中錢袋 + 中量金/BR_TreasureDing_M (1)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_ABOVE_NODE
        },
        {
            "A3_SG1/Room/Prefab/EventBinder/LootProvider/Step Unlock FSM Two Step Floor Secret/FSM Animator/View/Sealed Treasure/View/Platform/[Mech]SealedBoxTreasure FSM Interactable Variant/FSM Animator/View/SealedBox_view/LogicRoot/Loot Spawner_1",
            Location.LYR_CHEST_STATUES_ROOM
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM 瑤池碑文/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.LYR_LAKE_STELE
        },
        {
            "A3_SG1/Room/Prefab/LootProvider/DroneHackElevatorPlatform FSM 10x/Elevator_TwoPoint_x10 FSM Variant/FSM Animator/View/ElevatorPlatform FSM/FSM Animator/View/MovingPlatformBase/Platform/[Mech]SealedBoxTreasure FSM Interactable Variant/FSM Animator/View/SealedBox_view/LogicRoot/Loot Spawner_1",
            Location.LYR_CHEST_NYMPH_ROOM
        },
        {
            "A3_S1/Room/Prefab/寶箱 Chests/LootProvider 小錢袋###0/BR_TreasureDing_M (2)/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.LYR_CHEST_RIGHT_EXIT
        },

        {
            "A3_S2/Room/Prefab/寶箱 Chests/LootProvider 中錢袋/Giant Treasure_A3_S2/[Mech]GiantTreasureChest FSM_InteractVer Variant/FSM Animator/View/TreasureBox_L/LogicRoot/Loot Spawner_1",
            Location.GH_CHEST_NYMPH_ROPE
        },
        {
            "A3_S2/Room/Prefab/1_DropPickable SceneObserve FSM (1)/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_WATER_REPORT
        },
        {
            "A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.GH_SHANHAI_CHIP // removal by force
        },
        {
            "A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M87_A3_S2_A3地圖魚索取記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.GH_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###3/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GH_CHEST_RIGHT_HANGING_POOL
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###4/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GH_UPPER_LEVEL_FOLIAGE
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/LootProvider 文物種子/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_UNDERWATER_VASE
        },
        {
            "A3_S2/Room/Prefab/寶箱 Chests/LootProvider 小錢袋 & 中量金/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.GH_CHEST_LEFT_HANGING_POOL
        },
        {
            "A3_S2/Room/Prefab/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_MUTATED_CROPS
        },
        {
            "A3_S2/Room/Prefab/Gameplay_8/RCGEventSharingGroup/LootProvider 貪財玉/0_DropPickable 貪財玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.GH_DROP_SHUIGUI
        },

        {
            "A3_S3/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.WOS_CHEST_HIGH_PLATFORMS_RIGHT
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/[Mech]GiantTreasureChest FSM_InteractVer Variant 大量金/FSM Animator/View/TreasureBox_L/LogicRoot/Loot Spawner_1",
            Location.WOS_CHEST_HIGH_PLATFORMS_LEFT
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Absorbed/[Action] PlayerIncreaseSkillPointAction",
            Location.WOS_FLOWER
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/Pickable_DIESunDeadbody 太陽人屍體/ItemProvider/KillSite/1_DropPickable SceneObserve FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.WOS_DGRD
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/1_DropPickable SceneObserve FSM 造水造氧管線/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.WOS_PIPELINE_PANEL
        },
        {
            "A3_S3/Room/Prefab/寶箱 Chests/LootProvider 丹藥催化器/0_DropPickable 丹藥催化器 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.WOS_SHAFT_NEAR_NODE
        },

        {
            "A3_S5_BossGouMang_GameLevel/Room/Treasure Chests 寶箱/LootProvider_文物即食口糧&中錢袋/0_DropPickable_即食口糧 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AH_CHEST_GOUMANG_1
        },
        {
            "A3_S5_BossGouMang_GameLevel/Room/Treasure Chests 寶箱/LootProvider_文物即食口糧&中錢袋/0_DropPickable_中錢袋 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.AH_CHEST_GOUMANG_2
        },
        {
            "A3_S5_BossGouMang_GameLevel/Room/Sleeppod/[CutScene]BackFromSleeppod/--[States]/FSM/[State] PlayCutScene/[Action] ItemGetUIShowAction",
            Location.AH_GOUMANG_SANCTUM
        },

        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 修練玉/0_DropPickable 修練玉 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_CHEST_UPPER_EXIT
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###5/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.YC_CHEST_UPPER_CAVES
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/1_DropPickable 耕地標示/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_FARMLAND_MARKINGS
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 大錢袋/4_DropPickable 大錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_ABOVE_MARKINGS
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 小錢袋/2_DropPickable 小錢袋 FSM Variant/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_CHEST_MIDDLE_CAVE
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/LootProvider 應龍金卵/0_DropPickable 應龍金卵 FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.YC_CAVE_EGG
        },
        {
            "A3_S7/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金###0/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.YC_NEAR_NODE
        },

        {
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得弓箭",
            Location.FSP_KUAFU_GIFT_1
        },
        {
            "AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得貫穿弓箭",
            Location.FSP_KUAFU_GIFT_2
        },
        /*
         * when Kuafu gives you these, you also receive:
         * - Azure Sand from AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得蒼砂
         *  this one is probably just the database entry?
         * - Azure Bow again??? from AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得[狀態欄]蒼弓
         * - [unnamed] from AG_S2/Room/NPCs/議會演出相關Binding/NPC_KuaFoo_Base/NPC_KuaFoo_BaseFSM/FSM Animator/LogicRoot/NPC_KuaFoo/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation_初次到達議會/[EndAction]###4/[Action] 取得弓箭能力
         * I suspect we might need to force one or more of these on to make the randomizer work right
         */
        /*
        {
            "",
            Location.FSP_SHUANSHUAN_MUSIC
        },
        {
            "",
            Location.FSP_SHUANSHUAN_PORTRAIT
        },
        {
            "",
            Location.FSP_SHENNONG_PBV_QUEST
        },
        {
            "",
            Location.FSP_SHUANSHUAN_BOOK
        },
        {
            "",
            Location.FSP_CHIYOU_BOOK
        },
        {
            "",
            Location.FSP_SHENNONG_SNAKE_QUEST
        },
        {
            "",
            Location.FSP_SHUANSHUAN_HIDDEN_POEM
        },
        {
            "",
            Location.FSP_CHEST_HALF_TREE
        },
        {
            "",
            Location.FSP_CHEST_FULL_TREE_1
        },
        {
            "",
            Location.FSP_CHEST_FULL_TREE_2
        },
        {
            "",
            Location.FSP_MUTANT_QUEST
        },
        */

        /*
        {
            "",
            Location.CC_LADY_ETHEREAL
        },
        {
            "",
            Location.CC_FLOWER_LADY_ETHEREAL
        },*/
        {
            "A7_S1/Room/NPC_GuideFish_A7/General FSM Object/--[States]/FSM/[State] ShutDownu演出/[Action] 拿到晶片",
            Location.CC_SHANHAI_CHIP // removal by force
        },
        {
            "A7_S1/Room/NPC_GuideFish_A7/General FSM Object/Animator(FSM)/LogicRoot/NPC_Talking_Controller/Config/Conversations/Conversation  晶片對話/Dialogue 晶片對話/M190_A7_S1_A7地圖魚索取記憶體_Chat02_Option1_Ans00/[Action] 買到晶片 (1)",
            Location.CC_SHANHAI_CHIP // peaceful purchase
        },
        {
            "A7_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_S 小量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.CC_CHEST_CAVES_UPPER_RIGHT
        },
        {
            "A7_S1/Room/Prefab/寶箱 Chests/BR_TreasureDing_M 中量金/BoxRoot/Breakable_Prototype/General FSM Object/FSM Animator/LogicRoot/Loot Spawner",
            Location.CC_CHEST_CAVES_CENTER
        },
        {
            "A7_S1/Room/Prefab/寶箱 Chests/LootProvider 名畫作/0_DropPickable Bag FSM/ItemProvider/DropPickable FSM Prototype/--[States]/FSM/[State] Picking/[Action] GetItem",
            Location.CC_CHEST_LEFT_EXIT
        },
        /* todo:
         * most of FSP
         *
         * OW
         * IW
         * BR
         * YH
         * 
         * FGH
         * FU
         * P
         * FMR
         * FPA
         * SH
         * AM
         * 
         * CC post Lady E
         * CTH
         * UC + LHP + GD
         * 
         * GOSY
         * GOSE
         * GOSW
         * ASP
         * 
         * ST
         * EDP
         * EDLA
         * EDS
         * NH
         * 
         * TRC
         * NKCH
         */
    };
    /* unused gopaths:
     * 
     * [Action] GetItem/[State] Picking/FSM/--[States]/DropPickable FSM Prototype/ItemProvider/RitualFlowerItem/LootProvider/After/LogicRoot/FSM Animator/SimpleCutSceneFSM_EncounterBoar (開頭介紹野豬的演出)/CullingGroup/[On]Node/LogicRoot/FSM Animator/軒軒野豬情境OnOff FSM/Room/A0_S4 gameLevel
     * for the Crimson Hibiscus dropped by the boar in the intro sequence
     *
     * AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/Butterfly_CutSceneFSM/--[States]/FSM/[State] GetButterfly/[Action]GetButterfly 玄蝶 狀態列
     * for the mystic nymph next to Yi's vital sanctum, won't be reachable once we make the FSP into a proper starting hub area
     * 
     * AG_S2/Room/Prefab/ControlRoom FSM Binding Tool/NPC_AICore_Base/NPC_AICore_Base_FSM/FSM Animator/LogicRoot/NPC_AICore_FSM/General FSM Object/--[States]/FSM/[State]  初次對話演出/[Action] 取得玉石系統
     * for the Jade System received from Abacus/Ruyi
     * 
     * AG_LeeEar_S1/Room/NPC_Lear Variant (1)/General FSM Object/--[States]/FSM/[State] 立繪結束後演出/[Action] 取得笛子
     * for Fusang Horn from 1st Lear chat
     * 
     * a whole bunch for character database entries I won't bother recording
     */

    // Receiving items from cutscenes, including:
    // - removing map chips from Shanhai 9000s by force
    [HarmonyPrefix, HarmonyPatch(typeof(ItemGetUIShowAction), "Implement")]
    static bool ItemGetUIShowAction_Implement(ItemGetUIShowAction __instance) {
        Log.Info($"ItemGetUIShowAction_Implement called on {__instance.item.Title}");

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"ItemGetUIShowAction_Implement called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        return true; // not a randomized location, let vanilla impl handle this
    }

    // Buying items from NPCs, including:
    // - peacefully buying map chips from Shanhai 9000s
    [HarmonyPrefix, HarmonyPatch(typeof(MerchandiseTradeAction), "OnStateEnterImplement")]
    static bool MerchandiseTradeAction_OnStateEnterImplement(MerchandiseTradeAction __instance) {
        Log.Info($"MerchandiseTradeAction_OnStateEnterImplement called on {__instance.merchandiseData.item.Title}");

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"MerchandiseTradeAction_OnStateEnterImplement called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        return true; // not a randomized location, let vanilla impl handle this
    }

    // Picking up chest items, enemy item drops, examining database entries
    [HarmonyPrefix, HarmonyPatch(typeof(PickItemAction), "OnStateEnterImplement")]
    static bool PickItemAction_OnStateEnterImplement(PickItemAction __instance) {
        Log.Info($"PickItemAction_OnStateEnterImplement called on {__instance.GetInstanceID()} containing: {__instance.pickItemData.name}\n{__instance.pickItemData?.Title}\n{__instance.pickItemData?.Summary}\n{__instance.pickItemData?.Description}");
        if (__instance.scheme != PickableScheme.GetItem) {
            Log.Info($"PickItemAction_OnStateEnterImplement: this is not a GetItem action, letting vanilla code handle it");
            return true;
        }

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"PickItemAction_OnStateEnterImplement called on GO: {goPath}");

        // Use Traverse to access private field
        ItemProvider itemProvider = Traverse.Create(__instance).Field("itemProvider").GetValue() as ItemProvider;

        GameFlagDescriptable gameFlagDescriptable = ((!(itemProvider != null) || !(itemProvider.item != null)) ? __instance.pickItemData : itemProvider.item);
        Log.Info($"PickItemAction_OnStateEnterImplement gfd: {gameFlagDescriptable.Title}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"PickItemAction_OnStateEnterImplement ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        //Log.Info($"PickItemAction_OnStateEnterImplement ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }

    // Absorbing tianhou flowers/tao fruits
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerIncreaseSkillPointAction), "OnStateEnterImplement")]
    static bool PlayerIncreaseSkillPointAction_OnStateEnterImplement(PlayerIncreaseSkillPointAction __instance) {
        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement called on {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        //Log.Info($"PlayerIncreaseSkillPointAction_OnStateEnterImplement ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }

    // This gets called for anything that drops items, including killing enemies, but we only use it
    // for "money-only" chests since those locations don't involve an "item" the above patch would get.
    // We patch CheckGenerateItems instead of GenerateItems because CGI only gets invoked at actual drop time, while
    // GI also gets invoked preemptively by EnterLevelReset every time the chest/enemy/etc gets loaded into a scene.
    [HarmonyPostfix, HarmonyPatch(typeof(LootSpawner), "CheckGenerateItems")]
    static void LootSpawner_CheckGenerateItems(LootSpawner __instance) {
        // Most chests and scenery objects drop a tiny amount of jin just for hitting them, separate from their content.
        // This "hit loot" almost always comes from a GO named "HitLootSpawner", so ignoring this name makes the logs a lot nicer.
        if (__instance.name == "HitLootSpawner")
            return;

        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"LootSpawner_CheckGenerateItems called on GO: {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            var dropItemPrefabs = AccessTools.FieldRefAccess<LootSpawner, List<DropItem>>("dropItemPrefabs").Invoke(__instance);
            string dropDesc = "";
            foreach (var dropItem in dropItemPrefabs) {
                if (dropItem.TryGetComponent<DropMoney>(out var dm))
                    dropDesc += $", {dm.moneyValue} jin";
                else
                    dropDesc += $", {dropItem.name} (not money)";
            }

            Log.Info($"LootSpawner_CheckGenerateItems ContainsKey() true for GO\n{goPath}\nClearing dropItemPrefabs which were {dropDesc}.");
            dropItemPrefabs.Clear();

            CheckLocation(goPathToLocation[goPath]);
        }

        //Log.Info($"LootSpawner_CheckGenerateItems ContainsKey() false");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GuideFishLogic), "ConfirmKillFish")]
    static bool GuideFishLogic_ConfirmKillFish(GuideFishLogic __instance) {
        var goPath = GetFullDisambiguatedPath(__instance.gameObject);
        Log.Info($"GuideFishLogic_ConfirmKillFish called on {goPath}");

        if (goPathToLocation.ContainsKey(goPath)) {
            Log.Info($"GuideFishLogic_ConfirmKillFish ContainsKey() true");
            CheckLocation(goPathToLocation[goPath]);
            return false;
        }

        Log.Info($"GuideFishLogic_ConfirmKillFish ContainsKey() false");
        return true; // not a randomized location, let vanilla impl handle this
    }
}
