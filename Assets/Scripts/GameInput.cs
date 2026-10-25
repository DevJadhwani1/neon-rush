using UnityEngine;

public static class GameInput
{
    public static float GetHorizontal()
    {
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) < 0.05f)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        }
        return Mathf.Clamp(h, -1f, 1f);
    }

    public static float GetVertical()
    {
        float v = Input.GetAxis("Vertical");
        if (Mathf.Abs(v) < 0.05f)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
        }
        return Mathf.Clamp(v, -1f, 1f);
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
        return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
    }

    public static bool GetBrake()
    {
        return GetBrakeHeld();
    }

    public static bool GetDrift()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl);
    }

    public static bool GetAntiGravityDown()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F);
    }

    public static bool GetResetDown()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    public static bool GetPauseDown()
    {
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
    }

    public static bool GetDamageTestDown()
    {
        return Input.GetKeyDown(KeyCode.K);
    }

    public static bool GetFinishTestDown()
    {
        return Input.GetKeyDown(KeyCode.L);
    }
}
