using CEOWars.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CEOWars.Game
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<GameController>() != null) return;

            var gameRoot = new GameObject("CEO Wars - Runtime");
            var controller = gameRoot.AddComponent<GameController>();

            CreateLighting();
            CreateCamera();

            var worldObject = new GameObject("Office World");
            var world = worldObject.AddComponent<OfficeWorld>();
            world.Build(controller.State);
            controller.VisualsChanged += () => world.Build(controller.State);

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            var uiObject = new GameObject("CEO Wars UI");
            uiObject.AddComponent<GameUI>();
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
            var lightObject = new GameObject("Key Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.95f, 0.88f);
            lightObject.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
        }

        private static void CreateCamera()
        {
            var existing = Object.FindFirstObjectByType<Camera>();
            if (existing != null) Object.Destroy(existing.gameObject);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.055f, 0.075f);
            cameraObject.transform.position = new Vector3(10f, 11f, -12f);
            cameraObject.transform.rotation = Quaternion.Euler(32f, -38f, 0f);
        }
    }
}
