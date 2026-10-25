using UnityEngine;

public static class GameInput
{
    public static float GetHorizontal()
    {
        return Input.GetAxis("Horizontal");
    }

    public static float GetVertical()
    {
        return Input.GetAxis("Vertical");
    }

    public static bool GetInteractDown()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    public static bool GetJumpDown()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public static bool GetBoostHeld()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public static bool GetBrakeHeld()
    {
        return Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W);
    }

    public static bool GetResetDown()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    public static bool GetDamageTestDown()
    {
        return Input.GetKeyDown(KeyCode.K);
    }

    public static bool GetFinishTestDown()
    {
        return Input.GetKeyDown(KeyCode.F);
    }
}
