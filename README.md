# MeowScript
Dark Souls 1 script inserter thing. Lets you insert AI scripts more easily.

## Initial Setup
1. Download the latest release from the Releases tab.
2. Extract the zip.
3. Open MeowScript_Config.ini and make sure `DarkSoulsDataPath=` points to directory in which your Dark Souls EXE is located and that it has `IsDarkSoulsRemastered=1` if you're inserting scripts into Dark Souls Remastered and `IsDarkSoulsRemastered=0` if you're inserting scripts into Dark Souls: Prepare to Die Edition (this version **requires** the game to be unpacked by UnpackDarkSoulsForModding by HotPocketRemix. Note: the remaster works right out of the box with no additional tools.)

## Modding In An AI Script
1. Create a lua AI script. For this example we will make a custom script for Asylum Demon (script ID 223200).
    ```lua
    function MiniGreaterDemon223200Battle_Activate (ai, goal)
       --Repeatedly do the butt slam.
       goal:AddSubGoal(GOAL_COMMON_Attack, 10, 3008, TARGET_ENE_0, DIST_Middle, 0)
    end
    
    function MiniGreaterDemon223200Battle_Update (ai, goal)
       return GOAL_RESULT_Continue
    end
    
    function MiniGreaterDemon223200Battle_Terminate (ai, goal)
    end
    
    function MiniGreaterDemon223200Battle_Interupt (ai, goal)
       return false
    end
    ```
2. Add comments to the top of the script (before the first function), telling MeowScript which luabnd to insert it into and what battle goal to register:
    ```lua
    --@package: m18_01_00_00.luabnd, 223200_battle.lua
    --@battle_goal: 223200, MiniGreaterDemon223200Battle
    
    function MiniGreaterDemon223200Battle_Activate (ai, goal)
       --Repeatedly do the butt slam.
       goal:AddSubGoal(GOAL_COMMON_Attack, 10, 3008, TARGET_ENE_0, DIST_Middle, 0)
    end
    
    function MiniGreaterDemon223200Battle_Update (ai, goal)
       return GOAL_RESULT_Continue
    end
    
    function MiniGreaterDemon223200Battle_Terminate (ai, goal)
    end
    
    function MiniGreaterDemon223200Battle_Interupt (ai, goal)
       return false
    end
    ```
3. Drag your .lua script file onto MeowScript_Build.exe (or you can do `MeowScript_Build <script_file>` from command-line)
4. Reload the map ingame to see changes applied without relaunching the game! Simply *enter any loading screen* to reload all AI scripts. (Note: The map select screen in the debug build of PTDE is great for this)
5. Once you trigger a loading screen and enter the Asylum Demon fight, he should be doing the butt slam move repeatedly.
