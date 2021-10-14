using UnityEngine;

namespace MiniGames
{
    public class CrateController : MonoBehaviour
    {

        public static void InitNewCreate(Vector3 position, Quaternion rotation)
        {
            var gm = new GameObject("Crate");
            var cc = gm.AddComponent<CrateController>();
            cc.transform.position = position;
            cc.transform.rotation = rotation;
        }
    }
}
