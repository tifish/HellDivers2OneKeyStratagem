CallStratagem(keys)
{
    global

    Send, % "{" TriggerKey " down}"
    Sleep, 50

    Loop, parse, keys
    {
        key := A_LoopField
        if (key == "↓")
            key := DownKey
        else if (key == "↑")
            key := UpKey
        else if (key == "←")
            key := LeftKey
        else if (key == "→")
            key := RightKey
        else
            continue

        Send, % "{" key " down}"
        Sleep, 30
        Send, % "{" key " up}"
        Sleep, 40
    }

    Send, % "{" TriggerKey " up}"
}