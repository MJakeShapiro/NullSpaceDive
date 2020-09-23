using NaughtyAttributes;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Properties
    public static GameManager instance;
    [Header("Utility")]
    public bool runDataTestsOnAwake = true;
    [OnValueChanged("SetTargetFrameRate")]
    [Tooltip("Limits FPS to this number, set to -1 to disable")]
    public int targetFPS = -1;
    [OnValueChanged("SetBulletDrawMode")]
    public Utility.DrawMode bulletDrawMode = 0;
    #endregion Properties

    #region UtilityHandlers
    private void SetBulletDrawMode()
    {
        Debug.Log("Setting bullet-path DrawMode to " + bulletDrawMode);
        Utility._bulletDrawMode = bulletDrawMode;
    }

    private void SetTargetFrameRate()
    {
        if (targetFPS > 0)
        {
            Debug.LogWarning("New target frame rate: " + targetFPS + "fps");
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFPS;
        }
        else
            Application.targetFrameRate = -1;
    }
#endregion

    #region Initialization
    private void Awake ()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogError("A second GameManager was detected! Time: " + Time.time + "\n" + this);
            Destroy(gameObject);
            return;
        }

        SetBulletDrawMode();
        SetTargetFrameRate();

        if (runDataTestsOnAwake)
            RunDataTests();
    }

    /// <summary>
    /// Runs a series of tests to ensure the games files are set-up properly.
    /// <para>Logs the results in Unitys Debug window</para>
    /// </summary>
    [Button]
    void RunDataTests ()
    {
        Debug.Log("===================\nStarting Tests!");
        System.DateTime startTime = System.DateTime.Now;
            
        System.DateTime equipmentTime = System.DateTime.Now;
        if (!EquipmentManager.TestAllWeapons(out int infractions, out int totalCases))
            Debug.LogError($"Weapons Set-up: Failed {infractions}/{totalCases} cases\nTimeElapsed: {System.DateTime.Now.Subtract(equipmentTime).TotalMilliseconds}ms");
        else
            Debug.Log($"Weapons Set-up: Passed {totalCases} cases\nTimeElapsed: {System.DateTime.Now.Subtract(equipmentTime).TotalMilliseconds}ms");

        System.DateTime projModTime = System.DateTime.Now;
        if (!ProjMod.TestModTypes(out infractions, out totalCases))
            Debug.LogError($"ProjMod Set-up: Failed {infractions}/{totalCases} cases\nTimeElapsed: {System.DateTime.Now.Subtract(projModTime).TotalMilliseconds} ms");
        else
            Debug.Log($"ProjMod Set-up: Passed {totalCases} cases\nTimeElapsed: {System.DateTime.Now.Subtract(projModTime).TotalMilliseconds}ms");

        double elapsedTime = System.DateTime.Now.Subtract(startTime).TotalMilliseconds;
        Debug.Log("Tests complete! Elapsed time: " + elapsedTime + "ms\n===================");
    }

    [Button]
    public void RunMiscTests () // A temp debug class, throw whatever stuff you wanna test in here. Overwrite away!
    {
        Debug.Log ("===================\nStarting Misc test now!");

        ///=================================///

        int count = 100000; // How many times to run the test
        float[] rand = new float[count];
        float[] modd = new float[count];
        for (int i = 0; i < count; i++)
            rand[i] = Random.value;

        System.DateTime miscTime = System.DateTime.Now;
        for (int i = 0; i < count; i++)
        {
            float rdm = Random.value;
            modd[i] = Mathf.Pow(rdm, 1f / (1f - rand[i]));
        }
        double total = System.DateTime.Now.Subtract(miscTime).TotalMilliseconds;
        Debug.Log("1 - Total: " + total + "ms, Individual: " + (total / count).ToString("f7") + "ms");

        ///=================================///

        modd = new float[count];
        System.DateTime miscTime2 = System.DateTime.Now;
        for (int i = 0; i < count; i++)
        {
            float rdm = Random.value;
            if (rdm <= 0.5f + rand[i] / 2)
                modd[i] = rdm * (1 - rand[i]);
            else
                modd[i] = 1 - (1 / (1 - rand[i]) * (1 - rdm));
        }
        double total2 = System.DateTime.Now.Subtract(miscTime2).TotalMilliseconds;
        Debug.Log("2 - Total: " + total2 + "ms, Individual: " + (total2/count).ToString("f7") + "ms");

        ///=================================///
        
        if (total<total2)
            Debug.LogWarning("Method 1 is " + (total2 / total).ToString("f3") + " times faster than method 2\n===================");
        else
            Debug.LogWarning("Method 2 is " + (total / total2).ToString("f3") + " times faster than method 1\n===================");
    }
    #endregion Initialization

    [System.Serializable]
    public class Utility
    {
        #region Properties
        public static bool drawDebug = true;
        public static DrawMode _bulletDrawMode;
        static readonly Color _pathColor = new Color(0.5f, 0.5f, 0.5f, 0.55f);
        static readonly float pathDuration = 2;
        #endregion

        #region SceneDrawer
        #region DrawCircle
        /// <summary>
        /// Draws an Ellipse with the specific paramaters using Debug.DrawLine segements
        /// <para>https://forum.unity.com/threads/solved-debug-drawline-circle-ellipse-and-rotate-locally-with-offset.331397/</para>
        /// </summary>
        public static void DrawEllipse (Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, Color color, float duration = 0, int segments = 17)
        {
            segments = Mathf.Clamp(segments, 3, 37); // Sanitize input

            float angle = 0f;
            Quaternion rot = Quaternion.LookRotation(forward, up);
            Vector3 lastPoint = Vector3.zero;
            Vector3 thisPoint = Vector3.zero;

            for (int i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0)
                {
                    Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }

        /// <summary>
        /// Draws an ellipse in the scene view, normal to the Z axis
        /// </summary>
        public static void DrawEllipse2D (Vector2 center, float radiusX, float radiusY, Color color, float duration = 0, int segments = 17)
        {
            segments = Mathf.Clamp(segments, 3, 37); // Sanitize input

            float angle = 0f;
            Quaternion rot = Quaternion.identity;
            Vector2 lastPoint = Vector2.zero;
            Vector2 thisPoint = Vector2.zero;

            for (int i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0)
                    Debug.DrawLine((Vector2)(rot * lastPoint) + center, (Vector2)(rot * thisPoint) + center, color, duration);

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }

        /// <summary>
        /// Draws a circle in the scene view, normal to the z-axis
        /// </summary>
        public static void DrawCircle (Vector2 center, Color color, float radius = 0.2f, float duration = 0, int segments = 17)
        {
            DrawEllipse2D(center, radius, radius, color, duration, segments);
        }
        #endregion DrawCircle

        #region DrawX
        /// <summary>
        /// Draws an X in the scene view, normal to the z-axis
        /// </summary>
        public static void DrawX (Vector2 center, Vector2 direction, Color color, float radius = 0.2f, float duration = 0)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);

            Vector2 vec1Dir = RotateVector2(Vector2.up * radius, angle - 45);
            Vector2 vec2Dir = RotateVector2(Vector2.up * radius, angle + 45);

            Debug.DrawLine(center + vec1Dir, center - vec1Dir, color, duration);
            Debug.DrawLine(center + vec2Dir, center - vec2Dir, color, duration);
        }

        /// <summary>
        /// Draws an X in the scene view, normal to the z-axis
        /// </summary>
        public static void DrawX(Vector2 center, Color color, float radius = 0.2f, float duration = 0)
        {
            DrawX(center, Vector2.zero, color, radius, duration);
        }

        /// <summary>
        /// Draws an X in the scene view, normal to the z-axis
        /// </summary>
        public static void DrawX (Vector2 center, float radius = 0.2f, float duration = 0)
        {
            DrawX(center, Vector2.zero, Color.red, radius, duration);
        }
        #endregion DrawX

        #region DrawArrow
        /// <summary>
        /// Draws a directional arrow in the scene view, normal to the z-axis
        /// </summary>
        public static void DrawArrow (Vector2 from, Vector2 direction, Color color, float duration = 0)
        {
            float headLength = direction.magnitude/2;
            float headAngle = 30;
            Debug.DrawRay(from, direction, color, duration);
            Debug.DrawRay(from + direction, RotateVector2(-direction, headAngle).normalized*headLength, color, duration);
            Debug.DrawRay(from + direction, RotateVector2(-direction, -headAngle).normalized*headLength, color, duration);
        }
        #endregion DrawArrow

        #region DrawDirectionCircle
        /// <summary>
        /// Draws a circle with an arrow extending from its perimeter in the direction provided
        /// </summary>
        public static void DrawDirectionalCircle (Vector2 center, Vector2 direction, Color color, float radius = 0.2f, float duration = 0, int segments = 17)
        {
            DrawCircle(center, color, radius, duration, segments);
            DrawArrow(center + direction.normalized*radius, direction.normalized*radius, color, duration);
        }

        /// <summary>
        /// Draws a circle with an arrow extending from its perimeter in the direction provided
        /// </summary>
        public static void DrawDirectionalCircle(Vector2 center, Vector2 direction, float radius = 0.2f, float duration = 0)
        {
            DrawDirectionalCircle(center, direction, Color.white, radius, duration);
        }
        #endregion DrawDirection Circle
        #endregion SceneDrawer

        #region General
        /// <summary>
        /// Draws the current position in the scene view
        /// </summary>
        public static void DrawBulletPath (Vector2 center, Vector2 direction, float radius = 0.2f, bool partial = false)
        {
            Color pathColor = _pathColor;
            if (partial)
                pathColor = new Color(0.45f, 0.45f, 0.8f, 0.55f);

            switch (_bulletDrawMode)
            {
                case 0:
                    break;
                case DrawMode.Arrows:
                    DrawArrow(center, direction * Time.fixedDeltaTime, pathColor, duration: pathDuration);
                    break;
                case DrawMode.Circles:
                    DrawCircle(center, pathColor, radius, duration: pathDuration);
                    break;
                case DrawMode.DirectionalCircles:
                    DrawDirectionalCircle(center, direction, pathColor, radius, duration: pathDuration);
                    break;
                case DrawMode.Trail:
                    Debug.DrawRay(center, -direction * Time.fixedDeltaTime, pathColor, duration: pathDuration);
                    break;
                case DrawMode.TrailDotted:
                    Debug.DrawRay(center, -direction * Time.fixedDeltaTime, pathColor, duration: pathDuration);
                    DrawCircle(center, pathColor, radius/3, duration: pathDuration);
                    break;
                default:
                    Debug.LogWarning("DrawBulletPath does not currently support case " + _bulletDrawMode + "\n Go bug Staik about it");
                    break;
            }
        }

        /// <summary>
        /// Draws a path between the last and current position in the scene view
        /// </summary>
        public static void DrawBulletPath(Vector2 center, Vector2 direction, Vector2 lastPosition, float radius = 0.2f, bool partial = false)
        {
            Color pathColor = _pathColor;
            if (partial)
                pathColor = new Color(0.45f, 0.45f, 0.8f, 0.55f);

            switch (_bulletDrawMode)
            {
                case 0:
                    break;
                case DrawMode.Arrows:
                    DrawArrow(lastPosition, center-lastPosition, pathColor, duration: pathDuration);
                    break;
                case DrawMode.Circles:
                    DrawCircle(center, pathColor, radius, duration: pathDuration);
                    break;
                case DrawMode.DirectionalCircles:
                    DrawDirectionalCircle(center, direction, pathColor, radius, duration: pathDuration);
                    break;
                case DrawMode.Trail:
                    Debug.DrawLine(lastPosition, center, pathColor, duration: pathDuration);
                    break;
                case DrawMode.TrailDotted:
                    Debug.DrawLine(lastPosition, center, pathColor, duration: pathDuration);
                    DrawCircle(center, pathColor, radius/3, duration: pathDuration);
                    break;
                default:
                    Debug.LogWarning("DrawBulletPath does not currently support case " + _bulletDrawMode + "\n Go bug Staik about it");
                    break;
            }
        }

        public static Vector2 RotateVector2 (Vector2 vec, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = vec.x;
            float ty = vec.y;

            vec.x = (cos * tx) - (sin * ty);
            vec.y = (sin * tx) + (cos * ty);

            return vec;
        }
        #endregion

        #region Enums
        public enum DrawMode
        {
            None = 0,
            Arrows,
            Circles,
            DirectionalCircles,
            Trail,
            TrailDotted
        }
        #endregion
    }
}