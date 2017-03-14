using UnityEngine;
using System.Collections;

public class RadialButtonMainMenu : RadialButtons
{
    public RadialMenu mainMenu;
    public RadialMenu saveLoadMenu;

    public override void ButtonActions(int wedge)
    {
        switch (wedge)
        {
            case 0:
                mainMenu.ToggleShow();
                
                break;
        }
    }
}
