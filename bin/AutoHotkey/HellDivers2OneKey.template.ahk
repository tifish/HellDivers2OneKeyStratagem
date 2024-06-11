#Requires AutoHotkey v2.0
#SingleInstance Force
#MaxThreadsPerHotkey 2
#HotIf WinActive("HELLDIVERS™ 2")

#Include Keys.ahk

CallStratagem(keys)
{
    Send Format("{{1} down}", TriggerKey)
    Sleep 50

    Loop parse, keys
    {
        key := A_LoopField
        if (key == "↓")
        {
            key := DownKey
        }
        else if (key == "↑")
        {
            key := UpKey
        }
        else if (key == "←")
        {
            key := LeftKey
        }
        else if (key == "→")
        {
            key := RightKey
        }
        else
        {
            continue
        }

        Send Format("{{1} down}", key)
        Sleep 30
        Send Format("{{1} up}", key)
        Sleep 40
    }

    Send Format("{{1} up}", TriggerKey)
}