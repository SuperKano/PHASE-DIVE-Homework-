using UnityEngine;


public class InputManager : MonoBehaviour
{
    [Header("·¹ÀÎ ¿¬°á")]
    public Lane[] lanes; // 0~3: ÀÏ¹Ý, 4: FX-L, 5: FX-R (ÀÎ½ºÆåÅÍ¿¡¼­ ¿¬°á)

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // D
        if (Input.GetKeyDown(KeyCode.D)) lanes[0].PressKey();   // ´©¸£¸é ÄÑÁü 
        if (Input.GetKeyUp(KeyCode.D)) lanes[0].ReleaseKey(); // ¶¼¸é ²¨Áü

        // F
        if (Input.GetKeyDown(KeyCode.F)) lanes[1].PressKey();
        if (Input.GetKeyUp(KeyCode.F)) lanes[1].ReleaseKey();

        // J
        if (Input.GetKeyDown(KeyCode.J)) lanes[2].PressKey();
        if (Input.GetKeyUp(KeyCode.J)) lanes[2].ReleaseKey();

        // K
        if (Input.GetKeyDown(KeyCode.K)) lanes[3].PressKey();
        if (Input.GetKeyUp(KeyCode.K)) lanes[3].ReleaseKey();

        // Left Shift
        if (Input.GetKeyDown(KeyCode.S)) lanes[4].PressKey();
        if (Input.GetKeyUp(KeyCode.S)) lanes[4].ReleaseKey();

        // Right Shift
        if (Input.GetKeyDown(KeyCode.L)) lanes[5].PressKey();
        if (Input.GetKeyUp(KeyCode.L)) lanes[5].ReleaseKey();
    }
}
