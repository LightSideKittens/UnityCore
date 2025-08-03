using System.Linq;
using UnityEngine;

namespace LSCore
{
    public static partial class LSInput
    {
        private static readonly KeyCode[] negX = { KeyCode.LeftArrow, KeyCode.A };
        private static readonly KeyCode[] posX = { KeyCode.RightArrow, KeyCode.D };
        private static readonly KeyCode[] negY = { KeyCode.DownArrow, KeyCode.S };
        private static readonly KeyCode[] posY = { KeyCode.UpArrow, KeyCode.W };

        public static void ProcessDirectionalInput(ref Vector2Int direction, float inputThreshold = 0.5f)
        {
#if USE_JOYSTICK
            Vector2 inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            direction.x = Mathf.Abs(inputAxis.x) > inputThreshold ? (int)Mathf.Sign(inputAxis.x) : 0;
            direction.y = Mathf.Abs(inputAxis.y) > inputThreshold ? (int)Mathf.Sign(inputAxis.y) : 0;
#else
            direction.x = GetKeyBasedDirection(direction.x, negX, posX);
            direction.y = GetKeyBasedDirection(direction.y, negY, posY);

            int GetKeyBasedDirection(int current, KeyCode[] negKeys, KeyCode[] posKeys)
            {
                if (negKeys.Any(Input.GetKeyDown)) return -1;
                if (posKeys.Any(Input.GetKeyDown)) return +1;

                return current switch
                {
                    -1 when negKeys.Any(Input.GetKeyUp) => posKeys.Any(Input.GetKey) ? +1 : 0,
                    +1 when posKeys.Any(Input.GetKeyUp) => negKeys.Any(Input.GetKey) ? -1 : 0,
                    _ => current
                };
            }
#endif
        }
    }
}